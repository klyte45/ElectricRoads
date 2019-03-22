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
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.ElectricRoads.Overrides
{

    public class ElectricRoads : Redirector<ElectricRoads>
    {
        private delegate Color GetColorNodePLAIDelegate(PowerLineAI obj, ushort nodeID, ref NetNode data, InfoManager.InfoMode infoMode);
        private delegate Color GetColorSegmentPLAIDelegate(PowerLineAI obj, ushort segmentID, ref NetSegment data, InfoManager.InfoMode infoMode);

        private static readonly MethodInfo origMethodColorNode = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegment = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly MethodInfo origMethodColorNodeRoadBase = typeof(RoadBaseAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegmentRoadBase = typeof(RoadBaseAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly GetColorNodePLAIDelegate GetColorNodePLAI = (GetColorNodePLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorNode, typeof(GetColorNodePLAIDelegate));
        private static readonly GetColorSegmentPLAIDelegate GetColorSegmentPLAI = (GetColorSegmentPLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorSegment, typeof(GetColorSegmentPLAIDelegate));

        private static readonly PowerLineAI defPLAI = new PowerLineAI();

        public override void AwakeBody()
        {
            var src = typeof(ElectricityManager).GetMethod("SimulationStepImpl", allFlags);
            var trp = GetType().GetMethod("TranspileSimulation", allFlags);
            var src2 = typeof(ElectricityManager).GetMethod("ConductToNode", allFlags);
            var trp2 = GetType().GetMethod("TranspileConduction", allFlags);
            doLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            doLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
            AddRedirect(origMethodColorNodeRoadBase, null, GetType().GetMethod("AfterGetColorNode", allFlags));
            AddRedirect(origMethodColorSegmentRoadBase, null, GetType().GetMethod("AfterGetColorSegment", allFlags));
            GetHarmonyInstance();
        }

        private static void AfterGetColorNode(ref Color __result, ushort nodeID, ref NetNode data, InfoManager.InfoMode infoMode)
        {
            if (infoMode == InfoManager.InfoMode.Electricity)
            {
                __result = GetColorNodePLAI(defPLAI, nodeID, ref data, infoMode);
            }
        }
        private static void AfterGetColorSegment(ref Color __result, ushort segmentID, ref NetSegment data, InfoManager.InfoMode infoMode)
        {
            if (infoMode == InfoManager.InfoMode.Electricity)
            {
                __result = GetColorSegmentPLAI(defPLAI, segmentID, ref data, infoMode);
            }
        }

        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            return TranspileOffset(70, instr);
        }

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            return TranspileOffset(23, instr);
        }
        private static IEnumerable<CodeInstruction> TranspileOffset(int offset, IEnumerable<CodeInstruction> instr)
        {
            var instrList = instr.ToList();
            //DTBUtils.doLog($"instrList[offset+3] op:{instrList[offset + 3].opcode} { instrList[offset + 3].operand}");
            if (instrList[offset + 3].opcode != OpCodes.Ldc_I4_S)
            {
                doLog2("GAME VERSION INVALID!!!!!! WAIT FOR A FIX SOON!!!!!!!");
                return instr;
            }
            //instrList[offset + 3].operand = (sbyte)9;
            var orInstr74 = instrList[offset + 4];
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
                new CodeInstruction(instrList[offset+3])
                {
                    operand = (sbyte) 9
                }
            };
            instrList[offset + 5].labels.Add(lbl);
            instrList.InsertRange(offset + 4, insertList);
            return instrList;
        }

        public override void doLog(string text, params object[] param)
        {
            doLog2(text, param);
        }

        public static void doLog2(string text, params object[] param)
        {
            Console.WriteLine($"ElectricRoads v{ElectricRoadsMod.version}: {text}", param);
        }
    }

}
