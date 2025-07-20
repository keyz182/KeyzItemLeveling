    using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KeyzItemLeveling.Comps;
using RimWorld;
using Verse;

namespace KeyzItemLeveling;

public class UpgradeWorker_Persona(UpgradeDef def) : UpgradeWorker(def)
{
    public static Lazy<FieldInfo> traitInfo = new(()=> AccessTools.Field(typeof(CompBladelinkWeapon), "traits"));
    public static Lazy<MethodInfo> CanAddTraitInfo = new(()=> AccessTools.Method(typeof(CompBladelinkWeapon), "CanAddTrait"));

    public override bool Apply(CompItemLevelling compItemLevelling)
    {
        bool flag = base.Apply(compItemLevelling);
        if(!flag) return false;

        Rand.PushState(compItemLevelling.parent.HashOffset());

        CompBladelinkWeapon comp = compItemLevelling.extraComps.OfType<CompBladelinkWeapon>().FirstOrDefault();
        if (comp == null)
        {
            comp = GetComp(compItemLevelling);
            compItemLevelling.extraComps.Add(comp);
        }

        if (traitInfo.Value.GetValue(comp) is not List<WeaponTraitDef> traits) traits = [];

        List<WeaponTraitDef> source = DefDatabase<WeaponTraitDef>.AllDefs.Where(x => (bool)CanAddTraitInfo.Value.Invoke(comp, [x])).ToList();
        if (source.Any())
            traits.Add(source.RandomElementByWeight(x => x.commonality));

        traitInfo.Value.SetValue(comp, traits);

        if (compItemLevelling.parent.ParentHolder is Pawn_EquipmentTracker p)
        {
            comp.CodeFor(p.pawn);
        }

        Rand.PopState();
        return true;
    }

    public List<WeaponTraitDef> GetTraits(CompItemLevelling compItemLevelling)
    {
        CompBladelinkWeapon comp = compItemLevelling.extraComps.OfType<CompBladelinkWeapon>().FirstOrDefault();
        if (comp == null) return [];
        return (List<WeaponTraitDef>)traitInfo.Value.GetValue(comp);
    }

    public void SetTraits(CompItemLevelling compItemLevelling, List<WeaponTraitDef> traits)
    {
        CompBladelinkWeapon comp = compItemLevelling.extraComps.OfType<CompBladelinkWeapon>().FirstOrDefault();
        if (comp == null) return;
        traitInfo.Value.SetValue(comp, traits);
    }

    public CompBladelinkWeapon GetComp(CompItemLevelling compItemLevelling)
    {
        CompBladelinkWeapon comp = (CompBladelinkWeapon) Activator.CreateInstance(typeof(CompBladelinkWeapon));
        CompProperties_BladelinkWeapon props = new() { biocodeOnEquip = false };
        comp.parent = compItemLevelling.parent;
        comp.Initialize(props);
        compItemLevelling.parent.AllComps.Add(comp);

        return comp;
    }

    public override IEnumerable<ThingComp> GetComps(CompItemLevelling compItemLevelling)
    {
        foreach (ThingComp thingComp in base.GetComps(compItemLevelling))
        {
            yield return thingComp;
        }

        if(compItemLevelling.extraComps.Any(c=>c.GetType() == typeof(CompBladelinkWeapon)))
            yield break;

        yield return GetComp(compItemLevelling);
    }

}
