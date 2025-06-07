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
        
        public static void Configure(ConfigFile config, ManualLogSource log)
        {
            ConfigFile = config;
            Log = log;
            Log.LogInfo("Configuring ProliferatorMultiplier...");
        }

        public static void Init()
        {
            Log.LogInfo("Initializing...");
            
            ReloadConfig();
        }
        
        public static void ReloadConfig()
        {
            Log.LogInfo("Before...");
            PrintTables();
            
            ConfigFile.Reload();
            InitConfig(ConfigFile);
            Patch();
        }

        private static void Patch()
        {
            // Patch spray bonus tables
            PatchIncTable(MultProduction.Value, BackupIncTableMilli,  ref Cargo.incTableMilli);
            PatchIncTable(MultProduction.Value, BackupIncTable, ref Cargo.incTable);
            PatchIncDivisionTable(ref Cargo.incFastDivisionNumerator);
            PatchIncTable(MultSpeed.Value, BackupAccTableMilli, ref Cargo.accTableMilli);
            PatchIncTable(MultSpeed.Value, BackupAccTable, ref Cargo.accTable);
            PatchIncTable(MultEnergy.Value, BackupPowerTable, ref Cargo.powerTable);
            PatchPowerTableRatio(MultEnergy.Value, BackupPowerTableRatio, ref Cargo.powerTableRatio);
            
            PrintTables();
        }

        public static void PrintTables()
        {
            if (!Utils.IsDev) return;
            
            PrintTable(BackupIncTableMilli, "Backup_incTableMilli");
            PrintTable(BackupIncTable, "Backup_incTable");
            PrintTable(BackupIncFastDivisionNumerator, "Backup_incFastDivisionNumerator");
            PrintTable(BackupAccTableMilli, "Backup_accTableMilli");
            PrintTable(BackupAccTable, "Backup_accTable");
            PrintTable(BackupPowerTable, "Backup_powerTable");
            PrintTable(BackupPowerTableRatio, "Backup_powerTableRatio");
            
            PrintTable(Cargo.incTableMilli, "Cargo.incTableMilli");
            PrintTable(Cargo.incTable, "Cargo.incTable");
            PrintTable(Cargo.incFastDivisionNumerator, "Cargo.incFastDivisionNumerator");
            PrintTable(Cargo.accTableMilli, "Cargo.accTableMilli");
            PrintTable(Cargo.accTable, "Cargo.accTable");
            PrintTable(Cargo.powerTable, "Cargo.powerTable");
            PrintTable(Cargo.powerTableRatio, "Cargo.powerTableRatio");
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
            Log.LogInfo("Teardown...");
            RestoreTable(ref Cargo.incTableMilli, BackupIncTableMilli);
            RestoreTable(ref Cargo.incTable, BackupIncTable);
            RestoreTable(ref Cargo.incFastDivisionNumerator, BackupIncFastDivisionNumerator);
            RestoreTable(ref Cargo.accTableMilli, BackupAccTableMilli);
            RestoreTable(ref Cargo.accTable, BackupAccTable);
            RestoreTable(ref Cargo.powerTable, BackupPowerTable);
            RestoreTable(ref Cargo.powerTableRatio, BackupPowerTableRatio);
            Log.LogInfo("Teardown done.");
        }

        private static T[] CopyTable<T>(ref T[] original)
        {
            var backup = new T[original.Length];
            Array.Copy(original, backup, original.Length);
            return backup;
        }
        
        private static void PatchIncTable(int mult, double[] original, ref double[] dest)
        {
            for (var i = 0; i < original.Length; i++)
            {
                dest[i] = original[i] * mult;
            }
        }
        
        private static void PatchIncTable(int mult, int[] original, ref int[] dest)
        {
            for (var i = 0; i < original.Length; i++)
            {
                dest[i] = original[i] * mult;
            }
        }
        
        private static void PatchIncDivisionTable(ref int[] dest)
        {
            for (var i = 0; i < dest.Length; i++)
            {
                dest[i] = Cargo.incFastDivisionDenominator + (int)Math.Round(Cargo.incTableMilli[i] * Cargo.incFastDivisionDenominator);
            }
        }
        
        private static void PatchPowerTableRatio(int mult, double[] original, ref double[] dest)
        {
            for (var i = 0; i < original.Length; i++)
            {
                dest[i] = ((original[i] - 1) * mult) + 1;
            }
        }
        
        private static void PrintTable<T>(T[] table, string tableName)
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