using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TakeTheMaskOff.Patches;

namespace TakeTheMaskOff
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class UnMaskTheDeadBase : BaseUnityPlugin
    {
        private const string modGUID = "impulse.TakeTheMaskOff";
        private const string modName = "TakeTheMaskOff";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static UnMaskTheDeadBase Instance;

        internal ManualLogSource mls; // Corrected here

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Time to de the sus"); // Corrected here

            harmony.PatchAll(typeof(UnMaskTheDeadBase));
            harmony.PatchAll(typeof(MaskedPlayerEnemy_Patches));
        }
    }
}
