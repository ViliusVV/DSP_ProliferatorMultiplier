using BepInEx;
using UnityEngine;

namespace ProliferatorMultiplier
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ProliferatorMultiplier : BaseUnityPlugin
    {
        private const string PluginGuid = "proliferatormultiplier";
        private const string PluginName = "ProliferatorMultiplier";
        private const string PluginVersion = "1.0";
        
        private static bool _wasF5DownLastFrame = false;
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded! Version: {PluginVersion}, Dev Mode: {Utils.IsDev}");
            if (Utils.IsDev)
            {
                Logger.LogInfo("Running in development mode. F5 key will reload config");   
            }
            
            Logger.LogInfo(Config.ToString());
            Logger.LogInfo(Logger.ToString());
            PatchProliferator.Init(Config, Logger);
        }
        
        private void Update()
        {
            if(!Utils.IsDev) return;
            
            var isF5Down = Input.GetKey(KeyCode.F5);

            if (isF5Down && !_wasF5DownLastFrame)
            {
                PatchProliferator.ReloadConfig();
                Logger.LogInfo("F5 was pressed!");
            }

            _wasF5DownLastFrame = isF5Down;
        }

        private void OnDestroy()
        {
            PatchProliferator.Teardown();
        }
    }
}