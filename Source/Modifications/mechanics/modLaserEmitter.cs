using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modLaserEmitter : IGameModification
    {
        private static Type LaserEmitterType;
        private static Hook HookUpdate;

        private delegate void orig_Update(LaserEmitter self);

        public override void Load()
        {
            LaserEmitterType = typeof(LaserEmitter);
            MethodInfo updateMethod = LaserEmitterType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            HookUpdate = new Hook(updateMethod, typeof(modLaserEmitter).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
        }

        private static void modUpdate(orig_Update orig, LaserEmitter self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self) || self.KillPlayer)
            {
                orig(self);
            }
            else
            {
                self.Collidable = false;
            }
        }

        private static bool isActive(LaserEmitter self)
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TORQUOISE_LASER) && self.ColorChannel == "00ffff" ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_LASER) && self.ColorChannel == "00ff00" ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_LASER) && self.ColorChannel == "ffff00";
        }
    }
}
