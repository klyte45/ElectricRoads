using Klyte.ElectricRoads;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode { get; } = ElectricRoadsMod.DebugMode;
        public static string Version { get; } = ElectricRoadsMod.Version;
        public static string ModName { get; } = ElectricRoadsMod.Instance.SimpleName;
        public static string Acronym { get; } = "ER";
        public static string ModRootFolder { get; } = ElectricRoadsController.FOLDER_PATH;

        public static string GitHubRepoPath { get; } = "klyte45/ElectricRoads";
        public static string[] AssetExtraDirectoryNames { get; } = new string[0];
        public static string[] AssetExtraFileNames { get; } = new string[0];
        public static string ModIcon { get; } = ElectricRoadsMod.Instance.IconName;
        public static string ModDllRootFolder { get; } = ElectricRoadsMod.RootFolder;
    }
}