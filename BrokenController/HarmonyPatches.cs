using HarmonyLib;
using System;
using System.Reflection;

using UnityEngine;

namespace BrokenController
{
    public class HarmonyPatches
    {
        private static Harmony instance;

        public static bool IsPatched { get; private set; }
        public const string InstanceId = PluginInfo.GUID;

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched) {
                if (instance == null) {
                    instance = new Harmony(InstanceId);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (instance != null && IsPatched) {
                instance.UnpatchAll(InstanceId);
                IsPatched = false;
            }
        }
    }

    [HarmonyPatch]
    public class InstantiateControllerManager
    {
        internal static GameObject PlayerObject = null;

        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPostfix, HarmonyPatch("Awake", MethodType.Normal)]
        internal static void InitiateControllerManager(GorillaLocomotion.Player __instance)
        {
            PlayerObject = __instance.gameObject;
            __instance.gameObject.AddComponent<ControllerManager>();
        }

        [HarmonyPatch(typeof(VRRig))]
        [HarmonyPostfix, HarmonyPatch("Start", MethodType.Normal)]
        internal static void AddOnlineRig(VRRig __instance)
        {
            if (PlayerObject == null || __instance.photonView == null) return;
            PlayerObject.GetComponent<ControllerManager>()?.AddOnlineRig(__instance);
        }

        [HarmonyPatch(typeof(VRRig))]
        [HarmonyPostfix, HarmonyPatch("OnDestroy", MethodType.Normal)]
        internal static void RemoveOnlineRig(VRRig __instance)
        {
            if (PlayerObject == null || __instance.photonView == null) return;
            PlayerObject.GetComponent<ControllerManager>()?.RemoveOnlineRig(__instance);
        }
    }

}
