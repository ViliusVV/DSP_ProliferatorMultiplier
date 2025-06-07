using System;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ProliferatorMultiplier
{
    public class Patch_Proliferator
    {
        public static ConfigEntry<int> MultProduction;
        public static ConfigEntry<int> MultSpeed;

        private static double[] CachedArray_AdditionalProduction = new double[0];
        private static double[] CachedArray_SpeedOfProduction = new double[0];

        private ConfigFile configFile;
        private ManualLogSource log;

        public Patch_Proliferator(ConfigFile configFile, ManualLogSource log)
        {
            this.configFile = configFile;
            this.log = log;
        }
        
        public void Init()
        {
            log.LogInfo("Initializing...");
            InitConfig(configFile);

            // Additional production for:
            // Smelter, assembler, chemical plant, lab extra matrix and extra hashes
            ArrayHelperStart(MultProduction.Value, ref Cargo.incTableMilli, ref CachedArray_AdditionalProduction);

            // Increased production speed
            ArrayHelperStart(MultSpeed.Value, ref Cargo.accTableMilli, ref CachedArray_SpeedOfProduction);
        }


        void InitConfig(ConfigFile confFile)
        {
            log.LogInfo("Initializing config...");
            MultProduction = confFile.Bind("1. Additional production",
                nameof(MultProduction),
                1,
                new ConfigDescription("Multiplies proliferator effect - Additional production",
                    new AcceptableValueRange<int>(1, 100000)));

            MultSpeed = confFile.Bind("1. Speed of production",
                nameof(MultSpeed),
                1,
                new ConfigDescription("Multiplies proliferator effect - Speed of production",
                    new AcceptableValueRange<int>(1, 100000)));
            
            log.LogInfo("Config initialized with MultProduction: " + MultProduction.Value + ", MultSpeed: " + MultSpeed.Value);
        }


        public void End()
        {
            log.LogInfo("ProliferatorMultiplier: Ending...");
            ArrayHelperEnd(ref Cargo.incTableMilli, ref CachedArray_AdditionalProduction);
            ArrayHelperEnd(ref Cargo.accTableMilli, ref CachedArray_SpeedOfProduction);
            log.LogInfo("ProliferatorMultiplier: Ended.");
        }


        void ArrayHelperStart(int value, ref double[] copyFrom, ref double[] copyTo)
        {
            log.LogInfo("ArrayHelperStart called with value: " + value);
            int sourceArrayLength = copyFrom.Length;
            Array.Resize(ref copyTo, sourceArrayLength);
            Array.Copy(copyFrom, copyTo, sourceArrayLength);
            for (int i = 0; i < sourceArrayLength; i++)
            {
                copyFrom[i] *= value;
            }
        }


        void ArrayHelperEnd(ref double[] restoreTo, ref double[] restoreFrom)
        {
            log.LogInfo("ArrayHelperEnd called.");
            Assert.True(restoreTo.Length == restoreFrom.Length);

            for (int i = 0; i < restoreFrom.Length; i++)
            {
                restoreTo[i] = restoreFrom[i];
            }
        }
    }
}