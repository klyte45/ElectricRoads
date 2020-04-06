using ColossalFramework.Globalization;
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
            List<Type> targetTypes = ElectricRoadsOverrides.Get81TilesFakeManagerTypes();
            if (targetTypes.Count == 0)
            {
                LogUtils.DoWarnLog("Loading 81 tiles hooks stopped because the mod is not active");
                return;
            }
            LogUtils.DoWarnLog("Loading 81 tiles hooks");
            try
            {
                foreach (Type fakeElMan in targetTypes)
                {
                    MethodInfo src = fakeElMan.GetMethod("SimulationStepImpl", RedirectorUtils.allFlags);
                    MethodInfo trp = GetType().GetMethod("TranspileSimulation", RedirectorUtils.allFlags);
                    MethodInfo src2 = fakeElMan.GetMethod("ConductToNode", RedirectorUtils.allFlags);
                    MethodInfo trp2 = GetType().GetMethod("TranspileConduction", RedirectorUtils.allFlags);
                    LogUtils.DoLog($"TRANSPILE Electric ROADS NODES: {src} => {trp}");
                    AddRedirect(src, null, null, trp);
                    LogUtils.DoLog($"TRANSPILE Electric ROADS SEGMENTS: {src2} => {trp2}");
                    AddRedirect(src2, null, null, trp2);
                }
                GetHarmonyInstance();
            }
            catch (Exception e)
            {
                LogUtils.DoErrorLog($"EXCEPTION WHILE LOADING: {e.GetType()} - {e.Message}\n {e.StackTrace}");

                K45DialogControl.ShowModal(new K45DialogControl.BindProperties()
                {
                    icon = ElectricRoadsMod.Instance.IconName,
                    title = "Exception on Hooking 81 Tiles",
                    message = $"Electric Roads failed loading 81 tiles code detours. Send this print at the workshop page to Klyte45 check what's going on, and send the output_log.txt (or player.log on Mac) too if possible.:\n\n<color #ffff00>{e.GetType()} - {e.Message}\n\n{e.StackTrace}</color>",
                    showButton1 = true,
                    textButton1 = Locale.Get("K45_ER_OK_BUTTON"),
                    showButton3 = true,
                    textButton3 = Locale.Get("K45_ER_GO_TO_MOD_PAGE_BUTTON"),
                    useFullWindowWidth = true
                }, (x) =>
                {
                    if (x == 3)
                    {
                        ColossalFramework.Utils.OpenUrlThreaded("https://steamcommunity.com/sharedfiles/filedetails/?id=1689984220");
                        return false;
                    }
                    return true;
                });
            }
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
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }
    }

}
