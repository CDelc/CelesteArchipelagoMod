using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modMovingBlock : IGameModification
    {

        public override void Load()
        {
            On.Celeste.MoveBlock.OnStaticMoverTrigger += modMoveBlock_OnStaticMoverTrigger;
            On.Celeste.MoveBlock.UpdateColors += modMoveBlock_UpdateColors;
            On.Celeste.MoveBlock.MoveCheck += modMoveBlock_MoveCheck;
        }

        public override void Unload()
        {
            On.Celeste.MoveBlock.OnStaticMoverTrigger -= modMoveBlock_OnStaticMoverTrigger;
            On.Celeste.MoveBlock.UpdateColors -= modMoveBlock_UpdateColors;
            On.Celeste.MoveBlock.MoveCheck -= modMoveBlock_MoveCheck;
        }

        private static void modMoveBlock_OnStaticMoverTrigger(On.Celeste.MoveBlock.orig_OnStaticMoverTrigger orig, MoveBlock self, StaticMover sm)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, sm);
            }
        }

        private static void modMoveBlock_UpdateColors(On.Celeste.MoveBlock.orig_UpdateColors orig, MoveBlock self)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
            }
            else
            {
                self.fillColor = MoveBlock.breakingBgFill;
                foreach (Monocle.Image image in self.topButton)
                {
                    image.Color = self.fillColor;
                }
                foreach (Monocle.Image image2 in self.leftButton)
                {
                    image2.Color = self.fillColor;
                }
                foreach (Monocle.Image image3 in self.rightButton)
                {
                    image3.Color = self.fillColor;
                }

                self.state = MoveBlock.MovementState.Idling;
            }
        }

        private static bool modMoveBlock_MoveCheck(On.Celeste.MoveBlock.orig_MoveCheck orig, MoveBlock self, Microsoft.Xna.Framework.Vector2 speed)
        {
            if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVING_BLOCK) || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return orig(self, speed);
            }
            else
            {
                return true;
            }
        }

    }
}
