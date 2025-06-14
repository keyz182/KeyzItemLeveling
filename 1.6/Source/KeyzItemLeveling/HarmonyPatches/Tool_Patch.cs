using HarmonyLib;
using KeyzItemLeveling.Comps;
using Verse;

namespace KeyzItemLeveling.HarmonyPatches;

[HarmonyPatch(typeof(Tool))]
public static class Tool_Patch
{
    [HarmonyPatch(nameof(Tool.AdjustedBaseMeleeDamageAmount), [typeof(Thing), typeof(DamageDef)])]
    [HarmonyPostfix]
    public static void AdjustedBaseMeleeDamageAmount(Tool __instance, Thing ownerEquipment, DamageDef damageDef, ref float __result)
    {
        if(damageDef is null) return;
        CompItemLevelling comp = ownerEquipment.TryGetComp<CompItemLevelling>();
        if(comp is null) return;

        __result *= comp.GetFactorForStat(damageDef.armorCategory.multStat);
    }
}
