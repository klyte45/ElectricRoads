using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using System.Reflection;

[assembly: AssemblyVersion("2.0.0.*")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : BasicIUserMod<ElectricRoadsMod, ElectricRoadsController, ERPanel>
    {
        public ElectricRoadsMod() => Construct();

        public override string SimpleName => "Electric Roads Mod";

        public override string Description => "This mod turns all roads electric conductive.";

        public override string IconName => "K45_ER_Icon";

        public override void LoadSettings() { }
        public override void TopSettingsUI(UIHelperExtension ext)
        {
            //var classes = ((FastList<PrefabCollection<NetInfo>.PrefabData>)typeof(PrefabCollection<NetInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))?.m_buffer?.Select(x => x.m_prefab)?.Where(x => x?.m_class != null && (x?.m_class.m_layer == ItemClass.Layer.Default || x?.m_class.m_layer == ItemClass.Layer.MetroTunnels || x?.m_class.m_layer == ItemClass.Layer.WaterPipes || x?.m_class.m_layer == ItemClass.Layer.WaterStructures)).GroupBy(x => x?.m_class.name)?.ToList();
            //if (classes != null && classes.Count > 0)
            //{
            //    ElectricRoadsOverrides.conductsElectricity = new System.Collections.Generic.Dictionary<string, bool>();
            //    UIHelperBase gr = ext.AddGroup("Available net types: (check to conduct electricity)");
            //    foreach (IGrouping<string, NetInfo> clazz in classes)
            //    {
            //        if (!ElectricRoadsOverrides.conductsElectricity.ContainsKey(clazz.Key))
            //        {
            //            var itemList = clazz.ToList();
            //            ElectricRoadsOverrides.conductsElectricity[clazz.Key] = ElectricRoadsOverrides.getDefaultValueFor(itemList[0].m_class);
            //            var checkbox = (UICheckBox)gr.AddCheckbox(clazz.Key, ElectricRoadsOverrides.conductsElectricity[clazz.Key], (x) => ElectricRoadsOverrides.conductsElectricity[clazz.Key] = x);
            //            checkbox.tooltip = $"Affected assets:\n{string.Join("\n", itemList?.Select(x => Locale.Exists("NET_TITLE", x?.name) ? Locale.Get("NET_TITLE", x?.name) : x.m_isCustomContent ? null : x?.name)?.Where(x => x != null)?.ToArray())}";
            //        }
            //    }
            //}
            //else
            //{
            //    ext.AddGroup("Please load a city to select the configuration!");
            //}
        }
    }
}
