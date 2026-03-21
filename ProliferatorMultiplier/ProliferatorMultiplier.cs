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

        internal new static ManualLogSource Logger;

        private readonly Harmony _harmony = new(PluginGuid);

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"==> {PluginName} [{VersionInfo.VersionBuild}], Dev Mode: {Utils.IsDev}");

            Config.Save();
            PatchProliferator.Init(Config);

            _harmony.PatchAll(typeof(GameLoad_Patch));
        }

        private void Update()
        {
            if (!Utils.IsDev) return;

            if (!Input.GetKeyDown(KeyCode.F5)) return;
            
            PatchProliferator.ReloadConfig();
        }

        private void OnDestroy()
        {
            PatchProliferator.Teardown();
            _harmony.UnpatchSelf();
        }
    }
}
