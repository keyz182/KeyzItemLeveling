using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace KeyzItemLeveling.Graph;

public class Node
{
    public UpgradeDef def;
    public List<Node> Parents = new List<Node>();
    public List<Node> Children = new List<Node>();

    public int Depth
    {
        get
        {
            return 1 + Children.Max(child=>child.Depth);
        }
    }
}
