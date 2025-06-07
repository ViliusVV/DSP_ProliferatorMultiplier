using BepInEx;

namespace ProliferatorMultiplier
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ProliferatorMultiplier : BaseUnityPlugin
    {
        private const string PluginGuid = "proliferatormultiplier";
        private const string PluginName = "ProliferatorMultiplier";
        private const string PluginVersion = "1.0";

        private static Patch_Proliferator _plugin;
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginName} is loaded!");

            _plugin = new Patch_Proliferator(Config, Logger);
            _plugin.Init();
        }


        private void OnDestroy()
        {
            _plugin.End();
        }
    }
}