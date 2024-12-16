using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KeyzItemLeveling.Graph;

public class UpgradeGraph(ThingType thingType)
{
    public ThingType ThingType = thingType;

    protected static Dictionary<ThingType, UpgradeGraph> _Graphs = new();

    public static UpgradeGraph GraphForThingType(ThingType thingType)
    {
        if (!_Graphs.ContainsKey(thingType))
        {
            _Graphs[thingType] = new UpgradeGraph(thingType);
            _Graphs[thingType].Initialize();
        }

        return _Graphs[thingType];
    }

    public HashSet<Node> Roots = new HashSet<Node>();

    public void Initialize()
    {
        Roots.Clear();

        List<UpgradeDef> defs = DefDatabase<UpgradeDef>.AllDefs.Where(def=>def.ForThingType == ThingType).ToList();

        List<UpgradeDef> roots = defs.Where(def => def.Prerequisite == null).ToList();

        defs = defs.Except(roots).ToList();

        foreach (UpgradeDef upgradeDef in roots)
        {
            Roots.Add(ProcessNode(ref defs, upgradeDef));
        }

        if (defs.Count > 0)
        {
            ModLog.Warn("UpgradeGraph.Initialize left orphaned upgrades:");
            foreach (UpgradeDef upgradeDef in defs)
            {
                ModLog.Warn($"- {upgradeDef.defName}");
            }
        }
    }

    public Node ProcessNode(ref List<UpgradeDef> defs, UpgradeDef upgradeDef)
    {
        Node node = new() { def = upgradeDef };

        List<UpgradeDef> children = defs.Where(def=>def.Prerequisite == upgradeDef).ToList();
        defs.RemoveAll((def) => children.Contains(def));

        foreach (UpgradeDef child in children)
        {
            node.Children.Add(ProcessNode(ref defs, child));
        }

        return node;
    }

    protected int? CachedDepth;
    protected int? CachedWidth;

    protected int CalcWidth()
    {
        int maxWidth = 0;

        List<Node> row = Roots.ToList();

        while (row.Count > 0)
        {
            maxWidth = Math.Max(maxWidth, row.Count);

            row = row.SelectMany(node=>node.Children).ToList();
        }

        return maxWidth;
    }

    public int Depth
    {
        get
        {
            CachedDepth ??= Roots.Max(node => node.Depth);
            return CachedDepth.Value;
        }
    }

    public int Width
    {
        get
        {
            CachedWidth ??= CalcWidth();
            return CachedWidth.Value;
        }
    }
}
