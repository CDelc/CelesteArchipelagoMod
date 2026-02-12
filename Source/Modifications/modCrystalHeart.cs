using Celeste.Mod.CelesteArchipelago.Archipelago;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCrystalHeart : IGameModification
    {
        private static Hook _totalHeartGemsHook;
        private static Hook _levelSetTotalHeartGemsHook;

        /// <summary>
        /// Cache mapping level set names to their lobby LevelCategory.
        /// Cleared on Unload to avoid stale data across sessions.
        /// </summary>
        private static Dictionary<string, LevelCategory?> _levelSetCategoryCache = new();

        public override void Load()
        {
            On.Celeste.SaveData.RegisterHeartGem += modSaveData_RegisterHeartGem;

            // Hook SaveData.TotalHeartGems property getter so vanilla heart gates
            // only count vanilla + heartside crystal heart AP items.
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

            // Hook LevelSetStats.TotalHeartGems for collab lobby heart gates.
            // Each lobby's gate should only count mini heart items for that lobby.
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

            _levelSetCategoryCache.Clear();
        }

        /// <summary>
        /// Hook for SaveData.RegisterHeartGem.
        /// Called when any crystal heart is collected in-game (both vanilla HeartGem
        /// and CollabUtils2 MiniHeart entities call this).
        /// We let the original run (sets HeartGem = true for visual display)
        /// and also send the appropriate location check to Archipelago.
        /// Lobby levels send a mini heart location (500B range).
        /// Vanilla/heartside levels send a crystal heart location (600B range).
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
                    // getHeartLocationID uses the level's category to decide between
                    // mini heart (500B) and crystal heart (600B) location ranges.
                    long locationID = ArchipelagoMapper.getHeartLocationID(area.SID, area.Mode);
                    CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
                    CelesteArchipelagoModule.Log(
                        $"Heart location checked in {area.SID} ({area.Mode}), location id {locationID:X}");
                }
                catch (Exception ex)
                {
                    CelesteArchipelagoModule.Log(
                        $"Failed to map heart location for {area.SID} ({area.Mode}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Override SaveData.TotalHeartGems to return only vanilla crystal heart
        /// AP items. Vanilla heart gates (HeartGemDoor) read this property, so
        /// they will only open based on received vanilla crystal heart items.
        /// Per-level visual display (AreaModeStats.HeartGem) is unaffected.
        /// </summary>
        private delegate int orig_SaveDataTotalHeartGems(SaveData self);
        private static int GetTotalHeartGems(orig_SaveDataTotalHeartGems orig, SaveData self)
        {
            if (ArchipelagoManager.Instance != null && ArchipelagoManager.Instance.Ready
                && CelesteArchipelagoModule.SaveData != null)
            {
                return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
            }
            return orig(self);
        }

        /// <summary>
        /// Override LevelSetStats.TotalHeartGems for collab lobby heart gates.
        /// Each lobby's heart gate checks TotalHeartGems on its level set.
        /// We determine which lobby the level set belongs to by checking the
        /// LevelCategory of its areas, then return the matching lobby item count.
        /// For the "Celeste" level set, returns vanilla + heartside count.
        /// For unrecognized level sets, falls through to original behavior.
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

                // Determine which lobby this level set belongs to
                LevelCategory? lobbyCategory = GetLevelSetLobbyCategory(self);
                if (lobbyCategory.HasValue)
                {
                    int key = (int)lobbyCategory.Value;
                    if (CelesteArchipelagoModule.SaveData.CrystalHeartsByLobby.TryGetValue(key, out var set))
                    {
                        return set.Count;
                    }
                    // Known lobby level set but no items received yet
                    return 0;
                }
            }
            return orig(self);
        }

        /// <summary>
        /// Determines the lobby LevelCategory for a given level set by checking
        /// the LevelCategory of its first non-interlude area.
        /// Results are cached for performance since this is called frequently.
        /// </summary>
        private static LevelCategory? GetLevelSetLobbyCategory(LevelSetStats stats)
        {
            if (_levelSetCategoryCache.TryGetValue(stats.Name, out var cached))
            {
                return cached;
            }

            LevelCategory? result = null;
            foreach (var area in stats.Areas)
            {
                try
                {
                    var areaData = AreaData.Get(area.ID_Safe);
                    if (areaData != null && !areaData.Interlude_Safe)
                    {
                        var category = ArchipelagoMapper.getLevelCategory(areaData.SID);
                        if (ArchipelagoMapper.isLobbyCategory(category))
                        {
                            result = category;
                            break;
                        }
                    }
                }
                catch { /* Level not in mapper */ }
            }

            _levelSetCategoryCache[stats.Name] = result;
            return result;
        }
    }
}
