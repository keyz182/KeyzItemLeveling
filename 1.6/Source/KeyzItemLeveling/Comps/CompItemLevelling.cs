using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeyzItemLeveling.Graph;
using RimWorld;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.Comps;

[StaticConstructorOnStartup]
public class CompItemLevelling : ThingComp
{
    private float _experience = 0;

    public float experience
    {
        get => _experience;
        set
        {
            // no upgrades on unworn items
            if(pawn == null) return;
            _experience = value;
            CheckShouldNotifyLevelUp();
        }
    }
    public HashSet<UpgradeDef> upgrades = new();

    public int tickEquipped = -1;

    public Dictionary<StatDef, float> statFactorCache = new();
    public Dictionary<StatDef, float> statOffsetCache = new();

    public List<ThingComp> extraComps = new();

    public Pawn pawn
    {
        get
        {
            return parent.ParentHolder switch
            {
                Pawn_EquipmentTracker tracker => tracker.pawn,
                Pawn_ApparelTracker appTracker => appTracker.pawn,
                _ => null
            };
        }
    }

    public virtual int Level => upgrades.NullOrEmpty() ? 0 : upgrades.Count;

    public CompProperties_ItemLevelling Props => (CompProperties_ItemLevelling)props;

    public string NewName = null;

    public virtual bool AllowRenaming => !upgrades.NullOrEmpty() && upgrades.Any(upg => upg.allowRenaming);

    public HashSet<UpgradeDef> HaveNotifiedFor = [];

    public HashSet<UpgradeDef> _CachedAllNodes = [];

    public HashSet<UpgradeDef> CachedAllNodes
    {
        get
        {
            if (_CachedAllNodes.NullOrEmpty())
            {
                UpgradeGraph graph = UpgradeGraph.GraphForThingType(Props.thingType);
                _CachedAllNodes = graph.AllNodes.Select(node => node.def).ToHashSet();
            }

            return _CachedAllNodes;
        }
    }

    public void CheckShouldNotifyLevelUp()
    {
        if(!KeyzItemLevelingMod.settings.UpgradeAvailableNotifications) return;
        if(!pawn.Faction.IsPlayer) return;

        bool shouldNotify = false;
        foreach (UpgradeDef upgradeDef in CachedAllNodes.Where(IsUpgradeValid).Where(def=>!HaveNotifiedFor.Contains(def)))
        {
            shouldNotify = true;
            HaveNotifiedFor.Add(upgradeDef);
        }

        if (shouldNotify)
        {
            Message msg = new("KIL_UpgradeAvailable".Translate(pawn.LabelCap, parent.LabelCap), MessageTypeDefOf.PositiveEvent, new LookTargets([parent]));
            Messages.Message(msg);
        }
    }

    public void RecalculateLevelUpNotifications()
    {
        if(!KeyzItemLevelingMod.settings.UpgradeAvailableNotifications) return;
        HaveNotifiedFor = [];

        foreach (UpgradeDef upgradeDef in CachedAllNodes.Where(IsUpgradeValid))
        {
            HaveNotifiedFor.Add(upgradeDef);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref upgrades, "upgrades", LookMode.Def);
        Scribe_Values.Look(ref _experience, "experience", 0);
        Scribe_Values.Look(ref tickEquipped, "tickEquipped", -1);
        Scribe_Values.Look(ref NewName, "NewName", null);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && upgrades == null)
        {
            upgrades = new HashSet<UpgradeDef>();
            HaveNotifiedFor = new HashSet<UpgradeDef>();

            if(KeyzItemLevelingMod.settings.UpgradeAvailableNotifications) RecalculateLevelUpNotifications();
        }

        if (Scribe.mode == LoadSaveMode.LoadingVars)
        {
            if(upgrades == null) upgrades = new HashSet<UpgradeDef>();
            if(HaveNotifiedFor == null) HaveNotifiedFor = new HashSet<UpgradeDef>();
            foreach (ThingComp extraComp in upgrades.SelectMany(u=>u.Worker.GetComps(this)))
            {
                if (extraComp != null)
                {
                    extraComps.Add(extraComp);

                }
                else
                {
                    ModLog.Warn("Found null extra comp in loading");
                }
            }
            foreach (ThingComp extraComp in extraComps)
            {
                extraComp.PostExposeData();
            }
        }
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
        StringBuilder sb = new();
        // sb.Append("\n");
        sb.Append("KIL_CompItemLevelling_CompInspectStringExtra".Translate(Experience, Level));

        foreach (UpgradeDef upgrade in upgrades)
        {
            sb.Append($"\n - {upgrade.label}");
        }
        return sb.ToString();
    }

    public virtual bool IsUpgradeValid(UpgradeDef upgrade)
    {
        return Props.thingType == upgrade.forThingType
               && (upgrade.prerequisite == null || upgrades.Any(def => def == upgrade.prerequisite))
               && upgrade.Worker.CanApply(this)
               && experience > AdjustedCost(upgrade)
               && (upgrades.NullOrEmpty() || !upgrades.Contains(upgrade));
    }

    public virtual bool TryApplyUpgrade(UpgradeDef upgrade)
    {
        if (!IsUpgradeValid(upgrade)) return false;
        experience -= AdjustedCost(upgrade);
        upgrade.Worker.Apply(this);
        statFactorCache.Clear();
        statOffsetCache.Clear();

        foreach (StatDef statDef in DefDatabase<StatDef>.AllDefs)
        {
            statDef.Worker.TryClearCache();
        }

        if(KeyzItemLevelingMod.settings.UpgradeAvailableNotifications) RecalculateLevelUpNotifications();

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
        ModLog.Debug($"Verb processed {verb.ToString()}");
    }

    public virtual void ProcessProjectileLaunch(Projectile projectile)
    {
        ModLog.Debug($"Projectile launch processed {projectile.ToString()}");
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
        ModLog.Debug($"{parent.LabelCap} Notify_Killed {dinfo}");
    }

    public override void Notify_KilledPawn(Pawn pawn)
    {
        ModLog.Debug($"{parent.LabelCap} Notify_KilledPawn {pawn.LabelCap}");
        experience += KeyzItemLevelingMod.settings.ExperiencePerKill;
    }

    public override void Notify_WearerDied()
    {
    }

    public virtual float GetFactorForStat(StatDef stat)
    {
        if(statFactorCache.TryGetValue(stat, out float factor)) return factor;

        float statFactor = 1f;

        if(upgrades.NullOrEmpty()) return statFactor;

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
        if(upgrades.NullOrEmpty()) return 0f;

        offset = upgrades.Where(upg=>!upg.statOffsets.NullOrEmpty()).SelectMany(upgrade=>upgrade.statOffsets.Where(statMod => statMod.stat == stat)).Sum((statMod) => statMod.value);
        statOffsetCache[stat] = offset;

        return offset;
    }

    public virtual string GetExplanationForStat(StatDef stat)
    {
        StringBuilder sb = new();
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
