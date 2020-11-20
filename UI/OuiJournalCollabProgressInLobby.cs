using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.CollabUtils2.Triggers;

namespace Celeste.Mod.CollabUtils2.UI {
    class OuiJournalCollabProgressInLobby : OuiJournalPage {

        private Table table;
        private List<string> sidList = new List<string>();
        private bool separatorAdded = false;

        private static Color getRankColor(CollabMapDataProcessor.SpeedBerryInfo speedBerryInfo, long pb) {
            float pbSeconds = (float) TimeSpan.FromTicks(pb).TotalSeconds;
            if (pbSeconds < speedBerryInfo.Gold) {
                return Calc.HexToColor("B07A00");
            } else if (pbSeconds < speedBerryInfo.Silver) {
                return Color.Gray;
            }
            return Calc.HexToColor("B96F11");
        }

        private static string getRankIcon(CollabMapDataProcessor.SpeedBerryInfo speedBerryInfo, long pb) {
            float pbSeconds = (float) TimeSpan.FromTicks(pb).TotalSeconds;
            if (pbSeconds < speedBerryInfo.Gold) {
                return "CollabUtils2/speed_berry_gold";
            } else if (pbSeconds < speedBerryInfo.Silver) {
                return "CollabUtils2/speed_berry_silver";
            }
            return "CollabUtils2/speed_berry_bronze";
        }

        public static List<OuiJournalCollabProgressInLobby> GeneratePages(OuiJournal journal, string levelSet) {
            List<OuiJournalCollabProgressInLobby> pages = new List<OuiJournalCollabProgressInLobby>();
            int rowCount = 0;
            OuiJournalCollabProgressInLobby currentPage = new OuiJournalCollabProgressInLobby(journal, levelSet);
            pages.Add(currentPage);

            int totalStrawberries = 0;
            int totalDeaths = 0;
            int sumOfBestDeaths = 0;
            long totalTime = 0;
            long sumOfBestTimes = 0;

            bool allMapsDone = true;

            bool allLevelsDone = true;
            bool allSpeedBerriesDone = true;

            string heartTexture = MTN.Journal.Has("CollabUtils2Hearts/" + levelSet) ? "CollabUtils2Hearts/" + levelSet : "heartgem0";

            foreach (AreaStats item in SaveData.Instance.Areas_Safe) {
                AreaData areaData = AreaData.Get(item.ID_Safe);
                if (!areaData.Interlude_Safe) {
                    currentPage.sidList.Add(areaData.GetSID());
                    if (LobbyHelper.IsHeartSide(areaData.GetSID())) {
                        if (allMapsDone || item.TotalTimePlayed > 0) {
                            // add a separator, like the one between regular maps and Farewell
                            currentPage.table.AddRow();
                            currentPage.separatorAdded = true;
                        } else {
                            // all maps weren't complete yet, and the heart side was never accessed: hide the heart side for now.
                            continue;
                        }
                    }

                    string strawberryText = null;
                    if (areaData.Mode[0].TotalStrawberries > 0 || item.TotalStrawberries > 0) {
                        strawberryText = item.TotalStrawberries.ToString();
                        if (item.Modes[0].Completed) {
                            strawberryText = strawberryText + "/" + areaData.Mode[0].TotalStrawberries;
                        }
                    } else {
                        strawberryText = "-";
                    }

                    Row row = currentPage.table.AddRow()
                        .Add(new TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, currentPage.TextColor));
                    if (CollabModule.Instance.Settings.ShowLevelIconsInJournal) {
                        row.Add(new TextureCell(GFX.Gui[areaData.Icon], 64f));
                    }
                    row.Add(null)
                        .Add(new IconCell(item.Modes[0].HeartGem ? heartTexture : "dot"))
                        .Add(new TextCell(strawberryText, currentPage.TextJustify, 0.5f, currentPage.TextColor));

                    if (item.TotalTimePlayed > 0) {
                        row.Add(new TextCell(Dialog.Deaths(item.Modes[0].Deaths), currentPage.TextJustify, 0.5f, currentPage.TextColor));
                    } else {
                        row.Add(new IconCell("dot"));
                    }


                    AreaStats stats = SaveData.Instance.GetAreaStatsFor(areaData.ToKey());
                    if (CollabMapDataProcessor.SilverBerries.TryGetValue(areaData.GetLevelSet(), out Dictionary<string, EntityID> levelSetBerries)
                        && levelSetBerries.TryGetValue(areaData.GetSID(), out EntityID berryID)
                        && stats.Modes[0].Strawberries.Contains(berryID)) {

                        // silver berry was obtained!
                        row.Add(new IconCell("CollabUtils2/silver_strawberry"));
                    } else if (stats.Modes[0].Strawberries.Any(berry => areaData.Mode[0].MapData.Goldenberries.Any(golden => golden.ID == berry.ID && golden.Level.Name == berry.Level))) {
                        // golden berry was obtained!
                        row.Add(new IconCell("CollabUtils2/golden_strawberry"));
                    } else if (item.Modes[0].SingleRunCompleted) {
                        row.Add(new TextCell(Dialog.Deaths(item.Modes[0].BestDeaths), currentPage.TextJustify, 0.5f, currentPage.TextColor));
                        sumOfBestDeaths += item.Modes[0].BestDeaths;
                    } else {
                        // the player didn't ever do a single run.
                        row.Add(new IconCell("dot"));
                        allLevelsDone = false;
                    }

                    if (item.TotalTimePlayed > 0) {
                        row.Add(new TextCell(Dialog.Time(item.TotalTimePlayed), currentPage.TextJustify, 0.5f, currentPage.TextColor));
                    } else {
                        row.Add(new IconCell("dot"));
                    }

                    if (CollabModule.Instance.Settings.BestTimeToDisplayInJournal != CollabSettings.BestTimeInJournal.ChapterTimer
                        && CollabMapDataProcessor.SpeedBerries.TryGetValue(item.GetSID(), out CollabMapDataProcessor.SpeedBerryInfo speedBerryInfo)
                        && CollabModule.Instance.SaveData.SpeedBerryPBs.TryGetValue(item.GetSID(), out long speedBerryPB)) {

                        row.Add(new TextCell(Dialog.Time(speedBerryPB), currentPage.TextJustify, 0.5f, getRankColor(speedBerryInfo, speedBerryPB)));
                        row.Add(new IconCell(getRankIcon(speedBerryInfo, speedBerryPB)));
                        sumOfBestTimes += speedBerryPB;
                    } else if (CollabModule.Instance.Settings.BestTimeToDisplayInJournal != CollabSettings.BestTimeInJournal.SpeedBerry && item.Modes[0].BestTime > 0f) {
                        row.Add(new TextCell(Dialog.Time(item.Modes[0].BestTime), currentPage.TextJustify, 0.5f, currentPage.TextColor)).Add(null);
                        sumOfBestTimes += item.Modes[0].BestTime;
                    } else {
                        row.Add(new IconCell("dot")).Add(null);
                        allSpeedBerriesDone = false;
                    }

                    totalStrawberries += item.TotalStrawberries;
                    totalDeaths += item.Modes[0].Deaths;
                    totalTime += item.TotalTimePlayed;

                    if (!item.Modes[0].HeartGem) {
                        allMapsDone = false;
                    }

                    rowCount++;
                    if (rowCount > 11) {
                        // split the next zones into another page.
                        rowCount = 0;
                        currentPage = new OuiJournalCollabProgressInLobby(journal, levelSet);
                        pages.Add(currentPage);
                    }
                }
            }

            if (currentPage.table.Rows > 1) {
                currentPage.table.AddRow();
                Row totalsRow = currentPage.table.AddRow()
                    .Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, currentPage.TextColor)).Add(null)
                    .Add(null);
                if (CollabModule.Instance.Settings.ShowLevelIconsInJournal) {
                    totalsRow.Add(null);
                }
                totalsRow.Add(new TextCell(totalStrawberries.ToString(), currentPage.TextJustify, 0.6f, currentPage.TextColor))
                    .Add(new TextCell(Dialog.Deaths(totalDeaths), currentPage.TextJustify, 0.6f, currentPage.TextColor))
                    .Add(new TextCell(allLevelsDone ? Dialog.Deaths(sumOfBestDeaths) : "-", currentPage.TextJustify, 0.6f, currentPage.TextColor))
                    .Add(new TextCell(Dialog.Time(totalTime), currentPage.TextJustify, 0.6f, currentPage.TextColor))
                    .Add(new TextCell(allSpeedBerriesDone ? Dialog.Time(sumOfBestTimes) : "-", currentPage.TextJustify, 0.6f, currentPage.TextColor)).Add(null);

                for (int l = 1; l < SaveData.Instance.UnlockedModes; l++) {
                    totalsRow.Add(null);
                }
                totalsRow.Add(new TextCell(Dialog.Time(SaveData.Instance.Time), currentPage.TextJustify, 0.6f, currentPage.TextColor));
                currentPage.table.AddRow();
            }

            return pages;
        }

        public OuiJournalCollabProgressInLobby(OuiJournal journal, string levelSet)
            : base(journal) {

            string skullTexture = MTN.Journal.Has("CollabUtils2Skulls/" + levelSet) ? "CollabUtils2Skulls/" + levelSet : "skullblue";
            string minDeathsTexture = MTN.Journal.Has("CollabUtils2MinDeaths/" + levelSet) ? "CollabUtils2MinDeaths/" + levelSet : "CollabUtils2MinDeaths/SpringCollab2020/1-Beginner";

            PageTexture = "page";
            table = new Table();
            if (CollabModule.Instance.Settings.ShowLevelIconsInJournal) {
                table.AddColumn(new TextCell(Dialog.Clean("journal_progress"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 420f - 64f))
                    .AddColumn(new EmptyCell(64f));
            } else {
                table.AddColumn(new TextCell(Dialog.Clean("journal_progress"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 420f));
            }
            table.AddColumn(new EmptyCell(20f))
                .AddColumn(new EmptyCell(64f))
                .AddColumn(new IconCell("strawberry", 150f))
                .AddColumn(new IconCell(skullTexture, 100f))
                .AddColumn(new IconCell(minDeathsTexture, 100f))
                .AddColumn(new IconCell("time", 220f))
                .AddColumn(new IconCell("CollabUtils2/speed_berry_pbs_heading", 220f))
                .AddColumn(new EmptyCell(30f));
        }

        public override void Redraw(VirtualRenderTarget buffer) {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            table.Render(new Vector2(60f, 20f));

            if (CollabModule.Instance.Settings.LevelSelectionInJournal && sidList.Count > 0) {
                MTexture mtexture = MTN.Journal["poemArrow"];
                Vector2 position = new Vector2(50f, GetY(slider));
                mtexture.DrawCentered(position, Color.White, 1f);
            }
            Draw.SpriteBatch.End();
        }

        public float GetY(float index) {
            float separatorAddition = 15f * Calc.LerpClamp(0f, 1f, index - (sidList.Count - 2));
            return 130f + 60f * index + 20f + (separatorAdded ? separatorAddition : 0f);
        }

        private float slider;

        private int index;

        public override void Update() {
            base.Update();

            if (!CollabModule.Instance.Settings.LevelSelectionInJournal || sidList.Count == 0) {
                return;
            }

            InGameOverworldHelper.OverworldWrapper.WrappedScene.GetUI<OuiJournal>().PageTurningLocked = JournalLevelSelectionHelper.IsSelectingLevel;

            if (Input.MenuConfirm.Pressed) {
                Player player = Engine.Scene.Tracker.GetEntity<Player>();
                string map = sidList[index];
                JournalLevelSelectionHelper.OpenChapterPanel(player, map, ChapterPanelTrigger.ReturnToLobbyMode.SetReturnToHere);
            }
            if (!JournalLevelSelectionHelper.IsSelectingLevel) {
                if (Input.MenuUp.Pressed && index > 0) {
                    index--;
                } else if (Input.MenuDown.Pressed && index + 1 < sidList.Count) {
                    index++;
                }
            }
            slider = Calc.Approach(slider, index, 16f * Engine.DeltaTime);
            Redraw(Journal.CurrentPageBuffer);
        }

    }
}
