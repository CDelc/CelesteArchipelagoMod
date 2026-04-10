using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modBubbleEmitters : IGameModification
    {

        private static Type BubbleEmitterType;

        private static Hook FireHook;
        private delegate void orig_Fire(FloatingBubbleEmitter self);

        public override void Load()
        {
            BubbleEmitterType = typeof(FloatingBubbleEmitter);
            FireHook = new Hook(BubbleEmitterType.GetMethod("Fire", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                typeof(modBubbleEmitters).GetMethod(nameof(modFire), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
        }

        public override void Unload()
        {
            FireHook?.Dispose();
            FireHook = null;
        }

        private static void modFire(orig_Fire orig, FloatingBubbleEmitter self)
        {
            if(!CelesteArchipelagoModule.shouldModMechanics || ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FLOATING_BUBBLE_EMITTERS))
            {
                orig(self);
            }
        }
    }
}
