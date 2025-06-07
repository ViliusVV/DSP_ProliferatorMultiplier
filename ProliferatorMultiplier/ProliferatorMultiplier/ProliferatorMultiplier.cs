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

        private static PatchProliferator _plugin;
        private static bool _wasF5DownLastFrame = false;
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded!");

            _plugin = new PatchProliferator(Config, Logger);
            _plugin.Init();
        }
        
        private void Update()
        {
            var isF5Down = Input.GetKey(KeyCode.F5);

            if (isF5Down && !_wasF5DownLastFrame)
            {
                OnF5Pressed();
            }

            _wasF5DownLastFrame = isF5Down;
        }

        private void OnF5Pressed()
        {
            Logger.LogInfo("F5 was pressed!");
        }


        private void OnDestroy()
        {
            _plugin.End();
        }
    }
}