using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.ElectricRoads.Interfaces;
using Klyte.ElectricRoads.Overrides;
using Klyte.ElectricRoads.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.2.3.2")]
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

        public void OnSettingsUI(UIHelperBase helper)
        {
            var classes = ((FastList<PrefabCollection<NetInfo>.PrefabData>)typeof(PrefabCollection<NetInfo>).GetField("m_scenePrefabs", Redirector.allFlags).GetValue(null))?.m_buffer?.Select(x => x.m_prefab)?.Where(x => x?.m_class != null && (x?.m_class.m_layer == ItemClass.Layer.Default || x?.m_class.m_layer == ItemClass.Layer.MetroTunnels || x?.m_class.m_layer == ItemClass.Layer.WaterPipes || x?.m_class.m_layer == ItemClass.Layer.WaterStructures)).GroupBy(x => x?.m_class.name)?.ToList();
            if (classes != null && classes.Count > 0)
            {
                ElectricRoadsOverrides.conductsElectricity = new System.Collections.Generic.Dictionary<string, ColossalFramework.SavedBool>();
                var gr = helper.AddGroup("Available net types: (check to conduct electricity)");
                foreach (var clazz in classes)
                {
                    if (!ElectricRoadsOverrides.conductsElectricity.ContainsKey(clazz.Key))
                    {
                        var itemList = clazz.ToList();
                        ElectricRoadsOverrides.conductsElectricity[clazz.Key] = new ColossalFramework.SavedBool(clazz.Key, LoadingExtensionElectric.CONFIG_FILENAME, ElectricRoadsOverrides.getDefaultValueFor(itemList[0].m_class), true);
                        var checkbox = (UICheckBox)gr.AddCheckbox(clazz.Key, ElectricRoadsOverrides.conductsElectricity[clazz.Key].value, (x) => ElectricRoadsOverrides.conductsElectricity[clazz.Key].value = x);
                        checkbox.tooltip = $"Affected assets:\n{string.Join("\n", itemList?.Select(x => Locale.Exists("NET_TITLE", x?.name) ? Locale.Get("NET_TITLE", x?.name) : x.m_isCustomContent ? null : x?.name)?.Where(x => x != null)?.ToArray())}";
                    }
                }
            }
            else
            {
                helper.AddGroup("Please load a city to select the configuration!");
            }
        }

    }
}
