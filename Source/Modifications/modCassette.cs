using Celeste.Mod.CelesteArchipelago.ArchipelagoData;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCassette : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Cassette.OnPlayer += modCassette_OnPlayer;
        }

        public override void Unload()
        {
            On.Celeste.Cassette.OnPlayer -= modCassette_OnPlayer;
        }

        private static void modCassette_OnPlayer(On.Celeste.Cassette.orig_OnPlayer orig, Cassette self, Player player)
        {
            orig(self, player);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave || SaveData.Instance == null)
            {
                return;
            }


            int ID = (self.Scene as Level).Session.Area.ID;
            SaveData.Instance.Areas_Safe[ID].Cassette = false;

            CelesteArchipelagoModule.Log($"{SaveData.Instance.Areas_Safe[ID].Cassette}");

            AreaKey areaKey = SaveData.Instance.CurrentSession_Safe.Area;

            long locationID = ArchipelagoMapper.getCassetteLocationID(areaKey.SID, areaKey.Mode);

            CelesteArchipelagoModule.Log($"Cassette for {areaKey.SID} {areaKey.Mode}, mapping to location id {locationID}");

            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
        }
    }
}
