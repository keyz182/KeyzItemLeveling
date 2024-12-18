using UnityEngine;
using Verse;

namespace KeyzItemLeveling;

public class UpgradeDialog: Window
{
    public override Vector2 InitialSize => new Vector2(800f, 600f);

    public override void DoWindowContents(Rect inRect)
    {
        RectDivider window = new RectDivider(inRect, 25267574);

        RectDivider titleBar = window.NewRow(50f, marginOverride: 0f);
        RectDivider contentArea = window.NewRow(window.Rect.height - titleBar.Rect.height, marginOverride: 0f);

        Widgets.Label(titleBar.Rect.ContractedBy(10f), "Upgrades!");

        RectDivider detailPane = contentArea.NewCol(200f, marginOverride: 0f);
        RectDivider upgradeLabel = detailPane.NewRow(50f, marginOverride: 0f);
        RectDivider upgradeDescription = detailPane.NewRow(150f, marginOverride: 0f);
        RectDivider spacer = detailPane.NewRow(350f, marginOverride: 0f);
        RectDivider acceptBtn = detailPane.NewRow(50f, marginOverride: 0f);

        Widgets.DrawRectFast(detailPane.Rect.ContractedBy(1f), Color.black);

        Widgets.TextArea(upgradeLabel.Rect.ContractedBy(2f), "Select an upgrade", true);
        Widgets.TextArea(upgradeDescription.Rect.ContractedBy(2f), "Select an upgrade", true);
        if (Widgets.ButtonText(acceptBtn, "Accept"))
        {

        }

        RectDivider treePane = contentArea.NewCol(contentArea.Rect.width - detailPane.Rect.width, marginOverride: 0f);



        throw new System.NotImplementedException();
    }
}
