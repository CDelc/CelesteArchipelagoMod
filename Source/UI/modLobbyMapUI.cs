using Celeste.Mod.CelesteArchipelago.ArchipelagoData;
using Celeste.Mod.CelesteArchipelago.Modifications;
using Celeste.Mod.CelesteArchipelago.Modifications.mechanics;
using Celeste.Mod.CollabLobbyUI.Entities;
using Celeste.Mod.CollabUtils2;
using Celeste.Mod.CollabUtils2.UI;
using Celeste.Mod.StrawberryJam2021.Entities;
using FMOD;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Celeste.Mod.CollabUtils2.Triggers;

namespace Celeste.Mod.CelesteArchipelago.UI
{
    internal class modLobbyMapUI : IGameModification
    {

        private static Type LobbyVisitManagerType;
        private static Type StrawberryJamJarType;
        private static Type ChapterPanelTriggerType;
        private static Hook LoadHook;
        private static Hook AddedHook;
        private static Hook AddedChapterPanelHook;
        private static Hook CtorHook;
        private static ILHook renderHook;
        private static FieldInfo mapField;
        private delegate void orig_Load(LobbyVisitManager self);
        private delegate void orig_Added(StrawberryJamJar self, Scene scene);
        private delegate void orig_Render(NavPointer self);
        private delegate void orig_ctor(ChapterPanelTrigger self, EntityData data, Vector2 offset);

        private static Dictionary<string, NavPointer> heartside_pointers = new Dictionary<string, NavPointer>();

        public override void Load()
        {
            LobbyVisitManagerType = typeof(LobbyVisitManager);
            StrawberryJamJarType = typeof(StrawberryJamJar);
            ChapterPanelTriggerType = typeof(ChapterPanelTrigger);

            LoadHook = new Hook(LobbyVisitManagerType.GetMethod("Load", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
                typeof(modLobbyMapUI).GetMethod(nameof(modLoad), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
            AddedHook = new Hook(StrawberryJamJarType.GetMethod("Added", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                typeof(modLobbyMapUI).GetMethod(nameof(modAdded), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static));
            CtorHook = new Hook(ChapterPanelTriggerType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(EntityData), typeof(Vector2) }),
                typeof(modLobbyMapUI).GetMethod(nameof(ChapterPanelTrigger_ctor), BindingFlags.NonPublic | BindingFlags.Static));

            MethodInfo render = typeof(NavPointer).GetMethod(
                "Render",
                BindingFlags.Public | BindingFlags.Instance);

            renderHook = new ILHook(render, modRender);

            mapField = StrawberryJamJarType.GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        }



        public override void Unload()
        {
            LoadHook?.Dispose();
            AddedHook?.Dispose();
            renderHook?.Dispose();
        }

        private static Color GetPointerColor(string sid)
        {
            long? levelID = ArchipelagoMapper.getLevelID(sid, AreaMode.Normal);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave || levelID == null)
            {
                return Color.White;
            }

            else if (CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress.Contains(levelID.Value) && !CelesteArchipelagoModule.Settings.HideProgressionHighlighting)
            {
                return Color.Lime;
            }
            else if (ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal))
            {
                return Color.Cyan;
            }
            else if (!ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal))
            {
                return Color.DarkGray * 0.4f;
            }
            return Color.DarkRed * 0.4f;

        }

        private static Vector2 GetPointerScale(string sid)
        {
            long? levelID = ArchipelagoMapper.getLevelID(sid, AreaMode.Normal);

            if (!CelesteArchipelagoModule.IsInArchipelagoSave || levelID == null)
            {
                return new Vector2(0.7f, 0.7f);
            }

            else if (CelesteArchipelagoModule.SaveData.LevelsWithPotentialProgress.Contains(levelID.Value) && !CelesteArchipelagoModule.Settings.HideProgressionHighlighting)
            {
                return new Vector2(1.0f, 1.0f);
            }
            else return new Vector2(0.7f, 0.7f);
        }

        // Color the pointers
        private static void modRender(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdcR4(0.7f),
                instr => instr.MatchLdcR4(0.7f),
                instr => instr.MatchNewobj<Vector2>()))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Callvirt, typeof(NavPointer).GetProperty("Map").GetGetMethod());
                cursor.EmitDelegate<Func<string, Vector2>>(GetPointerScale);
            }


            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_White")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Callvirt, typeof(NavPointer).GetProperty("Map").GetGetMethod());
                cursor.EmitDelegate<Func<string, Color>>(GetPointerColor);
            }
        }

        // Spawn the pointers
        private static void modAdded(orig_Added orig, StrawberryJamJar self, Scene scene)
        {
            orig(self, scene);
            if (CelesteArchipelagoModule.IsInArchipelagoSave && CelesteArchipelagoModule.Settings.ShowLevelWaypoints)
            {
                string sid = (string)mapField.GetValue(self);
                if (ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal) && !ArchipelagoUtils.allLocationsChecked(sid, AreaMode.Normal))
                {
                    CollabLobbyUI.Entities.NavPointer pointer = new CollabLobbyUI.Entities.NavPointer(self, sid);
                    pointer.Initialize();
                    scene.Add(pointer);
                }

                switch (SaveData.Instance.CurrentSession_Safe.Area.SID)
                {
                    case "StrawberryJam2021/0-Lobbies/1-Beginner":
                        {
                            bool pointerExists = heartside_pointers.TryGetValue("StrawberryJam2021/1-Beginner/ZZ-HeartSide", out NavPointer pointer);
                            if (!pointerExists) break;
                            if (pointer.Scene == null)
                            {
                                scene.Add(pointer);
                            }
                        }
                        break;
                    case "StrawberryJam2021/0-Lobbies/2-Intermediate":
                        {
                            bool pointerExists = heartside_pointers.TryGetValue("StrawberryJam2021/2-Intermediate/ZZ-HeartSide", out NavPointer pointer);
                            if (!pointerExists) break;
                            if (pointer.Scene == null)
                            {
                                scene.Add(pointer);
                            }
                        }
                        break;
                    case "StrawberryJam2021/0-Lobbies/3-Advanced":
                        {
                            bool pointerExists = heartside_pointers.TryGetValue("StrawberryJam2021/3-Advanced/ZZ-HeartSide", out NavPointer pointer);
                            if (!pointerExists) break;
                            if (pointer.Scene == null)
                            {
                                scene.Add(pointer);
                            }
                        }
                        break;
                    case "StrawberryJam2021/0-Lobbies/4-Expert":
                        {
                            bool pointerExists = heartside_pointers.TryGetValue("StrawberryJam2021/4-Expert/ZZ-HeartSide", out NavPointer pointer);
                            if (!pointerExists) break;
                            if (pointer.Scene == null)
                            {
                                scene.Add(pointer);
                            }
                        }
                        break;
                    case "StrawberryJam2021/0-Lobbies/5-Grandmaster":
                        {
                            bool pointerExists = heartside_pointers.TryGetValue("StrawberryJam2021/5-Grandmaster/ZZ-HeartSide", out NavPointer pointer);
                            if (!pointerExists) break;
                            if (pointer.Scene == null)
                            {
                                scene.Add(pointer);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ChapterPanelTrigger_ctor(orig_ctor orig, ChapterPanelTrigger self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                string sid = data.Attr("map");
                if (ArchipelagoUtils.levelIsUnlocked(sid, AreaMode.Normal) && !ArchipelagoUtils.allLocationsChecked(sid, AreaMode.Normal))
                {
                    CollabLobbyUI.Entities.NavPointer pointer = new CollabLobbyUI.Entities.NavPointer(self, sid);
                    pointer.Initialize();
                    heartside_pointers.TryAdd(sid, pointer);
                }
            }
        }

        // Get the whole lobby map to open up
        private static void modLoad(orig_Load orig, LobbyVisitManager self)
        {
            orig(self);

            if (CelesteArchipelagoModule.IsInArchipelagoSave)
            {
                self.VisitAll();
            }
        }
    }
}
