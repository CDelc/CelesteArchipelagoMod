using Celeste.Mod.CelesteArchipelago.Archipelago;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modStrawberry : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Strawberry.ctor += modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect += modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool += modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.SaveData.CheckStrawberry_EntityID += modSaveData_CheckStrawberry_EntityID;
            On.Celeste.TotalStrawberriesDisplay.Update += modTotalStrawberriesDisplay_Update;
        }

        public override void Unload()
        {
            On.Celeste.Strawberry.ctor -= modStrawberry_ctor;
            On.Celeste.Strawberry.OnCollect -= modStrawberry_OnCollect;
            On.Celeste.SaveData.AddStrawberry_AreaKey_EntityID_bool -= modSaveData_AddStrawberry_AreaKey_EntityID_bool;
            On.Celeste.SaveData.CheckStrawberry_EntityID -= modSaveData_CheckStrawberry_EntityID;
            On.Celeste.TotalStrawberriesDisplay.Update -= modTotalStrawberriesDisplay_Update;
        }

        private static void modStrawberry_ctor(On.Celeste.Strawberry.orig_ctor orig, Strawberry self, EntityData data, Microsoft.Xna.Framework.Vector2 offset, EntityID gid)
        {
            orig(self, data, offset, gid);

            if (SaveData.Instance != null)
            {
                if (self.Golden)
                {
                    string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                    AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
                    LevelCategory levelCategory = ArchipelagoMapper.getLevelCategory(SID, mode);

                    self.Active = false;
                    self.Visible = false;
                    self.Collidable = false;
                }
            }
        }

        private static void modStrawberry_OnCollect(On.Celeste.Strawberry.orig_OnCollect orig, Strawberry self)
        {
            orig(self);
            string strawberryString = $"{SaveData.Instance.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{self.ID}";

            Celeste_MultiworldModule.SaveData.StrawberryLocations.Add(strawberryString);
            Logger.Verbose("AP", strawberryString);
        }

        private static void modSaveData_AddStrawberry_AreaKey_EntityID_bool(On.Celeste.SaveData.orig_AddStrawberry_AreaKey_EntityID_bool orig, SaveData self, AreaKey area, EntityID strawberry, bool golden)
        {
            AreaModeStats areaModeStats = self.Areas_Safe[area.ID].Modes[(int)area.Mode];
            if (!areaModeStats.Strawberries.Contains(strawberry))
            {
                areaModeStats.Strawberries.Add(strawberry);
                areaModeStats.TotalStrawberries += 1;
            }
            Stats.Increment(golden ? Stat.GOLDBERRIES : Stat.BERRIES, 1);
        }

        private static bool modSaveData_CheckStrawberry_EntityID(On.Celeste.SaveData.orig_CheckStrawberry_EntityID orig, SaveData self, EntityID strawberry)
        {
            string AP_ID = $"{self.CurrentSession_Safe.Area.ID}_{(int)SaveData.Instance.CurrentSession_Safe.Area.Mode}_{strawberry}";
            return Celeste_MultiworldModule.SaveData.StrawberryLocations.Contains(AP_ID);
        }

        private static void modTotalStrawberriesDisplay_Update(On.Celeste.TotalStrawberriesDisplay.orig_Update orig, TotalStrawberriesDisplay self)
        {
            self.strawberries.showOutOf = true;
            self.strawberries.OutOf = ArchipelagoManager.Instance.StrawberriesRequired;
            orig(self);
        }
    }
}
