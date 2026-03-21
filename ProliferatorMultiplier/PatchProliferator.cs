using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace ProliferatorMultiplier
{
    internal static class PatchProliferator
    {
        private const string ConfigEffectGroup = "Proliferator effect";
        private const string ConfigRecipeGroup = "Proliferator recipe";

        private const string KeyMultProduction    = "MultProduction";
        private const string KeyMultSpeed         = "MultSpeed";
        private const string KeyMultEnergy        = "MultEnergy";
        private const string KeyMultRecipeCost    = "MultRecipeCost";
        private const string KeyMultRecipeDuration = "MultRecipeDuration";

        private static readonly int[] RecipeIds = { 106, 107, 108 }; // Proliferator recipe ids

        private static ConfigFile _configFile;

        private static ConfigEntry<float> _multProduction;
        private static ConfigEntry<float> _multSpeed;
        private static ConfigEntry<float> _multEnergy;
        private static ConfigEntry<float> _multRecipeCost;
        private static ConfigEntry<float> _multRecipeDuration;

        private static readonly double[] _backupIncTableMilli            = Utils.CopyTable(Cargo.incTableMilli);
        private static readonly double[] _backupAccTableMilli            = Utils.CopyTable(Cargo.accTableMilli);
        private static readonly int[]    _backupIncFastDivisionNumerator = Utils.CopyTable(Cargo.incFastDivisionNumerator);
        private static readonly int[]    _backupIncTable                 = Utils.CopyTable(Cargo.incTable);
        private static readonly int[]    _backupAccTable                 = Utils.CopyTable(Cargo.accTable);
        private static readonly int[]    _backupPowerTable               = Utils.CopyTable(Cargo.powerTable);
        private static readonly double[] _backupPowerTableRatio          = Utils.CopyTable(Cargo.powerTableRatio);

        private static readonly Dictionary<int, int[]> _originalRecipeCost     = new();
        private static readonly Dictionary<int, int>   _originalRecipeDuration = new();

        public static void Init(ConfigFile config)
        {
            _configFile = config;
            BindConfig();
            BackupOriginalRecipes();
        }

        public static void Setup() => ApplyConfig();

        private static void ApplyConfig()
        {
            PatchTables();
            PatchProliferatorRecipes();
            PrintTables();
        }

        public static void ReloadConfig()
        {
            var prevProduction     = _multProduction.Value;
            var prevSpeed          = _multSpeed.Value;
            var prevEnergy         = _multEnergy.Value;
            var prevRecipeCost     = _multRecipeCost.Value;
            var prevRecipeDuration = _multRecipeDuration.Value;

            _configFile.Reload();

            var changed = false;
            changed |= LogIfChanged(KeyMultProduction,    prevProduction,     _multProduction.Value);
            changed |= LogIfChanged(KeyMultSpeed,         prevSpeed,          _multSpeed.Value);
            changed |= LogIfChanged(KeyMultEnergy,        prevEnergy,         _multEnergy.Value);
            changed |= LogIfChanged(KeyMultRecipeCost,    prevRecipeCost,     _multRecipeCost.Value);
            changed |= LogIfChanged(KeyMultRecipeDuration, prevRecipeDuration, _multRecipeDuration.Value);

            if (!changed) ProliferatorMultiplier.Logger.LogInfo("Config reloaded: nothing changed.");

            ApplyConfig();
        }

        private static bool LogIfChanged(string name, float prev, float curr)
        {
            if (Mathf.Approximately(prev, curr)) return false;
            ProliferatorMultiplier.Logger.LogInfo($"Config changed: {name} {prev} -> {curr}");
            return true;
        }

        public static void Teardown()
        {
            ProliferatorMultiplier.Logger.LogInfo("Restoring original proliferator state...");

            Array.Copy(_backupIncTableMilli,            Cargo.incTableMilli,            _backupIncTableMilli.Length);
            Array.Copy(_backupIncTable,                 Cargo.incTable,                 _backupIncTable.Length);
            Array.Copy(_backupIncFastDivisionNumerator, Cargo.incFastDivisionNumerator, _backupIncFastDivisionNumerator.Length);
            Array.Copy(_backupAccTableMilli,            Cargo.accTableMilli,            _backupAccTableMilli.Length);
            Array.Copy(_backupAccTable,                 Cargo.accTable,                 _backupAccTable.Length);
            Array.Copy(_backupPowerTable,               Cargo.powerTable,               _backupPowerTable.Length);
            Array.Copy(_backupPowerTableRatio,          Cargo.powerTableRatio,          _backupPowerTableRatio.Length);

            foreach (var id in RecipeIds)
            {
                var recipe = GetRecipe(id);
                if (recipe == null) continue;
                if (_originalRecipeCost.TryGetValue(id, out var cost))         recipe.ItemCounts = cost;
                if (_originalRecipeDuration.TryGetValue(id, out var duration)) recipe.TimeSpend  = duration;
            }

            ProliferatorMultiplier.Logger.LogInfo("Teardown done.");
        }

        private static void BindConfig()
        {
            _multProduction    = Bind(ConfigEffectGroup, KeyMultProduction,    "Production multiplier for proliferator effect");
            _multSpeed         = Bind(ConfigEffectGroup, KeyMultSpeed,         "Speed multiplier for proliferator effect");
            _multEnergy        = Bind(ConfigEffectGroup, KeyMultEnergy,        "Energy consumption multiplier for proliferator effect");
            _multRecipeCost    = Bind(ConfigRecipeGroup, KeyMultRecipeCost,    "Multiplier for recipe cost");
            _multRecipeDuration = Bind(ConfigRecipeGroup, KeyMultRecipeDuration, "Multiplier for spray duration");

            ProliferatorMultiplier.Logger.LogInfo(
                $"Config: Production={_multProduction.Value}, Speed={_multSpeed.Value}, Energy={_multEnergy.Value}");
        }

        private static ConfigEntry<float> Bind(string section, string key, string desc) =>
            _configFile.Bind(section, key, 1f,
                new ConfigDescription(desc, new AcceptableValueRange<float>(0.01f, 1000f)));

        private static void PatchTables()
        {
            MultiplyArrayToDest(_multProduction.Value, _backupIncTable,    Cargo.incTable);
            MultiplyArrayToDest(_multProduction.Value, _backupIncTableMilli, Cargo.incTableMilli);
            PatchIncDivisionTable(Cargo.incFastDivisionNumerator);
            MultiplyArrayToDest(_multSpeed.Value, _backupAccTableMilli, Cargo.accTableMilli);
            MultiplyArrayToDest(_multSpeed.Value, _backupAccTable,      Cargo.accTable);
            MultiplyArrayToDest(_multEnergy.Value, _backupPowerTable,   Cargo.powerTable);
            PatchPowerTableRatio(_multEnergy.Value, _backupPowerTableRatio, Cargo.powerTableRatio);
        }

        private static void PatchProliferatorRecipes()
        {
            foreach (var id in RecipeIds)
            {
                var recipe = GetRecipe(id);
                if (recipe == null) continue;
                MultiplyArrayToDest(_multRecipeCost.Value, _originalRecipeCost[id], recipe.ItemCounts);
                recipe.TimeSpend = (int)Math.Round(_originalRecipeDuration[id] * (double)_multRecipeDuration.Value);
            }

            if (!Utils.IsDev) return;
            foreach (var r in LDB.recipes.dataArray)
                ProliferatorMultiplier.Logger.LogDebug(
                    $"{r.ID} | {r.Name.Translate()} | t={r.TimeSpend} | [{r.ItemCounts.Join()}]");
        }

        private static void BackupOriginalRecipes()
        {
            foreach (var id in RecipeIds)
            {
                var recipe = GetRecipe(id);
                if (recipe == null) continue;
                _originalRecipeCost[recipe.ID]     = recipe.ItemCounts;
                _originalRecipeDuration[recipe.ID] = recipe.TimeSpend;
                ProliferatorMultiplier.Logger.LogDebug(
                    $"Backed up recipe {recipe.Name.Translate()}: cost=[{recipe.ItemCounts.Join()}], t={recipe.TimeSpend}");
            }
        }

        private static void PrintTables()
        {
            if (!Utils.IsDev) return;

            void Print<T>(T[] table, string name) =>
                ProliferatorMultiplier.Logger.LogDebug($"{name}: [{string.Join(", ", table)}]");

            Print(_backupIncTableMilli,            "Backup.incTableMilli");
            Print(_backupIncTable,                 "Backup.incTable");
            Print(_backupIncFastDivisionNumerator, "Backup.incFastDivisionNumerator");
            Print(_backupAccTableMilli,            "Backup.accTableMilli");
            Print(_backupAccTable,                 "Backup.accTable");
            Print(_backupPowerTable,               "Backup.powerTable");
            Print(_backupPowerTableRatio,          "Backup.powerTableRatio");

            Print(Cargo.incTableMilli,            "Cargo.incTableMilli");
            Print(Cargo.incTable,                 "Cargo.incTable");
            Print(Cargo.incFastDivisionNumerator, "Cargo.incFastDivisionNumerator");
            Print(Cargo.accTableMilli,            "Cargo.accTableMilli");
            Print(Cargo.accTable,                 "Cargo.accTable");
            Print(Cargo.powerTable,               "Cargo.powerTable");
            Print(Cargo.powerTableRatio,          "Cargo.powerTableRatio");
        }

        private static void MultiplyArrayToDest(float mult, double[] original, double[] dest)
        {
            for (var i = 0; i < original.Length; i++)
                dest[i] = original[i] * mult;
        }

        private static void MultiplyArrayToDest(float mult, int[] original, int[] dest)
        {
            for (var i = 0; i < original.Length; i++)
                dest[i] = (int)(original[i] * mult);
        }

        private static void PatchIncDivisionTable(int[] dest)
        {
            for (var i = 0; i < dest.Length; i++)
                dest[i] = Cargo.incFastDivisionDenominator +
                           (int)Math.Round(Cargo.incTableMilli[i] * Cargo.incFastDivisionDenominator);
        }

        private static void PatchPowerTableRatio(float mult, double[] original, double[] dest)
        {
            for (var i = 0; i < original.Length; i++)
                dest[i] = (original[i] - 1) * mult + 1;
        }

        private static RecipeProto GetRecipe(int id)
        {
            var recipe = LDB.recipes.dataArray.FirstOrDefault(r => r.ID == id);
            if (recipe == null)
                ProliferatorMultiplier.Logger.LogError($"Recipe with ID {id} not found!");
            return recipe;
        }
    }
}
