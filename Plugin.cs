using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Diagnostics;
using TakeTheMaskOff.Patches;

namespace TakeTheMaskOff
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class UnMaskTheDeadBase : BaseUnityPlugin
    {
        private const string modGUID = "impulse.TakeTheMaskOff";
        private const string modName = "TakeTheMaskOff";
        private const string modVersion = "1.3.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static UnMaskTheDeadBase Instance;

        internal ManualLogSource mls; // Corrected here
        internal ConfigEntry<int> MaskValue;
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Time to de the sus"); // Corrected here
            MaskValue = Config.Bind("General",      // The section under which the option is shown
                                    "MaskValue",  // The key of the configuration option in the configuration file
                                    52, // The default value
                                    "This value controls the value of the dropped masks"); // Description of the option to show in the config file
            harmony.PatchAll(typeof(UnMaskTheDeadBase));
            harmony.PatchAll(typeof(MaskedPlayerEnemy_Patches));
        }


    }
}
