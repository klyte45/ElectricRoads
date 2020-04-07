using ColossalFramework.Globalization;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ElectricRoads.Overrides;
using System.Reflection;

[assembly: AssemblyVersion("2.0.0.8")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : BasicIUserMod<ElectricRoadsMod, ElectricRoadsController, ERPanel>
    {
        public override string SimpleName => "Electric Roads Mod";

        public override string Description => "This mod turns all roads electric conductive.";

        public override string IconName => "K45_ER_Icon";

        public override void TopSettingsUI(UIHelperExtension ext)
        {
            base.TopSettingsUI(ext);

            UIHelperExtension grpPatch = ext.AddGroupExtended("Patching information");

            ColossalFramework.UI.UILabel lblPatch = grpPatch.AddLabel("!!!");
            lblPatch.text = ShowPatchingInfo();

            grpPatch.AddButton("Details", () => ShowPatchingInfo(true));
        }

        private static string ShowPatchingInfo(bool force = false)
        {
            string text = "????";
            switch (m_currentPatched)
            {
                case 0:
                    text = "No patch applied. It should not happen!";
                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                    {
                        icon = ElectricRoadsMod.Instance.IconName,
                        title = "Something got wrong on Hooking",
                        message = "Electric Roads failed loading code detours. Please send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
                        "There's a link for a Worshop guide by <color #008800>aubergine18</color> explaining how to find your log file, depending of OS you're using." +
                            "\n\n Technical details: " + ElectricRoadsOverrides.GetAssembliesDebugString(),
                        showButton1 = true,
                        textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                        showButton2 = true,
                        textButton2 = Locale.Get("K45_ER_GO_TO_THE_GUIDE"),
                        showButton3 = true,
                        textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                        useFullWindowWidth = true
                    }, (x) =>
                    {
                        if (x == 2)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                            return false;
                        }
                        if (x == 3)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                            return false;
                        }
                        return true;
                    });
                    break;
                case PatchFlags.RegularGame:
                    text = "Regular patch applied. This happens when 81 tiles mod is not enabled.";
                    if (force || DebugMode)
                    {
                        K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                        {
                            icon = ElectricRoadsMod.Instance.IconName,
                            title = "Hooking successful - Regular game",
                            message = "Electric Roads succeed loading code detours. Regular patch applied. This happens when 81 tiles mod is not enabled.\n\n" +
                            "If you think this is wrong (like if your 81 tiles mod WAS enabled and the wrong patch was applied), feel free to send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
                            "There's a link for a Worshop guide by <color #008800>aubergine18</color> explaining how to find your log file, depending of OS you're using." +
                            ((!force && DebugMode) ? "\n\n<color #ffff00>NOTE: To disable this warning when loading the game, just turn off the debug mode in mod options.</color>" : "") +
                            "\n\n Technical details: " + ElectricRoadsOverrides.GetAssembliesDebugString(),
                            showButton1 = true,
                            textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                            showButton2 = true,
                            textButton2 = Locale.Get("K45_ER_GO_TO_THE_GUIDE"),
                            showButton3 = true,
                            textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                            useFullWindowWidth = true
                        }, (x) =>
                        {
                            if (x == 2)
                            {
                                ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                                return false;
                            }
                            if (x == 3)
                            {
                                ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                                return false;
                            }
                            return true;
                        });
                    }
                    break;
                case PatchFlags.Mod81TilesGame:
                    text = "81 tiles mod patch applied. This happens when 81 tiles mod is enabled.";
                    if (force || DebugMode)
                    {
                        K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                        {
                            icon = ElectricRoadsMod.Instance.IconName,
                            title = "Hooking successful - 81 Tiles Mod",
                            message = "Electric Roads succeed loading code detours. 81 tiles mod patch was applied. This happens when 81 tiles mod enabled.\n\n" +
                            "If you think this is wrong (like if your 81 tiles mod WAS NOT enabled and the wrong patch was applied), feel free to send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
                            "There's a link for a Worshop guide by <color #008800>aubergine18</color> explaining how to find your log file, depending of OS you're using." +
                            ((!force && DebugMode) ? "\n\n<color #ffff00>NOTE: To disable this warning when loading the game, just turn off the debug mode in mod options.</color>" : "") +
                            "\n\n Technical details: " + ElectricRoadsOverrides.GetAssembliesDebugString(),
                            showButton1 = true,
                            textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                            showButton2 = true,
                            textButton2 = Locale.Get("K45_ER_GO_TO_THE_GUIDE"),
                            showButton3 = true,
                            textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                            useFullWindowWidth = true
                        }, (x) =>
                        {
                            if (x == 2)
                            {
                                ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                                return false;
                            }
                            if (x == 3)
                            {
                                ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                                return false;
                            }
                            return true;
                        });
                    }
                    break;
                case PatchFlags.Mod81TilesGame | PatchFlags.RegularGame:
                    text = "Both regular patch AND 81 tiles mod patch applied. This should not happen!";
                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                    {
                        icon = ElectricRoadsMod.Instance.IconName,
                        title = "Exception on Hooking",
                        message = "Electric Roads loaded the two code detours at same time. This behavior is weird and never should to happen. It's like 81 tiles mod is and isn't enabled at same time. o.O <color #888888>Schrödinger's Mod</color>\n" +
                        "Please send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
                        "There's a link for a Worshop guide by <color #008800>aubergine18</color> explaining how to find your log file, depending of OS you're using." +
                            "\n\n Technical details: " + ElectricRoadsOverrides.GetAssembliesDebugString(),
                        showButton1 = true,
                        textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                        showButton2 = true,
                        textButton2 = Locale.Get("K45_ER_GO_TO_THE_GUIDE"),
                        showButton3 = true,
                        textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                        useFullWindowWidth = true
                    }, (x) =>
                    {
                        if (x == 2)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=463645931");
                            return false;
                        }
                        if (x == 3)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                            return false;
                        }
                        return true;
                    });
                    break;
            }
            return text;
        }

        internal static PatchFlags m_currentPatched;

        public enum PatchFlags
        {
            RegularGame = 0x1,
            Mod81TilesGame = 0x2
        }
    }
}
