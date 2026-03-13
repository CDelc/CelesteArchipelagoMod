using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.HonlyHelper;
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
    internal class modBouncySpikes : IGameModification
    {
        private static Type BouncySpikesType;
        private static Hook HookUpdate;

        private delegate void orig_Update(BouncySpikes self);

        public override void Load()
        {
            BouncySpikesType = typeof(BouncySpikes);

            HookUpdate = new Hook(BouncySpikesType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modBouncySpikes).GetMethod(nameof(modUpdate), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
        }

        private static void modUpdate(orig_Update orig, BouncySpikes self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BOUNCY_SPIKES))
            {
                self.Collidable = false;
                foreach(Component c in self.Components)
                {
                    if (c.GetType() == typeof(Image))
                    {
                        ((Image)c).SetColor(new Microsoft.Xna.Framework.Color(50, 0, 0, 0.5f));
                    }
                }
            }
            else
            {
                self.Collidable = true;
                orig(self);
            }
        }
    }
}
