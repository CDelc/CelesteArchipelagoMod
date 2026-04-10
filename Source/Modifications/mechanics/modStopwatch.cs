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
    internal class modStopwatch : IGameModification
    {

        private static Type StopwatchType;

        private static FieldInfo StopTypeField;
        private static FieldInfo OutlineField;
        private static FieldInfo ClockFrontField;
        private static FieldInfo ClockColorField;
        private static FieldInfo RespawnDelayField;

        private static Hook RenderHook;
        private delegate void orig_Render(Entity self);

        public override void Load()
        {
            StopwatchType = CelesteArchipelagoModule.FindType("Celeste.Mod.Spirialis.Stopwatch");

            StopTypeField = StopwatchType.GetField("stopType", BindingFlags.NonPublic | BindingFlags.Instance);
            OutlineField = StopwatchType.GetField("outline", BindingFlags.NonPublic | BindingFlags.Instance);
            ClockFrontField = StopwatchType.GetField("clockFront", BindingFlags.NonPublic | BindingFlags.Instance);
            ClockColorField = StopwatchType.GetField("clockColor", BindingFlags.NonPublic | BindingFlags.Instance);
            RespawnDelayField = StopwatchType.GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);

            RenderHook = new Hook(StopwatchType.GetMethod("Render", BindingFlags.Public | BindingFlags.Instance),
                typeof(modStopwatch).GetMethod(nameof(modRender), BindingFlags.Static | BindingFlags.NonPublic));
        }

        public override void Unload()
        {
            RenderHook?.Dispose();
            RenderHook = null;
        }

        private static void modRender(orig_Render orig, Entity self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !isActive(self))
            {
                self.Collidable = false;
                ((Image)OutlineField.GetValue(self)).Visible = true;
                ((Image)ClockFrontField.GetValue(self)).Visible = false;
                ((Image)ClockColorField.GetValue(self)).Visible = false;
                RespawnDelayField.SetValue(self, 1.0f);
            }
            else self.Collidable = true;
            orig(self);
        }

        private static bool isActive(Entity self)
        {
            bool isGreen = false;
            bool isGray = false;
            if((int)StopTypeField.GetValue(self) == 2) isGreen = true;
            else if((int)StopTypeField.GetValue(self) == 0) isGray = true;
            return !isGreen && !isGray && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_STOPWATCH) ||
                isGray && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GRAY_STOPWATCH) ||
                isGreen && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_STOPWATCH);
        }
    }
}
