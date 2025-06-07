using System;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace ProliferatorMultiplier
{
    public class PatchProliferator
    {
        public ConfigEntry<int> MultProduction;
        public ConfigEntry<int> MultSpeed;
        public ConfigEntry<int> MultEnergy;

        private double[] _backupIncTableMilli = Array.Empty<double>();
        private double[] _backupAccTableMilli = Array.Empty<double>();
        private int[] _backupIncTable = Array.Empty<int>();
        private int[] _backupAccTable = Array.Empty<int>();
        private int[] _backupPowerTable = Array.Empty<int>();
        private double[] _backupPowerTableRatio = Array.Empty<double>();

        private readonly ConfigFile _configFile;
        private readonly ManualLogSource Log;

        public PatchProliferator(ConfigFile configFile, ManualLogSource log)
        {
            this._configFile = configFile;
            this.Log = log;
        }
        
        public void Init()
        {
            Log.LogInfo("Initializing...");
            InitConfig(_configFile);
            
            BackupTables();
            Patch();
        }

        private void Patch()
        {
            // Patch spray bonus tables
            PatchIncTable(MultProduction.Value, ref Cargo.incTableMilli);
            PatchIncTable(MultProduction.Value, ref Cargo.incTable);
            PatchIncTable(MultSpeed.Value, ref Cargo.accTableMilli);
            PatchIncTable(MultSpeed.Value, ref Cargo.accTable);
            PatchIncTable(MultEnergy.Value, ref Cargo.powerTable);
            PatchPowerTableRatio(MultEnergy.Value, ref Cargo.powerTableRatio);
            
            PrintTables(_backupIncTableMilli, "Backup_incTableMilli");
            PrintTables(_backupIncTable, "Backup_incTable");
            PrintTables(_backupAccTableMilli, "Backup_accTableMilli");
            PrintTables(_backupAccTable, "Backup_accTable");
            PrintTables(_backupPowerTable, "Backup_powerTable");
            PrintTables(_backupPowerTableRatio, "Backup_powerTableRatio");
            
            PrintTables(Cargo.incTableMilli, "Cargo.incTableMilli");
            PrintTables(Cargo.incTable, "Cargo.incTable");
            PrintTables(Cargo.accTableMilli, "Cargo.accTableMilli");
            PrintTables(Cargo.accTable, "Cargo.accTable");
            PrintTables(Cargo.powerTable, "Cargo.powerTable");
            PrintTables(Cargo.powerTableRatio, "Cargo.powerTableRatio");
        }


        private void InitConfig(ConfigFile confFile)
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
            
            Log.LogInfo("Config initialized with MultProduction: " + MultProduction.Value + ", MultSpeed: " + MultSpeed.Value);
        }


        public void End()
        {
            Log.LogInfo("ProliferatorMultiplier: Ending...");
            RestoreTable(ref Cargo.incTableMilli, ref _backupIncTableMilli);
            RestoreTable(ref Cargo.incTable, ref _backupIncTable);
            RestoreTable(ref Cargo.accTableMilli, ref _backupAccTableMilli);
            RestoreTable(ref Cargo.accTable, ref _backupAccTable);
            RestoreTable(ref Cargo.powerTable, ref _backupPowerTable);
            RestoreTable(ref Cargo.powerTableRatio, ref _backupPowerTableRatio);
            Log.LogInfo("ProliferatorMultiplier: Ended.");
        }

        private static void BackupTable<T>(ref T[] original, ref T[] dest)
        {
            var len = original.Length;
            Array.Resize(ref dest, len);
            Array.Copy(original, dest, len);
        }

        private void BackupTables()
        {
            Log.LogInfo("Backing up tables...");
            BackupTable(ref Cargo.incTableMilli, ref _backupIncTableMilli);
            BackupTable(ref Cargo.incTable, ref _backupIncTable);
            BackupTable(ref Cargo.accTableMilli, ref _backupAccTableMilli);
            BackupTable(ref Cargo.accTable, ref _backupAccTable);
            BackupTable(ref Cargo.powerTable, ref _backupPowerTable);
            BackupTable(ref Cargo.powerTableRatio, ref _backupPowerTableRatio);
            Log.LogInfo("Tables backed up.");
        }


        private void PatchIncTable(int mult, ref double[] original)
        {
            Log.LogInfo("PatchIncTable<double> called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] *= mult;
            }
        }
        
        private void PatchIncTable(int mult, ref int[] original)
        {
            Log.LogInfo("PatchIncTable<double> called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] *= mult;
            }
        }
        
        private void PatchPowerTableRatio(int mult, ref double[] original)
        {
            Log.LogInfo("PatchPowerTableRatio<double> called");
            for (var i = 0; i < original.Length; i++)
            {
                original[i] = ((original[i] - 1) * mult) + 1;
            }
        }
        

        private void PrintTables<T>(T[] table, string tableName)
        {
            Log.LogInfo($"Proliferator table {tableName}: {string.Join(", ", table)}");
        }


        private void RestoreTable<T>(ref T[] restoreTo, ref T[] backup)
        {
            Log.LogInfo("ArrayHelperEnd called.");
            Assert.True(restoreTo.Length == backup.Length);

            for (var i = 0; i < backup.Length; i++)
            {
                restoreTo[i] = backup[i];
            }
        }
    }
}