using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace KeyzItemLeveling.Comps;

public class CompItemLevelling : ThingComp
{
    public float experience = 0;
    public HashSet<UpgradeDef> upgrades = new();

    public Dictionary<StatDef, float> statFactorCache = new Dictionary<StatDef, float>();
    public Dictionary<StatDef, float> statOffsetCache = new Dictionary<StatDef, float>();

    public int Level => upgrades.Count;

    public CompProperties_ItemLevelling Props => (CompProperties_ItemLevelling)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref upgrades, "upgrades", LookMode.Def);
        Scribe_Values.Look(ref experience, "experience", 0);
    }

    public override string TransformLabel(string label) => Level < 1 ? label : $"{label} [Lvl {Level}]";

    public override string CompInspectStringExtra()
    {
        StringBuilder sb = new StringBuilder();
        TaggedString str = "KIL_CompItemLevelling_CompInspectStringExtra".Translate(experience.ToString("0.0000"), Level);
            sb.Append(str);

        foreach (UpgradeDef upgrade in upgrades)
        {
            sb.Append($"\n - {upgrade.label}");
        }
        return sb.ToString();
    }

    public bool IsUpgradeValid(UpgradeDef upgrade)
    {
        return Props.thingType == upgrade.ForThingType && (upgrade.Prerequisite == null || upgrades.Any(def => def == upgrade.Prerequisite)) && upgrade.Worker.CanApply(this);
    }

    public bool TryApplyUpgrade(UpgradeDef upgrade)
    {
        if (!IsUpgradeValid(upgrade)) return false;
        upgrade.Worker.Apply(this);
        statFactorCache.Clear();
        statOffsetCache.Clear();
        return true;
    }

    public override string GetDescriptionPart() => CompInspectStringExtra();

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra() => Enumerable.Empty<Gizmo>();

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        experience += KeyzItemLevelingMod.settings.ExperiencePerDamageReceived * totalDamageDealt;
    }


    public override void CompTick()
    {
        base.CompTick();
        if(Find.TickManager.TicksGame % GenDate.TicksPerDay != 0) return;
        if(parent.holdingOwner == null) return;

        experience += KeyzItemLevelingMod.settings.ExperiencePerHourEquipped / GenDate.TicksPerHour;
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

        foreach (StatModifier mod in upgrades.SelectMany(upgrade=>upgrade.StatFactors.Where(statMod => statMod.stat == stat)))
        {
            statFactor *= mod.value;
        }

        statFactorCache[stat] = statFactor;

        return statFactor;
    }

    public override float GetStatOffset(StatDef stat)
    {
        if(statOffsetCache.TryGetValue(stat, out float offset)) return offset;

        offset = upgrades.SelectMany(upgrade=>upgrade.StatOffsets.Where(statMod => statMod.stat == stat)).Sum((statMod) => statMod.value);
        statOffsetCache[stat] = offset;

        return offset;
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
    {
        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>upgrade.StatFactors.Any(factor=>factor.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.StatFactors.First(factor => factor.stat == stat).ToStringAsFactor}");
        }

        foreach (UpgradeDef upgradeDef in upgrades.Where(upgrade=>upgrade.StatOffsets.Any(offset=>offset.stat == stat)))
        {
            sb.AppendLine($"{upgradeDef.LabelCap} {upgradeDef.StatOffsets.First(offset => offset.stat == stat).ValueToStringAsOffset}");
        }
    }
}
