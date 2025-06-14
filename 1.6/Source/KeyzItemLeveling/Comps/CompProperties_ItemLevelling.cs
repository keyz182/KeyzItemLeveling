using Verse;

namespace KeyzItemLeveling.Comps;

public class CompProperties_ItemLevelling: CompProperties
{
    public ThingType thingType;

    public CompProperties_ItemLevelling()
    {
        compClass = typeof(CompItemLevelling);
    }
}
