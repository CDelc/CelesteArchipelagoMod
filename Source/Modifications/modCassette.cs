using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCassette : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Cassette.OnPlayer += modCassette_OnPlayer;
            On.Celeste.Cassette.UnlockedBSide.Added += modUnlockedBSide;
        }

        public override void Unload()
        {
            On.Celeste.Cassette.OnPlayer -= modCassette_OnPlayer;
            On.Celeste.Cassette.UnlockedBSide.Added -= modUnlockedBSide;
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

        private static void modUnlockedBSide(On.Celeste.Cassette.UnlockedBSide.orig_Added orig, Entity self, Scene scene)
        {
            orig(self, scene);

            AreaKey area = SaveData.Instance.CurrentSession_Safe.Area;

            bool hasMapping = ArchipelagoMapper.cassette_crystal_heart_display_text.TryGetValue(ArchipelagoMapper.getCassetteLocationID(area.SID, area.Mode), out string displayText);

            if (!hasMapping) return;

            ((Cassette.UnlockedBSide)self).text = ActiveFont.FontSize.AutoNewline(displayText, 900);
        }
    }
}
