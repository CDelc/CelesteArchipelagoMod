using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCompleteScreen : IGameModification
    {
        public override void Load()
        {
            On.Celeste.AreaComplete.GetCustomCompleteScreenTitle += modCustomCompleteScreenTitle;
        }


        public override void Unload()
        {
            On.Celeste.AreaComplete.GetCustomCompleteScreenTitle -= modCustomCompleteScreenTitle;
        }

        private string modCustomCompleteScreenTitle(On.Celeste.AreaComplete.orig_GetCustomCompleteScreenTitle orig, AreaComplete self)
        {
            string sid = self.Session.Area.SID;
            AreaMode mode = self.Session.Area.Mode;
            
            bool hasMapping = ArchipelagoMapper.cassette_crystal_heart_display_text.TryGetValue(ArchipelagoMapper.getLevelCompleteLocationID(sid, mode), out string displayText);

            if (!hasMapping) return orig(self);

            else return displayText;
        }
    }
}
