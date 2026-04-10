using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.JackalHelper.Entities;
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
    internal class modStopWatchRefill : IGameModification
    {
        private static BindingFlags privateLookup = BindingFlags.Instance | BindingFlags.NonPublic;
        private static BindingFlags privateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private static BindingFlags publicLookup = BindingFlags.Public | BindingFlags.Instance;

        private static Type StopwatchRefillType;

        private static FieldInfo SpriteField;
        private static FieldInfo OutlineField;
        private static FieldInfo RespawnTimerField;

        private static Hook HookRender;
        private delegate void orig_Render(StopwatchRefill self);

        public override void Load()
        {
            StopwatchRefillType = typeof(StopwatchRefill);

            SpriteField = StopwatchRefillType.GetField("sprite", privateLookup);
            OutlineField = StopwatchRefillType.GetField("outline", privateLookup);
            RespawnTimerField = StopwatchRefillType.GetField("respawnTimer", privateLookup);

            HookRender = new Hook(StopwatchRefillType.GetMethod("Render", publicLookup), typeof(modStopWatchRefill).GetMethod(nameof(modRender), privateStatic));
        }

        public override void Unload()
        {
            HookRender?.Dispose();
            HookRender = null;
        }

        private static void modRender(orig_Render orig, StopwatchRefill self)
        {
            if(CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.STOPWATCH_CRYSTAL))
            {
                self.Collidable = false;
                ((Sprite)SpriteField.GetValue(self)).Visible = false;
                ((Image)OutlineField.GetValue(self)).Visible = true;
                RespawnTimerField.SetValue(self, 1.0f);
            }
            orig(self);
        }
    }
}
