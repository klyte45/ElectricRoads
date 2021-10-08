using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.ElectricRoads.Overrides;
using System.Reflection;

[assembly: AssemblyVersion("2.1.1.0")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : BasicIUserMod<ElectricRoadsMod, ElectricRoadsController, ERPanel>
    {
        public override string SimpleName => "Electric Roads Mod";

        public override string Description => "This mod turns all roads electric conductive.";

        public override string IconName => "K45_ER_Icon";

        private bool m_dontShowUI = false;

        private bool m_mustUpdateOnOpenPanel = true;

        protected override bool LoadUI => !m_dontShowUI;

        public override void TopSettingsUI(UIHelperExtension ext)
        {
            base.TopSettingsUI(ext);

            UIHelperExtension grpPatch = ext.AddGroupExtended("Patching information");

            ColossalFramework.UI.UILabel lblPatch = grpPatch.AddLabel("!!!");
            if (!ElectricRoadsOverrides.GetAssembliesDebugString().IsNullOrWhiteSpace())
            {
                lblPatch.text = ShowPatchingInfo();
            }

            grpPatch.AddButton("Details", () => lblPatch.text = ShowPatchingInfo(true));

            var dontShowUI = grpPatch.AddCheckbox("Don't display any UI ingame", m_dontShowUI, (x) => m_dontShowUI = x) as UICheckBox;
            if (SimulationManager.instance.m_metaData != null)
            {
                dontShowUI.Disable();
                dontShowUI.text += " (Reload the game to change this option!)";
            }
            else
            {
                dontShowUI.tooltip = "Selecting this option, the mod will not display any UI ingame";
            }

            ext.Self.eventVisibilityChanged += (x, y) =>
            {
                if (y && m_mustUpdateOnOpenPanel)
                {
                    lblPatch.text = ShowPatchingInfo(false, true);
                }
            };
        }

        private static string ShowPatchingInfo(bool force = false, bool showModal = true)
        {
            string text = "????";
            switch (m_currentPatched)
            {
                case 0:
                    text = "No patch applied. It should not happen!";
                    if (showModal)
                    {
                        K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                        {
                            icon = ElectricRoadsMod.Instance.IconName,
                            title = "Something got wrong on Hooking",
                            message = "<color #FFFF00>NOTICE: If you just enabled this mod in the mod selection menu, ignore this warning and just close and open again your game to make this mod work properly!</color>\n\n" +
                        "If not, Electric Roads failed loading code detours. Please send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
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
                    }

                    break;
                case PatchFlags.RegularGame:
                    text = "Regular patch applied. This happens when 81 tiles mod is not enabled.";
                    if (force || DebugMode)
                    {
                        if (showModal)
                        {
                            K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                            {
                                icon = ElectricRoadsMod.Instance.IconName,
                                title = "Hooking successful - Regular game",
                                message = "Electric Roads succeed loading code detours. Regular patch applied. This happens when 81 tiles mod is not found on mod list.\n\n" +
                            "If you think this is wrong (like if you subscribed 81 tiles mod and the patch wasn't applied), feel free to send me a print from this screen with the output_log.txt (or player.log on Mac/Linux) in the mod Workshop page.\n\n" +
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
                    }
                    break;
                case PatchFlags.Mod81TilesGame:
                case PatchFlags.Mod81TilesGame | PatchFlags.RegularGame:
                    text = "Regular AND 81 tiles mod patches applied. This happens when 81 tiles mod is found.";
                    if (force || DebugMode)
                    {
                        if (showModal)
                        {
                            K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                            {
                                icon = ElectricRoadsMod.Instance.IconName,
                                title = "Hooking successful - Regular game + 81 Tiles Mod",
                                message = "Electric Roads succeed loading code detours. 81 tiles mod patch was applied. This happens when 81 tiles mod is found on mod list. Don't worry, if you desable it, the regular patch will be used.\n\n" +
                            ((!force && DebugMode) ? "\n\n<color #ffff00>NOTE: To disable this warning when loading the game, just turn off the debug mode in mod options.</color>" : "") +
                            "\n\n Technical details: " + ElectricRoadsOverrides.GetAssembliesDebugString(),
                                showButton1 = true,
                                textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
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
                    }               
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
