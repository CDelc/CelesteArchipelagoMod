using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modLevel : IGameModification
    {
        public override void Load()
        {
            On.Celeste.Level.CompleteArea_bool_bool_bool += modLevel_CompleteArea_bool_bool_bool;
        }

        public override void Unload()
        {
            On.Celeste.Level.CompleteArea_bool_bool_bool -= modLevel_CompleteArea_bool_bool_bool;
        }

        private static ScreenWipe modLevel_CompleteArea_bool_bool_bool(On.Celeste.Level.orig_CompleteArea_bool_bool_bool orig, Level self, bool spotlightWipe, bool skipScreenWipe, bool skipCompleteScreen)
        {
            if(self.Session == null || !CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                return orig(self, spotlightWipe, skipScreenWipe, skipCompleteScreen);
            }
            string SID = self.Session.Area.SID;
            AreaMode mode = self.Session.Area.Mode;
            long LocationID = ArchipelagoMapper.getLevelCompleteLocationID(SID, mode);

            CelesteArchipelagoModule.Log($"Level {SID} | {mode} cleared, mapping to location id {LocationID}");
            CelesteArchipelagoModule.SaveData.LocationsChecked.Add(ArchipelagoMapper.getLevelCompleteLocationID(SID, mode));
            
            return orig(self, spotlightWipe, skipScreenWipe, skipCompleteScreen);
        }
    }
}
