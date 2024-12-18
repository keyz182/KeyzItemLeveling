using KeyzItemLeveling.Comps;

namespace KeyzItemLeveling;

public class UpgradeWorker(UpgradeDef def)
{
    public UpgradeDef def = def;

    public virtual bool CanApply(CompItemLevelling compItemLevelling)
    {
        return true;
    }

    public virtual bool Apply(CompItemLevelling compItemLevelling)
    {
        compItemLevelling.upgrades.Add(this.def);
        return true;
    }
}
