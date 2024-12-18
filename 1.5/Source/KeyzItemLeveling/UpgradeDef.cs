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

    public List<StatModifier> statOffsets;
    public List<StatModifier> statFactors;

    public Type WorkerClass = typeof(UpgradeWorker);
    protected UpgradeWorker _UpgradeWorkerInt;

    public UpgradeWorker Worker
    {
        get
        {
            return _UpgradeWorkerInt ??= Activator.CreateInstance(WorkerClass, [this]) as UpgradeWorker;
        }
    }
}
