using Celeste.Mod.BGswitch;
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
    internal class modCoreSwitch : IGameModification
    {

        private static Type BackgroundSwitchType;
        private static FieldInfo BackgroundSwitchSpriteField;
        private static FieldInfo BackgroundCooldownTimerField;

        private static Hook BackgroundSwitchHook;
        private delegate void orig_BGUpdate(BGModeToggle self);

        public override void Load()
        {
            On.Celeste.CoreModeToggle.Update += modCoreModeToggle_Update;

            BackgroundSwitchType = typeof(BGModeToggle);
            BackgroundSwitchSpriteField = BackgroundSwitchType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            BackgroundCooldownTimerField = BackgroundSwitchType.GetField("cooldownTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            BackgroundSwitchHook = new Hook(BackgroundSwitchType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modCoreSwitch).GetMethod(nameof(modBGToggle_Update), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            On.Celeste.CoreModeToggle.Update -= modCoreModeToggle_Update;
        
            BackgroundSwitchHook?.Dispose();
            BackgroundSwitchHook = null;
        }

        private static void modCoreModeToggle_Update(On.Celeste.CoreModeToggle.orig_Update orig, CoreModeToggle self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.CORE_SWITCH))
            {
                self.cooldownTimer = 1.6f;
                self.sprite.Play(self.iceMode ? "ice" : "hot", false, false);
            }

            orig(self);
        }

        private static void modBGToggle_Update(orig_BGUpdate orig, BGModeToggle self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BACKGROUND_SWITCH))
            {
                BackgroundCooldownTimerField.SetValue(self, 1.6f);
                ((Sprite)BackgroundSwitchSpriteField.GetValue(self)).Play("fg", false, false);
            }

            orig(self);
        }
    }
}
