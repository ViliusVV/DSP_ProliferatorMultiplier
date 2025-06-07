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
        private const string PluginVersion = "1.0.3";
        
        private static bool _wasF5DownLastFrame = false;
        private Harmony harmony = new(PluginGuid);
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded! Version: {PluginVersion}, Dev Mode: {Utils.IsDev}");
            if (Utils.IsDev)
            {
                Logger.LogInfo("Running in development mode. F5 key will reload config");   
            }
            
            Logger.LogInfo(Config.ToString());
            Logger.LogInfo(Logger.ToString());
            
            PatchProliferator.Configure(Config, Logger);
            
            GameLoad_Patch.Log = Logger;
            harmony.PatchAll(typeof(GameLoad_Patch));
        }
        
        private void Update()
        {
            if(!Utils.IsDev) return;
            
            var isF5Down = Input.GetKey(KeyCode.F5);

            if (isF5Down && !_wasF5DownLastFrame)
            {
                PatchProliferator.ReloadConfig();
                Logger.LogInfo($"F5 was pressed! Reloading config...");
            }

            _wasF5DownLastFrame = isF5Down;
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
            Log.LogInfo($"OnGameMainObjectCreated patch");
            PatchProliferator.Init();
        }
    }
}