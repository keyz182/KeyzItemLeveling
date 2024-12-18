using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.Graph;

public class Node
{
    public UpgradeDef def;
    public Node Parent;
    public List<Node> Children = new List<Node>();

    public Vector2 Position;

    public float GetX(float BlockWidth)
    {
        return Position.x * BlockWidth;
    }

    public float GetY(float BlockHeight)
    {
        return Position.y * BlockHeight;
    }

    public int Depth
    {
        get
        {
            return 1 + (Children.NullOrEmpty() ? 0 : Children.Max(child=>child.Depth));
        }
    }

    public int Width
    {
        get
        {
            int maxWidth = 0;

            List<Node> row = Children.ToList();

            while (row.Count > 0)
            {
                maxWidth = Math.Max(maxWidth, row.Count);

                row = row.SelectMany(node=>node.Children).ToList();
            }

            return maxWidth;
        }
    }
}
