using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modRainbowBerry : IGameModification
    {
        private static Type rainbowBerryType;
        private static Type holoRainbowBerryType;
        private static Type collabMapDataProcessorType;

        private static FieldInfo levelSetField;
        private static FieldInfo mapsField;
        private static FieldInfo requiredBerriesField;

        private static FieldInfo silverBerriesStaticField;

        private static ConstructorInfo holoConstructor;

        private static Hook hookAdded;

        private static Action<Entity, Scene> baseEntityAdded;

        private delegate void orig_Added(Entity self, Scene scene);

        public override void Load()
        {
            rainbowBerryType = FindType("Celeste.Mod.CollabUtils2.Entities.RainbowBerry");
            holoRainbowBerryType = FindType("Celeste.Mod.CollabUtils2.Entities.HoloRainbowBerry");
            collabMapDataProcessorType = FindType("Celeste.Mod.CollabUtils2.CollabMapDataProcessor");

            if (rainbowBerryType == null || holoRainbowBerryType == null || collabMapDataProcessorType == null)
                return;

            levelSetField = rainbowBerryType.GetField("levelSet", BindingFlags.NonPublic | BindingFlags.Instance);
            mapsField = rainbowBerryType.GetField("maps", BindingFlags.NonPublic | BindingFlags.Instance);
            requiredBerriesField = rainbowBerryType.GetField("requiredBerries", BindingFlags.NonPublic | BindingFlags.Instance);

            silverBerriesStaticField = collabMapDataProcessorType.GetField("SilverBerries", BindingFlags.Public | BindingFlags.Static);

            holoConstructor = holoRainbowBerryType.GetConstructor(
                new[] { typeof(Vector2), typeof(int), typeof(int) });

            SetupBaseEntityAdded();

            var addedMethod = rainbowBerryType.GetMethod("Added", BindingFlags.Public | BindingFlags.Instance);
            if (addedMethod != null)
                hookAdded = new Hook(addedMethod,
                    typeof(modRainbowBerry).GetMethod(nameof(ModAdded), BindingFlags.NonPublic | BindingFlags.Static));

            On.Celeste.Strawberry.OnCollect += ModOnCollect;
        }

        public override void Unload()
        {
            hookAdded?.Dispose();
            hookAdded = null;
            On.Celeste.Strawberry.OnCollect -= ModOnCollect;
        }

        private static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }
            return null;
        }

        private static void SetupBaseEntityAdded()
        {
            var method = typeof(Entity).GetMethod("Added", BindingFlags.Public | BindingFlags.Instance);
            var dm = new DynamicMethod("base_Entity_Added", null,
                new[] { typeof(Entity), typeof(Scene) }, typeof(Entity), true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, method);
            il.Emit(OpCodes.Ret);
            baseEntityAdded = (Action<Entity, Scene>)dm.CreateDelegate(typeof(Action<Entity, Scene>));
        }

        public static bool IsRainbowBerry(Strawberry self)
        {
            return rainbowBerryType != null && self.GetType() == rainbowBerryType;
        }

        private static void ModAdded(orig_Added orig, Entity self, Scene scene)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, scene);
                return;
            }

            string levelSet = (string)levelSetField.GetValue(self);
            string[] maps = (string[])mapsField.GetValue(self);
            int? requiredBerries = (int?)requiredBerriesField.GetValue(self);

            var silverBerries = (Dictionary<string, Dictionary<string, EntityID>>)silverBerriesStaticField.GetValue(null);

            int totalBerries = 0;
            int collectedBerries = 0;
            Dictionary<string, EntityID> levelSetBerries = null;

            if (silverBerries != null && silverBerries.TryGetValue(levelSet, out levelSetBerries))
            {
                foreach (KeyValuePair<string, EntityID> requiredSilver in levelSetBerries)
                {
                    if (maps == null || maps.Contains(requiredSilver.Key))
                    {
                        totalBerries++;
                        if (IsApSilverBerryUnlocked(requiredSilver.Key, requiredSilver.Value))
                        {
                            collectedBerries++;
                        }
                    }
                }
            }

            if (requiredBerries.HasValue)
            {
                collectedBerries = Math.Min(collectedBerries, requiredBerries.Value);
                totalBerries = requiredBerries.Value;
            }

            if (collectedBerries >= totalBerries && totalBerries > 0)
            {
                // All AP silvers collected: inject into vanilla save data so
                // the original RainbowBerry.Added sees them as collected,
                // then let normal behavior proceed (solid, collectible berry).
                if (levelSetBerries != null)
                    InjectSilverBerriesIntoSaveData(levelSetBerries, maps);

                orig(self, scene);
            }
            else
            {
                // Not all collected: show hologram with AP-based progress.
                baseEntityAdded(self, scene);

                Entity hologram = (Entity)holoConstructor.Invoke(
                    new object[] { self.Position, collectedBerries, totalBerries });
                scene.Add(hologram);

                self.RemoveSelf();
            }
        }

        private static void InjectSilverBerriesIntoSaveData(Dictionary<string, EntityID> berries, string[] maps)
        {
            if (SaveData.Instance == null) return;

            foreach (KeyValuePair<string, EntityID> silver in berries)
            {
                if (maps != null && !maps.Contains(silver.Key))
                    continue;

                AreaData areaData = AreaData.Get(silver.Key);
                if (areaData == null) continue;

                AreaStats stats = SaveData.Instance.GetAreaStatsFor(areaData.ToKey());
                if (stats != null)
                {
                    stats.Modes[0].Strawberries.Add(silver.Value);
                }
            }
        }

        private static void ModOnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;
            if (!IsRainbowBerry(self)) return;

            try
            {
                string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;

                long locationID = 1100000000000
                    + ArchipelagoMapper.getLocationOffset(SID, mode, self.ID.Level)
                    + self.ID.ID;

                CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);

                CelesteArchipelagoModule.Log(
                    $"Rainbow Berry {self.ID.Key} checked, mapping to location id {locationID:X}");
            }
            catch (Exception e)
            {
                CelesteArchipelagoModule.Log($"Failed to check Rainbow Berry location: {e.Message}");
            }
        }

        private static bool IsApSilverBerryUnlocked(string SID, EntityID silverBerryEntityID)
        {
            if (CelesteArchipelagoModule.SaveData?.SilverBerriesUnlocked == null)
                return false;

            try
            {
                long levelID = ArchipelagoMapper.getLevelID(SID, AreaMode.Normal);
                long roomID = ArchipelagoMapper.getRoomID(SID, AreaMode.Normal, silverBerryEntityID.Level);
                long silverBerryItemID = 1000000000000 + levelID * 100000000 + roomID * 100000 + silverBerryEntityID.ID;

                return CelesteArchipelagoModule.SaveData.SilverBerriesUnlocked.Contains(silverBerryItemID);
            }
            catch
            {
                return false;
            }
        }
    }
}
