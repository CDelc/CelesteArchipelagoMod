using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using vitmod;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modTimeCrystal : IGameModification
    {

        private static Type TimeCrystalType;
        private static Hook hookRender;

        private delegate void orig_Render(TimeCrystal self);

        private static FieldInfo OutlineField;
        private static FieldInfo SpriteField;
        private static FieldInfo RespawnTimerField;
        private static FieldInfo UntilDashField;

        public override void Load()
        {

            TimeCrystalType = CelesteArchipelagoModule.FindType("vitmod.TimeCrystal");

            OutlineField = TimeCrystalType.GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
            SpriteField = TimeCrystalType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            RespawnTimerField = TimeCrystalType.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            UntilDashField = TimeCrystalType.GetField("untilDash", BindingFlags.NonPublic | BindingFlags.Instance);

            MethodInfo renderMethod = TimeCrystalType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance);
            hookRender = new Hook(renderMethod, typeof(modTimeCrystal).GetMethod(nameof(modRender), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookRender?.Dispose();
            hookRender = null;
        }

        private static void modRender(orig_Render orig, TimeCrystal self)
        {
            if (!isActive(self) & CelesteArchipelagoModule.shouldModMechanics)
            {
                Monocle.Image outline = ((Monocle.Image)OutlineField.GetValue(self));
                if(outline != null)
                {
                    ((Monocle.Image)OutlineField.GetValue(self)).Visible = true;
                }
                ((Monocle.Image)SpriteField.GetValue(self)).Visible = false;
                self.Collidable = false;
                RespawnTimerField.SetValue(self, 2.5f);
            }
            orig(self);
        }

        private static bool isActive(TimeCrystal self)
        {
            bool untilDash = (bool)UntilDashField.GetValue(self);

            return untilDash && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_TIME_CRYSTAL) ||
                !untilDash && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GRAY_TIME_CRYSTAL);
        }
    }
}
