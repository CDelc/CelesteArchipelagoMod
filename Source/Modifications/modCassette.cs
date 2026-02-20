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
            
            AreaKey areaKey = SaveData.Instance.CurrentSession_Safe.Area;

            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(ArchipelagoMapper.getCassetteLocationID(areaKey.SID, areaKey.Mode));
        }
    }
}
