using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.ElectricRoads.Overrides;
using Klyte.ElectricRoads.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Klyte.ElectricRoads.Interfaces
{
    public class LoadingExtensionElectric : ILoadingExtension
    {
        internal const string CONFIG_FILENAME = "KElectricRoads";

        private GameObject topObj;

        public void OnCreated(ILoading loading)
        {
            if (GameSettings.FindSettingsFileByName(CONFIG_FILENAME) == null)
            {
                try
                {
                    SettingsFile settingFile = new SettingsFile
                    {
                        pathName = CONFIG_FILENAME

                    };
                    GameSettings.AddSettingsFile(settingFile);
                }
                catch (Exception e)
                {
                    KlyteUtils.doErrorLog($"EXCEPTION LOADING ON CREATED: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        public void OnLevelLoaded(LoadMode mode)
        {
            topObj = new GameObject(nameof(ElectricRoadsMod));
            var typeTarg = typeof(Redirector<>);
            var instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, GetType());
            KlyteUtils.doLog($"RoadElectric Redirectors: {instances.Count()}");
            foreach (Type t in instances)
            {
                topObj.AddComponent(t);
            }

        }
        public void OnLevelUnloading()
        {
            //try
            //{
            //    var typeTarg = typeof(Redirector<>);
            //    var instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, GetType());
            //    KlyteUtils.doLog($"RoadElectric Redirectors: {instances.Count()}");
            //    foreach (Type t in instances)
            //    {
            //        GameObject.Destroy((Redirector)KlyteUtils.GetPrivateStaticField("instance", t));
            //    }
            //    GameObject.Destroy(topObj);
            //    typeTarg = typeof(Singleton<>);
            //    instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, GetType());

            //    foreach (Type t in instances)
            //    {
            //        GameObject.Destroy(((MonoBehaviour)KlyteUtils.GetPrivateStaticProperty("instance", t)));
            //    }
            //}
            //catch { }
            Application.Quit();
        }
        public virtual void OnReleased()
        {
            OnLevelUnloading();
        }
    }

}
