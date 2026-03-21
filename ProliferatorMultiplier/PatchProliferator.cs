using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ProliferatorMultiplier
{
    public static class PatchProliferator
    {
        private const string ConfigEffectGroup = "Proliferator effect";
        private const string ConfigRecipeGroup = "Proliferator recipe";

        private static readonly int[] RecipeIds = { 106, 107, 108}; // Proliferator recipe ids
        
        public static ConfigEntry<float> MultProduction;
        public static ConfigEntry<float> MultSpeed;
        public static ConfigEntry<float> MultEnergy;
        public static ConfigEntry<float> MultRecipeCost;
        public static ConfigEntry<float> MultRecipeDuration;
        
        
        private static readonly double[] BackupIncTableMilli = CopyTable(ref Cargo.incTableMilli);
        private static readonly double[] BackupAccTableMilli = CopyTable(ref Cargo.accTableMilli);
        private static readonly int[] BackupIncFastDivisionNumerator = CopyTable(ref Cargo.incFastDivisionNumerator);
        private static readonly int[] BackupIncTable = CopyTable(ref Cargo.incTable);
        private static readonly int[] BackupAccTable = CopyTable(ref Cargo.accTable);
        private static readonly int[] BackupPowerTable = CopyTable(ref Cargo.powerTable);
        private static readonly double[] BackupPowerTableRatio = CopyTable(ref Cargo.powerTableRatio);
        
        private static readonly Dictionary<int, int[]> OriginalRecipeCost = new();
        private static readonly Dictionary<int, int> OriginalRecipeDuration = new();

        private static ConfigFile ConfigFile;
        private static ManualLogSource Log;
        
        public static void Init(ConfigFile config, ManualLogSource log)
        {
            ConfigFile = config;
            Log = log;
            Log.LogInfo("Configuring ProliferatorMultiplier...");
            BackupOriginalRecipes();
            Log.LogInfo("ProliferatorMultiplier done configuring.");
        }

        public static void Setup()
        {
            Log.LogInfo("Initializing...");
            
            ReloadConfig();
        }
        
        public static void ReloadConfig()
        {
            Log.LogInfo("Before...");
            PrintTables();
            
            Log.LogInfo("Config file path: " + ConfigFile.ConfigFilePath);
            ConfigFile.Reload();
            InitConfig();
            PatchTables();
            PatchProliferatorRecipes();
        }

        private static void PatchProliferatorRecipes()
        {
            Log.LogInfo("Patching spray recipes...");
            
            foreach(var r in RecipeIds)
            {
                var recipe = GetRecipe(r);
                Log.LogInfo("Patching recipe: " + recipe.Name.Translate());
                MultiplyArrayToDest(MultRecipeCost.Value, OriginalRecipeCost[r], ref recipe.ItemCounts);
                recipe.TimeSpend = (int)Math.Round((double)OriginalRecipeDuration[r] * MultRecipeDuration.Value);
            }
            
            
            foreach (var r in LDB.recipes.dataArray)
            {
                Log.LogInfo($"{r.ID} | {r.SID} | {r.index} | {r.Name.Translate()} | {r.TimeSpend} | [{r.Items.Join()}] [{r.ItemCounts.Join()}] [{r.Results.Join()}]");
            }
            

            Log.LogInfo("Spray recipes patched.");
        }

        private static void PatchTables()
        {
            // Patch spray bonus tables
            MultiplyArrayToDest(MultProduction.Value, BackupIncTable, ref Cargo.incTable);
            MultiplyArrayToDest(MultProduction.Value, BackupIncTableMilli,  ref Cargo.incTableMilli);
            PatchIncDivisionTable(ref Cargo.incFastDivisionNumerator);
            MultiplyArrayToDest(MultSpeed.Value, BackupAccTableMilli, ref Cargo.accTableMilli);
            MultiplyArrayToDest(MultSpeed.Value, BackupAccTable, ref Cargo.accTable);
            MultiplyArrayToDest(MultEnergy.Value, BackupPowerTable, ref Cargo.powerTable);
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
        
        private static void InitConfig()
        {
            Log.LogInfo($"Initializing config from {ConfigFile.ConfigFilePath}...");

            MultProduction = CreateConfigEntry(nameof(MultProduction), ConfigEffectGroup, "Production multiplier for proliferator effect");
            MultSpeed = CreateConfigEntry(nameof(MultSpeed), ConfigEffectGroup, "Speed multiplier for proliferator effect");
            MultEnergy = CreateConfigEntry(nameof(MultEnergy), ConfigEffectGroup, "Energy consumption multiplier for proliferator effect");
            MultRecipeCost = CreateConfigEntry(nameof(MultRecipeCost), ConfigRecipeGroup, "Multiplier for recipe cost");
            MultRecipeDuration = CreateConfigEntry(nameof(MultRecipeDuration), ConfigRecipeGroup, "Multiplier for spray duration");
            
            Log.LogInfo("Config initialized with MultProduction: " + MultProduction.Value + ", MultSpeed: " + MultSpeed.Value + ", MultEnergy: " + MultEnergy.Value);
        }

        private static ConfigEntry<float> CreateConfigEntry(string key, string section, string desc)
        {
            return ConfigFile.Bind(
                section: section,
                key: key,
                defaultValue: 1f, 
                configDescription: new ConfigDescription(desc, new AcceptableValueRange<float>(0.01f, 1000f))
            );
        }

        public static void Teardown()
        {
            Log.LogInfo("Teardown...");
            Log.LogInfo("Restoring original proliferator tables..");
            RestoreTable(ref Cargo.incTableMilli, BackupIncTableMilli);
            RestoreTable(ref Cargo.incTable, BackupIncTable);
            RestoreTable(ref Cargo.incFastDivisionNumerator, BackupIncFastDivisionNumerator);
            RestoreTable(ref Cargo.accTableMilli, BackupAccTableMilli);
            RestoreTable(ref Cargo.accTable, BackupAccTable);
            RestoreTable(ref Cargo.powerTable, BackupPowerTable);
            RestoreTable(ref Cargo.powerTableRatio, BackupPowerTableRatio);
            
            Log.LogInfo("Restoring recipes..");
            foreach (var r in RecipeIds)
            {
                var recipe = GetRecipe(r);
                if (OriginalRecipeCost.TryGetValue(r, out var originalCost))
                {
                    recipe.ItemCounts = originalCost;
                }
                if (OriginalRecipeDuration.TryGetValue(r, out var originalDuration))
                {
                    recipe.TimeSpend = originalDuration;
                }
            }
            
            Log.LogInfo("Teardown done.");
        }

        private static T[] CopyTable<T>(ref T[] original)
        {
            var backup = new T[original.Length];
            Array.Copy(original, backup, original.Length);
            return backup;
        }
        
        private static void MultiplyArrayToDest(float mult, double[] original, ref double[] dest)
        {
            for (var i = 0; i < original.Length; i++)
            {
                dest[i] = original[i] * mult;
            }
        }
        
        private static void MultiplyArrayToDest(float mult, int[] original, ref int[] dest)
        {
            for (var i = 0; i < original.Length; i++)
            {
                dest[i] = (int)(original[i] * mult);
            }
        }
        
        private static void PatchIncDivisionTable(ref int[] dest)
        {
            for (var i = 0; i < dest.Length; i++)
            {
                dest[i] = Cargo.incFastDivisionDenominator + (int)Math.Round(Cargo.incTableMilli[i] * Cargo.incFastDivisionDenominator);
            }
        }
        
        private static void PatchPowerTableRatio(float mult, double[] original, ref double[] dest)
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

        private static void BackupOriginalRecipes()
        {
            Log.LogInfo("Backing up original recipes...");
            
            // foreach (var r in LDB.recipes.dataArray)
            // {
            //     Log.LogInfo($"{r.ID} | {r.SID} | {r.index} | {r.Name.Translate()} | {r.TimeSpend} | [{r.Items.Join()}] [{r.ItemCounts.Join()}] [{r.Results.Join()}]");
            // }

            
            foreach (var r in RecipeIds)
            {
                Log.LogInfo($"Backing up: {r}");
                var recipe = GetRecipe(r);
                OriginalRecipeCost[recipe.ID] = recipe.ItemCounts;
                OriginalRecipeDuration[recipe.ID] = recipe.TimeSpend;
                Log.LogInfo($"Original recipe {recipe.Name} cost: [{recipe.ItemCounts.Join()}], duration: {recipe.TimeSpend}");
            }
            Log.LogInfo("Original recipes backed up.");
        }
        
        private static void RestoreTable<T>(ref T[] restoreTo, T[] backup)
        {
            Log.LogInfo("RestoreTable called.");
            for (var i = 0; i < backup.Length; i++)
            {
                restoreTo[i] = backup[i];
            }
        }
        
        private static RecipeProto GetRecipe(int id)
        {
            var recipe =  LDB.recipes.dataArray.FirstOrDefault(r => r.ID == id);
            Log.LogError(recipe == null ? $"Recipe with ID {id} not found!" : $"Recipe with ID {id} ([{recipe.Name.Translate()}]) found!");
            return recipe;
        }
    }
}