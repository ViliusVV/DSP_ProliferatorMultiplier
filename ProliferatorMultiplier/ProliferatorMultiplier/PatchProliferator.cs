using System;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ProliferatorMultiplier
{
    public static class PatchProliferator
    {
        public static ConfigEntry<int> MultProduction;
        public static ConfigEntry<int> MultSpeed;
        public static ConfigEntry<int> MultEnergy;

        private static readonly double[] BackupIncTableMilli = CopyTable(ref Cargo.incTableMilli);
        private static readonly double[] BackupAccTableMilli = CopyTable(ref Cargo.accTableMilli);
        private static readonly int[] BackupIncFastDivisionNumerator = CopyTable(ref Cargo.incFastDivisionNumerator);
        private static readonly int[] BackupIncTable = CopyTable(ref Cargo.incTable);
        private static readonly int[] BackupAccTable = CopyTable(ref Cargo.accTable);
        private static readonly int[] BackupPowerTable = CopyTable(ref Cargo.powerTable);
        private static readonly double[] BackupPowerTableRatio = CopyTable(ref Cargo.powerTableRatio);

        private static ConfigFile ConfigFile;
        private static ManualLogSource Log;
        
        public static void Init(ConfigFile config, ManualLogSource log)
        {
            Log.LogInfo("Initializing...");
            
            ConfigFile = config;
            Log = log;
            
            InitConfig(ConfigFile);
            Patch();
        }
        
        public static void ReloadConfig()
        {
            Log.LogInfo("Reloading config...");
            InitConfig(ConfigFile);
            Patch();
            Log.LogInfo("Config reloaded.");
        }

        private static void Patch()
        {
            // Patch spray bonus tables
            PatchIncTable(MultProduction.Value, ref Cargo.incTableMilli);
            PatchIncTable(MultProduction.Value, ref Cargo.incTable);
            PatchIncDivisionTable(ref Cargo.incFastDivisionNumerator);
            PatchIncTable(MultSpeed.Value, ref Cargo.accTableMilli);
            PatchIncTable(MultSpeed.Value, ref Cargo.accTable);
            PatchIncTable(MultEnergy.Value, ref Cargo.powerTable);
            PatchPowerTableRatio(MultEnergy.Value, ref Cargo.powerTableRatio);
            
            PrintTables(BackupIncTableMilli, "Backup_incTableMilli");
            PrintTables(BackupIncTable, "Backup_incTable");
            PrintTables(BackupIncFastDivisionNumerator, "Backup_incFastDivisionNumerator");
            PrintTables(BackupAccTableMilli, "Backup_accTableMilli");
            PrintTables(BackupAccTable, "Backup_accTable");
            PrintTables(BackupPowerTable, "Backup_powerTable");
            PrintTables(BackupPowerTableRatio, "Backup_powerTableRatio");
            
            PrintTables(Cargo.incTableMilli, "Cargo.incTableMilli");
            PrintTables(Cargo.incTable, "Cargo.incTable");
            PrintTables(Cargo.incFastDivisionNumerator, "Cargo.incFastDivisionNumerator");
            PrintTables(Cargo.accTableMilli, "Cargo.accTableMilli");
            PrintTables(Cargo.accTable, "Cargo.accTable");
            PrintTables(Cargo.powerTable, "Cargo.powerTable");
            PrintTables(Cargo.powerTableRatio, "Cargo.powerTableRatio");
        }
        
        private static void InitConfig(ConfigFile confFile)
        {
            Log.LogInfo($"Initializing config from {confFile.ConfigFilePath}...");
            
            MultProduction = confFile.Bind(
                section: "1. Additional production",
                key: nameof(MultProduction), 
                defaultValue: 1,
                configDescription: new ConfigDescription("Multiplies proliferator effect - Additional production", new AcceptableValueRange<int>(1, 100))
            );

            MultSpeed = confFile.Bind("1. Speed of production",
                key: nameof(MultSpeed),
                defaultValue: 1, 
                configDescription: new ConfigDescription("Multiplies proliferator effect - Speed of production", new AcceptableValueRange<int>(1, 100))
            );
            
            MultEnergy= confFile.Bind("1. Energy consumption",
                key: nameof(MultEnergy),
                defaultValue: 1, 
                configDescription: new ConfigDescription("Multiplies proliferator effect - Energy consumption", new AcceptableValueRange<int>(1, 100))
            );
            
            Log.LogInfo("Config initialized with MultProduction: " + MultProduction.Value + ", MultSpeed: " + MultSpeed.Value + ", MultEnergy: " + MultEnergy.Value);
        }

        public static void Teardown()
        {
            Log.LogInfo("ProliferatorMultiplier: Ending...");
            RestoreTable(ref Cargo.incTableMilli, BackupIncTableMilli);
            RestoreTable(ref Cargo.incTable, BackupIncTable);
            RestoreTable(ref Cargo.incFastDivisionNumerator, BackupIncFastDivisionNumerator);
            RestoreTable(ref Cargo.accTableMilli, BackupAccTableMilli);
            RestoreTable(ref Cargo.accTable, BackupAccTable);
            RestoreTable(ref Cargo.powerTable, BackupPowerTable);
            RestoreTable(ref Cargo.powerTableRatio, BackupPowerTableRatio);
            Log.LogInfo("ProliferatorMultiplier: Ended.");
        }

        private static T[] CopyTable<T>(ref T[] original)
        {
            var backup = new T[original.Length];
            Array.Copy(original, backup, original.Length);
            return backup;
        }
        
        private static void PatchIncTable(int mult, ref double[] original)
        {
            Log.LogInfo("PatchIncTable<double> called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] *= mult;
            }
        }
        
        private static void PatchIncTable(int mult, ref int[] original)
        {
            Log.LogInfo("PatchIncTable<double> called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] *= mult;
            }
        }
        
        private static void PatchIncDivisionTable(ref int[] original)
        {
            Log.LogInfo("PatchIncDivisionTable called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] = Cargo.incFastDivisionDenominator + (int)Math.Round(Cargo.incTableMilli[i] * Cargo.incFastDivisionDenominator);
            }
        }
        
        private static void PatchPowerTableRatio(int mult, ref double[] original)
        {
            Log.LogInfo("PatchPowerTableRatio called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] = ((original[i] - 1) * mult) + 1;
            }
        }
        
        private static void PrintTables<T>(T[] table, string tableName)
        {
            Log.LogInfo($"{tableName}: {string.Join(", ", table)}");
        }
        
        private static void RestoreTable<T>(ref T[] restoreTo, T[] backup)
        {
            Log.LogInfo("RestoreTable called.");
            for (var i = 0; i < backup.Length; i++)
            {
                restoreTo[i] = backup[i];
            }
        }
    }
}