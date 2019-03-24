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
            AddRedirect(origMethodColorNodeRoadBase, null, GetType().GetMethod("AfterGetColorNode", allFlags));
            AddRedirect(origMethodColorSegmentRoadBase, null, GetType().GetMethod("AfterGetColorSegment", allFlags));


            //var src3 = typeof(NetLane).GetMethod("RenderInstance", allFlags);
            //var src4 = typeof(NetLane).GetMethod("RenderDestroyedInstance", allFlags);
            //var src5 = typeof(NetLane).GetMethod("PopulateGroupData", allFlags);
            //var src6 = typeof(NetLane).GetMethod("CalculateGroupData", allFlags);
            //var trp3_4 = GetType().GetMethod("TranspileRenderInstancesForProps", allFlags);

            //doLog($"TRANSPILE roads Lights: {src3} |  {src4} => {trp3_4}");
            //AddRedirect(src3, null, null, trp3_4);
            //AddRedirect(src4, null, null, trp3_4);
            //AddRedirect(src5, null, null, trp3_4);
            //AddRedirect(src6, null, null, trp3_4);

            var Assembly81 = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == "EightyOne, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (Assembly81 != null)
            {
                var iuserMod = Assembly81.GetType("EightyOne.Mod");
                if (iuserMod != null)
                {
                    return;
                }
            }

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


        public static bool CheckElectricity(NetNode.Flags startFlags, NetNode.Flags endFlags)
        {
            return ((startFlags | endFlags) & NetNode.Flags.Electricity) > NetNode.Flags.None;
        }



        public static void RunCheckingElectricity(PropInfo info, NetNode.Flags startFlags, NetNode.Flags endFlags, Action<PropInfo, bool> run)
        {
            //PropInfo.Effect[] cachedEffects = null;
            bool showEffects = CheckElectricity(startFlags, endFlags);
            //if (!showEffects && info.m_hasEffects)
            //{
            //    cachedEffects = info.m_effects;
            //    info.m_effects = new PropInfo.Effect[0];
            //}

            run(info, showEffects);

            //if (cachedEffects != null)
            //{
            //    info.m_effects = cachedEffects;
            //}
        }

        public static void RenderPropInstance9(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, NetNode.Flags startFlags, NetNode.Flags endFlags)
        {
            RunCheckingElectricity(info, startFlags, endFlags, (pi, showEffects) =>
            {
                if (info.m_effectLayer != RenderManager.instance.lightSystem.m_lightLayer || showEffects) PropInstance.RenderInstance(cameraInfo, pi, id, position, scale, angle, color, objectIndex, active || showEffects);
            });

        }
        public static void RenderPropInstance15(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping, Texture waterHeightMap, Vector4 waterHeightMapping, Vector4 waterSurfaceMapping, NetNode.Flags startFlags, NetNode.Flags endFlags)
        {

            RunCheckingElectricity(info, startFlags, endFlags, (pi, showEffects) =>
            {
                if (info.m_effectLayer != RenderManager.instance.lightSystem.m_lightLayer || showEffects) PropInstance.RenderInstance(cameraInfo, pi, id, position, scale, angle, color, objectIndex, active || showEffects, heightMap, heightMapping, surfaceMapping, waterHeightMap, waterHeightMapping, waterSurfaceMapping);
            });

        }

        public static bool CalculateGroupData(PropInfo info, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays, NetNode.Flags startFlags, NetNode.Flags endFlags)
        {
            bool result = false;
            int vc = 0;
            int tr = 0;
            int oc = 0;
            RenderGroup.VertexArrays va = RenderGroup.VertexArrays.Normals;
            RunCheckingElectricity(info, startFlags, endFlags, (pi, showEffects) =>
            {
                if (info.m_effectLayer != RenderManager.instance.lightSystem.m_lightLayer || showEffects) result = PropInstance.CalculateGroupData(pi, layer, ref vc, ref tr, ref oc, ref va);
            });
            vertexCount = vc;
            triangleCount = tr;
            objectCount = oc;
            vertexArrays = va;
            return result;
        }

        public static void PopulateGroupData(PropInfo info, int layer, InstanceID id, Vector3 position, float scale, float angle, Color color, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, NetNode.Flags startFlags, NetNode.Flags endFlags)
        {

            int vi = 0;
            int ti = 0;
            float maxid = 0;
            float maxrd = 0;
            Vector3 mn = Vector3.zero;
            Vector3 mx = Vector3.zero;

            RunCheckingElectricity(info, startFlags, endFlags, (pi, showEffects) =>
            {
                if (info.m_effectLayer != RenderManager.instance.lightSystem.m_lightLayer || showEffects) PropInstance.PopulateGroupData(pi, layer, id, position, scale, angle, color, ref vi, ref ti, groupPosition, data, ref mn, ref mx, ref maxrd, ref maxid);
            });
            vertexIndex = vi;
            triangleIndex = ti;
            maxInstanceDistance = maxid;
            maxRenderDistance = maxrd;
            max = mx;
            min = mn;
        }

        private static IEnumerable<CodeInstruction> TranspileRenderInstancesForProps(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase method)
        {
            var indexedParams = method.GetParameters().Select((x, i) => Tuple.New(i, x));
            var startFlagsArgIdx = indexedParams.FirstOrDefault(x => x.Second.Name == "startFlags")?.First ?? -1;
            var endFlagsArgIdx = indexedParams.FirstOrDefault(x => x.Second.Name == "endFlags")?.First ?? -1;
            if (startFlagsArgIdx < 0 || endFlagsArgIdx < 0)
            {
                doLog2($"WRONG ARGS! {startFlagsArgIdx} || {endFlagsArgIdx}");
                return instr;
            }
            doLog2($"ARGS: {startFlagsArgIdx} || {endFlagsArgIdx}");
            var listResult = instr.ToList();
            for (int i = 0; i < listResult.Count; i++)
            {
                if (listResult[i].opcode == OpCodes.Call && listResult[i].operand is MethodInfo currentInfo && currentInfo.Name == "RenderInstance" && currentInfo.DeclaringType == typeof(PropInstance))
                {
                    var paramsMethodCt = currentInfo.GetParameters().Length;
                    listResult.RemoveAt(i);
                    listResult.InsertRange(i, new List<CodeInstruction>
                        {
                            new CodeInstruction(OpCodes.Ldarg_S, startFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Ldarg_S, endFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricRoads), $"RenderPropInstance{paramsMethodCt}")),
                        });
                    doLog2($"{method.Name}: Added patch at instructsion {i} => RenderPropInstance{paramsMethodCt}");
                }
                else if (listResult[i].opcode == OpCodes.Call && listResult[i].operand is MethodInfo currentInfo2 && currentInfo2.Name == "CalculateGroupData" && currentInfo2.IsStatic && currentInfo2.DeclaringType == typeof(PropInstance))
                {
                    listResult.RemoveAt(i);
                    listResult.InsertRange(i, new List<CodeInstruction>
                        {
                            new CodeInstruction(OpCodes.Ldarg_S, startFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Ldarg_S, endFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricRoads), $"CalculateGroupData")),
                        });
                    doLog2($"{method.Name}: Added patch at instructsion {i} => CalculateGroupData");
                }
                else if (listResult[i].opcode == OpCodes.Call && listResult[i].operand is MethodInfo currentInfo3 && currentInfo3.Name == "PopulateGroupData" && currentInfo3.IsStatic && currentInfo3.DeclaringType == typeof(PropInstance))
                {
                    listResult.RemoveAt(i);
                    listResult.InsertRange(i, new List<CodeInstruction>
                        {
                            new CodeInstruction(OpCodes.Ldarg_S, startFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Ldarg_S, endFlagsArgIdx+1),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricRoads), $"PopulateGroupData")),
                        });
                    doLog2($"{method.Name}: Added patch at instructsion {i} => PopulateGroupData");
                }
            }
            return listResult;
        }
    }

}
