using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CollabUtils2.Entities;
using Celeste.Mod.FemtoHelper;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modCustomFakeHeart : IGameModification
    {

        private static Type CustomFakeHeartType;
        private static Type CustomFakeMiniHeartCollabType;

        private static FieldInfo SpriteField;
        private static FieldInfo MiniModeField;
        private static FieldInfo BloomField;
        private static FieldInfo WigglerField;

        private static FieldInfo SpriteFieldCollab;
        private static FieldInfo BloomFieldCollab;

        private static Hook HookUpdate;
        private static Hook HookUpdateCollab;

        private delegate void orig_Update(CustomFakeHeart self);
        private delegate void orig_Update_Collab(FakeMiniHeart self);

        public override void Load()
        {
            CustomFakeHeartType = typeof(CustomFakeHeart);
            SpriteField = CustomFakeHeartType.GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic);
            MiniModeField = CustomFakeHeartType.GetField("miniMode", BindingFlags.Instance | BindingFlags.NonPublic);
            BloomField = CustomFakeHeartType.GetField("bloom", BindingFlags.Instance | BindingFlags.NonPublic);
            WigglerField = CustomFakeHeartType.GetField("moveWiggler", BindingFlags.Instance | BindingFlags.NonPublic);

            CustomFakeMiniHeartCollabType = typeof(FakeMiniHeart);
            SpriteFieldCollab = CustomFakeMiniHeartCollabType.GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic);
            BloomFieldCollab = CustomFakeMiniHeartCollabType.GetField("bloom", BindingFlags.Instance | BindingFlags.NonPublic);

            HookUpdate = new Hook(CustomFakeHeartType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCustomFakeHeart).GetMethod(nameof(modUpdate), BindingFlags.Static | BindingFlags.NonPublic));

            HookUpdateCollab = new Hook(CustomFakeMiniHeartCollabType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCustomFakeHeart).GetMethod(nameof(modUpdateCollab), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
            HookUpdateCollab?.Dispose();
            HookUpdateCollab = null;
        }

        private static void modUpdate(orig_Update orig, CustomFakeHeart self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                self.Collidable = false;
                Sprite sprite = (Sprite)SpriteField.GetValue(self);
                BloomPoint bloom = (BloomPoint)BloomField.GetValue(self);
                Wiggler wiggler = (Wiggler)WigglerField.GetValue(self);
                sprite.Stop();
                bloom.Visible = false;
                wiggler.RemoveSelf();
                self.ScaleWiggler.RemoveSelf();
                SpriteField.SetValue(self, sprite);
                BloomField.SetValue(self, bloom);

            }
            else
            {
                self.Collidable = true;
            }
        }

        private static void modUpdateCollab(orig_Update_Collab orig, FakeMiniHeart self)
        {
            orig(self);
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MINI_FAKE_CRYSTAL_HEART))
            {
                self.Collidable = false;
                Sprite sprite = (Sprite)SpriteFieldCollab.GetValue(self);
                BloomPoint bloom = (BloomPoint)BloomFieldCollab.GetValue(self);
                sprite.Stop();
                bloom.Visible = false;
                SpriteFieldCollab.SetValue(self, sprite);
                BloomFieldCollab.SetValue(self, bloom);
            }
            else
            {
                self.Collidable = true;
            }
        }

        private static bool isActive(CustomFakeHeart self)
        {
            bool miniMode = (bool)MiniModeField.GetValue(self);
            return miniMode && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MINI_FAKE_CRYSTAL_HEART) ||
                !miniMode && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.FAKE_CRYSTAL_HEART);
        }
    }
}
