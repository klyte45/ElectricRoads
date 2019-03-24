using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.ElectricRoads.Interfaces;
using Klyte.ElectricRoads.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.1.0.0")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : IUserMod
    {
        public string SimpleName => "Klyte's Electric Roads Mod";

        public string Description => "This mod turns all roads electric conductive.";

        public string Name => $"{SimpleName} {version}";


        public static string minorVersion => majorVersion + "." + typeof(ElectricRoadsMod).Assembly.GetName().Version.Build;
        public static string majorVersion => typeof(ElectricRoadsMod).Assembly.GetName().Version.Major + "." + typeof(ElectricRoadsMod).Assembly.GetName().Version.Minor;
        public static string fullVersion => minorVersion + " r" + typeof(ElectricRoadsMod).Assembly.GetName().Version.Revision;
        public static string version
        {
            get {
                if (typeof(ElectricRoadsMod).Assembly.GetName().Version.Minor == 0 && typeof(ElectricRoadsMod).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(ElectricRoadsMod).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(ElectricRoadsMod).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }

    }
}
