using Celeste.Mod.CelesteArchipelago.Modifications;
using Celeste.Mod.CelesteArchipelago.UI;
using System.Collections.Generic;

namespace Celeste.Mod.CelesteArchipelago
{
    internal class Constants
    {

        public static List<IGameModification> modifications = new List<IGameModification>()
        {
            new modMainMenu(),
            new modOuiChapterSelect(),
            new modStrawberry(),
            new modCrystalHeart(),
            new modRoom(),
            new modPlayer()
        };

        public static int SAVE_ID = 144;

        public static string LOG_PREFIX = "CelesteArchipelago";

    }
}
