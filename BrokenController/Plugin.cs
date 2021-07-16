using BepInEx;
using System;
using UnityEngine;

namespace BrokenController
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void Update()
        {

        }
    }
}
