using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.IsaGrabBag;
using Microsoft.Xna.Framework;
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
    internal class modZipline : IGameModification
    {

        private static Type ZiplineType;
        private static FieldInfo SpriteField;

        private static Hook hookUpdate;

        private delegate void orig_Update(ZipLine self);

        public override void Load()
        {
            ZiplineType = typeof(ZipLine);
            SpriteField = ZiplineType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo updateMethod = ZiplineType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
            hookUpdate = new Hook(updateMethod, typeof(modZipline).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
            
        }

        public override void Unload()
        {
            hookUpdate?.Dispose();
            hookUpdate = null;
        }


        private static void modUpdate(orig_Update orig, ZipLine self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            Sprite sprite = (Sprite)SpriteField.GetValue(self);

            if (!ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.ZIPLINE))
            {
                self.Collidable = false;
                sprite.Color.R = (byte)(0.5f * 255);
                sprite.Color.G = (byte)(0.5f * 255);
                sprite.Color.B = (byte)(0.5f * 255);
                sprite.Color.A = (byte)(0.1f * 255);
                SpriteField.SetValue(self, sprite);
                orig(self);
            }
            else
            {
                self.Collidable = true;
                sprite.Color.R = (byte)255;
                sprite.Color.G = (byte)255;
                sprite.Color.B = (byte)255;
                sprite.Color.A = (byte)255;
                SpriteField.SetValue(self, sprite);
                orig(self);
            }
        }
    }
}
