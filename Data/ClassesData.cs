using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Klyte.ElectricRoads.Data
{

    public class ClassesData : ExtensionInterfaceDictionaryStructValSimplImpl<ClassesData, string, bool>
    {
        public override void LoadDefaults()
        {
            if (File.Exists(ElectricRoadsController.DEFAULT_CONFIG_FILE))
            {
                try
                {
                    if (Deserialize(typeof(ClassesData), File.ReadAllBytes(ElectricRoadsController.DEFAULT_CONFIG_FILE)) is ClassesData defaultData)
                    {
                        m_cachedDictDataSaved = defaultData.m_cachedDictDataSaved;
                        eventAllChanged?.Invoke();
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoErrorLog($"EXCEPTION WHILE LOADING: {e.GetType()} - {e.Message}\n {e.StackTrace}");

                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                    {
                        icon = ElectricRoadsMod.Instance.IconName,
                        title = Locale.Get("K45_ER_ERROR_LOADING_DEFAULTS_TITLE"),
                        message = string.Format(Locale.Get("K45_ER_ERROR_LOADING_DEFAULTS_MESSAGE"), ElectricRoadsController.DEFAULT_CONFIG_FILE, e.GetType(), e.Message, e.StackTrace),
                        showButton1 = true,
                        textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                        showButton2 = true,
                        textButton2 = Locale.Get("K45_ER_OPEN_FOLDER_ON_EXPLORER_BUTTON"),
                        showButton3 = true,
                        textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                        useFullWindowWidth = true
                    }, (x) =>
                    {
                        if (x == 2)
                        {
                            ColossalFramework.Utils.OpenInFileBrowser(ElectricRoadsController.FOLDER_PATH);
                            return false;
                        }
                        else if (x == 3)
                        {
                            ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                            return false;
                        }
                        return true;
                    });

                }
            }
        }

        public void SaveAsDefault() => File.WriteAllBytes(ElectricRoadsController.DEFAULT_CONFIG_FILE, Serialize());

        public bool GetConductibility(ItemClass clazz)
        {
            bool? val = SafeGet(clazz.name);
            bool conducts = val ?? GetDefaultValueFor(clazz);
            if (val == null)
            {
                SafeSet(clazz.name, conducts);
            }

            return conducts;
        }

        public void SetConductibility(ItemClass clazz, bool value) => SafeSet(clazz.name, value);

        private static bool GetDefaultValueFor(ItemClass m_class)
        {
            return m_class.m_service == ItemClass.Service.Electricity
                || m_class.m_service == ItemClass.Service.Road
                || m_class.m_service == ItemClass.Service.Beautification
                || (m_class.m_service == ItemClass.Service.PublicTransport
                    && (m_class.m_subService == ItemClass.SubService.PublicTransportTrain
                        || m_class.m_subService == ItemClass.SubService.PublicTransportTram
                        || m_class.m_subService == ItemClass.SubService.PublicTransportMonorail
                        || m_class.m_subService == ItemClass.SubService.PublicTransportMetro
                        || m_class.m_subService == ItemClass.SubService.PublicTransportPlane)
                    && (m_class.m_layer == ItemClass.Layer.Default || m_class.m_layer == ItemClass.Layer.MetroTunnels));
        }
        public override string SaveId => "K45_ER_ClassesData";

        public event Action eventAllChanged;

        internal void SelectAll()
        {
            var keys = m_cachedDictDataSaved.Keys.ToList();
            foreach (string item in keys)
            {
                m_cachedDictDataSaved[item] = true;
            }
            eventAllChanged?.Invoke();
        }
        internal void UnselectAll()
        {
            var keys = m_cachedDictDataSaved.Keys.ToList();
            foreach (string item in keys)
            {
                m_cachedDictDataSaved[item] = false;
            }
            eventAllChanged?.Invoke();
        }
        internal void SafeCleanAll(IEnumerable<ItemClass> items)
        {
            foreach (ItemClass item in items)
            {
                m_cachedDictDataSaved[item.name] = GetDefaultValueFor(item);
            }
            eventAllChanged?.Invoke();

        }
    }

}
