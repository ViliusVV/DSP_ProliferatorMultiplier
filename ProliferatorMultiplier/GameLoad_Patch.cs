using HarmonyLib;

namespace ProliferatorMultiplier
{
    internal class GameLoad_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIRoot), nameof(UIRoot.OnGameMainObjectCreated))]
        public static void OnGameMainObjectCreated()
        {
            PatchProliferator.Setup();
        }
    }
}
