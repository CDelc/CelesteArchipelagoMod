using Celeste.Mod.CelesteArchipelago.Archipelago;
using Celeste.Mod.CelesteArchipelago.Modifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    public class modMainMenu : IGameModification
    {
        public override void Load()
        {
            On.Celeste.OuiMainMenu.Enter += modOuiMainMenu_Enter;
            On.Celeste.MainMenuClimb.Render += modMainMenuClimb_Render;
            On.Celeste.MainMenuClimb.Confirm += modMainMenuClimb_Confirm;
        }

        public override void Unload()
        {
            On.Celeste.OuiMainMenu.Enter -= modOuiMainMenu_Enter;
            On.Celeste.MainMenuClimb.Render -= modMainMenuClimb_Render;
            On.Celeste.MainMenuClimb.Confirm -= modMainMenuClimb_Confirm;
        }


        private static System.Collections.IEnumerator modOuiMainMenu_Enter(On.Celeste.OuiMainMenu.orig_Enter orig, OuiMainMenu self, Oui from)
        {
            ArchipelagoManager.Instance.Disconnect(false);

            yield return orig(self, from);
        }

        private static void modMainMenuClimb_Render(On.Celeste.MainMenuClimb.orig_Render orig, MainMenuClimb self)
        {
            orig(self);
            self.label = "Connect";
        }

        private static void modMainMenuClimb_Confirm(On.Celeste.MainMenuClimb.orig_Confirm orig, MainMenuClimb self)
        {
            (self.Scene as Overworld).Goto<OuiConnection>();
        }
    }
}
