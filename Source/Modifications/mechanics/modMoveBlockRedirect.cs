using Celeste.Mod.CommunalHelper.Entities;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CelesteArchipelago.Modifications.mechanics
{
    internal class modMoveBlockRedirect : IGameModification
    {
        private static Type MoveBlockRedirectType;
        private static FieldInfo OverrideColorField;
        private static FieldInfo StartColorField;

        private static Hook HookUpdate;
        private delegate void orig_Update(MoveBlockRedirect self);
        
        public override void Load()
        {
            MoveBlockRedirectType = typeof(MoveBlockRedirect);
            OverrideColorField = MoveBlockRedirectType.GetField("overrideColor", BindingFlags.NonPublic | BindingFlags.Instance);
            StartColorField = MoveBlockRedirectType.GetField("startColor", BindingFlags.NonPublic | BindingFlags.Instance);
            HookUpdate = new Hook(MoveBlockRedirectType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                typeof(modMoveBlockRedirect).GetMethod(nameof(modUpdate), BindingFlags.NonPublic | BindingFlags.Static));
        }

        public override void Unload()
        {
            HookUpdate?.Dispose();
            HookUpdate = null;
        }

        private static void modUpdate(orig_Update orig, MoveBlockRedirect self)
        {
            if (!CelesteArchipelagoModule.shouldModMechanics || isActive(self))
            {
                self.Collidable = true;
            }
            else
            {
                self.Collidable = false;
                OverrideColorField.SetValue(self, null);
                StartColorField.SetValue(self, Color.Gray);
            }
            orig(self);

        }

        private static bool isActive(MoveBlockRedirect self)
        {
            MoveBlockRedirect.Operations operation = self.Operation;
            bool isDelete = self.DeleteBlock;
            bool isRedirect = self.Modifier == 0 && self.Operation == MoveBlockRedirect.Operations.Add || self.Modifier == 0 && self.Operation == MoveBlockRedirect.Operations.Subtract || self.Modifier == 1 && self.Operation == MoveBlockRedirect.Operations.Multiply;
            bool isAccel = self.Modifier > 0 && self.Operation == MoveBlockRedirect.Operations.Add || self.Modifier > 1 && self.Operation == MoveBlockRedirect.Operations.Multiply;

            return isDelete && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVE_BLOCK_DELETE_FIELD) ||
                isAccel && !isDelete && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVE_BLOCK_ACCELERATOR_FIELD) ||
                isRedirect && !isDelete && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVE_BLOCK_REDIRECT_FIELD) ||
                !isRedirect && !isAccel && !isDelete && ArchipelagoMapper.mechanicEnabled(ArchipelagoMapper.Mechanic.MOVE_BLOCK_DECELERATOR_FIELD);
        }
    }
}
