using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.PandorasBox;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modPipe : IGameModification
    {

        private static Type PipeType;

        private static Hook hookUpdate;
        private delegate void orig_Update(MarioClearPipe self);

        public override void Load()
        {
            PipeType = typeof(MarioClearPipe);

            MethodInfo updateMethod = PipeType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            hookUpdate = new Hook(updateMethod, typeof(modPipe).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            hookUpdate?.Dispose();
            hookUpdate = null;
        }

        private static void modUpdate(orig_Update orig, MarioClearPipe self)
        {
            if (CelesteArchipelagoModule.shouldModMechanics && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PIPE)) return;
            orig(self);
        }
    }
}
