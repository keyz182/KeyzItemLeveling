using RimWorld;
using Verse;

namespace KeyzItemLeveling;

[DefOf]
public static class KeyzItemLevelingDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;
    
    static KeyzItemLevelingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(KeyzItemLevelingDefOf));
}
