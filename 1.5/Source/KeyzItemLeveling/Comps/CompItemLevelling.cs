using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.Comps;

[StaticConstructorOnStartup]
public class CompItemLevelling : ThingComp
{
    public float experience = 0;
    public HashSet<UpgradeDef> upgrades = new();

    public int tickEquipped = -1;

    public Dictionary<StatDef, float> statFactorCache = new Dictionary<StatDef, float>();
    public Dictionary<StatDef, float> statOffsetCache = new Dictionary<StatDef, float>();

    public int Level => upgrades.Count;

    public CompProperties_ItemLevelling Props => (CompProperties_ItemLevelling)props;

    public string NewName = null;

    public bool AllowRenaming => upgrades.Any(upg => upg.allowRenaming);

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref upgrades, "upgrades", LookMode.Def);
        Scribe_Values.Look(ref experience, "experience", 0);
        Scribe_Values.Look(ref tickEquipped, "tickEquipped", -1);
        Scribe_Values.Look(ref NewName, "NewName", null);
    }


    public virtual string NewLabel()
    {
        if (AllowRenaming && !NewName.NullOrEmpty()) return NewName;
        return GenLabel.ThingLabel(parent, 1);
    }

    public override string TransformLabel(string label)
    {
        if (AllowRenaming && !NewName.NullOrEmpty()) label = NewName;

        return Level < 1 ? label : $"{label} [Lvl {Level}]";
    }

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new StringBuilder();
        // sb.Append("\n");
        sb.Append("KIL_CompItemLevelling_CompInspectStringExtra".Translate(Experience, Level));

        foreach (UpgradeDef upgrade in upgrades)
        {
            sb.Append($"\n - {upgrade.label}");
        }
        return sb.ToString();
    }

    public bool IsUpgradeValid(UpgradeDef upgrade)
    {
        return Props.thingType == upgrade.forThingType && (upgrade.prerequisite == null || upgrades.Any(def => def == upgrade.prerequisite)) && upgrade.Worker.CanApply(this) && experience > AdjustedCost(upgrade);
    }

    public bool TryApplyUpgrade(UpgradeDef upgrade)
    {
        if (!IsUpgradeValid(upgrade)) return false;
        upgrade.Worker.Apply(this);
        statFactorCache.Clear();
        statOffsetCache.Clear();
        experience -= AdjustedCost(upgrade);
        return true;
    }

    public virtual int AdjustedCost(UpgradeDef upgrade)
    {
        return Mathf.RoundToInt(Mathf.Pow(1.1f, upgrades.Count + 1)) * (upgrade.cost + (2*upgrades.Count));
    }

    public override string GetDescriptionPart() => CompInspectStringExtra();


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

    public virtual void ProcessVerb(Verb verb)
    {
        ModLog.Log($"Verb processed {verb.ToString()}");
    }

    public virtual void ProcessProjectileLaunch(Projectile projectile)
    {
        ModLog.Log($"Projectile launch processed {projectile.ToString()}");
    }
    public override void Notify_Unequipped(Pawn pawn)
    {
        ApplyEquippedExperience(true);
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
        experience += KeyzItemLevelingMod.settings.ExperiencePerKill;
    }

    public override void Notify_WearerDied()
    {
    }

    public virtual float GetFactorForStat(StatDef stat)
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

    public virtual float GetOffsetForStat(StatDef stat)
    {
        if(statOffsetCache.TryGetValue(stat, out float offset)) return offset;

        offset = upgrades.Where(upg=>!upg.statOffsets.NullOrEmpty()).SelectMany(upgrade=>upgrade.statOffsets.Where(statMod => statMod.stat == stat)).Sum((statMod) => statMod.value);
        statOffsetCache[stat] = offset;

        return offset;
    }

    public virtual string GetExplanationForStat(StatDef stat)
    {
        StringBuilder sb = new StringBuilder();
        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>!upgrade.statFactors.NullOrEmpty() && upgrade.statFactors.Any(factor=>factor.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.statFactors.First(factor => factor.stat == stat).ToStringAsFactor}");
        }

        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>!upgrade.statOffsets.NullOrEmpty() && upgrade.statOffsets.Any(offset=>offset.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.statOffsets.First(offset => offset.stat == stat).ValueToStringAsOffset}");
        }

        return sb.ToString();
    }
}
