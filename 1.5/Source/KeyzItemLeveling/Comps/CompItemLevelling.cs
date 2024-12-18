﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.Comps;

[StaticConstructorOnStartup]
public class CompItemLevelling : ThingComp
{
    protected float experience = 0;
    public HashSet<UpgradeDef> upgrades = new();

    public int tickEquipped = -1;

    public Dictionary<StatDef, float> statFactorCache = new Dictionary<StatDef, float>();
    public Dictionary<StatDef, float> statOffsetCache = new Dictionary<StatDef, float>();

    public int Level => upgrades.Count;

    public CompProperties_ItemLevelling Props => (CompProperties_ItemLevelling)props;

    private static readonly Texture2D Upgrades = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton");
    private static readonly Texture2D Add10 = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton10");
    private static readonly Texture2D Add100 = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton100");
    private static readonly Texture2D Reset = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButtonReset");

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref upgrades, "upgrades", LookMode.Def);
        Scribe_Values.Look(ref experience, "experience", 0);
        Scribe_Values.Look(ref tickEquipped, "tickEquipped", -1);
    }

    public override string TransformLabel(string label) => Level < 1 ? label : $"{label} [Lvl {Level}]";

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\nKIL_CompItemLevelling_CompInspectStringExtra".Translate(Experience, Level));

        foreach (UpgradeDef upgrade in upgrades)
        {
            sb.Append($"\n - {upgrade.label}");
        }
        return sb.ToString();
    }

    public bool IsUpgradeValid(UpgradeDef upgrade)
    {
        return Props.thingType == upgrade.forThingType && (upgrade.prerequisite == null || upgrades.Any(def => def == upgrade.prerequisite)) && upgrade.Worker.CanApply(this);
    }

    public bool TryApplyUpgrade(UpgradeDef upgrade)
    {
        if (!IsUpgradeValid(upgrade)) return false;
        upgrade.Worker.Apply(this);
        statFactorCache.Clear();
        statOffsetCache.Clear();
        return true;
    }

    public virtual int AdjustedCost(UpgradeDef upgrade)
    {
        return (upgrades.Count + 1) * upgrade.cost;
    }

    public override string GetDescriptionPart() => CompInspectStringExtra();

    public virtual IEnumerable<Gizmo> GetGizmos()
    {
        Command_Action upgBtn = new Command_Action();
        upgBtn.defaultLabel = "Upgrades";
        upgBtn.icon = Upgrades;
        upgBtn.action = delegate()
        {
            Find.WindowStack.Add(new UpgradeDialog(this));
        };

        yield return upgBtn;


        if (!Prefs.DevMode || !DebugSettings.godMode) yield break;

        Command_Action resetBtn = new Command_Action();
        resetBtn.defaultLabel = "Reset XP";
        resetBtn.icon = Reset;
        resetBtn.action = delegate()
        {
            experience = 0;
        };

        yield return resetBtn;

        Command_Action add10 = new Command_Action();
        add10.defaultLabel = "+10 XP";
        add10.icon = Add10;
        add10.action = delegate()
        {
            experience += 10;
        };

        yield return add10;

        Command_Action add100 = new Command_Action();
        add100.defaultLabel = "+100 XP";
        add100.icon = Add100;
        add100.action = delegate()
        {
            experience += 100;
        };

        yield return add100;
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        experience += KeyzItemLevelingMod.settings.ExperiencePerDamageReceived * totalDamageDealt;
    }

    public override void Notify_Equipped(Pawn pawn)
    {
        if(parent.holdingOwner == null) return;
        tickEquipped = Find.TickManager.TicksGame;
    }

    public virtual string Experience
    {
        get{
            ApplyEquippedExperience();
            return $"{experience:0.0000} xp";
        }
    }

    public virtual void ApplyEquippedExperience(bool unequipped = false)
    {
        if(tickEquipped < 0) return;

        int ticks = Find.TickManager.TicksGame - tickEquipped;

        experience += (ticks / (float) GenDate.TicksPerHour) * KeyzItemLevelingMod.settings.ExperiencePerHourEquipped;

        tickEquipped = unequipped || parent.holdingOwner == null ? -1 : Find.TickManager.TicksGame;
    }

    // No good, not all things tick
    // public override void CompTick()
    // {
    //     base.CompTick();
    //     if(Find.TickManager.TicksGame % GenDate.TicksPerHour != 0) return;
    //     ApplyEquippedExperience();
    // }

    public override void Notify_Unequipped(Pawn pawn)
    {
        ApplyEquippedExperience(true);
    }

    public override void Notify_UsedVerb(Pawn pawn, Verb verb)
    {
        experience += KeyzItemLevelingMod.settings.ExperiencePerUse;
    }

    public override void Notify_UsedWeapon(Pawn pawn)
    {
        experience += KeyzItemLevelingMod.settings.ExperiencePerUse;
    }

    public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
    {
        ModLog.Log($"{parent.LabelCap} Notify_Killed {dinfo}");
    }

    public override void Notify_KilledPawn(Pawn pawn)
    {
        ModLog.Log($"{parent.LabelCap} Notify_KilledPawn {pawn.LabelCap}");
    }

    public override void Notify_WearerDied()
    {
    }

    public override float GetStatFactor(StatDef stat)
    {
        if(statFactorCache.TryGetValue(stat, out float factor)) return factor;

        float statFactor = 1f;

        foreach (StatModifier mod in upgrades.Where(upg=>!upg.statFactors.NullOrEmpty()).SelectMany(upgrade=>upgrade.statFactors.Where(statMod => statMod.stat == stat)))
        {
            statFactor *= mod.value;
        }

        statFactorCache[stat] = statFactor;

        return statFactor;
    }

    public override float GetStatOffset(StatDef stat)
    {
        if(statOffsetCache.TryGetValue(stat, out float offset)) return offset;

        offset = upgrades.Where(upg=>!upg.statOffsets.NullOrEmpty()).SelectMany(upgrade=>upgrade.statOffsets.Where(statMod => statMod.stat == stat)).Sum((statMod) => statMod.value);
        statOffsetCache[stat] = offset;

        return offset;
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
    {
        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>upgrade.statFactors.Any(factor=>factor.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.statFactors.First(factor => factor.stat == stat).ToStringAsFactor}");
        }

        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>upgrade.statOffsets.Any(offset=>offset.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.statOffsets.First(offset => offset.stat == stat).ValueToStringAsOffset}");
        }
    }
}
