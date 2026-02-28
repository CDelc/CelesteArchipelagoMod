using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modDreamBlock : IGameModification
    {

        private static Type NormalDreamBlockType;
        private static Type CustomDreamBlockType;
        private static Type ConnectedDreamBlockType;

        private static FieldInfo RefillCountField;

        public override void Load()
        {
            NormalDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.DreamBlock");
            CustomDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.CustomDreamBlock");
            ConnectedDreamBlockType = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.ConnectedDreamBlock");

            RefillCountField = CustomDreamBlockType.GetField("RefillCount", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Celeste.DreamBlock.Render += modDreamBlock_Render;
            On.Celeste.DreamBlock.Update += modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck += modPlayer_DreamDashCheck;
        }

        public override void Unload()
        {
            On.Celeste.DreamBlock.Render -= modDreamBlock_Render;
            On.Celeste.DreamBlock.Update -= modDreamBlock_Update;
            On.Celeste.Player.DreamDashCheck -= modPlayer_DreamDashCheck;
        }

        private static void modDreamBlock_Render(On.Celeste.DreamBlock.orig_Render orig, DreamBlock self)
        {
            orig(self);

            if(!CelesteArchipelagoModule.shouldModMechanics)
            {
                return;
            }

            if(self.GetType() == NormalDreamBlockType)
            {
                self.DisableLightsInside = !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK);
            }
            else if(self.GetType() == ConnectedDreamBlockType)
            {
                int refillCount = (int)RefillCountField.GetValue(self);
                self.DisableLightsInside = !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_DREAM_BLOCK) && refillCount == 2;
            }
            
        }

        private static void modDreamBlock_Update(On.Celeste.DreamBlock.orig_Update orig, DreamBlock self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if (self.GetType() == NormalDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK))
            {
                orig(self);
            }
            else if (self.GetType() == ConnectedDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_DREAM_BLOCK))
            {
                int refillCount = (int)RefillCountField.GetValue(self);
                if(refillCount == 2)
                {
                    orig(self);
                }
            }
        }

        private static bool modPlayer_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                return orig(self, dir);
            }

            if (self.GetType() == NormalDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DREAM_BLOCK))
            {
                return orig(self, dir);
            }
            else if (self.GetType() == ConnectedDreamBlockType && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DOUBLE_DASH_DREAM_BLOCK))
            {
                int refillCount = (int)RefillCountField.GetValue(self);
                if (refillCount == 2)
                {
                    return orig(self, dir);
                }
                else return false;
            }
            else return false;
        }
    }
}
