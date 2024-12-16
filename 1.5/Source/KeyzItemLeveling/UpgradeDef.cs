using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace KeyzItemLeveling;

public class UpgradeDef: Def
{
    public ThingType ForThingType;
    public UpgradeDef Prerequisite;
    public float Cost;
    public string IconPath;

    public List<StatModifier> StatOffsets;
    public List<StatModifier> StatMultipliers;

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
