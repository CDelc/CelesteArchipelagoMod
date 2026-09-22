using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;
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

            On.Celeste.HeartGemDoor.Added += modHeartGemDoorAdded;
        }


        public override void Unload()
        {
            On.Celeste.SaveData.RegisterHeartGem -= modSaveData_RegisterHeartGem;

            _totalHeartGemsHook?.Dispose();
            _totalHeartGemsHook = null;

            _levelSetTotalHeartGemsHook?.Dispose();
            _levelSetTotalHeartGemsHook = null;

            On.Celeste.HeartGemDoor.Added -= modHeartGemDoorAdded;
        }

        private static void modHeartGemDoorAdded(On.Celeste.HeartGemDoor.orig_Added orig, HeartGemDoor self, Scene scene)
        {
            if (CelesteArchipelagoModule.IsInArchipelagoSave && ArchipelagoManager.Instance.open_heart_gates)
            {
                (scene as Level).Session.SetFlag("opened_heartgem_door_" + self.Requires);
            }

            orig(self, scene);
        }


        private static void modSaveData_RegisterHeartGem(On.Celeste.SaveData.orig_RegisterHeartGem orig, SaveData self, AreaKey area)
        {
            orig(self, area);
            if (!CelesteArchipelagoModule.IsInArchipelagoSave) return;
            long locationID = ArchipelagoMapper.getHeartLocationID(area.SID, area.Mode);
            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(locationID);
        }


        private delegate int orig_SaveDataTotalHeartGems(SaveData self);
        private static int GetTotalHeartGems(orig_SaveDataTotalHeartGems orig, SaveData self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return orig(self);
            }
            return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
        }


        private delegate int orig_LevelSetTotalHeartGems(LevelSetStats self);
        private static int GetLevelSetTotalHeartGems(orig_LevelSetTotalHeartGems orig, LevelSetStats self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return orig(self);
            }

            switch (self.Name)
            {
                case "Celeste":
                    return CelesteArchipelagoModule.SaveData.CrystalHeartsVanilla.Count;
                case "StrawberryJam2021/1-Beginner":
                    return ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.BEGINNER);
                case "StrawberryJam2021/2-Intermediate":
                    return ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.INTERMEDIATE);
                case "StrawberryJam2021/3-Advanced":
                    return ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.ADVANCED);
                case "StrawberryJam2021/4-Expert":
                    return ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.EXPERT);
                case "StrawberryJam2021/5-Grandmaster":
                    return ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.GRANDMASTER) + ArchipelagoUtils.getLobbyNumHeartsCollected(LevelCategory.CRACKED_GRANDMASTER);
                default:
                    return 0;
            }
        }
    }
}
