using System.Collections.Generic;
using System.Linq;
using KeyzItemLeveling.Comps;
using KeyzItemLeveling.Graph;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace KeyzItemLeveling;

public class UpgradeDialog(CompItemLevelling compItemLevelling, IWindowDrawing customWindowDrawing = null) : Window(customWindowDrawing)
{
    public static readonly float UpgradeButtonHeight = 50;
    public static readonly float UpgradeButtonWidth = 150;
    public static readonly float UpgradeButtonSpacing = 10;

    public static float UpgradeWidth => UpgradeButtonWidth + UpgradeButtonSpacing;
    public static float UpgradeHeight => UpgradeButtonHeight + UpgradeButtonSpacing;

    public UpgradeDef selectedUpgrade;

    public override Vector2 InitialSize => new Vector2(1200f, 800f);

    public Vector2 scrollPosition = Vector2.zero;

    public float ScrollViewWidth(UpgradeGraph graph)
    {
        return graph.Width * (UpgradeButtonWidth + UpgradeButtonSpacing) + UpgradeButtonWidth + UpgradeButtonSpacing;
    }

    public float ScrollViewHeight(UpgradeGraph graph)
    {
        return graph.Depth * (UpgradeButtonHeight + UpgradeButtonSpacing) + UpgradeButtonHeight + UpgradeButtonSpacing;
    }

    public override void DoWindowContents(Rect inRect)
    {
        UpgradeGraph graph = UpgradeGraph.GraphForThingType(compItemLevelling.Props.thingType);

        RectDivider window = new RectDivider(inRect, 25267574);

        RectDivider titleBar = window.NewRow(50f, marginOverride: 0f);
        // Widgets.DrawRectFast(titleBar.Rect.ContractedBy(1f), Color.blue);

        RectDivider titleLabel = titleBar.NewCol(1060f, marginOverride: 0f);
        GameFont font =  Text.Font;
        Text.Font = GameFont.Medium;
        Widgets.Label(titleLabel.Rect.ContractedBy(10f), $"<size=20%>{compItemLevelling.parent.LabelNoCount} Upgrades</size>");
        Text.Font = font;
        RectDivider CloseButton = titleBar.NewCol(100f, marginOverride: 0f);
        if (Widgets.ButtonText(CloseButton.Rect.ContractedBy(2f), $"Close"))
        {
            Close();
        }
        RectDivider contentArea = window.NewRow(inRect.height - 50f, marginOverride: 0f);
        // Widgets.DrawRectFast(contentArea.Rect.ContractedBy(1f), Color.red);

        RectDivider detailPane = contentArea.NewCol(200f, marginOverride: 0f);
        // Widgets.DrawRectFast(detailPane.Rect.ContractedBy(1f), Color.yellow);

        RectDivider upgradeLabel = detailPane.NewRow(50f, marginOverride: 0f);
        // Widgets.DrawRectFast(upgradeLabel.Rect.ContractedBy(1f), Color.green);
        Widgets.Label(upgradeLabel.Rect.ContractedBy(5f), selectedUpgrade == null ? "KIL_UpgradeDialog_LabelEmpty".Translate().ToString() : selectedUpgrade.LabelCap.ToString());

        RectDivider upgradeDescription = detailPane.NewRow(614f-60f, marginOverride: 0f);
        // Widgets.DrawRectFast(upgradeDescription.Rect.ContractedBy(1f), Color.white);
        Widgets.Label(upgradeDescription.Rect.ContractedBy(5f),  selectedUpgrade == null ? "" : "KIL_UpgradeDialog_Description".Translate(compItemLevelling.AdjustedCost(selectedUpgrade).ToString(), selectedUpgrade.description).ToString() );

        RectDivider xpLabel = detailPane.NewRow(60f, marginOverride: 0f);
        Widgets.Label(xpLabel.Rect.ContractedBy(5f), "KIL_UpgradeDialog_Experience".Translate(compItemLevelling.Experience) );
        RectDivider acceptBtn = detailPane.NewRow(50f, marginOverride: 0f);
        // Widgets.DrawRectFast(acceptBtn.Rect.ContractedBy(1f), Color.magenta);
        Rect btnRect = acceptBtn.Rect.ContractedBy(5f);
        btnRect.yMin += 10;
        if (Widgets.ButtonText(btnRect, "KIL_UpgradeDialog_Apply".Translate(), active: selectedUpgrade != null && compItemLevelling.IsUpgradeValid(selectedUpgrade)))
        {
            if (!compItemLevelling.TryApplyUpgrade(selectedUpgrade))
            {
                Messages.Message("Cannot apply this upgrade.", MessageTypeDefOf.RejectInput, false);
            }

            selectedUpgrade = null;
        }

        RectDivider treePane = contentArea.NewCol(inRect.width - detailPane.Rect.width, marginOverride: 0f);
        Widgets.DrawRectFast(treePane.Rect.ContractedBy(1f),  Color.gray);

        Rect scrollHolderRect = treePane.Rect.ContractedBy(3f);
        Widgets.DrawRectFast(scrollHolderRect, Widgets.WindowBGFillColor);

        Rect scrollRect = new Rect(0, 0, ScrollViewWidth(graph), ScrollViewHeight(graph));

        Widgets.BeginScrollView(scrollHolderRect, ref scrollPosition, scrollRect);

        try
        {
            // line pass
            foreach (Node graphRoot in graph.Roots)
            {
                List<Node> row = graphRoot.Children;
                while (row.Count > 0)
                {
                    foreach (Node node in row)
                    {
                        Vector2 parentPos = new(node.Parent.GetX(UpgradeWidth) + (UpgradeWidth/2), node.Parent.GetY(UpgradeHeight) + (UpgradeHeight/2));
                        Vector2 currentPos = new(node.GetX(UpgradeWidth) + (UpgradeWidth/2), node.GetY(UpgradeHeight) + (UpgradeHeight/2));

                        Widgets.DrawLine(parentPos, currentPos, TexUI.DefaultLineResearchColor, 3f);
                    }
                    row = row.SelectMany(nodeInRow=>nodeInRow.Children).ToList();
                }
            }

            // button pass
            foreach (Node graphRoot in graph.Roots)
            {
                List<Node> row = [graphRoot];
                while (row.Count > 0)
                {
                    foreach (Node node in row)
                    {
                        Rect rowBtn = new(node.GetX(UpgradeWidth), node.GetY(UpgradeHeight), UpgradeButtonWidth, UpgradeButtonHeight);

                        Color borderColor = TexUI.DefaultBorderResearchColor;
                        Color labelColor = Widgets.NormalOptionColor;
                        Color researchColor = TexUI.OtherActiveResearchColor;

                        int borderSize =  1;

                        if (node.def == selectedUpgrade)
                        {
                            Widgets.DrawStrongHighlight(rowBtn.ExpandedBy(4f));
                            borderSize = 2;
                            researchColor += TexUI.HighlightBgResearchColor;
                            borderColor = TexUI.BorderResearchSelectedColor;
                            labelColor = TexUI.BorderResearchSelectedColor;
                        }

                        if (compItemLevelling.upgrades.Contains(node.def))
                        {
                            researchColor = TexUI.FinishedResearchColor;
                        }else if (!compItemLevelling.IsUpgradeValid(node.def))
                        {
                            researchColor = TexUI.LockedResearchColor;
                        }

                        if (Widgets.CustomButtonText(ref rowBtn, "", researchColor, labelColor, borderColor, Widgets.WindowBGFillColor, borderSize: borderSize))
                        {
                            SoundDefOf.Click.PlayOneShotOnCamera();
                            selectedUpgrade = node.def;
                        }
                        int anchor = (int) Text.Anchor;
                        Color origTextCol = GUI.color;
                        GUI.color = labelColor;
                        Text.Anchor = TextAnchor.UpperCenter;
                        Widgets.Label(rowBtn, node.def.LabelCap);
                        GUI.color = origTextCol;
                        Text.Anchor = (TextAnchor) anchor;
                    }
                    row = row.SelectMany(nodeInRow=>nodeInRow.Children).ToList();
                }
            }
        }
        finally
        {
            Widgets.EndScrollView();
        }
    }
}
