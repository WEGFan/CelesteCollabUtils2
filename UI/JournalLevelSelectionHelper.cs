using System;
using System.Collections;
using Celeste.Mod.CollabUtils2.Triggers;
using Celeste.Mod.Entities;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.CollabUtils2.UI {
    public static class JournalLevelSelectionHelper {
        public static SceneWrappingEntity<Overworld> levelSelectoverworldWrapper;
        private static bool skipSetMusic;
        private static bool skipSetAmbience;

        public static bool IsSelectingLevel => levelSelectoverworldWrapper?.Scene == Engine.Scene;

        public static void Load() {
            Everest.Events.Level.OnPause += OnPause;
            On.Celeste.OuiJournal.Close += OnJournalClose;
            On.Celeste.Audio.SetMusic += OnSetMusic;
            On.Celeste.Audio.SetAmbience += OnSetAmbience;
        }

        public static void Unload() {
            Everest.Events.Level.OnPause -= OnPause;
            On.Celeste.OuiJournal.Close -= OnJournalClose;
            On.Celeste.Audio.SetMusic -= OnSetMusic;
            On.Celeste.Audio.SetAmbience -= OnSetAmbience;
        }

        private static void OnPause(Level level, int startIndex, bool minimal, bool quickReset) {
            if (levelSelectoverworldWrapper != null) {
                Close(true);
            }
        }

        private static void OnJournalClose(On.Celeste.OuiJournal.orig_Close orig, OuiJournal self) {
            if (IsSelectingLevel) {
                return;
            }
            orig(self);
        }

        private static bool OnSetMusic(On.Celeste.Audio.orig_SetMusic orig, string path, bool startPlaying, bool allowFadeOut) {
            if (skipSetMusic) {
                skipSetMusic = false;
                return false;
            }

            return orig(path, startPlaying, allowFadeOut);
        }

        private static bool OnSetAmbience(On.Celeste.Audio.orig_SetAmbience orig, string path, bool startPlaying) {
            if (skipSetAmbience) {
                skipSetAmbience = false;
                return false;
            }

            return orig(path, startPlaying);
        }

        public static void OpenChapterPanel(Player player, string sid, ChapterPanelTrigger.ReturnToLobbyMode returnToLobbyMode) {
            player.Drop();
            Open(player, AreaData.Get(sid) ?? AreaData.Get(0), out OuiHelper_EnterChapterPanel.Start,
                overworld => new DynData<Overworld>(overworld).Set("returnToLobbyMode", returnToLobbyMode));
        }

        public static void Open(Player player, AreaData area, out bool opened, Action<Overworld> callback = null) {
            opened = false;

            if (IsSelectingLevel) {
                return;
            }

            player.StateMachine.State = Player.StDummy;

            opened = true;

            skipSetMusic = true;
            skipSetAmbience = true;

            Level level = player.Scene as Level;
            // level.Entities.FindFirst<TotalStrawberriesDisplay>().Active = false;

            levelSelectoverworldWrapper = new SceneWrappingEntity<Overworld>(new Overworld(new OverworldLoader((Overworld.StartMode) (-1),
                new HiresSnow() {Alpha = 0f, ParticleAlpha = 0f}
            )));
            levelSelectoverworldWrapper.OnBegin += (overworld) => {
                overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is MountainRenderer));
                overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is ScreenWipe));
                overworld.RendererList.UpdateLists();
            };
            levelSelectoverworldWrapper.OnEnd += (overworld) => {
                if (levelSelectoverworldWrapper?.WrappedScene == overworld) {
                    levelSelectoverworldWrapper = null;
                }
            };

            level.Add(levelSelectoverworldWrapper);
            new DynData<Overworld>(levelSelectoverworldWrapper.WrappedScene).Set("collabInGameForcedArea", area);
            new DynData<Overworld>(InGameOverworldHelper.OverworldWrapper.WrappedScene).Set("collabInGameForcedArea", area);
            callback?.Invoke(levelSelectoverworldWrapper.WrappedScene);

            levelSelectoverworldWrapper.Add(new Coroutine(UpdateRoutine()));
        }

        public static void Close(bool removeScene) {
            if (removeScene) {
                levelSelectoverworldWrapper?.RemoveSelf();
                levelSelectoverworldWrapper = null;

                if (InGameOverworldHelper.LastArea != null && SaveData.Instance != null) {
                    SaveData.Instance.LastArea = InGameOverworldHelper.LastArea.Value;
                    InGameOverworldHelper.LastArea = null;
                }
            }
        }

        private static IEnumerator DelayedCloseRoutine(Level level) {
            yield return null;
            Close(false);
        }

        private static IEnumerator UpdateRoutine() {
            Level level = levelSelectoverworldWrapper.Scene as Level;
            Overworld overworld = levelSelectoverworldWrapper.WrappedScene;

            while (levelSelectoverworldWrapper?.Scene == Engine.Scene) {
                if (overworld.Next is OuiChapterSelect) {
                    overworld.Next.RemoveSelf();
                    levelSelectoverworldWrapper.Add(new Coroutine(DelayedCloseRoutine(level)));
                }

                if (overworld.Current != null || overworld.Next?.Scene != null) {
                    overworld.Snow.Alpha = Calc.Approach(overworld.Snow.Alpha, 0.1f, Engine.DeltaTime * 0.2f);
                } else {
                    overworld.Snow.Alpha = Calc.Approach(overworld.Snow.Alpha, 0, Engine.DeltaTime * 0.2f);
                    if (overworld.Snow.Alpha <= 0.01f) {
                        Close(true);
                    }
                }

                yield return null;
            }
        }
    }
}