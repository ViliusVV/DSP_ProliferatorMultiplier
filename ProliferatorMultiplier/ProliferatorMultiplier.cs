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
        private const string PluginVersion = VersionInfo.Version;
        
        private Harmony harmony = new(PluginGuid);
        
        private void Awake()
        {
            Logger.LogInfo($"==> {PluginName} [{VersionInfo.VersionBuild}], Dev Mode: {Utils.IsDev}");
            
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