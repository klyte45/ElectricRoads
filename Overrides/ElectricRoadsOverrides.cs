using ColossalFramework;
using ColossalFramework.Plugins;
using Harmony;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.ElectricRoads.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Klyte.ElectricRoads.Overrides
{

    public class ElectricRoadsOverrides : Redirector, IRedirectable
    {

        public Redirector RedirectorInstance => this;

        private delegate Color GetColorNodePLAIDelegate(PowerLineAI obj, ushort nodeID, ref NetNode data, InfoManager.InfoMode infoMode);
        private delegate Color GetColorSegmentPLAIDelegate(PowerLineAI obj, ushort segmentID, ref NetSegment data, InfoManager.InfoMode infoMode);

        private static readonly MethodInfo origMethodColorNode = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegment = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly MethodInfo origMethodColorNodeRoadBase = typeof(PlayerNetAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegmentRoadBase = typeof(PlayerNetAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly GetColorNodePLAIDelegate GetColorNodePLAI = (GetColorNodePLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorNode, typeof(GetColorNodePLAIDelegate));
        private static readonly GetColorSegmentPLAIDelegate GetColorSegmentPLAI = (GetColorSegmentPLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorSegment, typeof(GetColorSegmentPLAIDelegate));

        private static readonly PowerLineAI defPLAI = new PowerLineAI();

        public static List<Type> Get81TilesFakeManagerTypes()
        {
            return Singleton<PluginManager>.instance.GetPluginsInfo().Where((PluginManager.PluginInfo pi) =>
                pi.assemblyCount > 0
                && pi.isEnabled
                && pi.GetAssemblies().Where(x => "EightyOne" == x.GetName().Name).Where(x => x.GetType("EightyOne.ResourceManagers.FakeElectricityManager") != null).Count() > 0
             ).SelectMany(pi => pi.GetAssemblies().Where(x => "EightyOne" == x.GetName().Name).Select(x => x.GetType("EightyOne.ResourceManagers.FakeElectricityManager"))).ToList();


        }

        public void Awake()
        {
            AddRedirect(origMethodColorNodeRoadBase, null, GetType().GetMethod("AfterGetColorNode", RedirectorUtils.allFlags));
            AddRedirect(origMethodColorSegmentRoadBase, null, GetType().GetMethod("AfterGetColorSegment", RedirectorUtils.allFlags));
            MethodInfo src3 = typeof(ElectricityManager).GetMethod("UpdateNodeElectricity", RedirectorUtils.allFlags);
            MethodInfo trp3 = GetType().GetMethod("DetourToCheckTransition", RedirectorUtils.allFlags);
            LogUtils.DoLog($"TRANSPILE Electric ROADS TRS: {src3} => {trp3}");
            AddRedirect(src3, null, null, trp3);
            PluginManager.instance.eventPluginsStateChanged += RecheckMods;

            if (Get81TilesFakeManagerTypes().Count > 0)
            {
                LogUtils.DoWarnLog("Loading default hooks stopped because the 81 tiles mod is active");
                return;

            }
            LogUtils.DoWarnLog("Loading default hooks");

            MethodInfo src = typeof(ElectricityManager).GetMethod("SimulationStepImpl", RedirectorUtils.allFlags);
            MethodInfo trp = GetType().GetMethod("TranspileSimulation", RedirectorUtils.allFlags);
            MethodInfo src2 = typeof(ElectricityManager).GetMethod("ConductToNode", RedirectorUtils.allFlags);
            MethodInfo trp2 = GetType().GetMethod("TranspileConduction", RedirectorUtils.allFlags);
            LogUtils.DoLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
            GetHarmonyInstance();
        }

        private static void RecheckMods()
        {
            Redirector.UnpatchAll();
            Redirector.PatchAll();
            PluginManager.instance.eventPluginsStateChanged -= RecheckMods;
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

        public static bool CheckElectricConductibility(ref NetNode node)
        {
            bool conducts = ClassesData.Instance.GetConductibility(node.Info.m_class);

            if (!conducts)
            {
                node.m_flags &= ~NetNode.Flags.Electricity;
            }

            return conducts;
        }



        public static bool CheckNodeTransition(int nodeID)
        {
            NetNode node = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID];
            return (node.m_flags & NetNode.Flags.Transition) != NetNode.Flags.None && node.Info.m_class.m_service == ItemClass.Service.Electricity;
        }


        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method) => DetourToCheckElectricConductibility(67, instr);

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method) => DetourToCheckElectricConductibility(20, instr);
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
            LogUtils.DoLog($"{ instrList[i - 1]}\n{ instrList[i]}\n{ instrList[i + 1]}\n");
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }
        private static IEnumerable<CodeInstruction> DetourToCheckTransition(IEnumerable<CodeInstruction> instr)
        {
            var instrList = instr.ToList();
            int i = 14;
            while (instrList[i].opcode != OpCodes.Brfalse)
            {
                instrList.RemoveAt(i);
            }
            instrList.InsertRange(i, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call,typeof(ElectricRoadsOverrides).GetMethod("CheckNodeTransition"))
            });
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }

    }

}
