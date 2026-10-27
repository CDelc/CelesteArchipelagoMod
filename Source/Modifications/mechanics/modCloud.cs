using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Monocle;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCloud : IGameModification
    {

        static Microsoft.Xna.Framework.Color originalColor;
        static bool setColor = false;

        private static Type CustomCloudType;
        private static Hook CustomRender;
        private static Hook CustomUpdate;
        private static FieldInfo CustomCloudFragile;
        private static FieldInfo CustomSprite;
        private delegate void orig_Render(JumpThru self);
        private delegate void orig_Update(JumpThru self);

        public override void Load()
        {
            CustomCloudType = CelesteArchipelagoModule.FindType("FlaglinesAndSuch.CustomCloud");

            On.Celeste.Cloud.Render += modCloud_Render;
            On.Celeste.Cloud.Update += modCloud_Update;

            CustomCloudFragile = CustomCloudType.GetField("fragile", BindingFlags.NonPublic | BindingFlags.Instance);
            CustomSprite = CustomCloudType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            CustomRender = new Hook(CustomCloudType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCloud).GetMethod(nameof(customRender), BindingFlags.Static | BindingFlags.NonPublic));

            CustomUpdate = new Hook(CustomCloudType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCloud).GetMethod(nameof(customUpdate), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            On.Celeste.Cloud.Render -= modCloud_Render;
            On.Celeste.Cloud.Update -= modCloud_Update;

            CustomRender?.Dispose();
            CustomUpdate?.Dispose();

            CustomRender = null;
            CustomUpdate = null;
        }

        private static void customRender(orig_Render orig, JumpThru self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }
            Sprite sprite = (Sprite)CustomSprite.GetValue(self);
            if (isEnabled(self))
            {
                sprite.Color = originalColor;
            }
            else
            {
                sprite.Color = new Microsoft.Xna.Framework.Color(0.7f, 0.7f, 0.7f, 0.5f);
            }

            orig(self);
        }
        
        private static void customUpdate(orig_Update orig, JumpThru self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!isEnabled(self))
            {
                self.Collidable = false;
            }

        }

        private static void modCloud_Render(On.Celeste.Cloud.orig_Render orig, Cloud self)
        {

            if (!CelesteArchipelagoModule.shouldModMechanics)
            {
                orig(self);
                return;
            }

            if (!setColor)
            {
                originalColor = self.sprite.Color;
                setColor = true;
            }

            if (isEnabled(self))
            {
                self.sprite.Color = originalColor;
            }
            else
            {
                self.sprite.Color = new Microsoft.Xna.Framework.Color(0.7f, 0.7f, 0.7f, 0.5f);
            }

            orig(self);
        }

        private static void modCloud_Update(On.Celeste.Cloud.orig_Update orig, Cloud self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            if (!isEnabled(self))
            {
                self.Collidable = false;
            }
        }

        private static bool isEnabled(Cloud self)
        {
            return self.fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CLOUDS) ||
                !self.fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CLOUDS);
        }

        private static bool isEnabled(JumpThru self)
        {
            bool fragile = (bool)CustomCloudFragile.GetValue(self);

            return fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CLOUDS) ||
                !fragile && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CLOUDS);
        }
    }
}
