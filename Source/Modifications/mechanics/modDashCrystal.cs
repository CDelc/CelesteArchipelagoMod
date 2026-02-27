using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDashCrystal : IGameModification
    {

        private static Type DashCrystalType;
        private static Type JumpRefillType;
        private static Type DreamRefillType;

        private static FieldInfo ExtraJumpsField;

        public override void Load()
        {
            DashCrystalType = CelesteArchipelagoModule.FindType("Celeste.Refill");
            JumpRefillType = CelesteArchipelagoModule.FindType("ExtendedVariants.Entities.ForMappers.JumpRefill");
            DreamRefillType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.DashStates.DreamTunnelRefill");

            ExtraJumpsField = JumpRefillType.GetField("extraJumps", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Celeste.Refill.Render += modRefill_Render;
            //On.Celeste.Refill.OnPlayer += modRefill_OnPlayer;
        }

        public override void Unload()
        {
            On.Celeste.Refill.Render -= modRefill_Render;
            //On.Celeste.Refill.OnPlayer -= modRefill_OnPlayer;
        }

        private static void modRefill_Render(On.Celeste.Refill.orig_Render orig, Refill self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            if(self.GetType() == DashCrystalType)
            {
                if (self.twoDashes && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL))
                {
                    self.outline.Visible = true;
                    self.sprite.Visible = false;
                    self.Collidable = false;
                    self.respawnTimer = 2.5f;
                }
                else if (!self.twoDashes && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
                {
                    self.outline.Visible = true;
                    self.sprite.Visible = false;
                    self.Collidable = false;
                    self.respawnTimer = 2.5f;
                }
            }
            if (self.GetType() == DreamRefillType)
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_DASH_CRYSTALS))
                {
                    self.outline.Visible = true;
                    self.sprite.Visible = false;
                    self.Collidable = false;
                    self.respawnTimer = 2.5f;
                }
            }
            if (self.GetType() == JumpRefillType)
            {
                int extraJumps = (int)ExtraJumpsField.GetValue(self);
                if ((extraJumps == 1 && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SINGLE_JUMP_REFILL)) ||
                    (extraJumps == 3 && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRIPLE_JUMP_REFILL)))
                {
                    foreach (Component c in self)
                    {
                        c.Visible = false;
                    }
                    self.outline.Visible = true;
                    self.Collidable = false;
                    self.respawnTimer = 2.5f;
                }
            }
            orig(self);
        }

        //private static void modRefill_OnPlayer(On.Celeste.Refill.orig_OnPlayer orig, Refill self, Player player)
        //{

        //    if (!CelesteArchipelagoModule.shouldModMechanics)
        //    {
        //        orig(self, player);
        //        return;
        //    }

        //    if (self.GetType() == DashCrystalType)
        //    {
        //        if (self.twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_CRYSTAL))
        //        {
        //            orig(self, player);
        //        }
        //        else if (!self.twoDashes && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASH_CRYSTALS))
        //        {
        //            orig(self, player);
        //        }
        //    }
        //    else if(self.GetType() == DreamRefillType)
        //    {
        //        if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_DASH_CRYSTALS))
        //        {
        //            orig(self, player);
        //        }
        //    }
        //    if (self.GetType() == JumpRefillType)
        //    {
        //        int extraJumps = (int)ExtraJumpsField.GetValue(self);
        //        if (extraJumps == 1 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SINGLE_JUMP_REFILL))
        //        {
        //            orig(self, player);
        //        }
        //        else if (extraJumps == 3 && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRIPLE_JUMP_REFILL))
        //        {
        //            orig(self, player);
        //        }
        //        else if(extraJumps != 1 && extraJumps != 3)
        //        {
        //            orig(self, player);
        //        }
        //    }
        //    else
        //    {
        //        orig(self, player);
        //    }
        //}
    }
}
