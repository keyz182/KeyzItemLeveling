using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KeyzItemLeveling;

public class UpgradeDef: Def
{
    public ThingType forThingType;
    public UpgradeDef prerequisite;
    public int cost;
    public string iconPath;

    public bool allowRenaming = false;
    public bool allowPersona = false;

    public int order = 9999;

    public List<StatModifier> statOffsets;
    public List<StatModifier> statFactors;

    public Type workerClass = typeof(UpgradeWorker);
    protected UpgradeWorker _UpgradeWorkerInt;

    public UpgradeWorker Worker
    {
        get
        {
            return _UpgradeWorkerInt ??= Activator.CreateInstance(workerClass, [this]) as UpgradeWorker;
        }
    }
}
