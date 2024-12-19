using HarmonyLib;
using KeyzItemLeveling.Comps;
using UnityEngine;
using Verse;

namespace KeyzItemLeveling.HarmonyPatches;

[HarmonyPatch(typeof(Projectile))]
public static class Projectile_Patch
{
    [HarmonyPatch(nameof(Projectile.Launch), [typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef)])]
    [HarmonyPostfix]
    public static void Launch(Projectile __instance, Thing equipment)
    {
        if(equipment == null) return;
        if (!equipment.HasComp<CompItemLevelling>()) return;

        equipment.TryGetComp<CompItemLevelling>().ProcessProjectileLaunch(__instance);
    }
}
