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
                && pi.GetAssemblies().Where(x => "EightyOne" == x.GetName().Name).Where(x => x.GetType("EightyOne.ResourceManagers.FakeElectricityManager") != null).Count() > 0
             ).SelectMany(pi => pi.GetAssemblies().Where(x => "EightyOne" == x.GetName().Name).Select(x => x.GetType("EightyOne.ResourceManagers.FakeElectricityManager"))).ToList();
        }

        private static string lastAssemblyDebugString = "";

        public static string GetAssembliesDebugString() => lastAssemblyDebugString;
        public static string GenerateAssembliesDebugString() => string.Join(" | ", Singleton<PluginManager>.instance.GetPluginsInfo()
            .Where((PluginManager.PluginInfo pi) => pi.assemblyCount > 0)
            .Select(pi => $"<color {(pi.isEnabled ? "#00ff00" : "#FF0000")}>[{string.Join(",", pi.GetAssemblies().Select(x => ("EightyOne" == x.GetName().Name) ? $"*{GetStringDebugCheck81Assembly(x)}*" : GetStringDebugCheck81Assembly(x)).ToArray())}]</color>")
            .ToArray());

        private static string GetStringDebugCheck81Assembly(Assembly x) => (x.GetType("EightyOne.ResourceManagers.FakeElectricityManager") != null ? $"#{x.GetName().Name}#" : x.GetName().Name);

        public void Start()
        {
            if (GenerateAssembliesDebugString().IsNullOrWhiteSpace())
            {
                LogUtils.DoWarnLog("Fake start...");
                return;
            }

            AddRedirect(origMethodColorNodeRoadBase, null, GetType().GetMethod("AfterGetColorNode", RedirectorUtils.allFlags));
            AddRedirect(origMethodColorSegmentRoadBase, null, GetType().GetMethod("AfterGetColorSegment", RedirectorUtils.allFlags));
            MethodInfo src3 = typeof(ElectricityManager).GetMethod("UpdateNodeElectricity", RedirectorUtils.allFlags);
            MethodInfo trp3 = GetType().GetMethod("DetourToCheckTransition", RedirectorUtils.allFlags);
            LogUtils.DoLog($"TRANSPILE Electric ROADS TRS: {src3} => {trp3}");
            AddRedirect(src3, null, null, trp3);
            PluginManager.instance.eventPluginsStateChanged += RecheckMods;

            AddRedirect(typeof(PropInstance).GetMethod("RenderInstance", RedirectorUtils.allFlags, null, new Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4), typeof(Texture), typeof(Vector4), typeof(Vector4) }, null), null, null, GetType().GetMethod("LightsOnCheckDetour", RedirectorUtils.allFlags));
            AddRedirect(typeof(PropInstance).GetMethod("RenderInstance", RedirectorUtils.allFlags, null, new Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool) }, null), null, null, GetType().GetMethod("LightsOnCheckDetour", RedirectorUtils.allFlags));
            AddRedirect(typeof(LightEffect).GetMethod("PopulateGroupData", RedirectorUtils.allFlags), GetType().GetMethod("CheckElectrityNetForLight", RedirectorUtils.allFlags));
          
            ElectricRoadsMod.m_currentPatched &= ~ElectricRoadsMod.PatchFlags.Mod81TilesGame;
            ElectricRoadsMod.m_currentPatched &= ~ElectricRoadsMod.PatchFlags.RegularGame;


            lastAssemblyDebugString = GenerateAssembliesDebugString();

            LogUtils.DoWarnLog("Loading default hooks");

            MethodInfo src = typeof(ElectricityManager).GetMethod("SimulationStepImpl", RedirectorUtils.allFlags);
            MethodInfo trp = GetType().GetMethod("TranspileSimulation", RedirectorUtils.allFlags);
            MethodInfo src2 = typeof(ElectricityManager).GetMethod("ConductToNode", RedirectorUtils.allFlags);
            MethodInfo trp2 = GetType().GetMethod("TranspileConduction", RedirectorUtils.allFlags);
            MethodInfo src4 = typeof(ElectricityManager).GetMethod("UpdateNodeElectricity", RedirectorUtils.allFlags);
            MethodInfo pre4 = GetType().GetMethod("PreConduct", RedirectorUtils.allFlags);
            MethodInfo pos4 = GetType().GetMethod("PostConduct", RedirectorUtils.allFlags);
            LogUtils.DoLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
            AddRedirect(src, null, null, trp);
            LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
            AddRedirect(src2, null, null, trp2);
            LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS CHANGED: {src4} => {pre4} & {pos4}");
            AddRedirect(src4, pre4, pos4);
            GetHarmonyInstance();
            ElectricRoadsMod.m_currentPatched |= ElectricRoadsMod.PatchFlags.RegularGame;

            List<Type> targetTypes = ElectricRoadsOverrides.Get81TilesFakeManagerTypes();

            foreach (Type fakeElMan in targetTypes)
            {
                src = fakeElMan.GetMethod("SimulationStepImpl", RedirectorUtils.allFlags);
                src2 = fakeElMan.GetMethod("ConductToNode", RedirectorUtils.allFlags);
                src4 = fakeElMan.GetMethod("UpdateNodeElectricity", RedirectorUtils.allFlags);
                LogUtils.DoLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
                AddRedirect(src, null, null, trp);
                LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
                AddRedirect(src2, null, null, trp2);
                LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS CHANGED: {src4} => {pre4} & {pos4}");
                AddRedirect(src4, pre4, pos4);
                ElectricRoadsMod.m_currentPatched |= ElectricRoadsMod.PatchFlags.Mod81TilesGame;
            }
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

        public static void PreConduct(ref int nodeID, ref bool __state) => __state = (NetManager.instance.m_nodes.m_buffer[nodeID].m_flags & NetNode.Flags.Electricity) != 0;
        public static void PostConduct(int nodeID, bool __state)
        {
            if (__state != ((NetManager.instance.m_nodes.m_buffer[nodeID].m_flags & NetNode.Flags.Electricity) != 0))
            {
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[nodeID];
                for (int i = 0; i < 8; i++)
                {
                    NetManager.instance.UpdateSegmentRenderer(node.GetSegment(i), true);
                }
            }
        }




        public static bool CheckNodeTransition(int nodeID)
        {
            NetNode node = Singleton<NetManager>.instance.m_nodes.m_buffer[nodeID];
            return (node.m_flags & NetNode.Flags.Transition) != NetNode.Flags.None && node.Info.m_class.m_service == ItemClass.Service.Electricity;
        }


        private static IEnumerable<CodeInstruction> TranspileSimulation(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method) => DetourToCheckElectricConductibility(instr);

        private static IEnumerable<CodeInstruction> TranspileConduction(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method) => DetourToCheckElectricConductibility(instr);

        private static readonly FieldInfo m_serviceField = typeof(ItemClass).GetField("m_service");
        private static readonly FieldInfo m_classField = typeof(NetInfo).GetField("m_class");

        private static OpCode[] m_localCodesLd = new OpCode[]
        {
            OpCodes.Ldloc_0,
            OpCodes.Ldloc_1,
            OpCodes.Ldloc_2,
            OpCodes.Ldloc_3,
            OpCodes.Ldloc_S,
            OpCodes.Ldloc
        };

        private static OpCode[] m_localCodesSt = new OpCode[]
        {
            OpCodes.Stloc_0,
            OpCodes.Stloc_1,
            OpCodes.Stloc_2,
            OpCodes.Stloc_3,
            OpCodes.Stloc_S,
            OpCodes.Stloc
        };
        private static IEnumerable<CodeInstruction> DetourToCheckElectricConductibility(IEnumerable<CodeInstruction> instr)
        {
            var instrList = instr.ToList();
            for (int i = 2; i < instrList.Count - 2; i++)
            {

                if (instrList[i - 1].operand == m_classField
                    && instrList[i - 1].opcode == OpCodes.Ldfld
                    && instrList[i].operand == m_serviceField
                    && instrList[i].opcode == OpCodes.Ldfld)
                {

                    LogUtils.DoLog($"instrList[{i} + 1].operand - {instrList[i + 1].operand} ({instrList[i + 1].operand?.GetType()}) {instrList[i + 1].operand is IConvertible }");
                    LogUtils.DoLog($"tst == 10 = {(instrList[i + 1].operand is IConvertible x ? x.ToInt32(null) == 10 : false)}");
                    LogUtils.DoLog($"instrList[{i} + 1].opcode - {instrList[i + 1].opcode} ({instrList[i + 1].opcode == OpCodes.Ldc_I4_S})");
                    LogUtils.DoLog($"instrList[{i} + 2].opcode - {instrList[i + 2].opcode} ({instrList[i + 2].opcode == OpCodes.Bne_Un || instrList[i + 2].opcode == OpCodes.Bne_Un_S})");
                    if (instrList[i + 1].operand is IConvertible val
                    && val.ToInt32(null) == 10
                    && instrList[i + 1].opcode == OpCodes.Ldc_I4_S
                    && (instrList[i + 2].opcode == OpCodes.Bne_Un || instrList[i + 2].opcode == OpCodes.Bne_Un_S))
                    {
                        instrList[i + 1] = new CodeInstruction(OpCodes.Call, typeof(ElectricRoadsOverrides).GetMethod("CheckElectricConductibility"));
                        instrList[i + 2].opcode = OpCodes.Brfalse;

                        instrList.RemoveAt(i);
                        instrList.RemoveAt(i - 1);

                        if (m_localCodesLd.Contains(instrList[i - 2].opcode))
                        {
                            object operToSeek = instrList[i - 2].operand;
                            instrList.RemoveAt(i - 2);
                            for (int j = i - 3; j > 0; j--)
                            {
                                if (m_localCodesSt.Contains(instrList[j].opcode) && instrList[j].operand == operToSeek)
                                {
                                    instrList.RemoveAt(j);
                                    instrList.RemoveAt(j - 1);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            instrList.RemoveAt(i - 2);
                        }

                        LogUtils.DoLog($"Detour applied at {i} - {Environment.StackTrace}");
                    }
                }
            }
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }


        public static IEnumerable<CodeInstruction> DetourToCheckTransition(IEnumerable<CodeInstruction> instr)
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

        public static IEnumerable<CodeInstruction> LightsOnCheckDetour(IEnumerable<CodeInstruction> instr, ILGenerator il)
        {

            var instrList = instr.ToList();
            for (int i = 0; i < instrList.Count - 3; i++)
            {
                if (
                    instrList[i].opcode == OpCodes.Ldc_R4
                    && instrList[i + 1].opcode == OpCodes.Stloc_S
                    && instrList[i + 2].opcode == OpCodes.Ldarg_S
                )
                {
                    Label target = il.DefineLabel();
                    instrList.InsertRange(i, new CodeInstruction[]{
                        new CodeInstruction(OpCodes.Ldarg, 2),
                        new CodeInstruction(OpCodes.Call,  typeof(ElectricRoadsOverrides).GetMethod("CheckElectrityNetForLight", RedirectorUtils.allFlags)),
                        new CodeInstruction(OpCodes.Brfalse, target),
                    });
                    for (; i < instrList.Count - 3; i++)
                    {
                        if (
                      instrList[i].opcode == OpCodes.Ldarga_S
                      && instrList[i + 1].opcode == OpCodes.Ldc_R4
                  )
                        {
                            instrList[i].labels.Add(target);
                            break;
                        }
                    }
                }
            }
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }

        public static bool CheckElectrityNetForLight(InstanceID id)
        {
            if (id.NetSegment == 0)
            {
                return true;
            }
            NetManager instance = NetManager.instance;
            ref NetSegment segment = ref instance.m_segments.m_buffer[id.NetSegment];
            if (!ClassesData.Instance.GetConductibility(segment.Info.m_class))
            {
                return true;
            }

            return (instance.m_nodes.m_buffer[segment.m_startNode].m_flags & NetNode.Flags.Electricity) != 0 && (instance.m_nodes.m_buffer[segment.m_endNode].m_flags & NetNode.Flags.Electricity) != 0;
        }


    }

}
