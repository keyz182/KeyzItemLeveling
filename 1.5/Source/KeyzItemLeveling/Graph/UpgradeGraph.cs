using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        List<UpgradeDef> defs = DefDatabase<UpgradeDef>.AllDefs.Where(def=>def.forThingType == ThingType).ToList();

        List<UpgradeDef> roots = defs.Where(def => def.prerequisite == null).ToList();

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

        RecalculatePositions();
    }

    public void RecalculatePositions()
    {
        float x = 0f;

        foreach (Node rootNode in Roots)
        {
            int columnWidth = rootNode.Width;
            float columnY = 0.5f;

            rootNode.Position = new Vector2(x + (columnWidth / 2f), columnY);

            List<Node> row = rootNode.Children;

            while (!row.NullOrEmpty())
            {
                columnY += 1;

                int spacing = columnWidth / row.Count;
                float rowX = x + (spacing/2f);

                foreach (Node node in row)
                {
                    node.Position = new Vector2(rowX, columnY);
                    rowX += spacing;
                }

                row = row.SelectMany(rowNode => rowNode.Children).ToList();
            }

            x += columnWidth;
        }
    }

    public Node ProcessNode(ref List<UpgradeDef> defs, UpgradeDef upgradeDef, Node parentNode = null)
    {
        Node node = new() { def = upgradeDef, Parent = parentNode};

        List<UpgradeDef> children = defs.Where(def=>def.prerequisite == upgradeDef).ToList();
        defs.RemoveAll((def) => children.Contains(def));

        if(children.Count <= 0) return node;
        foreach (UpgradeDef child in children)
        {
            node.Children.Add(ProcessNode(ref defs, child, node));
        }

        return node;
    }

    protected int? CachedDepth;
    protected int? CachedWidth;

    public int Depth
    {
        get
        {
            if(CachedDepth.HasValue) return CachedDepth.Value;
            if(Roots.NullOrEmpty()) return 0;
            CachedDepth = Roots.Max(node => node.Depth);
            return CachedDepth.Value;
        }
    }

    public int Width
    {
        get
        {
            if(CachedWidth.HasValue) return CachedWidth.Value;
            if(Roots.NullOrEmpty()) return 0;
            CachedWidth = Roots.Sum(node=>node.Width);
            return CachedWidth.Value;
        }
    }
}
