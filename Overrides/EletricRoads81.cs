using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Harmony;
using Klyte.ElectricRoads.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.ElectricRoads.Overrides
{

    public class ElectricRoads81 : Redirector<ElectricRoads81>
    {

        public override void AwakeBody()
        {
            if (!ElectricRoadsOverrides.Is81TilesModEnabled()) return;
            var fakeElMan = Type.GetType("EightyOne.ResourceManagers.FakeElectricityManager, EightyOne, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (fakeElMan == null) return;
            var src = fakeElMan.GetMethod("SimulationStepImpl", allFlags);
            var trp = GetType().GetMethod("TranspileSimulation", allFlags);
            var src2 = fakeElMan.GetMethod("ConductToNode", allFlags);
            var trp2 = GetType().GetMethod("TranspileConduction", allFlags);
            doLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            doLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
            GetHarmonyInstance();
        }

        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            return DetourToCheckElectricCOnductibility(59, instr);
        }

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr)
        {
            return DetourToCheckElectricCOnductibility(20, instr);
        }

        private static IEnumerable<CodeInstruction> DetourToCheckElectricCOnductibility(int offset, IEnumerable<CodeInstruction> instr)
        {
            var instrList = instr.ToList();
            int i = offset + 1;
            while (instrList[i].opcode != OpCodes.Bne_Un)
            {
                instrList.RemoveAt(i);
            }

            instrList[i].opcode = OpCodes.Brfalse;
            instrList.InsertRange(i, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call,typeof(ElectricRoadsOverrides).GetMethod("CheckElectricConductibility"))
            });

            return instrList;
        }

        public override void doLog(string text, params object[] param)
        {
            doLog2(text, param);
        }

        public static void doLog2(string text, params object[] param)
        {
            Console.WriteLine($"ElectricRoads81 v{ElectricRoadsMod.version}: {text}", param);
        }
    }

}
