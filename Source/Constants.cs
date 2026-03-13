using Celeste.Mod.CelesteArchipelago.Modifications;
using Celeste.Mod.CelesteArchipelago.Modifications.mechanics;
using Celeste.Mod.CelesteArchipelago.UI;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

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
            new modPlayer(),
            new modSpring(),
            new modTrafficBlocks(),
            new modCassetteBlock(),
            new modDashCrystal(),
            new modCassette(),
            new modLevel(),
            new modKey(),
            new modLockBlock(),
            new modCrumblePlatform(),
            new modSummitGem(),
            new modTouchSwitch(),
            new modDreamBlock(),
            new modSinkingPlatform(),
            new modBadelineBooster(),
            new modBubbles(),
            new modCloud(),
            new modMovingBlock(),
            new modMovingPlatform(),
            new modSwapBlock(),
            new modDashSwitch(),
            new modFeather(),
            new modWhiteBlock(),
            new modBreakerBox(),
            new modBumper(),
            new modCoreBalls(),
            new modCoreBlock(),
            new modCoreSwitch(),
            new modKevin(),
            new modPufferFish(),
            new modBird(),
            new modSeeker(),
            new modActor(),
            new modLoopBlock(),
            new modDashStateRefill(),
            new modIntroCrusher(),
            new modDashZipMover(),
            new modGenericCustomBooster(),
            new modSwitchBlock(),
            new modGravityTrigger(),
            new modTimeCrystal(),
            new modRefillShard(),
            new modPipe(),
            new modUninterruptedNRCB(),
            new modLinkedZipMover(),
            new modPushBlock(),
            new modVertigo(),
            new modSeekerBarrier(),
            new modPortal(),
            new modStationBlock(),
            new modZipline(),
            new modLaserEmitter(),
            new modRefillWall(),
            new modBouncySpikes(),
            new modCustomFakeHeart(),
            new modMoveBlockRedirect(),
            new modDashBoostField(),
            new modForceJumpCrystal(),
            new modShroomRefill(),
            new modGardenMoss()
        };

        public static int SAVE_ID = 144;

        public static string LOG_PREFIX = "CelesteArchipelago";

        public static readonly Color DisabledColor = Color.DarkRed * 0.5f;

        public static void DrawDisabledRect(Collider collider, Color? color = null)
        {
            Color drawColor = color ?? DisabledColor;

            try
            {
                Draw.Rect(collider, drawColor);
            }
            catch(InvalidOperationException e)
            {

            }
        }
    }
}
