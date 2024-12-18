using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KeyzItemLeveling.Comps;
using Verse;

namespace KeyzItemLeveling.HarmonyPatches;

[HarmonyPatch(typeof(Pawn))]
public static class Pawn_Patch
{
    [HarmonyPatch(nameof(Pawn.GetGizmos))]
    [HarmonyPostfix]
    public static void GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
    {
        IEnumerable<Gizmo> apparelGizmos = __instance.apparel.WornApparel.SelectMany(apparel => apparel.AllComps).OfType<CompItemLevelling>().SelectMany(comp=>comp.GetGizmos());
        IEnumerable<Gizmo> weaponGizmos = __instance.equipment.AllEquipmentListForReading.SelectMany(thing => thing.AllComps).OfType<CompItemLevelling>().SelectMany(comp=>comp.GetGizmos());
        __result = __result.Concat(apparelGizmos).Concat(weaponGizmos);
    }
}
