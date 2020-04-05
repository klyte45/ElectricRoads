
using Klyte.ElectricRoads;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => ElectricRoadsMod.DebugMode;
        public static string Version => ElectricRoadsMod.Version;
        public static string ModName => ElectricRoadsMod.Instance.SimpleName;
        public static string Acronym => "ER";
        public static string ModRootFolder => ElectricRoadsController.FOLDER_PATH;
    }
}