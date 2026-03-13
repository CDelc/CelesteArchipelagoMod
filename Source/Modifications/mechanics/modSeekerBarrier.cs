using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modSeekerBarrier : IGameModification
    {
        public override void Load()
        {
            On.Celeste.SeekerBarrier.Render += modRender;
        }

        public override void Unload()
        {
            On.Celeste.SeekerBarrier.Render -= modRender;
        }

        private static void modRender(On.Celeste.SeekerBarrier.orig_Render orig, SeekerBarrier self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && modVertigo.isInVertigo() && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.VERTIGO_LINKED_TELEPORT))
            {
                Constants.DrawDisabledRect(self.Collider, Color.Black * 0.9f);
            }
            else if(CelesteArchipelagoModule.shouldModMechanics && (isInPotential() || isInRightsideDown()) && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GRAVITY_FIELD))
            {
                Constants.DrawDisabledRect(self.Collider, Color.Black * 0.9f);
            }
        }

        private static bool isInPotential()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/1-Beginner/frozenflygone") ||
                SaveData.Instance.CurrentSession_Safe.Level.StartsWith("cp2_15_heartside_frozenflygone_");
        }

        private static bool isInRightsideDown()
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/3-Advanced/Vamp") ||
                SaveData.Instance.CurrentSession_Safe.Level.Equals("heartside_Vamp");
        }
    }
}
