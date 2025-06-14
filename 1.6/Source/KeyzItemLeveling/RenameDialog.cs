using System.Collections.Generic;
using System.Linq;
using KeyzItemLeveling.Comps;
using KeyzItemLeveling.Graph;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace KeyzItemLeveling;

public class RenameDialog(CompItemLevelling compItemLevelling, string name, IWindowDrawing customWindowDrawing = null) : Window(customWindowDrawing)
{
    private string _name = name;
    public override Vector2 InitialSize => new Vector2(400f, 150f);


    public override void DoWindowContents(Rect inRect)
    {
        closeOnClickedOutside = true;
        RectDivider window = new RectDivider(inRect, 25267234);

        RectDivider label = window.NewRow(24f);
        Widgets.Label(label, "KIL_RenameDialog_Label".Translate());

        RectDivider textArea = window.NewRow(32f);
        _name = Widgets.TextArea(textArea.Rect, _name);

        RectDivider buttonRow = window.NewRow(32f);
        RectDivider sparer = buttonRow.NewCol(100f);
        RectDivider button = buttonRow.NewCol(60f);

        if (Widgets.ButtonText(button.Rect, "KIL_UpgradeDialog_Apply".Translate()))
        {
            compItemLevelling.NewName = _name;
            Close();
        }
    }
}
