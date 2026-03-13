using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CommunalHelper.Entities;
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
    internal class modBreakerBox : IGameModification
    {

        private static Type VanillaBreakerBox;
        private static Type SJFlagBreakerBox;
        private static Type HelpingHandFlagBreakerBox;
        private static Type CommunalFlagBreakerBox;
        private static Type TrackSwitchBoxType;

        private static Hook HookUpdateVanillaBreakerBox;
        private static Hook HookUpdateSJFlagBreakerBox;
        private static Hook HookUpdateHelpingHandFlagBreakerBox;
        private static Hook HookUpdateCommunalFlagBreakerBox;
        private static Hook HookUpdateTrackSwitchBoxType;
        private static Hook HookDashVanillaBreakerBox;
        private static Hook HookDashSJFlagBreakerBox;
        private static Hook HookDashHelpingHandFlagBreakerBox;
        private static Hook HookDashCommunalFlagBreakerBox;
        private static Hook HookDashTrackSwitchBoxType;

        private static FieldInfo VanillaBreakerBoxSpriteField;
        private static FieldInfo SJFlagBreakerBoxSpriteField;
        private static FieldInfo HelpingHandFlagBreakerBoxSpriteField;
        private static FieldInfo CommunalFlagBreakerBoxSpriteField;
        private static FieldInfo TrackSwitchBoxSpriteField;

        private delegate void orig_Update(Solid self);
        private delegate DashCollisionResults orig_Dash(Solid self, Player player, Microsoft.Xna.Framework.Vector2 dir);

        public override void Load()
        {
            VanillaBreakerBox = typeof(LightningBreakerBox);
            SJFlagBreakerBox = CelesteArchipelagoModule.FindType("Celeste.Mod.StrawberryJam2021.Entities.FlagBreakerBox");
            HelpingHandFlagBreakerBox = typeof(MaxHelpingHand.Entities.FlagBreakerBox);
            CommunalFlagBreakerBox = CelesteArchipelagoModule.FindType("Celeste.Mod.CommunalHelper.Entities.StrawberryJam.FlagBreakerBox");
            TrackSwitchBoxType = typeof(TrackSwitchBox);

            VanillaBreakerBoxSpriteField = VanillaBreakerBox.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            SJFlagBreakerBoxSpriteField = SJFlagBreakerBox.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            HelpingHandFlagBreakerBoxSpriteField = HelpingHandFlagBreakerBox.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            CommunalFlagBreakerBoxSpriteField = CommunalFlagBreakerBox.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            TrackSwitchBoxSpriteField = TrackSwitchBoxType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo VanillaUpdateMethod = VanillaBreakerBox.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo SJFlagUpdateMethod = SJFlagBreakerBox.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo HelpingHandFlagUpdateMethod = HelpingHandFlagBreakerBox.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo CommunalFlagUpdateMethod = CommunalFlagBreakerBox.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo TrackSwitchUpdateMethod = TrackSwitchBoxType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);

            MethodInfo VanillaDashMethod = VanillaBreakerBox.GetMethod("Dashed", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo SJFlagDashMethod = SJFlagBreakerBox.GetMethod("Dashed", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo HelpingHandFlagDashMethod = HelpingHandFlagBreakerBox.GetMethod("Dashed", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo CommunalFlagDashMethod = CommunalFlagBreakerBox.GetMethod("Dashed", BindingFlags.Public | BindingFlags.Instance);
            MethodInfo TrackSwitchDashMethod = TrackSwitchBoxType.GetMethod("Dashed", BindingFlags.Public | BindingFlags.Instance);

            HookUpdateVanillaBreakerBox = new Hook(VanillaUpdateMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Update), BindingFlags.Static | BindingFlags.NonPublic));
            HookUpdateSJFlagBreakerBox = new Hook(SJFlagUpdateMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Update), BindingFlags.Static | BindingFlags.NonPublic));
            HookUpdateHelpingHandFlagBreakerBox = new Hook(HelpingHandFlagUpdateMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Update), BindingFlags.Static | BindingFlags.NonPublic));
            HookUpdateCommunalFlagBreakerBox = new Hook(CommunalFlagUpdateMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Update), BindingFlags.Static | BindingFlags.NonPublic));
            HookUpdateTrackSwitchBoxType = new Hook(TrackSwitchUpdateMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Update), BindingFlags.Static | BindingFlags.NonPublic));

            HookDashVanillaBreakerBox = new Hook(VanillaDashMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Dashed), BindingFlags.Static | BindingFlags.NonPublic));
            HookDashSJFlagBreakerBox = new Hook(SJFlagDashMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Dashed), BindingFlags.Static | BindingFlags.NonPublic));
            HookDashHelpingHandFlagBreakerBox = new Hook(HelpingHandFlagDashMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Dashed), BindingFlags.Static | BindingFlags.NonPublic));
            HookDashCommunalFlagBreakerBox = new Hook(CommunalFlagDashMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Dashed), BindingFlags.Static | BindingFlags.NonPublic));
            HookDashTrackSwitchBoxType = new Hook(TrackSwitchDashMethod, typeof(modBreakerBox).GetMethod(nameof(modLightningBreakerBox_Dashed), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            HookUpdateVanillaBreakerBox?.Dispose();
            HookUpdateSJFlagBreakerBox?.Dispose();
            HookUpdateHelpingHandFlagBreakerBox?.Dispose();
            HookUpdateCommunalFlagBreakerBox?.Dispose();
            HookUpdateTrackSwitchBoxType?.Dispose();
            HookDashVanillaBreakerBox?.Dispose();
            HookDashSJFlagBreakerBox?.Dispose();
            HookDashHelpingHandFlagBreakerBox?.Dispose();
            HookDashCommunalFlagBreakerBox?.Dispose();
            HookDashTrackSwitchBoxType?.Dispose();

            HookUpdateVanillaBreakerBox = null;
            HookUpdateSJFlagBreakerBox = null;
            HookUpdateHelpingHandFlagBreakerBox = null;
            HookUpdateCommunalFlagBreakerBox = null;
            HookUpdateTrackSwitchBoxType = null;
            HookDashVanillaBreakerBox = null;
            HookDashSJFlagBreakerBox = null;
            HookDashHelpingHandFlagBreakerBox = null;
            HookDashCommunalFlagBreakerBox = null;
            HookDashTrackSwitchBoxType = null;
        }

        private static DashCollisionResults modLightningBreakerBox_Dashed(orig_Dash orig, Solid self, Player player, Microsoft.Xna.Framework.Vector2 dir)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                return DashCollisionResults.Bounce;
            }
            else
            {
                return orig(self, player, dir);
            }
        }

        private static void modLightningBreakerBox_Update(orig_Update orig, Solid self)
        {
            orig(self);

            if (!CelesteArchipelagoModule.shouldModMechanics) return;

            Sprite sprite = getSprite(self);

            if (!isActive(self))
            {
                sprite.Color.R = (byte)(0.3f * 255.0f);
                sprite.Color.G = (byte)(0.3f * 255.0f);
                sprite.Color.B = (byte)(0.3f * 255.0f);
                sprite.Color.A = (byte)(0.3f * 255.0f);
            }
            else
            {
                sprite.Color.R = (byte)255;
                sprite.Color.G = (byte)255;
                sprite.Color.B = (byte)255;
                sprite.Color.A = (byte)255;
            }
        }

        private static bool isActive(Solid self)
        {
            if (self.GetType() == VanillaBreakerBox ||
                self.GetType() == SJFlagBreakerBox ||
                self.GetType() == CommunalFlagBreakerBox ||
                self.GetType() == HelpingHandFlagBreakerBox)
            {
                return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BREAKER_SWITCH);
            }
            else if (self.GetType() == TrackSwitchBoxType)
            {
                return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TRACK_SWITCH_BOX);
            }
            else return true;
        }

        private static Sprite getSprite(Solid self)
        {
            if (self.GetType() == VanillaBreakerBox)
            {
                return (Sprite)VanillaBreakerBoxSpriteField.GetValue(self);
            }
            else if (self.GetType() == SJFlagBreakerBox)
            {
                return (Sprite)SJFlagBreakerBoxSpriteField.GetValue(self);
            }
            else if (self.GetType() == CommunalFlagBreakerBox)
            {
                return (Sprite)CommunalFlagBreakerBoxSpriteField.GetValue(self);
            }
            else if (self.GetType() == HelpingHandFlagBreakerBox)
            {
                return (Sprite)HelpingHandFlagBreakerBoxSpriteField.GetValue(self);
            }
            else if (self.GetType() == TrackSwitchBoxType)
            {
                return (Sprite)TrackSwitchBoxSpriteField.GetValue(self);
            }
            else return null;
        }
    }
}
