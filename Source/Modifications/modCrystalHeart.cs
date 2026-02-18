using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCrystalHeart : IGameModification
    {

        private static Hook _totalHeartGemsHook;
        private static Hook _levelSetTotalHeartGemsHook;

        public override void Load()
        {
            On.Celeste.SaveData.RegisterHeartGem += modSaveData_RegisterHeartGem;

            var saveDataTotalHeartGems = typeof(SaveData)
                .GetProperty("TotalHeartGems")?.GetGetMethod();
            if (saveDataTotalHeartGems != null)
            {
                _totalHeartGemsHook = new Hook(
                    saveDataTotalHeartGems,
                    typeof(modCrystalHeart).GetMethod(
                        nameof(GetTotalHeartGems),
                        BindingFlags.NonPublic | BindingFlags.Static)
                );
            }

            var levelSetTotalHeartGems = typeof(LevelSetStats)
                .GetProperty("TotalHeartGems")?.GetGetMethod();
            if (levelSetTotalHeartGems != null)
            {
                _levelSetTotalHeartGemsHook = new Hook(
                    levelSetTotalHeartGems,
                    typeof(modCrystalHeart).GetMethod(
                        nameof(GetLevelSetTotalHeartGems),
                        BindingFlags.NonPublic | BindingFlags.Static)
                );
            }
        }


        public override void Unload()
        {
            On.Celeste.SaveData.RegisterHeartGem -= modSaveData_RegisterHeartGem;

            _totalHeartGemsHook?.Dispose();
            _totalHeartGemsHook = null;

            _levelSetTotalHeartGemsHook?.Dispose();
            _levelSetTotalHeartGemsHook = null;
        }


        private void modSaveData_RegisterHeartGem(On.Celeste.SaveData.orig_RegisterHeartGem orig, SaveData self, AreaKey area)
        {
            orig(self, area);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return;
            }

            long locationID = ArchipelagoMapper.getHeartLocationID(area.SID, area.Mode);
            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
        }


        private delegate int orig_SaveDataTotalHeartGems(SaveData self);
        private static int GetTotalHeartGems(orig_SaveDataTotalHeartGems orig, SaveData self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || !ArchipelagoManager.Instance.Ready)
            {
                return orig(self);
            }
            return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
        }


        private delegate int orig_LevelSetTotalHeartGems(LevelSetStats self);
        private static int GetLevelSetTotalHeartGems(orig_LevelSetTotalHeartGems orig, LevelSetStats self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave || !ArchipelagoManager.Instance.Ready)
            {
                return orig(self);
            }

            switch (self.Name)
            {
                case "Celeste":
                    return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
                case "StrawberryJam2021/1-Beginner":
                    return ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.BEGINNER);
                case "StrawberryJam2021/2-Intermediate":
                    return ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.INTERMEDIATE);
                case "StrawberryJam2021/3-Advanced":
                    return ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.ADVANCED);
                case "StrawberryJam2021/4-Expert":
                    return ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.EXPERT);
                case "StrawberryJam2021/5-Grandmaster":
                    return ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.GRANDMASTER) + ArchipelagoMapper.getLobbyNumHeartsCollected(LevelCategory.CRACKED_GRANDMASTER);
                default:
                    return 0;
            }
        }
    }
}
