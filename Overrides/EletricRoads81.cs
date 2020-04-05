using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Klyte.ElectricRoads.Overrides
{

    public class ElectricRoads81 : Redirector, IRedirectable
    {

        public Redirector RedirectorInstance => this;

        public void Awake()
        {
            if (!ElectricRoadsOverrides.Is81TilesModEnabled())
            {
                return;
            }

            var fakeElMan = Type.GetType("EightyOne.ResourceManagers.FakeElectricityManager, EightyOne, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (fakeElMan == null)
            {
                return;
            }

            MethodInfo src = fakeElMan.GetMethod("SimulationStepImpl", RedirectorUtils.allFlags);
            MethodInfo trp = GetType().GetMethod("TranspileSimulation", RedirectorUtils.allFlags);
            MethodInfo src2 = fakeElMan.GetMethod("ConductToNode", RedirectorUtils.allFlags);
            MethodInfo trp2 = GetType().GetMethod("TranspileConduction", RedirectorUtils.allFlags);
            LogUtils.DoLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
            GetHarmonyInstance();
        }

        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method) => DetourToCheckElectricConductibility(59, instr);

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr) => DetourToCheckElectricConductibility(20, instr);

        private static IEnumerable<CodeInstruction> DetourToCheckElectricConductibility(int offset, IEnumerable<CodeInstruction> instr)
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
    }

}
