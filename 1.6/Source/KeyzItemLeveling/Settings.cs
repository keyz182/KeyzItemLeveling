using UnityEngine;
using Verse;

namespace KeyzItemLeveling;

public class Settings : ModSettings
{
    public float ExperiencePerHourEquipped = 0.01f;
    public float ExperiencePerUse = 0.1f;
    public float ExperiencePerKill = 1f;
    public float ExperiencePerDamageReceived = 0.1f;
    public bool UpgradeAvailableNotifications = true;

    public void DoWindowContents(Rect wrect)
    {
        Listing_Standard options = new Listing_Standard();
        options.Begin(wrect);

        ExperiencePerHourEquipped = options.SliderLabeled("KeyzItemLeveling_Settings_ExperiencePerHourEquipped".Translate(ExperiencePerHourEquipped.ToString("0.000")), ExperiencePerHourEquipped, 0.001f, 0.1f);
        options.Gap();

        ExperiencePerUse = options.SliderLabeled("KeyzItemLeveling_Settings_ExperiencePerUse".Translate(ExperiencePerUse.ToString("0.000")), ExperiencePerUse, 0.001f, 0.1f);
        options.Gap();

        ExperiencePerKill = options.SliderLabeled("KeyzItemLeveling_Settings_ExperiencePerKill".Translate(ExperiencePerKill.ToString("0.0")), ExperiencePerKill, 0.1f, 10f);
        options.Gap();

        ExperiencePerDamageReceived = options.SliderLabeled("KeyzItemLeveling_Settings_ExperiencePerDamageReceived".Translate(ExperiencePerDamageReceived.ToString("0.000")), ExperiencePerDamageReceived, 0.001f, 0.1f);
        options.Gap();

        options.CheckboxLabeled("KeyzItemLeveling_Settings_UpgradeAvailableNotifications".Translate(), ref UpgradeAvailableNotifications);
        options.Gap();



        options.End();
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref ExperiencePerHourEquipped, "ExperiencePerHourEquipped", 0.01f);
        Scribe_Values.Look(ref ExperiencePerUse, "ExperiencePerUse", 0.1f);
        Scribe_Values.Look(ref ExperiencePerUse, "ExperiencePerKill", 1f);
        Scribe_Values.Look(ref ExperiencePerUse, "ExperiencePerDamageReceived", 0.1f);
        Scribe_Values.Look(ref UpgradeAvailableNotifications, "UpgradeAvailableNotifications", true);
    }
}
