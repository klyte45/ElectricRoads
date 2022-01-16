using Klyte.Commons.Utils;
using Klyte.ElectricRoads;
using UnityEngine;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => ElectricRoadsMod.DebugMode;
        public static string Version => ElectricRoadsMod.Version;
        public static string ModName => ElectricRoadsMod.Instance.SimpleName;
        public static string Acronym => "ER";
        public static string ModRootFolder => ElectricRoadsController.FOLDER_PATH;
        public static string ModDllRootFolder => ElectricRoadsMod.RootFolder;

        public static string[] AssetExtraDirectoryNames => new string[0];
        public static string[] AssetExtraFileNames => new string[0];
        public static string ModIcon => ElectricRoadsMod.Instance.IconName;
        public static string GitHubRepoPath => "klyte45/ElectricRoads";

        public static float UIScale { get; } = 1f;
        public static Color ModColor { get; } = ColorExtensions.FromRGB("643E00");
        public static MonoBehaviour Controller => ElectricRoadsMod.Controller;
    }
}