using System;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ProliferatorMultiplier
{
    public class Patch_Proliferator
    {
        public static ConfigEntry<int> MultProduction;
        public static ConfigEntry<int> MultSpeed;

        private static double[] Backup_incTableMilli = Array.Empty<double>();
        private static double[] Backup_accTableMilli = Array.Empty<double>();
        private static int[] Backup_incTable = Array.Empty<int>();
        private static int[] Backup_accTable = Array.Empty<int>();

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
            ArrayHelperStart(MultProduction.Value, ref Cargo.incTableMilli, ref Backup_incTableMilli);
            ArrayHelperStart(MultProduction.Value, ref Cargo.incTable, ref Backup_incTable);
            ArrayHelperStart(MultSpeed.Value, ref Cargo.accTableMilli, ref Backup_accTableMilli);
            ArrayHelperStart(MultSpeed.Value, ref Cargo.accTable, ref Backup_accTable);
            
            
            PrintTables(Backup_incTableMilli, "Backup_incTableMilli");
            PrintTables(Backup_incTable, "Backup_incTable");
            PrintTables(Backup_accTableMilli, "Backup_accTableMilli");
            PrintTables(Backup_accTable, "Backup_accTable");
            
            PrintTables(Cargo.incTableMilli, "Cargo.incTableMilli");
            PrintTables(Cargo.incTable, "Cargo.incTable");
            PrintTables(Cargo.accTableMilli, "Cargo.accTableMilli");
            PrintTables(Cargo.accTable, "Cargo.accTable");
        }


        private void InitConfig(ConfigFile confFile)
        {
            log.LogInfo("Initializing config from {}...");
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
            ArrayHelperEnd(ref Cargo.incTableMilli, ref Backup_incTableMilli);
            ArrayHelperEnd(ref Cargo.incTable, ref Backup_incTable);
            ArrayHelperEnd(ref Cargo.accTableMilli, ref Backup_accTableMilli);
            ArrayHelperEnd(ref Cargo.accTable, ref Backup_accTable);
            log.LogInfo("ProliferatorMultiplier: Ended.");
        }


        private void ArrayHelperStart(int value, ref double[] copyFrom, ref double[] copyTo)
        {
            log.LogInfo("ArrayHelperStart<double> called");
            var sourceArrayLength = copyFrom.Length;
            Array.Resize(ref copyTo, sourceArrayLength);
            Array.Copy(copyFrom, copyTo, sourceArrayLength);
            for (var i = 0; i < sourceArrayLength; i++)
            {
                copyFrom[i] *= value;
            }
        }
        
        private void ArrayHelperStart(int value, ref int[] copyFrom, ref int[] copyTo)
        {
            log.LogInfo("ArrayHelperStart<double> called");
            var sourceArrayLength = copyFrom.Length;
            Array.Resize(ref copyTo, sourceArrayLength);
            Array.Copy(copyFrom, copyTo, sourceArrayLength);
            for (var i = 0; i < sourceArrayLength; i++)
            {
                copyFrom[i] *= value;
            }
        }
        

        private void PrintTables<T>(T[] table, string tableName)
        {
            log.LogInfo($"Proliferator table {tableName}: {string.Join(", ", table)}");
        }


        private void ArrayHelperEnd<T>(ref T[] restoreTo, ref T[] restoreFrom)
        {
            log.LogInfo("ArrayHelperEnd called.");
            Assert.True(restoreTo.Length == restoreFrom.Length);

            for (var i = 0; i < restoreFrom.Length; i++)
            {
                restoreTo[i] = restoreFrom[i];
            }
        }
    }
}