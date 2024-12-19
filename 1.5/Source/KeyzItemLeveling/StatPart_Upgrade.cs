using KeyzItemLeveling.Comps;
using RimWorld;
using Verse;

namespace KeyzItemLeveling;

public class StatPart_Upgrade: StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (!req.Thing.HasComp<CompItemLevelling>()) return;

        CompItemLevelling compItemLevelling = req.Thing.TryGetComp<CompItemLevelling>();
        val *= compItemLevelling.GetFactorForStat(parentStat);
        val += compItemLevelling.GetOffsetForStat(parentStat);
    }

    public override string ExplanationPart(StatRequest req)
    {
        if (!req.Thing.HasComp<CompItemLevelling>()) return null;

        CompItemLevelling compItemLevelling = req.Thing.TryGetComp<CompItemLevelling>();
        return compItemLevelling.GetExplanationForStat(parentStat);

    }
}
