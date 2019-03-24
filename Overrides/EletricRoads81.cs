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
            var offset = 55;
            var instrList = instr.ToList();
            //DTBUtils.doLog($"instrList[offset+3] op:{instrList[offset + 3].opcode} { instrList[offset + 3].operand}");
            if (instrList[offset + 8].opcode != OpCodes.Ldc_I4_S)
            {
                doLog2("GAME VERSION INVALID!!!!!! WAIT FOR A FIX SOON!!!!!!!");
                return instr;
            }
            //instrList[offset + 3].operand = (sbyte)9;
            var orInstr74 = instrList[offset + 9];
            var lbl = new Label();
            var insertList = new List<CodeInstruction>
            {
                new CodeInstruction(orInstr74)
                {
                    opcode = OpCodes.Beq,
                    operand = lbl
                },
                instrList[offset+0],
                instrList[offset+1],
                instrList[offset+2],
                instrList[offset+3],
                instrList[offset+4],
                instrList[offset+5],
                instrList[offset+6],
                instrList[offset+7],
                new CodeInstruction(instrList[offset+8])
                {
                    operand = (sbyte) 9
                }
            };
            instrList[offset + 10].labels.Add(lbl);
            instrList.InsertRange(offset + 9, insertList);
            return instrList;
        }

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr)
        {
            var offset = 20;
            var instrList = instr.ToList();
            //DTBUtils.doLog($"instrList[offset+3] op:{instrList[offset + 3].opcode} { instrList[offset + 3].operand}");
            if (instrList[offset + 4].opcode != OpCodes.Ldc_I4_S)
            {
                doLog2("GAME VERSION INVALID!!!!!! WAIT FOR A FIX SOON!!!!!!!");
                return instr;
            }
            //instrList[offset + 3].operand = (sbyte)9;
            var orInstr74 = instrList[offset + 5];
            var lbl = new Label();
            var insertList = new List<CodeInstruction>
            {
                new CodeInstruction(orInstr74)
                {
                    opcode = OpCodes.Beq,
                    operand = lbl
                },
                instrList[offset+0],
                instrList[offset+1],
                instrList[offset+2],
                instrList[offset+3],
                new CodeInstruction(instrList[offset+4])
                {
                    operand = (sbyte) 9
                }
            };
            instrList[offset + 6].labels.Add(lbl);
            instrList.InsertRange(offset + 5, insertList);
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
