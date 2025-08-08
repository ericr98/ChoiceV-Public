using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller {
    public class GridIsland {
        public int StartX;
        public int StartY;
        public int EndX;
        public int EndY;

        public GridIsland(int startX, int startY, int endX, int endY) {
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }
    }

    public class WorldController : ChoiceVScript {
        public static GridIsland AllGridIsland;
        public static List<WorldGrid> AllGrids = new List<WorldGrid>();
        public static RegionMap RegionMap;
        public static Dictionary<IPlayer, bool> TestModeRegister;
        public static int maxGridsX;
        public static int maxGridsY;
        public static int gridsCount;
        public static float lengthX;
        public static float lengthY;
        public static int step;
        public static int minX;
        public static int minY;
        public static bool testMode;

        public WorldController() {
            /*
            AllGridIslands.Add(new GridIsland(-3500, -4000, 4600, 8200));
            AllGridIslands.Add(new GridIsland(3000, -6500, 6000, -4000));
            */

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                HeightMap.initialise("resources\\ChoiceVServer\\data\\hmap.dat", -4100f, -4300f, 4900f, 9200f);
            } else {
                HeightMap.initialise("resources/ChoiceVServer/data/hmap.dat", -4100f, -4300f, 4900f, 9200f);
            }

            if(!HeightMap.HasFile) {
                Logger.logFatal("HeightMap not found. getGroundHeightAt not availble!");
            } else {
                Logger.logInfo(LogCategory.System, LogActionType.Event, "HeightMap found. getGroundHeightAt available!");
            }
            initController();
            createGrids();
            gridsCount = 0;
            RegionMap = new RegionMap();
            CharacterSettingsController.addListCharacterSettingBlueprint(
                "MINIMAP_SIZE", "NORMAL", "Minimap-Größe", "Die Größe der Minimap",
                new Dictionary<string, string> { { "OFF", "Aus" }, { "NORMAL", "Standard" }, { "BIGGER", "Größer" }, { "WHOLE", "Das ganze Ding, ne?" } },
                onChangeMinimapSize);

            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            EventController.PlayerChangeWorldGridDelegate += onPlayerChangeWorldGrid;


            WorldController.getOverlappingGrids(
             new Vector2(-3.77f * 100f, 9.48f * 100f),
             new Vector2(1.26f * 100f, 12.74f * 100f),
             new Vector2(0.59f * 100f, 2.77f * 100f),
             new Vector2(5.62f * 100f, 6.03f * 100f)
            );
        }

        private void initController() {
            step = Constants.GridWidthHeight;
            testMode = false;
            TestModeRegister = new Dictionary<IPlayer, bool>();
            //Map-borders seems good(-8000, -8000, 8000, 8000)
            // Original: AllGridIsland = new GridIsland(-3500, -6500, 6000, 8200); Following changed because of regionMap Heistzone was larger
            // first try AllGridIsland = new GridIsland(-3500, -6500, 8000, 8200)
            AllGridIsland = new GridIsland(-8000, -8000, 8000, 8000);
            lengthX = getMetric(AllGridIsland.EndX, AllGridIsland.StartX);
            lengthY = getMetric(AllGridIsland.EndY, AllGridIsland.StartY);
            maxGridsX = getGridsCount(lengthX);
            maxGridsY = getGridsCount(lengthY, true);
            minY = AllGridIsland.StartY + 500;
            minX = AllGridIsland.StartX;
        }

        public static int getGridsCount(float length, bool isY = false) {
            float residual = length % step;
            int count = Convert.ToInt32(Math.Round((length - residual) / step, 0));
            if(isY) { count++; }
            return count;
        }

        private void test(IPlayer player) {
            var menu = new Menu("", "");
            menu.addMenuItem(new ClickMenuItem("Name", "Beschreibung", "Test", ""));

            var virtSubMenu = new VirtualMenu("Sub", () => {
                var subMenu = new Menu("Sub", "Sub");
                for(int i = 0; i < 1000; i++) {
                    subMenu.addMenuItem(new StaticMenuItem($"TEST: {i}", "TEST", "TEST"));
                }

                return subMenu;
            });
            menu.addMenuItem(new MenuMenuItem(virtSubMenu.Name, virtSubMenu));

            player.showMenu(menu);
        }

        public static string getRegionDisplayName(string name) {
            return RegionMap.getRegionDisplayName(name);
        }

        public static string getRegionFromPlayer(IPlayer player) {
            return RegionMap.getRegionFromPlayer(player);
        }

        public static string getRegionName(Position pos) {
            return RegionMap.getRegionName(pos);
        }

        public static string getBigRegionName(Position pos) {
            return RegionMap.getBigRegionName(pos);
        }
        
        public static string getBigRegionDisplayName(Position pos) {
            return RegionMap.getBigRegionDisplayName(pos);
        }
        
        public static string getBigRegionDisplayName(string name) {
            return string.IsNullOrEmpty(name) ? null : RegionMap.getBigRegionDisplayName(name);
        }
        
        public static InputMenuItem getBigRegionSelectMenuItem(string name, string evt, bool addNoneOption = false) {
            return RegionMap.getBigRegionSelectMenuItem(name, evt, addNoneOption);
        }

        public static string getBigRegionIdentifierFromSelectMenuItemInput(string input) {
            return string.IsNullOrEmpty(input) ? null : RegionMap.getBigRegionIdentifierFromSelectMenuItemInput(input);

        }
        
        public static List<string> getAllBigRegions() {
            return RegionMap.getAllBigRegions();
        }

        public static void worldControllerTestMode(IPlayer player) {
            //activates Testmode for player
            if(TestModeRegister.ContainsKey(player)) {
                if(TestModeRegister[player]) {
                    TestModeRegister[player] = false;
                } else {
                    TestModeRegister[player] = true;
                }
            } else {
                TestModeRegister[player] = true;
            }
            InvokeController.AddTimedInvoke("worldControllerTestMode", invoke => {

            }, TimeSpan.FromSeconds(1), false);
        }
        public static void changeTestModeRunning() {
            //de-/activate sestmode on Server
            if(testMode) {
                testMode = false;
                //Logger.logDebug("WorldControllerTestmode stopped");
                foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                    InvokeController.AddTimedInvoke("worldControllerTestMode", invoke => {
                        WebController.removeDebugInfo(player, "WorldGrid");
                        WebController.removeDebugInfo(player, "RegionByPlayer");
                        WebController.removeDebugInfo(player, "RegionByServer");
                        WebController.removeDebugInfo(player, "PlayerPosByHeightmap");
                    }, TimeSpan.FromSeconds(1), false);
                }
            } else {
                testMode |= true;
                testModeInvokeFactory();
                //Logger.logDebug("WorldControllerTestmode started");
            }
        }

        private static void testModeInvokeFactory() {
            //Mode for testing on Region from Player, Region from Server and HeightMap
            if(testMode) {
                InvokeController.AddTimedInvoke("worldControllerTestMode", invoke => {
                    foreach(var tester in TestModeRegister) {
                        if(tester.Value) {
                            InvokeController.AddTimedInvoke("worldControllerTestMode", invoke => {
                                string playerGrid = getWorldGrid(tester.Key.Position).Id.ToString();
                                string regionByPlayer = getRegionDisplayName(getRegionFromPlayer(tester.Key));
                                string regionByServer = getRegionDisplayName(getRegionName(new Position(tester.Key.Position.X, tester.Key.Position.Y, tester.Key.Position.Z)));
                                string playerPosByHeigthMap = $"X: {tester.Key.Position.X}, Y: {tester.Key.Position.Y}, Z: {HeightMap.get(tester.Key.Position.X, tester.Key.Position.Y)}";
                                WebController.displayDebugInfo(tester.Key, "WorldGrid", $"<h4 style='background-color:grey;'>WorldGrid: {playerGrid} </h4><br>");
                                WebController.displayDebugInfo(tester.Key, "RegionByPlayer", $"<h4 style='background-color:grey;'>RegionByPlayer: {regionByPlayer} </h4><br>");
                                WebController.displayDebugInfo(tester.Key, "RegionByServer", $"<h4 style='background-color:grey;'>RegionByServer: {regionByServer} </h4><br>");
                                WebController.displayDebugInfo(tester.Key, "PlayerPosByHeightmap: ", $"<h4 style='background-color:grey;'>PlayerPosByHeightmap: {playerPosByHeigthMap} </h4><br>");
                                /*tester.Key.sendBlockNotification($"Grid: {playerGrid}\nRegionFromPlayer: {regionByPlayer}\nRegionFromServerMap {regionByServer}\n" +
                                    $"HeightFromHeightMap {playerPosByHeigthMap}:HeightFromPlayer: {tester.Key.Position.Z.ToString()}", "wcStatus");   */
                            }, TimeSpan.FromSeconds(1), false);
                        }
                    }
                    testModeInvokeFactory();
                    //Logger.logDebug("WorldControllerTestmode next Cycle");
                }, TimeSpan.FromSeconds(1), false);
            }
        }

        private void onPlayerChangeWorldGrid(object sender, IPlayer player, WorldGrid previousGrid, WorldGrid currentGrid) {
            var gridBlock = getGridBlock(player.Position);

            player.emitClientEvent("CHANGE_GRID", gridBlock.Select(g => g.Id).ToArray());
        }

        private void onPlayerConnect(IPlayer player, character character) {
            var sett = player.getCharSetting("MINIMAP_SIZE");
            player.emitClientEvent("CHANGE_MINIMAP_SIZE", sett);
        }

        private void onChangeMinimapSize(IPlayer player, string settingName, string value) {
            player.emitClientEvent("CHANGE_MINIMAP_SIZE", value);
        }

        private void createGrids() {
            //var startX = -3500;
            //var startY = -4000;
            //var endX = 4600;
            //var endY = 8200;

            //var startXCayo = 3000;
            //var startYCayo = -4000;
            //var endXCayo = 6000;
            //var endYCayo = -6500;

            //merged coords
            //var startX = -3500;
            //var startY = -6500;
            //var endX = 6000;
            //var endY = 8200;


            var step = Constants.GridWidthHeight;
            var count = 0;
            for(int i = AllGridIsland.StartX; i <= AllGridIsland.EndX; i += step) {
                for(int j = AllGridIsland.StartY; j <= AllGridIsland.EndY; j += step) {
                    var grid = new WorldGrid(count, new Vector2(i, j), step, step);
                    count++;
                    if(!AllGrids.Any(g => g.Rectangle.Equals(grid.Rectangle))) {
                        AllGrids.Add(grid);
                    } /* else {
                        Logger.logDebug($"WorldController: found a similar grid {AllGrids.Any(g => g.Rectangle.Equals(grid.Rectangle)).ToString()} {grid.Id}");
                        } */
                }
            }
            //create outerGrid
            int sizeX = Math.Abs(AllGridIsland.StartX) + Math.Abs(AllGridIsland.EndX);
            int sizeY = Math.Abs(AllGridIsland.StartY) + Math.Abs(AllGridIsland.EndY);
            AllGrids.Add(new WorldGrid(count + 1, new Vector2(minX - 99999, AllGridIsland.EndY + 99999), 99999 * 2 + sizeX, 99999 * 2 + sizeY));
            gridsCount = count;


            //Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"WorldController: {AllGrids.Count} WorldGrids have been created!");
        }

        public static WorldGrid getWorldGrid(Vector2 position) {
            return getWorldGrid(new Position(position.X, position.Y, 0));
        }

        public static WorldGrid getWorldGrid(Position position) {
            //first count for x, then add rest of y
            WorldGrid grid = AllGrids[getGridIndex(position)];
            //logger for debug purpose
            /*
            if (grid.isInGrid(position)) {
                Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"WorldController: estimated grid seems right");
            } else {
                Logger.logDebug(LogCategory.ServerStartup, LogActionType.Created, $"x1;{position.X};x2;{grid.Rectangle.X};y1;{position.Y};y2;{grid.Rectangle.Y}");
            }
            */
            return grid;
            //return AllGrids[getGridIndex(position)];
            //return AllGrids.FirstOrDefault(g => g.isInGrid(position));
        }

        public static WorldGrid[] getGridBlock(Position position) {
            /*
            //Get MiddlePoint of Grid
            var checkPos = new Vector2(position.X, position.Y);
            var grids = AllGrids.Where(g => g.isNeighborGrid(checkPos) || g.isInGrid(position)).ToArray();
            */

            //get middle grid
            WorldGrid grid = AllGrids[getGridIndex(position)];
            //get neighbourGrids
            var temp = getNeighborGrids(grid);
            int arrayLength = temp.Length;
            WorldGrid[] grids = new WorldGrid[arrayLength + 1];
            grids[0] = grid;
            int counter = 1;
            foreach(var item in temp) {
                grids[counter] = item;
                counter++;
            }

            return grids;
        }

        public static WorldGrid getNextGrid(int index, bool upper = false, bool lower = false, bool left = false, bool right = false) {
            WorldGrid result = AllGrids[index];

            if(upper) {
                return AllGrids[index + 1];
            }

            if(lower) {
                return AllGrids[index - 1];
            }

            if(left) {
                return AllGrids[index - maxGridsY];
            }

            if(right) {
                return AllGrids[index + maxGridsY];
            }

            return result;
        }

        public static WorldGrid[] getNeighborGrids(WorldGrid grid) {
            /*
            //Get MiddlePoint of Grid
            var checkPos = new Vector2(grid.Rectangle.X + (grid.Rectangle.Width/2), grid.Rectangle.Y - (grid.Rectangle.Height / 2));
            var grids = AllGrids.Where(g => g.isNeighborGrid(checkPos)).ToArray();
            */
            if(grid.Id == 1090) {
                int dingens = 0;
            }
            WorldGrid[] grids = new WorldGrid[0];
            //check for surrounding grid
            if(grid.Id == AllGrids.Count) {

                // Corners
                //left upper corner
                if(grid.Rectangle.Left < minX && grid.Rectangle.Top > AllGridIsland.EndY) {
                    grids = new WorldGrid[1];
                    grids[0] = AllGrids[maxGridsY];
                    //left lower corner
                } else if(grid.Rectangle.Left < minX && grid.Rectangle.Bottom < minY) {
                    grids = new WorldGrid[1];
                    grids[0] = AllGrids[0];
                    //right upper corner  
                } else if(grid.Rectangle.Right > AllGridIsland.EndX && grid.Rectangle.Top > AllGridIsland.EndY) {
                    grids = new WorldGrid[1];
                    int index = maxGridsY * maxGridsX;
                    grids[0] = AllGrids[index];
                    //right lower corner
                } else if(grid.Rectangle.Right > AllGridIsland.EndX && grid.Rectangle.Bottom < minY) {
                    grids = new WorldGrid[1];
                    int index = maxGridsY * maxGridsX - maxGridsY + 1;
                    grids[0] = AllGrids[index];
                    //left-over(coord is lower minx) side
                } else if(grid.Rectangle.Left < minX) {
                    //left upper corner
                    if(grid.Rectangle.Top > AllGridIsland.EndX - step) {
                        grids = new WorldGrid[2];
                        grids[0] = AllGrids[maxGridsY];
                        grids[1] = AllGrids[maxGridsY - 1];
                        //left lower corner
                    } else if(grid.Rectangle.Bottom < minY + step) {
                        grids = new WorldGrid[2];
                        grids[0] = AllGrids[0];
                        grids[1] = AllGrids[1];
                    } else {
                        grids = new WorldGrid[3];
                        int index = getGridIndex(new Position(minX + step / 2, grid.Rectangle.Top - step / 2, 0));
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - 1];
                        grids[1] = AllGrids[index + 1];
                    }
                    //right-over side
                } else if(grid.Rectangle.Right > AllGridIsland.EndX) {
                    //right upper corner
                    if(grid.Rectangle.Top > AllGridIsland.EndX - step) {
                        grids = new WorldGrid[2];
                        int index = maxGridsY * maxGridsX;
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - 1];
                        //right lower corner
                    } else if(grid.Rectangle.Bottom < minY + step) {
                        grids = new WorldGrid[2];
                        int index = maxGridsY * maxGridsX - maxGridsY + 1;
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index + 1];
                    } else {
                        grids = new WorldGrid[3];
                        int index = getGridIndex(new Position(AllGridIsland.EndX - step / 2, grid.Rectangle.Top - step / 2, 0));
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - 1];
                        grids[1] = AllGrids[index + 1];
                    }
                    //upper-over side
                } else if(grid.Rectangle.Top > AllGridIsland.EndY) {
                    //left corner
                    if(grid.Rectangle.Left < minX + step) {
                        grids = new WorldGrid[2];
                        grids[0] = AllGrids[maxGridsY];
                        grids[1] = AllGrids[maxGridsY * 2];
                        //right corner
                    } else if(grid.Rectangle.Right > AllGridIsland.EndX - step) {
                        grids = new WorldGrid[2];
                        int index = maxGridsY * maxGridsX;
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - maxGridsY];
                    } else {
                        grids = new WorldGrid[3];
                        int index = getGridIndex(new Position(grid.Rectangle.X + step / 2, AllGridIsland.EndY - step / 2, 0));
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - 1];
                        grids[1] = AllGrids[index + 1];
                    }
                    //lower-over side
                } else if(grid.Rectangle.Bottom < minY) {
                    //left corner
                    if(grid.Rectangle.Left < minX + step) {
                        grids = new WorldGrid[2];
                        grids[0] = AllGrids[0];
                        grids[1] = AllGrids[maxGridsY + 1];
                        //right corner
                    } else if(grid.Rectangle.Right > AllGridIsland.EndX - step) {
                        grids = new WorldGrid[2];
                        int index = maxGridsY * maxGridsX - maxGridsY + 1;
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - maxGridsY];
                    } else {
                        grids = new WorldGrid[3];
                        int index = getGridIndex(new Position(grid.Rectangle.X + step / 2, AllGridIsland.StartY - step / 2, 0));
                        grids[0] = AllGrids[index];
                        grids[1] = AllGrids[index - 1];
                        grids[1] = AllGrids[index + 1];
                    }

                }
            } else {
                //check for edge cases
                // left side
                if(minX + step >= grid.Rectangle.Left && minX < grid.Rectangle.Left) {
                    //left side lower corner
                    if(minY + step >= grid.Rectangle.Bottom) {
                        grids = new WorldGrid[4];
                        grids[0] = AllGrids[grid.Id + 1];
                        grids[1] = AllGrids[grid.Id + maxGridsY + 1];
                        grids[2] = AllGrids[grid.Id + maxGridsY];
                        grids[3] = AllGrids[AllGrids.Count - 1];
                        //left side upper corner
                    } else if(AllGridIsland.EndY - step <= grid.Rectangle.Top) {
                        grids = new WorldGrid[4];
                        grids[0] = AllGrids[grid.Id - 1];
                        grids[1] = AllGrids[grid.Id + maxGridsY - 1];
                        grids[2] = AllGrids[grid.Id + maxGridsY];
                        grids[3] = AllGrids[AllGrids.Count - 1];
                        //left side other
                    } else {
                        grids = new WorldGrid[6];
                        grids[0] = AllGrids[grid.Id - 1];
                        grids[1] = AllGrids[grid.Id + 1];
                        grids[2] = AllGrids[grid.Id + maxGridsY - 1];
                        grids[3] = AllGrids[grid.Id + maxGridsY];
                        grids[4] = AllGrids[grid.Id + maxGridsY + 1];
                        grids[5] = AllGrids[AllGrids.Count - 1];
                    }
                    //right side
                } else if(AllGridIsland.EndX - step <= grid.Rectangle.Right && AllGridIsland.EndX > grid.Rectangle.Right) {
                    //right side lower corner
                    if(minY + step >= grid.Rectangle.Bottom) {
                        grids = new WorldGrid[4];
                        grids[0] = AllGrids[grid.Id + 1];
                        grids[1] = AllGrids[grid.Id - maxGridsY - 1];
                        grids[2] = AllGrids[grid.Id - maxGridsY];
                        grids[3] = AllGrids[AllGrids.Count - 1];
                        //right side upper corner
                    } else if(AllGridIsland.EndY - step <= grid.Rectangle.Top) {
                        grids = new WorldGrid[4];
                        grids[0] = AllGrids[grid.Id - 1];
                        grids[1] = AllGrids[grid.Id - maxGridsY + 1];
                        grids[2] = AllGrids[grid.Id - maxGridsY];
                        grids[3] = AllGrids[AllGrids.Count - 1];
                        //right side other
                    } else {
                        grids = new WorldGrid[6];
                        grids[0] = AllGrids[grid.Id - 1];
                        grids[1] = AllGrids[grid.Id + 1];
                        grids[2] = AllGrids[grid.Id - maxGridsY - 1];
                        grids[3] = AllGrids[grid.Id - maxGridsY];
                        grids[4] = AllGrids[grid.Id - maxGridsY + 1];
                        grids[5] = AllGrids[AllGrids.Count - 1];
                    }
                    //lower side (corners should be identified by left/right check)
                } else if(minY + step >= grid.Rectangle.Bottom) {
                    grids = new WorldGrid[6];
                    grids[0] = AllGrids[grid.Id + 1];
                    grids[1] = AllGrids[grid.Id - maxGridsY];
                    grids[2] = AllGrids[grid.Id - maxGridsY + 1];
                    grids[3] = AllGrids[grid.Id + maxGridsY];
                    grids[4] = AllGrids[grid.Id + maxGridsY + 1];
                    grids[5] = AllGrids[AllGrids.Count - 1];
                    //upper side (corners should be identified by left/right check)
                } else if(AllGridIsland.EndY - step <= grid.Rectangle.Top) {
                    grids = new WorldGrid[6];
                    grids[0] = AllGrids[grid.Id - 1];
                    grids[1] = AllGrids[grid.Id - maxGridsY];
                    grids[2] = AllGrids[grid.Id - maxGridsY - 1];
                    grids[3] = AllGrids[grid.Id + maxGridsY];
                    grids[4] = AllGrids[grid.Id + maxGridsY - 1];
                    grids[5] = AllGrids[AllGrids.Count - 1];
                    //all other cases
                } else {
                    grids = new WorldGrid[8];
                    grids[0] = AllGrids[grid.Id - 1];
                    grids[1] = AllGrids[grid.Id + 1];
                    grids[2] = AllGrids[grid.Id - maxGridsY];
                    grids[3] = AllGrids[grid.Id - maxGridsY - 1];
                    grids[4] = AllGrids[grid.Id - maxGridsY + 1];
                    grids[5] = AllGrids[grid.Id + maxGridsY];
                    grids[6] = AllGrids[grid.Id + maxGridsY - 1];
                    grids[7] = AllGrids[grid.Id + maxGridsY + 1];
                }
            }
            return grids;
        }

        private record OverlapPoint(float Y, float MinX, float MaxX);

        //TODO Is Wrong. Needs to be done with Grid Coordinates
        //https://stackoverflow.com/questions/57484099/algorithm-for-finding-grid-cells-contained-in-arbitrary-rotated-rectangle-raste
        public static List<WorldGrid> getOverlappingGrids(Vector2 PosA, Vector2 PosB, Vector2 PosC, Vector2 PosD) {
            var pointList = new List<Vector2> { PosA, PosB, PosC, PosD };

            var topPoint = pointList.Aggregate((i1, i2) => i1.Y > i2.Y ? i1 : i2);
            var leftPoint = pointList.Aggregate((i1, i2) => i1.X < i2.X ? i1 : i2);
            var rightPoint = pointList.Aggregate((i1, i2) => i1.X > i2.X ? i1 : i2);
            var bottomPoint = pointList.Aggregate((i1, i2) => i1.Y < i2.Y ? i1 : i2);


            var list = new Dictionary<float, OverlapPoint>();
            if(topPoint.X == bottomPoint.X) {
                var maximalX = leftPoint.X;
                var minimalX = rightPoint.X;

                var maximalY = topPoint.Y;
                var minmalY = bottomPoint.Y;

                list.Add(maximalY, new OverlapPoint(maximalY, minimalX, maximalX));
                var count = maximalY - Constants.GridWidthHeight;

                while(count > minmalY) {
                    list.Add(count, new OverlapPoint(count, minimalX, maximalX));
                    count -= Constants.GridWidthHeight;
                }
            } else {
                list.Add(topPoint.Y, new OverlapPoint(topPoint.Y, topPoint.X, topPoint.X));

                var firstTopY = topPoint.Y - (topPoint.Y % Constants.GridWidthHeight);
                var firstBottomY = bottomPoint.Y - (bottomPoint.Y % Constants.GridWidthHeight);


                var leftTopMinX = topPoint.X + (firstTopY - topPoint.Y) * (leftPoint.X - topPoint.X) / (leftPoint.Y - topPoint.Y);
                var leftTopM = (leftPoint.X - topPoint.X) / (leftPoint.Y - topPoint.Y);

                var leftBottomMinX = leftPoint.X;
                var leftBottomM = (bottomPoint.X - leftPoint.X) / (bottomPoint.Y - leftPoint.Y);


                var rightTopMaxX = topPoint.X + (firstTopY - topPoint.Y) * (rightPoint.X - topPoint.X) / (rightPoint.Y - topPoint.Y);
                var rightTopM = (rightPoint.X - topPoint.X) / (rightPoint.Y - topPoint.Y);

                var rightBottomMaxX = rightPoint.X;
                var rightBottomM = (bottomPoint.X - rightPoint.X) / (bottomPoint.Y - rightPoint.Y);


                list.Add(firstTopY, new OverlapPoint(firstTopY, leftTopMinX, rightTopMaxX));

                var count = firstTopY - Constants.GridWidthHeight;
                var counter = 0;
                while(count > firstBottomY) {
                    var minimalX = -1f;
                    var maximalX = -1f;

                    if(count >= leftPoint.Y) {
                        minimalX = leftTopMinX + (leftTopM * counter);
                    } else {
                        minimalX = leftBottomMinX + (leftBottomM * counter);
                    }


                    if(count >= rightPoint.Y) {
                        maximalX = rightTopMaxX + (rightTopM * counter);
                    } else {
                        maximalX = rightBottomMaxX + (rightBottomM * counter);
                    }

                    list.Add(count, new OverlapPoint(count, minimalX, maximalX));
                    count -= Constants.GridWidthHeight;
                }

                list.Add(bottomPoint.Y, new OverlapPoint(bottomPoint.Y, bottomPoint.X, bottomPoint.X));
            }

            var allOverlappingGrids = new List<WorldGrid>();


            var yCounter = topPoint.Y;
            while(yCounter > bottomPoint.Y - Constants.GridWidthHeight) {
                var xCounter = leftPoint.X;

                while(xCounter < rightPoint.X + Constants.GridWidthHeight) {
                    var currentGrid = getWorldGrid(new Position(xCounter, yCounter, 0));
                    xCounter += Constants.GridWidthHeight;

                    if(list.ContainsKey(currentGrid.Rectangle.Top)) {
                        if(isOverlappingGrid(currentGrid, list[currentGrid.Rectangle.Top])) {
                            allOverlappingGrids.Add(currentGrid);
                            continue;
                        }
                    }

                    if(list.ContainsKey(currentGrid.Rectangle.Bottom)) {
                        if(isOverlappingGrid(currentGrid, list[currentGrid.Rectangle.Bottom])) {
                            allOverlappingGrids.Add(currentGrid);
                            continue;
                        }
                    }
                }

                yCounter -= Constants.GridWidthHeight;
            }

            return allOverlappingGrids;
            //throw new NotImplementedException();
        }

        private static bool isOverlappingGrid(WorldGrid grid, OverlapPoint point) {
            var left = grid.Rectangle.Left;
            var right = grid.Rectangle.Right;

            return (left > point.MinX && left < point.MaxX) || (right > point.MinX && right < point.MaxX);
        }

        public static int getGridIndex(Position pos) {
            //first count for x times y-steps, then add rest of y
            int result = 0;
            if(pos.X > AllGridIsland.EndX || pos.X < AllGridIsland.StartX || pos.Y > AllGridIsland.EndY || pos.Y < AllGridIsland.StartY) {
                result = AllGrids.Count - 1;
            } else {
                float x = getMetric(pos.X, minX);
                float y = getMetric(pos.Y, minY);
                int countX = getGridsCount(x);
                int countY = getGridsCount(y, true);
                result = countX * maxGridsY;
                result += countY;
                if(result < 0) {
                    //Logger.logDebug($"CountX: {countX}, CountY: {countY}, PosX: {pos.X}, PosY: {pos.Y}, X: {x}, Y: {y}");
                }
            }
            return result;
        }

        public static Position getPositionFromGridInex(int index) {
            Position pos = new Position(0, 0, 0);
            pos.X = AllGrids[index].Rectangle.X;
            pos.Y = AllGrids[index].Rectangle.Y;
            return pos;
        }

        private static float getMetric(float a, float b) {
            float aa = a;
            float bb = b;
            if(aa > bb) {
                aa = b;
                bb = a;
            }
            return bb - aa;
        }

        public static float getGroundHeightAt(Vector3 position) {
            try {
                return HeightMap.get(position.X, position.Y);
            } catch(Exception e) {
                Logger.logException(e, "getGroundHeightAt: Something was wrong");
                return position.Z;
            }
        }

        public static float getGroundHeightAt(float x, float y) {
            return HeightMap.get(x, y);
        }

        //public static float getGroundMaterialAt(Vector3 position) {
        //    return MaterialMap.get(position.X, position.Y);
        //}

        //public static float getGroundMaterialAt(float x, float y) {
        //    return MaterialMap.get(x, y);
        //}
    }

    [Serializable()]
    public class WorldGrid {
        public int Id;
        public RectangleF Rectangle;

        public WorldGrid(int id, Vector2 upperLeft, int width, int height) {
            Id = id;
            Rectangle = new RectangleF(upperLeft.X, upperLeft.Y, width, height);
        }

        public bool isInGrid(Position pos) {
            return Rectangle.Contains(pos.X, pos.Y);
        }

        public bool isInGrid(float x, float y) {
            return Rectangle.Contains(x, y);
        }

        public bool isNeighborGrid(Vector2 pos) {
            var x = pos.X;
            var y = pos.Y;
            var step = (Rectangle.Width + Rectangle.Height) / 2;

            return isInGrid(x + step, y) || isInGrid(x - step, y) || isInGrid(x, y + step) || isInGrid(x, y - step) || isInGrid(x + step, y + step) || isInGrid(x - step, y - step) || isInGrid(x + step, y - step) || isInGrid(x - step, y + step);
        }

        public void draw() {
            foreach(var player in ChoiceVAPI.GetAllPlayers()) {
                player.emitClientEvent("SHOW_GRID", Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height, 1, 0);
            }
        }
    }


    public static class HeightMap {

        private static float _startX, _startY;
        private static float _endX, _endY;
        private static string _file;
        public static bool HasFile = false;

        /// <summary>
        /// Load heightmap data from a file (resolution 1.0f x 1.0f)
        /// </summary>
        /// <param name="file">Heightmap containing floats (direction x first)</param>
        /// <param name="startX">Start X</param>
        /// <param name="startY">Start Y</param>
        /// <param name="endX">End X</param>
        /// <param name="endY">End Y</param>
        public static void initialise(string file, float startX, float startY, float endX, float endY) {
            _file = file;

            _startX = startX;
            _startY = startY;
            _endX = endX;
            _endY = endY;
            HasFile = File.Exists(file);
        }

        /// <summary>
        /// Get the z value of the ground at the given position
        /// </summary>
        /// <param name="posX">Position x</param>
        /// <param name="posY">Position y</param>
        /// <returns>Ground level</returns>
        public static float get(float posX, float posY) {
            try {
                if(!HasFile)
                    return -1f;
                if(!contains(posX, posY)) return -1f;

                var x = (int)posX - (int)_startX;
                var y = (long)(_endX - _startX) * ((long)posY - (long)_startY);
                using(var mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.Open)) {
                    using(var accessor = mmf.CreateViewAccessor((y + x) * 4, 4)) {
                        return accessor.ReadSingle(0);
                    }
                }
            } catch(Exception) {
                return -1;
            }
        }

        /// <summary>
        /// Checks if the height data of the given position is contained in the map
        /// </summary>
        /// <param name="x">Position x</param>
        /// <param name="y">Position y</param>
        /// <returns>True if contained</returns>
        public static bool contains(float x, float y) {
            return _startX < x && x < _endX && _startY < y && y < _endY;
        }
    }

    public static class MaterialMap {
        private static float _startX, _startY;
        private static float _endX, _endY;
        private static string _file;
        public static bool HasFile = false;

        /// <summary>
        /// Load heightmap data from a file (resolution 1.0f x 1.0f)
        /// </summary>
        /// <param name="file">Heightmap containing floats (direction x first)</param>
        /// <param name="startX">Start X</param>
        /// <param name="startY">Start Y</param>
        /// <param name="endX">End X</param>
        /// <param name="endY">End Y</param>
        public static void initialise(string file, float startX, float startY, float endX, float endY) {
            _file = file;

            _startX = startX;
            _startY = startY;
            _endX = endX;
            _endY = endY;
            HasFile = File.Exists(file);
        }

        /// <summary>
        /// Get the z value of the ground at the given position
        /// </summary>
        /// <param name="posX">Position x</param>
        /// <param name="posY">Position y</param>
        /// <returns>Ground level</returns>
        public static int get(float posX, float posY) {
            if(!HasFile)
                return -1;
            if(!contains(posX, posY)) return -1;

            var x = (int)posX - (int)_startX;
            var y = (long)(_endX - _startX) * ((long)posY - (long)_startY);
            using(var mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.Open)) {
                using(var accessor = mmf.CreateViewAccessor((y + x) * 4, 4)) {
                    return (int)accessor.ReadSingle(0);
                }
            }
        }

        /// <summary>
        /// Checks if the height data of the given position is contained in the map
        /// </summary>
        /// <param name="x">Position x</param>
        /// <param name="y">Position y</param>
        /// <returns>True if contained</returns>
        public static bool contains(float x, float y) {
            return _startX < x && x < _endX && _startY < y && y < _endY;
        }
    }
    #region RegionMap
    public class RegionMap {
        private string url;
        private HttpClient client;
        private Dictionary<int, List<RegionSlice>> regionTiles;
        private Dictionary<string, float> regionsSize;
        public Dictionary<string, string> regionDisplayNames;
        private Dictionary<string, string> smallToBigRegion;
        public Dictionary<string, string> bigRegionDisplayNames;
        
        public RegionMap() {
            LoadRegionMap();
        }
        protected bool LoadRegionMap() {
            bool sucessfulyLoaded = false;
            client = new();
            url = "https://github.com/DurtyFree/gta-v-data-dumps/blob/8ff8268bac0d86a7e384f8b66d6b8dc5dd7c1eea/zones.json";
            //load JSON ( https://github.com/DurtyFree/gta-v-data-dumps/blob/master/zones.json) into List
            regionTiles = loadRegionsFromJSON(url).Result;


            return sucessfulyLoaded;
        }

        protected async Task<Dictionary<int, List<RegionSlice>>> loadRegionsFromJSON(string url) {
            regionTiles = new();
            regionsSize = new Dictionary<string, float>();
            regionDisplayNames = new Dictionary<string, string>();
            smallToBigRegion = new Dictionary<string, string>();
            bigRegionDisplayNames = new Dictionary<string, string>();
            try {
                // load regions from local file and deserialize it into a AllRegions-object
                var result = File.ReadAllText(@"resources\ChoiceVServer\data\zones.json");
                var myDeserializedClass = JsonConvert.DeserializeObject<List<Region>>(result);
                AllRegions allRegions = new(myDeserializedClass);
                using(var db = new ChoiceVDb()) {
                    foreach(var item in allRegions.regions) {
                        //calc size of each region
                        foreach(var thing in item.bounds) {
                            if(!regionsSize.ContainsKey(item.name.ToUpper())) {
                                regionsSize[item.name.ToUpper()] = getSizeofReactangle(thing);
                            } else {
                                regionsSize[item.name.ToUpper()] += getSizeofReactangle(thing);
                            }

                            if(!regionDisplayNames.ContainsKey(item.name.ToUpper())) {
                                regionDisplayNames.Add(item.name.ToUpper(), item.displayName);
                            }
                        }
                        
                        if(!smallToBigRegion.ContainsKey(item.name.ToUpper())) {
                            var bigRegion = db.configzonegroupingszones.FirstOrDefault(z => z.gtaName == item.name.ToUpper()).zoneIdentifier;
                            smallToBigRegion.Add(item.name.ToUpper(), bigRegion);
                        }
                    }
                    
                    foreach(var item in db.configzonegroupings) {
                        if(!bigRegionDisplayNames.ContainsKey(item.groupingIdentifier)) {
                            bigRegionDisplayNames.Add(item.groupingIdentifier, item.groupingName);
                        }
                    }
                }
                createDictRegionTiles(allRegions);
            } catch(Exception ex) { Logger.logWarning(LogCategory.ServerStartup, LogActionType.Blocked, "Zone map not found!"); }

            return regionTiles;
        }

        protected void createDictRegionTiles(AllRegions allRegion) {
            AllRegions allRegions = allRegion;
            regionTiles = new();
            //create Preset OCEANA
            BoundReactangle minimum = new BoundReactangle(-3500, -6500, 0);
            BoundReactangle maximum = new BoundReactangle(6000, 8200, 0);
            Bound temp = new Bound(minimum, maximum);
            //preset for each list
            for(int i = 0; i < WorldController.AllGrids.Count; i++) {
                regionTiles[i] = new List<RegionSlice> {
                    new RegionSlice(i, "OCEANA", "Pacific Ocean", temp)
                };
            }
            //start with upper left corner, go step by step throu the grids
            //search all regions
            foreach(var item in allRegion.regions) {
                // Oceana is presetted
                if(!item.name.ToUpper().Equals("OCEANA")) {
                    //search in all bounds in a region
                    foreach(var thing in item.bounds) {
                        //itterate over x stepsize Worldgridsize
                        int counterX = 0;
                        int counterY = 0;
                        Position initialCoords = new Position(thing.minimum.x, thing.minimum.y, 0);
                        int tempIndex = WorldController.getGridIndex(initialCoords);
                        int x = Convert.ToInt32(initialCoords.X);
                        int y = Convert.ToInt32(initialCoords.Y);
                        //goes left to right
                        while(x <= Convert.ToInt32(thing.maximum.x)) {
                            // goes bottom to top
                            while(y < Convert.ToInt32(thing.maximum.y)) {
                                if(y > WorldController.AllGridIsland.EndY) {
                                    //Logger.logDebug($"y: {y} was to large");
                                    continue;

                                } else {
                                    tempIndex = WorldController.getGridIndex(new Position(x, y, 0));
                                    /*
                                    Position testing = WorldController.getPositionFromGridInex(tempIndex);
                                    // Logging for testing issues, indicates that a gridIndex shouldn't be right 
                                    if (x - testing.X > WorldController.step) {
                                        Logger.logDebug($"xTemp {x}, xGrid {testing.X} Abweichung {x - testing.X}");
                                    }
                                    if (y - testing.Y > WorldController.step) {
                                        Logger.logDebug($"yTemp {y}, yGrid {testing.Y} Abweichung {y - testing.Y}");
                                    }*/
                                    addTileToGridTiles(thing, item.name, item.displayName, tempIndex);
                                    // should step to upper border++ at first than alle steps in stepsize, step to max.y in upper border is to far for next step
                                    if(counterY == 0) {
                                        y = Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Top);
                                        y++;
                                    } else if(thing.maximum.y > Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Top) + WorldController.step) {
                                        y = Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Top + WorldController.step);
                                    } else if(thing.maximum.y < Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Top) + WorldController.step) {
                                        y = Convert.ToInt32(thing.maximum.y);
                                    } else {
                                        y += WorldController.step;
                                    }
                                    counterY++;
                                }
                            }
                            if(x > WorldController.AllGridIsland.EndX) {
                                //message for debugging purpose
                                //Logger.logDebug($"x: {x} was to large");
                                continue;
                            } else {
                                //reset y
                                y = Convert.ToInt32(initialCoords.Y);
                                // should step to right border++ at first than alle steps in stepsize, step to max.x in right border is to far for next step
                                if(counterX == 0) {
                                    x = Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Right);
                                    x++;
                                } else if(thing.maximum.x > Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Right) + WorldController.step) {
                                    y = Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Right + WorldController.step);
                                } else if(thing.maximum.x < Convert.ToInt32(WorldController.AllGrids[tempIndex].Rectangle.Right) + WorldController.step) {
                                    y = Convert.ToInt32(thing.maximum.x);
                                } else {
                                    y += WorldController.step;
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        protected void addTileToGridTiles(Bound bound, string name, string displayName, int gridTile) {
            if(gridTile > -1) {
                if(!regionTiles.ContainsKey(gridTile)) {
                    regionTiles[gridTile] = new List<RegionSlice>();
                }
                regionTiles[gridTile].Add(new RegionSlice(gridTile, name.ToUpper(), displayName, bound));
            } else {
                Logger.logError($"Gridtile {gridTile} war -1 ");
            }
        }

        // Prioritises player-pos for getting region-info
        public string getRegion(Position pos) {
            string region = "No Region";
            if(ChoiceVAPI.GetAllPlayers().Count > 0) {
                region = getRegionFromPlayer(ChoiceVAPI.GetAllPlayers().FirstOrDefault());
            } else {
                region = getRegion(pos);
            }

            return region;
        }

        public string getRegionFromPlayer(IPlayer player) {
            string region = "No Region";
            try {
                CallbackController.getPlayerRegion(player, (p, m) => {
                    region = m;
                });
            } catch(Exception e) { Logger.logDebug(LogCategory.System, LogActionType.Blocked, $" Something went wrong in getRegionFromPlayer"); region = "No Region"; }
            for(int i = 0; i < 20; i++) {
                if(region.Equals("No Region")) {
                    Thread.Sleep(20);
                } else {
                    break;
                }
            }
            return region;
        }

        public string getRegionName(Position pos) {
            string region = "No Region";
            float size = 0;

            // Get grid
            var gridIndex = WorldController.getGridIndex(pos);
            if(regionTiles.ContainsKey(gridIndex)) {
                //search all gridTiles in Grid
                foreach(var regionTile in regionTiles[gridIndex]) {
                    var testShit = regionTiles[gridIndex];
                    var slice = regionTile.regionTileBound;
                    //check coords
                    if(isInBound(pos.X, pos.Y, slice)) {
                        if(size == 0) {
                            //write automatic if size has no value
                            if(regionTile.name != region) {
                                region = regionTile.name;
                                size = regionsSize.GetValueOrDefault(regionTile.name);
                            }
                        } else {
                            //write if size is lower
                            if(size > regionsSize.GetValueOrDefault(regionTile.name)) {
                                if(regionTile.name != region) {
                                    region = regionTile.name;
                                    size = regionsSize.GetValueOrDefault(regionTile.name);
                                }
                            }
                        }
                    } /*else {
                        Logger.logFatal($"{regionTile.name} not inside Bounds");
                    }*/
                }
            }

            return region;
        }
        
        public string getBigRegionName(Position position) {
            var region = getRegionName(position);
            
            if(smallToBigRegion.ContainsKey(region)) {
                return smallToBigRegion.GetValueOrDefault(region);
            }

            return "NO_REGION";
        }
        
        public string getBigRegionDisplayName(string name) {
            if(bigRegionDisplayNames.ContainsKey(name)) {
                return bigRegionDisplayNames.GetValueOrDefault(name);
            } else {
                var smallRegion = smallToBigRegion.GetValueOrDefault(name);
                if(smallRegion != null && bigRegionDisplayNames.ContainsKey(smallRegion)) {
                    return bigRegionDisplayNames.GetValueOrDefault(smallRegion);
                } else {
                    return "NO_REGION";
                }
            }
        }

        public InputMenuItem getBigRegionSelectMenuItem(string name, string evt, bool addNoneOption) {
            var options = bigRegionDisplayNames.Values.ToList();
            if(addNoneOption) {
                options.Insert(0, "KEINE");
            }
            
            return new InputMenuItem(name, "Wähle eine Region", "", evt).withOptions(options.ToArray());
        }

        public string getBigRegionIdentifierFromSelectMenuItemInput(string input) {
           return bigRegionDisplayNames.FirstOrDefault(x => x.Value == input).Key;
        }
        
        public string getBigRegionDisplayName(Position position) {
            var region = getRegionName(position);
            
            if(smallToBigRegion.ContainsKey(region)) {
                return bigRegionDisplayNames.GetValueOrDefault(smallToBigRegion.GetValueOrDefault(region));
            }

            return "NO_REGION";
        }
        
        public List<string> getAllBigRegions() {
            return smallToBigRegion.Values.Distinct().ToList();
        }

        public string getRegionDisplayName(string name) {
            // ToDo find MapZonesLabels in Enums e.g.
            string result = "Keine Region";
            if(regionDisplayNames.ContainsKey(name.ToUpper())) {
                result = regionDisplayNames.Get(name.ToUpper());
            }
            return result;
        }

        protected bool isInBound(float x, float y, Bound bound) {
            //check if x,y is inside the borders of Bounds
            bool isInside = false;
            if(x >= bound.minimum.x && x <= bound.maximum.x && y >= bound.minimum.y && y <= bound.maximum.y) {
                isInside = true;
            }

            return isInside;
        }

        protected float getSizeofReactangle(Bound bound) {
            float size = 0;
            float minx = bound.minimum.x;
            float miny = bound.minimum.y;
            float maxx = bound.maximum.x;
            float maxy = bound.maximum.y;
            float height = Math.Abs(maxy - miny);
            float width = Math.Abs(maxx - minx);
            size = height * width;
            return size;
        }
        #endregion
        #region helperclasses

        public class RegionSlice {
            public int gridTile;
            public string name;
            public string displayName;
            public Bound regionTileBound;
            public RegionSlice() {

            }

            public RegionSlice(int gridTile, string name, string displayName, Bound regionTileBound) {
                this.gridTile = gridTile;
                this.name = name;
                this.displayName = displayName;
                this.regionTileBound = regionTileBound;
            }

        }
        public class AllRegions {
            public List<Region> regions;

            public AllRegions() {
                regions = new();
            }
            public AllRegions(List<Region> regions) {
                this.regions = regions;
            }
        }

        public class Region {
            public string name;
            public string displayName;
            public List<Bound> bounds;
            public Region(string name, string displayname, List<Bound> bounds) {
                this.name = name;
                this.displayName = displayname;
                this.bounds = bounds;
            }
        }

        public class Bound {
            public BoundReactangle minimum;
            public BoundReactangle maximum;

            public Bound(BoundReactangle minimum, BoundReactangle maximum) {
                this.minimum = minimum;
                this.maximum = maximum;
            }
        }

        public class BoundReactangle {
            public float x;
            public float y;
            public float z;
            public BoundReactangle(float x, float y, float z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }
        #endregion
    }


}
