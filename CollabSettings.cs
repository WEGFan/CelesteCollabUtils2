using Microsoft.Xna.Framework;
using YamlDotNet.Serialization;

namespace Celeste.Mod.CollabUtils2 {
    public class CollabSettings : EverestModuleSettings {
        public enum SpeedBerryTimerPositions { TopLeft, TopCenter };
        public enum BestTimeInJournal { SpeedBerry, ChapterTimer, Mixed };

        [YamlIgnore]
        public int ModificationsWarning { get; set; }

        public SpeedBerryTimerPositions SpeedBerryTimerPosition { get; set; } = SpeedBerryTimerPositions.TopLeft;

        public bool HideSpeedBerryTimerDuringGameplay { get; set; } = false;

        [SettingSubText("modoptions_collab_displayendscreenforallmaps_description")]
        public bool DisplayEndScreenForAllMaps { get; set; } = false;

        public BestTimeInJournal BestTimeToDisplayInJournal { get; set; } = BestTimeInJournal.SpeedBerry;

        public bool AllowOpenJournalInLobbiesOnGround { get; set; } = true;

        [SettingNeedsRelaunch]
        [SettingSubText("modoptions_collab_showcollablevelsetsinchapterselect_description")]
        public bool ShowCollabLevelSetsInChapterSelect { get; set; } = false;

        public bool ShowLevelIconsInJournal { get; set; } = true;

        public bool LevelSelectionInJournal { get; set; } = true;

        public void CreateModificationsWarningEntry(TextMenu textMenu, bool inGame) {
            TextMenuExt.SubHeaderExt item = new TextMenuExt.SubHeaderExt(Dialog.Clean("modoptions_collab_modificationswarning")) {
                TextColor = Color.OrangeRed
            };
            item.HeightExtra = (item.Title.Split('\n').Length - 1) * ActiveFont.LineHeight * 0.6f;
            item.Offset = new Vector2(0f, -item.HeightExtra / 2);
            textMenu.Add(item);
        }
    }
}
