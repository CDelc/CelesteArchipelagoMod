using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modRoom : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Level.LoadLevel += modLevel_LoadLevel;
            On.Celeste.Level.TransitionTo += modLevel_TransitionTo;
        }

        public override void Unload()
        {
            On.Celeste.Level.LoadLevel -= modLevel_LoadLevel;
            On.Celeste.Level.TransitionTo -= modLevel_TransitionTo;
        }

        private void modLevel_TransitionTo(On.Celeste.Level.orig_TransitionTo orig, Level self, LevelData next, Vector2 direction)
        {
            orig(self, next, direction);
            CheckRoom(self, next.Name);
        }

        private void modLevel_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            orig(self, playerIntro, isFromLoader);
            CheckRoom(self, self.Session.Level);
        }

        private static void CheckRoom(Level level, string room)
        {
            if (!ArchipelagoManager.Instance.Ready || !ArchipelagoManager.Instance.room_checks)
            {
                return;
            }

            string SID = level.Session.Area.SID;
            AreaMode mode = level.Session.Area.Mode;

            long locationID = ArchipelagoMapper.getRoomLocationID(SID, mode, room);
            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
            CelesteArchipelagoModule.Log($"Room {room} checked in {SID} {mode}, mapping to location id {locationID:X}");

        }
    }
}
