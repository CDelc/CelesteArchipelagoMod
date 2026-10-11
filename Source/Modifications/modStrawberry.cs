using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CollabUtils2.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modStrawberry : IGameModification
    {
        private static Type MultiRoomStrawberrySeedType;
        private static Hook MultiRoomCtorHook;
        private delegate void orig_multiberry_ctor(MultiRoomStrawberrySeed self, Vector2 position, int index, bool ghost, string sprite, string ghostSprite);

        public override void Load()
        {
            On.Celeste.Strawberry.ctor += modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect += modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool += modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.TotalStrawberriesDisplay.Update += modTotalStrawberriesDisplay_Update;

            MultiRoomStrawberrySeedType = typeof(MultiRoomStrawberrySeed);
            MultiRoomCtorHook = new Hook(MultiRoomStrawberrySeedType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, new Type[] { typeof(Vector2), typeof(int), typeof(bool), typeof(string), typeof(string) }),
                typeof(modStrawberry).GetMethod(nameof(modMultiRoomBerrySeed_ctor), BindingFlags.NonPublic | BindingFlags.Static));

        }

        public override void Unload()
        {
            On.Celeste.Strawberry.ctor -= modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect -= modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool -= modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.TotalStrawberriesDisplay.Update -= modTotalStrawberriesDisplay_Update;

            MultiRoomCtorHook?.Dispose();
        }

        private static void modMultiRoomBerrySeed_ctor(orig_multiberry_ctor orig, MultiRoomStrawberrySeed self, Vector2 position, int index, bool ghost, string sprite, string ghostSprite)
        {
            if(CelesteArchipelagoModule.IsInArchipelagoSave && ArchipelagoManager.Instance.exclude_multiroom_berries)
            {
                orig(self, position, index, true, sprite, ghostSprite);
            }
            else
            {
                orig(self, position, index, ghost, sprite, ghostSprite);
            }
        }

        private static void modStrawberry_ctor(On.Celeste.Strawberry.orig_ctor orig, Strawberry self, EntityData data, Microsoft.Xna.Framework.Vector2 offset, EntityID gid)
        {
            orig(self, data, offset, gid);

            if (SaveData.Instance != null && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                if ((self.Golden || self.GetType() == typeof(SilverBerry)) && !self.Winged)
                {
                    string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                    AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;

                    bool isEnabled = ArchipelagoUtils.deathlessEnabled(SID, mode);

                    self.Active = isEnabled;
                    self.Visible = isEnabled;
                    self.Collidable = isEnabled;
                }
                if (SaveData.Instance.CurrentSession_Safe.Area.SID == "Celeste/1-ForsakenCity" && SaveData.Instance.CurrentSession_Safe.Area.Mode == AreaMode.Normal && Engine.Scene is Level level && level.Session.Level == "end")
                {
                    bool isEnabled = ArchipelagoManager.Instance.winged_golden;

                    self.Active = isEnabled;
                    self.Visible = isEnabled;
                    self.Collidable = isEnabled;
                }
            }
        }

        private static void modStrawberry_OnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;

            if (IsRainbowBerry(self)) return;

            string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
            AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;

            long locationID = ArchipelagoMapper.getStrawberryLocationID(SID, mode, self.ID, self.Golden, self.Winged, isSilverLevel(SID));

            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);

            CelesteArchipelagoModule.Log($"Strawberry {self.ID.Key} checked, mapping to location id {locationID}");
        }

        private static bool isSilverLevel(string sid)
        {
            return sid.Contains("StrawberryJam2021") && !sid.Contains("ZZ-HeartSide");
        }

        private static void modSaveData_AddStrawberry_AreaKey_EntityID_bool(On.Celeste.SaveData.orig_AddStrawberry_AreaKey_EntityID_bool orig, SaveData self, AreaKey area, EntityID strawberry, bool golden)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, area, strawberry, golden);
            }
            else
            {
                AreaModeStats areaModeStats = self.Areas_Safe[area.ID].Modes[(int)area.Mode];
                if (!areaModeStats.Strawberries.Contains(strawberry))
                {
                    areaModeStats.Strawberries.Add(strawberry);
                    areaModeStats.TotalStrawberries += 1;
                }
                Stats.Increment(golden ? Stat.GOLDBERRIES : Stat.BERRIES, 1);
            }
        }


        private static void modTotalStrawberriesDisplay_Update(On.Celeste.TotalStrawberriesDisplay.orig_Update orig, TotalStrawberriesDisplay self)
        {
            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.strawberries.showOutOf = true;
                self.strawberries.OutOf = ArchipelagoManager.Instance.required_strawberries;
            }
            orig(self);
        }

        public static bool IsRainbowBerry(Strawberry self)
        {
            Type rainbowBerryType = CelesteArchipelagoModule.FindType("Celeste.Mod.CollabUtils2.Entities.RainbowBerry");
            return rainbowBerryType != null && self.GetType() == rainbowBerryType;
        }
    }
}
