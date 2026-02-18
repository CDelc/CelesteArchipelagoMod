using Celeste.Mod.CelesteArchipelago.ArchipelagoData;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modStrawberry : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Strawberry.ctor += modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect += modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool += modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.TotalStrawberriesDisplay.Update += modTotalStrawberriesDisplay_Update;
        }

        public override void Unload()
        {
            On.Celeste.Strawberry.ctor -= modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect -= modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool -= modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.TotalStrawberriesDisplay.Update -= modTotalStrawberriesDisplay_Update;
        }

        private static void modStrawberry_ctor(On.Celeste.Strawberry.orig_ctor orig, Strawberry self, EntityData data, Microsoft.Xna.Framework.Vector2 offset, EntityID gid)
        {
            orig(self, data, offset, gid);

            if (SaveData.Instance != null && CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                if (self.Golden)
                {
                    string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                    AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
                    LevelCategory levelCategory = ArchipelagoMapper.getLevelCategory(SID, mode);

                    bool isEnabled = ArchipelagoMapper.goldensEnabledOnCategory(levelCategory);

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

            string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
            AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;

            long locationID = ArchipelagoMapper.getStrawberryLocationID(SID, mode, self.ID, self.Golden, self.Winged);

            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);

            CelesteArchipelagoModule.Log($"Strawberry {self.ID.Key} checked, mapping to location id {locationID.ToString("X")}");
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
    }
}
