using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KeyzItemLeveling.Comps;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.HarmonyPatches;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Pawn))]
public static class Pawn_Patch
{
    private static readonly Texture2D Upgrades = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton");
    private static readonly Texture2D Add10 = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton10");
    private static readonly Texture2D Add100 = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButton100");
    private static readonly Texture2D Reset = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButtonReset");
    private static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Upgrades/KIL_UpgradeButtonRename");

    [HarmonyPatch(nameof(Pawn.GetGizmos))]
    [HarmonyPostfix]
    public static void GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        IEnumerable<CompItemLevelling> apparelGizmos = __instance.apparel.WornApparel.SelectMany(apparel => apparel.AllComps).OfType<CompItemLevelling>();
        List<CompItemLevelling> allComps = __instance.equipment.AllEquipmentListForReading.SelectMany(thing => thing.AllComps).OfType<CompItemLevelling>().Concat(apparelGizmos).ToList();

        if(!allComps.Any()) return;

        List<Gizmo> gizmos = new();

        Command_Action upgrade = new Command_Action();
        upgrade.defaultLabel = "Upgrades";
        upgrade.icon = Upgrades;
        upgrade.action = delegate()
        {
            List<FloatMenuOption> list = allComps.Select(comp => new FloatMenuOption(comp.parent.LabelCap, delegate { Find.WindowStack.Add(new UpgradeDialog(comp)); }, comp.parent, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null)).ToList();
            Find.WindowStack.Add(new FloatMenu(list));
        };

        gizmos.Add(upgrade);

        if (allComps.Any(comp => comp.AllowRenaming))
        {
            Command_Action rename = new Command_Action();
            rename.defaultLabel = "Rename";
            rename.icon = Rename;
            rename.action = delegate()
            {
                List<FloatMenuOption> list = allComps.Where(comp=>comp.AllowRenaming).Select(comp => new FloatMenuOption(comp.parent.LabelCap, delegate { Find.WindowStack.Add(new RenameDialog(comp, comp.NewLabel())); }, comp.parent, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null)).ToList();
                Find.WindowStack.Add(new FloatMenu(list));
            };
            gizmos.Add(rename);
        }

        if (Prefs.DevMode && DebugSettings.godMode)
        {
            Command_Action resetBtn = new Command_Action();
            resetBtn.defaultLabel = "Reset XP";
            resetBtn.icon = Reset;
            resetBtn.action = delegate()
            {
                List<FloatMenuOption> list = allComps.Select(comp => new FloatMenuOption(comp.parent.LabelCap, delegate { comp.experience = 0; }, comp.parent, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null)).ToList();
                Find.WindowStack.Add(new FloatMenu(list));
            };

            gizmos.Add(resetBtn);

            Command_Action add10 = new Command_Action();
            add10.defaultLabel = "+10 XP";
            add10.icon = Add10;
            add10.action = delegate()
            {
                List<FloatMenuOption> list = allComps.Select(comp => new FloatMenuOption(comp.parent.LabelCap, delegate { comp.experience+=10; }, comp.parent, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null)).ToList();
                Find.WindowStack.Add(new FloatMenu(list));
            };

            gizmos.Add(add10);

            Command_Action add100 = new Command_Action();
            add100.defaultLabel = "+100 XP";
            add100.icon = Add100;
            add100.action = delegate()
            {
                List<FloatMenuOption> list = allComps.Select(comp => new FloatMenuOption(comp.parent.LabelCap, delegate { comp.experience+=100; }, comp.parent, Color.white, MenuOptionPriority.Default, null, null, 0f, null, null)).ToList();
                Find.WindowStack.Add(new FloatMenu(list));
            };

            gizmos.Add(add100);
        }

        __result = __result.Concat(gizmos);
    }
}
