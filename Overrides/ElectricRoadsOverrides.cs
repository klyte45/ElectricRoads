using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using Harmony;
using ICities;
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

    public class ElectricRoadsOverrides : Redirector<ElectricRoadsOverrides>
    {
        internal static Dictionary<string, SavedBool> conductsElectricity = new Dictionary<string, SavedBool>();

        private delegate Color GetColorNodePLAIDelegate(PowerLineAI obj, ushort nodeID, ref NetNode data, InfoManager.InfoMode infoMode);
        private delegate Color GetColorSegmentPLAIDelegate(PowerLineAI obj, ushort segmentID, ref NetSegment data, InfoManager.InfoMode infoMode);

        private static readonly MethodInfo origMethodColorNode = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegment = typeof(PowerLineAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly MethodInfo origMethodColorNodeRoadBase = typeof(PlayerNetAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) });
        private static readonly MethodInfo origMethodColorSegmentRoadBase = typeof(PlayerNetAI).GetMethod("GetColor", new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) });

        private static readonly GetColorNodePLAIDelegate GetColorNodePLAI = (GetColorNodePLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorNode, typeof(GetColorNodePLAIDelegate));
        private static readonly GetColorSegmentPLAIDelegate GetColorSegmentPLAI = (GetColorSegmentPLAIDelegate)ReflectionUtils.GetMethodDelegate(origMethodColorSegment, typeof(GetColorSegmentPLAIDelegate));

        private static readonly PowerLineAI defPLAI = new PowerLineAI();

        public static bool Is81TilesModEnabled()
        {
            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                var Assembly81 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == "EightyOne, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (Assembly81 != null)
                {
                    var iuserMod = Assembly81.GetType("EightyOne.Mod");
                    if (iuserMod != null)
                    {
                        if (plugin.GetInstances<IUserMod>()[0].GetType() == iuserMod)
                        {
                            return plugin.isEnabled;
                        }
                    }
                }
            }
            return false;
        }

        public override void AwakeBody()
        {
            AddRedirect(origMethodColorNodeRoadBase, null, GetType().GetMethod("AfterGetColorNode", allFlags));
            AddRedirect(origMethodColorSegmentRoadBase, null, GetType().GetMethod("AfterGetColorSegment", allFlags));
            var src3 = typeof(ElectricityManager).GetMethod("UpdateNodeElectricity", allFlags);
            var trp3 = GetType().GetMethod("DetourToCheckTransition", allFlags);
            doLog($"TRANSPILE Electric ROADS TRS: {src3} => {trp3}");
            AddRedirect(src3, null, null, trp3);

            if (Is81TilesModEnabled()) return;

            var src = typeof(ElectricityManager).GetMethod("SimulationStepImpl", allFlags);
            var trp = GetType().GetMethod("TranspileSimulation", allFlags);
            var src2 = typeof(ElectricityManager).GetMethod("ConductToNode", allFlags);
            var trp2 = GetType().GetMethod("TranspileConduction", allFlags);
            doLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            doLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
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

        public static bool CheckElectricConductibility(ref NetNode node)
        {
            var conducts = conductsElectricity.TryGetValue(node.Info.m_class.name, out SavedBool item) && item.value;
            if (!conducts) node.m_flags &= ~NetNode.Flags.Electricity;
            return conducts;
        }

        public static bool CheckNodeTransition(int nodeID)
        {
            var node = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID];
            return (node.m_flags & NetNode.Flags.Transition) != NetNode.Flags.None && node.Info.m_class.m_service == ItemClass.Service.Electricity;
        }

        public static bool getDefaultValueFor(ItemClass m_class)
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

        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            return DetourToCheckElectricConductibility(67, instr);
        }

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            return DetourToCheckElectricConductibility(20, instr);
        }
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
            doLog2($"{ instrList[i-1]}\n{ instrList[i]}\n{ instrList[i + 1]}\n");
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
