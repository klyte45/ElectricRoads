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

[assembly: AssemblyVersion("1.0.0.*")]
namespace Klyte.ElectricRoads
{
    public class ElectricRoadsMod : BasicIUserMod<ElectricRoadsMod>
    {
        public override string SimpleName => "Klyte's Electric Roads Mod";

        public override string Description => "This mod turns all roads electric conductive.";

        public override void doErrorLog(string fmt, params object[] args)
        {
            KlyteUtils.doErrorLog(fmt, args);
        }

        public override void doLog(string fmt, params object[] args)
        {
            KlyteUtils.doLog(fmt, args);
        }

        public override void LoadSettings()
        {
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }



    }
}
