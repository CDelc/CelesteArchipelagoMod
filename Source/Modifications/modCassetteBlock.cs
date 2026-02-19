using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.Modifications
{
    internal class modCassetteBlock : IGameModification
    {
        public override void Load()
        {
            On.Celeste.CassetteBlock.Update += modCassetteBlock_Update;
        }

        public override void Unload()
        {
            On.Celeste.CassetteBlock.Update -= modCassetteBlock_Update;
        }

        private static void modCassetteBlock_Update(On.Celeste.CassetteBlock.orig_Update orig, CassetteBlock self)
        {
            if (!CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                orig(self);
            }
            else if (self.color == Calc.HexToColor("49aaf0") && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.BLUE_CASSETTE))
            {
                Disable(self);
            }
            else if (self.color == Calc.HexToColor("f049be") && !ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.PINK_CASSETTE))
            {
                Disable(self);
            }
            else
            {
                orig(self);
            }
        }

        private static void Disable(CassetteBlock self)
        {
            if (self.Activated)
            {
                self.ShiftSize(-1);
                self.SetActivatedSilently(false);
            }
        }
    }
}
