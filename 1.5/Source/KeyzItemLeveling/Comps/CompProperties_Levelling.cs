using Verse;

namespace KeyzItemLeveling.Comps;

public class CompProperties_Levelling: CompProperties
{
    public ThingType thingType;

    public CompProperties_Levelling()
    {
        compClass = typeof(CompItemLevelling);
    }
}
