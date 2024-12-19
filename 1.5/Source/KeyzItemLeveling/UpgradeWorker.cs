using KeyzItemLeveling.Comps;
using Verse;

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

    public virtual void ProcessProjectile(Projectile projectile)
    {

    }
}
