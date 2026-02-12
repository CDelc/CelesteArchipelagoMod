using Celeste.Mod.CelesteArchipelago.Archipelago;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCrystalHeart : IGameModification
    {
        private static Hook _totalHeartGemsHook;
        private static Hook _levelSetTotalHeartGemsHook;

        public override void Load()
        {
            On.Celeste.SaveData.RegisterHeartGem += modSaveData_RegisterHeartGem;

            // Hook SaveData.TotalHeartGems property getter so heart gates
            // only count received AP crystal heart items, not location checks.
            var saveDataTotalHeartGems = typeof(SaveData)
                .GetProperty("TotalHeartGems")?.GetGetMethod();
            if (saveDataTotalHeartGems != null)
            {
                _totalHeartGemsHook = new Hook(
                    saveDataTotalHeartGems,
                    typeof(modCrystalHeart).GetMethod(
                        nameof(GetTotalHeartGems),
                        BindingFlags.NonPublic | BindingFlags.Static)
                );
            }

            // Hook LevelSetStats.TotalHeartGems for collab lobby heart gates
            // which check per-level-set heart counts.
            var levelSetTotalHeartGems = typeof(LevelSetStats)
                .GetProperty("TotalHeartGems")?.GetGetMethod();
            if (levelSetTotalHeartGems != null)
            {
                _levelSetTotalHeartGemsHook = new Hook(
                    levelSetTotalHeartGems,
                    typeof(modCrystalHeart).GetMethod(
                        nameof(GetLevelSetTotalHeartGems),
                        BindingFlags.NonPublic | BindingFlags.Static)
                );
            }
        }

        public override void Unload()
        {
            On.Celeste.SaveData.RegisterHeartGem -= modSaveData_RegisterHeartGem;

            _totalHeartGemsHook?.Dispose();
            _totalHeartGemsHook = null;

            _levelSetTotalHeartGemsHook?.Dispose();
            _levelSetTotalHeartGemsHook = null;
        }

        /// <summary>
        /// Hook for SaveData.RegisterHeartGem.
        /// Called when a crystal heart is collected in-game (both vanilla HeartGem
        /// and CollabUtils2 MiniHeart entities call this).
        /// We let the original run (sets HeartGem = true for visual display)
        /// and also send the location check to Archipelago.
        /// </summary>
        private static void modSaveData_RegisterHeartGem(
            On.Celeste.SaveData.orig_RegisterHeartGem orig,
            SaveData self, AreaKey area)
        {
            // Call original to set AreaModeStats.HeartGem = true for visual display
            // in the chapter select UI and in-game (heart won't respawn).
            orig(self, area);

            // Send location check to Archipelago
            if (ArchipelagoManager.Instance != null && ArchipelagoManager.Instance.Ready
                && CelesteArchipelagoModule.SaveData != null)
            {
                try
                {
                    long locationID = ArchipelagoMapper.getCrystalHeartLocationID(area.SID, area.Mode);
                    CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
                    CelesteArchipelagoModule.Log(
                        $"Crystal Heart location checked in {area.SID} ({area.Mode}), location id {locationID:X}");
                }
                catch (Exception ex)
                {
                    CelesteArchipelagoModule.Log(
                        $"Failed to map crystal heart location for {area.SID} ({area.Mode}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Override SaveData.TotalHeartGems to return only the count of received
        /// crystal heart AP items. Heart gates read this property, so they will
        /// only open based on items received, not locations checked.
        /// The per-level visual display (AreaModeStats.HeartGem) is unaffected.
        /// </summary>
        private delegate int orig_SaveDataTotalHeartGems(SaveData self);
        private static int GetTotalHeartGems(orig_SaveDataTotalHeartGems orig, SaveData self)
        {
            if (ArchipelagoManager.Instance != null && ArchipelagoManager.Instance.Ready
                && CelesteArchipelagoModule.SaveData != null)
            {
                return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count
                     + CelesteArchipelagoModule.SaveData.CrystalHeartsCollab.Count;
            }
            return orig(self);
        }

        /// <summary>
        /// Override LevelSetStats.TotalHeartGems for collab heart gates.
        /// Collab lobby heart gates check the per-level-set heart count.
        /// We return the appropriate AP item count for each level set.
        /// </summary>
        private delegate int orig_LevelSetTotalHeartGems(LevelSetStats self);
        private static int GetLevelSetTotalHeartGems(orig_LevelSetTotalHeartGems orig, LevelSetStats self)
        {
            if (ArchipelagoManager.Instance != null && ArchipelagoManager.Instance.Ready
                && CelesteArchipelagoModule.SaveData != null)
            {
                if (self.Name == "Celeste")
                {
                    return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
                }
                else
                {
                    return CelesteArchipelagoModule.SaveData.CrystalHeartsCollab.Count;
                }
            }
            return orig(self);
        }
    }
}
