using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modSpring : IGameModification
    {
        private static Type DashlessSpringType;
        private static Type SpringType;

        private static Hook hookOnCollideDashless;

        private delegate void orig_OnCollideOverride(Entity self, Player player);

        public override void Load()
        {
            On.Celeste.Spring.Render += modSpring_Render;
            On.Celeste.Spring.OnCollide += modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable += modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer += modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker += modSpring_OnSeeker;

            DashlessSpringType = CelesteArchipelagoModule.FindType("Celeste.Mod.MaxHelpingHand.Entities.NoDashRefillSpring");
            SpringType = CelesteArchipelagoModule.FindType("Celeste.Spring");

            MethodInfo collideMethod = DashlessSpringType.GetMethod("OnCollideOverride", BindingFlags.NonPublic | BindingFlags.Instance);
            hookOnCollideDashless = new Hook(collideMethod, typeof(modSpring).GetMethod(nameof(modSpring_DashlessCollideOverride), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            On.Celeste.Spring.Render -= modSpring_Render;
            On.Celeste.Spring.OnCollide -= modSpring_OnCollide;
            On.Celeste.Spring.OnHoldable -= modSpring_OnHoldable;
            On.Celeste.Spring.OnPuffer -= modSpring_OnPuffer;
            On.Celeste.Spring.OnSeeker -= modSpring_OnSeeker;

            hookOnCollideDashless?.Dispose();
            hookOnCollideDashless = null;

        }

        private static void modSpring_Render(On.Celeste.Spring.orig_Render orig, Spring self)
        {
            orig(self);

            if (CelesteArchipelagoModule.shouldModMechanics)
            {
                if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) && self.GetType() == SpringType)
                {
                    self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
                }
                else if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASHLESS_SPRINGS) && self.GetType() == DashlessSpringType)
                {
                    self.sprite.Color = Microsoft.Xna.Framework.Color.DarkRed;
                }
            }
        }

        private static void modSpring_OnCollide(On.Celeste.Spring.orig_OnCollide orig, Spring self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) || self.GetType() != SpringType)
            {
                orig(self, player);
            }
        }

        private static void modSpring_OnHoldable(On.Celeste.Spring.orig_OnHoldable orig, Spring self, Holdable h)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) || self.GetType() != SpringType)
            {
                orig(self, h);
            }
        }

        private static void modSpring_OnPuffer(On.Celeste.Spring.orig_OnPuffer orig, Spring self, Puffer p)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) || self.GetType() != SpringType)
            {
                orig(self, p);
            }
        }

        private static void modSpring_OnSeeker(On.Celeste.Spring.orig_OnSeeker orig, Spring self, Seeker seeker)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SPRINGS) || self.GetType() != SpringType)
            {
                orig(self, seeker);
            }
        }

        private static void modSpring_DashlessCollideOverride(orig_OnCollideOverride orig, Entity self, Player player)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.DASHLESS_SPRINGS) || self.GetType() != DashlessSpringType)
            {
                orig(self, player);
            }
        }
    }
}
