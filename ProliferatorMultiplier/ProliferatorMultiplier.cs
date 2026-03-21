using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ProliferatorMultiplier
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ProliferatorMultiplier : BaseUnityPlugin
    {
        private const string PluginGuid = "viliusvv.proliferatormultiplier";
        private const string PluginName = "ProliferatorMultiplier";
        private const string PluginVersion = VersionInfo.Value;
        
        private Harmony harmony = new(PluginGuid);
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded! Version: {PluginVersion}, Dev Mode: {Utils.IsDev}");
            if (Utils.IsDev)
            {
                Logger.LogInfo("Running in development mode. F5 key will reload config");   
            }
            
            PatchProliferator.Init(Config, Logger);
            
            GameLoad_Patch.Log = Logger;
            harmony.PatchAll(typeof(GameLoad_Patch));
        }
        
        private void Update()
        {
            if(!Utils.IsDev) return;
            
            if (!Input.GetKeyDown(KeyCode.F5)) return;
            
            PatchProliferator.ReloadConfig();
            Logger.LogInfo("F5 was pressed! Reloading config...");
        }

        private void OnDestroy()
        {
            PatchProliferator.Teardown();
        }
    }

    public class GameLoad_Patch
    {
        public static ManualLogSource Log;
            
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIRoot), nameof(UIRoot.OnGameMainObjectCreated))]
        public static void OnGameMainObjectCreated()
        {
            Log.LogInfo("OnGameMainObjectCreated patch");
            PatchProliferator.Setup();
        }
    }
}