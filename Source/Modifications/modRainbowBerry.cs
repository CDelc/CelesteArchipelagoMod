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
        }

        public override void Unload()
        {
            hookAdded?.Dispose();
            hookAdded = null;
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

        private static void ModAdded(orig_Added orig, Entity self, Scene scene)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, scene);
                return;
            }

            // Non-virtual call to Entity.Added for minimal scene setup,
            // skipping both Strawberry.Added and RainbowBerry.Added logic.
            baseEntityAdded(self, scene);

            string levelSet = (string)levelSetField.GetValue(self);
            string[] maps = (string[])mapsField.GetValue(self);
            int? requiredBerries = (int?)requiredBerriesField.GetValue(self);

            var silverBerries = (Dictionary<string, Dictionary<string, EntityID>>)silverBerriesStaticField.GetValue(null);

            int totalBerries = 0;
            int collectedBerries = 0;

            if (silverBerries != null && silverBerries.TryGetValue(levelSet, out var levelSetBerries))
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

            Entity hologram = (Entity)holoConstructor.Invoke(
                new object[] { self.Position, collectedBerries, totalBerries });
            scene.Add(hologram);

            self.RemoveSelf();
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
