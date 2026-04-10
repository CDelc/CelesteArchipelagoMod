using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDashSwitch : IGameModification
    {

        private static Type FlagDashSwitchType;
        public override void Load()
        {
            FlagDashSwitchType = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.FlagDashSwitch");

            On.Celeste.DashSwitch.Update += modDashSwitch_Update;
            On.Celeste.DashSwitch.OnDashed += modDashSwitch_OnDashed;
        }

        public override void Unload()
        {
            On.Celeste.DashSwitch.Update -= modDashSwitch_Update;
            On.Celeste.DashSwitch.OnDashed -= modDashSwitch_OnDashed;
        }

        private static void modDashSwitch_Update(On.Celeste.DashSwitch.orig_Update orig, DashSwitch self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if (!isEnabled(self))
            {
                self.sprite.Play("idle", true);
            }
            orig(self);
        }

        private static DashCollisionResults modDashSwitch_OnDashed(On.Celeste.DashSwitch.orig_OnDashed orig, DashSwitch self, Player player, Microsoft.Xna.Framework.Vector2 direction)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isEnabled(self))
            {
                return orig(self, player, direction);
            }
            else return DashCollisionResults.NormalCollision;
        }

        private static bool isEnabled(DashSwitch self)
        {
            return self.GetType() == typeof(ResizableDashSwitch) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BIG_YELLOW_BUTTON) ||
                self.GetType() == typeof(DashSwitch) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH) ||
                self.GetType() == FlagDashSwitchType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH) ||
                self.GetType() == typeof(BarrierDashSwitch) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_SWITCH) ||
                self.GetType() == typeof(DashSwitch) && isInJavasCrypt() && !self.mirrorMode && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GROWTH_POTION);
        }

        private static bool isInJavasCrypt()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/3-Advanced/Tortoise") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("heartside_Tortoise") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("heartside_Tortoise_B");
        }

    }
}
