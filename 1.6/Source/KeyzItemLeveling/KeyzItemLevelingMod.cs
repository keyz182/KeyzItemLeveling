using Verse;
using UnityEngine;
using HarmonyLib;

namespace KeyzItemLeveling;

public class KeyzItemLevelingMod : Mod
{
    public static Settings settings;

    public KeyzItemLevelingMod(ModContentPack content) : base(content)
    {
        Log.Message("Hello world from KeyzItemLeveling");

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("keyz.rimworld.KeyzItemLeveling.main");	
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "KeyzItemLeveling_SettingsCategory".Translate();
    }
}
