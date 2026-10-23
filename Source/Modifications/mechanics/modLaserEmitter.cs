using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.StrawberryJam2021.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static FrostHelper.Entities.GrowBlock;

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
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self) || (self.KillPlayer && !workaroundDeactivate(self)))
            {
                orig(self);
            }
            else
            {
                self.Collidable = false;
            }
        }

        // Workaround for a situation where the player can lock themselves out of several rooms in In Filtration by unlocking the door early and having certain other mechanics open
        private static bool workaroundDeactivate(LaserEmitter self)
        {
            return SaveData.Instance.CurrentSession_Safe.Area.SID.Equals("StrawberryJam2021/2-Intermediate/Jems") && SaveData.Instance.CurrentSession_Safe.Level.Equals("btd-09") &&
                (self.SourceId.ID == 1379 || self.SourceId.ID == 1388) &&
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_LINKED_TRAFFIC_BLOCK) && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_LASER) &&
                CelesteArchipelagoModule.SaveData.UnlockedKeyDoors.Contains(ArchipelagoMapper.getLockDoorID("StrawberryJam2021/2-Intermediate/Jems", AreaMode.Normal, new EntityID {Level = "btd-09", ID=578}));
        }

        private static bool isActive(LaserEmitter self)
        {
            return ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.TORQUOISE_LASER) && self.ColorChannel.ToLower() == "00ffff" ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.GREEN_LASER) && self.ColorChannel.ToLower() == "00ff00" ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.YELLOW_LASER) && self.ColorChannel.ToLower() == "ffff00" ||
                ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.WHITE_LASER) && self.ColorChannel.ToLower() == "000000";
        }
    }
}
