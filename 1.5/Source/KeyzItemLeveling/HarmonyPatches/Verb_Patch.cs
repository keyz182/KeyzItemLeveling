using HarmonyLib;
using KeyzItemLeveling.Comps;
using Verse;

namespace KeyzItemLeveling.HarmonyPatches;

[HarmonyPatch(typeof(Verb))]
public static class Verb_Patch
{
    [HarmonyPatch(nameof(Verb.TryStartCastOn), [typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
    [HarmonyPostfix]
    public static void TryStartCastOn(Verb __instance, ref bool __result)
    {
        if(!__result) return;
        if (__instance.caster is not Pawn pawn) return;
        if (__instance.EquipmentSource == null || !__instance.EquipmentSource.HasComp<CompItemLevelling>()) return;

        __instance.EquipmentSource.GetComp<CompItemLevelling>().ProcessVerb(__instance);
    }
}
