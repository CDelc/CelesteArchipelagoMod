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
    internal class modSinkingPlatform : IGameModification
    {
        public override void Load()
        {
            On.Celeste.SinkingPlatform.Update += modSinkingPlatform_Update;
        }

        public override void Unload()
        {
            On.Celeste.SinkingPlatform.Update -= modSinkingPlatform_Update;
        }

        private static void modSinkingPlatform_Update(On.Celeste.SinkingPlatform.orig_Update orig, SinkingPlatform self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
            }
            else if (ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.SINKING_PLATFORM))
            {
                self.Collidable = true;
                self.Visible = true;
                orig(self);
            }
            else
            {
                Vector2 shakeAmount = new Vector2(Monocle.Calc.Random.NextFloat(), Monocle.Calc.Random.NextFloat());
                self.shaker.On = true;
                self.shaker.Value = shakeAmount;
                self.shaker.ShakeFor(0.1f, false);
                self.Collidable = false;
                self.Visible = false;
            }
        }
    }
}
