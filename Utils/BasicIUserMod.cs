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
    public abstract class BasicIUserMod<U> : IUserMod, ILoadingExtension
        where U : BasicIUserMod<U>, new()
    {

        public abstract string SimpleName { get; }
        public virtual bool UseGroup9 => true;
        public abstract void LoadSettings();
        public abstract void doLog(string fmt, params object[] args);
        public abstract void doErrorLog(string fmt, params object[] args);

        private GameObject topObj;
        public Transform refTransform => topObj?.transform;



        public string Name => $"{SimpleName} {version}";
        public abstract string Description { get; }


        public void OnCreated(ILoading loading)
        {

        }
        public void OnLevelLoaded(LoadMode mode)
        {
            topObj = new GameObject(typeof(U).Name);
            var typeTarg = typeof(Redirector<>);
            var instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, typeof(U));
            doLog($"{SimpleName} Redirectors: {instances.Count()}");
            foreach (Type t in instances)
            {
                topObj.AddComponent(t);
            }

        }

        public string GeneralName => $"{SimpleName} (v{version})";

        public void OnLevelUnloading()
        {
            var typeTarg = typeof(Redirector<>);
            var instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, typeof(U));
            doLog($"{SimpleName} Redirectors: {instances.Count()}");
            foreach (Type t in instances)
            {
                GameObject.Destroy((Redirector)KlyteUtils.GetPrivateStaticField("instance", t));
            }
            GameObject.Destroy(topObj);
            typeTarg = typeof(Singleton<>);
            instances = ReflectionUtils.GetSubtypesRecursive(typeTarg, typeof(U));

            foreach (Type t in instances)
            {
                GameObject.Destroy(((MonoBehaviour)KlyteUtils.GetPrivateStaticProperty("instance", t)));
            }
        }
        public virtual void OnReleased()
        {
            OnLevelUnloading();
        }

        public static string minorVersion => majorVersion + "." + typeof(U).Assembly.GetName().Version.Build;
        public static string majorVersion => typeof(U).Assembly.GetName().Version.Major + "." + typeof(U).Assembly.GetName().Version.Minor;
        public static string fullVersion => minorVersion + " r" + typeof(U).Assembly.GetName().Version.Revision;
        public static string version
        {
            get {
                if (typeof(U).Assembly.GetName().Version.Minor == 0 && typeof(U).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(U).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(U).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }

        private static U m_instance;
        public static U instance => m_instance;

        public bool needShowPopup;
        private static bool isLocaleLoaded = false;
        public static bool LocaleLoaded => isLocaleLoaded;

        
        public static bool isCityLoaded => Singleton<SimulationManager>.instance.m_metaData != null;




    }

}
