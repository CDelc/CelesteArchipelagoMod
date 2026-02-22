using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modKey : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Key.OnPlayer += modOnPlayer;
            On.Celeste.Key.Added += modAdded;
        }

        public override void Unload()
        {
            On.Celeste.Key.OnPlayer -= modOnPlayer;
            On.Celeste.Key.Added -= modAdded;
        }

        private static void modAdded(On.Celeste.Key.orig_Added orig, Key self, Scene scene)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || self.ID.Equals(new EntityID("0", 0)))
            {
                orig(self, scene);
                return;
            }
            string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
            AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
            long locationID = ArchipelagoMapper.getKeyLocationID(SID, mode, self.ID);

            if (CelesteArchipelagoModule.SaveData.LocationsChecked.Contains(locationID))
            {
                self.Visible = false;
                self.Collidable = false;
            }
            else
            {
                orig(self, scene);
            }
        }

        private static void modOnPlayer(On.Celeste.Key.orig_OnPlayer orig, Key self, Player player)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self, player);
            }
            else
            {
                orig(self, player);
                player.Leader.LoseFollower(self.follower);

                string SID = SaveData.Instance.CurrentSession_Safe.Area.SID;
                AreaMode mode = SaveData.Instance.CurrentSession_Safe.Area.Mode;
                long locationID = ArchipelagoMapper.getKeyLocationID(SID, mode, self.ID);

                CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
                CelesteArchipelagoModule.Log($"Collected Key {SID} {mode} {self.ID.Level} {self.ID.ID}, mapping to location id {locationID}");
            }
        }
    }
}
