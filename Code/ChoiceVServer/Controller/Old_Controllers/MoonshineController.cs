//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.InventorySystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using static ChoiceVServer.Base.Constants;

//namespace ChoiceVServer.Controller {
//    class MoonshineController : ChoiceVScript {
//        public static List<SkillLevel> skilllist = new List<SkillLevel>();
//        public MoonshineController() {
//            EventController.addCollisionShapeEvent("DEST_TALK", destTalk);
//            EventController.addCollisionShapeEvent("AT_SCRAP", getscrap);

//            EventController.addMenuEvent("ERIC_TALK", npcTalk);
//            EventController.addMenuEvent("ERK_SELL", npcSell);
//            loadSkillLevel();
//        }

//        private bool getscrap(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
//            AnimationController.animationTask(player, anim, () => {
//                generateScrap(player);
//            });
//            return true;
//        }

//        private bool npcSell(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var check = false;
//            var skill = getSkillLevel(player);
//            var progress = getProgressLevel(player);
//            if (data.ContainsKey("CHECK") == true) {
//                check = data["CHECK"];
//            }
//            if (check == true) {
//                if (data.ContainsKey("TYPE") == true) {
//                    var type = data["TYPE"];
//                    Item item = null;
//                    var amount = 0;
//                    var count = 0;
//                    if (type = "CORN") {
//                        item = player.getInventory().getItem(MOONSHINE_ID, i => i.Description.Contains("Mais"));
//                        amount = item.StackAmount * 150 ?? default;
//                        count = item.StackAmount ?? default;
//                    }
//                    if (type = "APPLE") {
//                        item = player.getInventory().getItem(MOONSHINE_ID, i => i.Description.Contains("Apfel"));
//                        amount = item.StackAmount * 200 ?? default;
//                        count = item.StackAmount ?? default;
//                    }
//                    if (type = "RASPBERRY") {
//                        item = player.getInventory().getItem(MOONSHINE_ID, i => i.Description.Contains("Himbeer"));
//                        amount = item.StackAmount * 250 ?? default;
//                        count = item.StackAmount ?? default;
//                    }
//                    if (type = "PEAR") {
//                        item = player.getInventory().getItem(MOONSHINE_ID, i => i.Description.Contains("Birne"));
//                        amount = (item.StackAmount * 300) ?? default;
//                        count = item.StackAmount ?? default;
//                    }
//                    if (type = "CHERRY") {
//                        item = player.getInventory().getItem(MOONSHINE_ID, i => i.Description.Contains("Kirsch"));
//                        amount = item.StackAmount * 350 ?? default;
//                        count = item.StackAmount ?? default;
//                    }
//                    player.getInventory().removeItem(item);
//                    player.getCharacterData().Cash += amount;
//                    addSkillLevel(player, false, count, true);
//                }
//            } else {

//                var sellmenu = new Menu("Schnappsauswahl", "");
//                sellmenu.addMenuItem(new ClickMenuItem("Maisschnaps verkaufen", "", "150$", "ERK_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TYPE", "CORN" }, { "CHECK", true } }));
//                sellmenu.addMenuItem(new ClickMenuItem("Apfelschnaps verkaufen", "", "200$", "ERK_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TYPE", "APPLE" }, { "CHECK", true } }));
//                sellmenu.addMenuItem(new ClickMenuItem("Himbeer verkaufen", "", "250$", "ERK_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TYPE", "RASPBERRY" }, { "CHECK", true } }));
//                sellmenu.addMenuItem(new ClickMenuItem("Birnenschnaps verkaufen", "", "300$", "ERK_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TYPE", "PEAR" }, { "CHECK", true } }));
//                sellmenu.addMenuItem(new ClickMenuItem("Kirschschnaps verkaufen", "", "350$", "ERK_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TYPE", "CHERRY" }, { "CHECK", true } }));
//                player.showMenu(sellmenu);
//            }
//            return true;
//        }

//        private bool npcTalk(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var skill = 0;
//            if (data.ContainsKey("TALK") == true) {
//                skill = data["TALK"];
//            }
//            var progress = getProgressLevel(player);
//            if (data.ContainsKey("ALK") == true) {
//                var alk = data["ALK"];
//                var inv = player.getInventory().getAllItems().FirstOrDefault(x => x is Alcohol && x.StackAmount >= 10);
//                if (inv != null && skill == 1) {
//                    player.getInventory().removeItem(inv, 10);
//                    player.sendNotification(NotifactionTypes.Info, "Ah fast so gut wie selbstgebraut!", ""); 
//                    player.sendNotification(NotifactionTypes.Info, "Wenn du willst baue ich dir eine Destille, dann kannst du mir selbstgebranntes verkaufen!", ""); 
//                    player.sendNotification(NotifactionTypes.Info, "Anbei noch ein Rezept für Maisschnaps", ""); 
//                    addSkillLevel(player, false, 0, false);

//                } else {
//                    player.sendNotification(NotifactionTypes.Info, "Ich sagte 10 Flaschen du Idiot!", "Nicht 10 Flaschen"); 
//                    return true;
//                }
//            }

//            if (data.ContainsKey("ASK") == true) {
//                var check = data["ASK"];
//                if (check == true) {
//                    if (skill == 2 && progress >= 40) {
//                        player.sendNotification(NotifactionTypes.Info, "Hier ein Rezept für Apfelschnaps!", "Neues Rezept"); 
//                        addSkillLevel(player, false, -40, false);
//                        return true;
//                    } else if (skill == 3 && progress >= 40) {
//                        player.sendNotification(NotifactionTypes.Info, "Hier ein Rezept für Himbeerschnaps!", "Neues Rezept"); 
//                        addSkillLevel(player, false, -40, false);
//                        return true;
//                    } else if (skill == 4 && progress >= 40) {
//                        player.sendNotification(NotifactionTypes.Info, "Hier ein Rezept für Birnenschnaps!", "Neues Rezept"); 
//                        addSkillLevel(player, false, -40, false);
//                        return true;
//                    } else if (skill == 5 && progress >= 40) {
//                        player.sendNotification(NotifactionTypes.Info, "Hier ein Rezept für Kirschschnaps!", "Neues Rezept"); 
//                        addSkillLevel(player, false, -40, false);
//                        return true;
//                    } else if (progress < 40) {
//                        player.sendNotification(NotifactionTypes.Info, "Bring mir erstmal mehr Schnaps bevor ich dir was neues zeige!", "Mehr Schnaps"); 
//                        return true;
//                    } else if (skill >= 6) {
//                        player.sendNotification(NotifactionTypes.Info, "Momentan hab ich nichts neues für dich!", "Keine Rezepte mehr"); 
//                        return true;
//                    }

//                }
//            }

//            if (skill == 0) {
//                player.sendNotification(NotifactionTypes.Info, "Bring mir erstmal 10 Flaschen Alkohol bevor du Infos bekommst!", "Hole Alk für Erik");
//                addSkillLevel(player, true, 0, false);
//            }
//            if (skill == 2 && data.ContainsKey("DEST") == true) {
//                var check = data["DEST"];
//                if (check == true) {
//                    //check if he got all items for destillery
//                    var copper = player.getInventory().getItem(COPPERPLATE_ID, i => i.StackAmount >= 15);
//                    var pipe = player.getInventory().getItem(PIPE_ID, i => true);
//                    var furnace = player.getInventory().getItem(FURNACE_ID, i => true);
//                    var therm = player.getInventory().getItem(THERM_ID, i => i.StackAmount >= 2);
//                    if (copper == null || pipe == null || furnace == null || therm == null) {
//                        player.sendNotification(NotifactionTypes.Warning, "Du hast nicht alle benötigten Gegenstände!", "");
//                        return false;
//                    } else {                 //if true give one Destillery
//                        player.getInventory().removeItem(copper, copper.StackAmount ?? 1);
//                        player.getInventory().removeItem(pipe, pipe.StackAmount ?? 1);
//                        player.getInventory().removeItem(furnace, furnace.StackAmount ?? 1);
//                        player.getInventory().removeItem(therm, therm.StackAmount ?? 1);
//                        var cf = InventoryController.AllConfigItems[DESTILLERY_ID];
//                        player.getInventory().addItem(new PlaceableObjectItem(cf));
//                    }
//                }
//            }
//            return true;
//        }

//        private bool destTalk(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
//            //NPC to get the Destillery
//            var skill = getSkillLevel(player);
//            var menu = new Menu("Eric Kirschbaum", "");
//            if (skill == 0) {
//                menu.addMenuItem(new ClickMenuItem("Nach Infos fragen", "", "", "ERIC_TALK", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TALK", skill } }));
//            }
//            if (skill == 1) {
//                menu.addMenuItem(new ClickMenuItem("Alkohol geben", "", "", "ERIC_TALK", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TALK", skill }, { "ALK", true } }));
//            }
//            if (skill >= 2) {
//                menu.addMenuItem(new ClickMenuItem("Destille bauen lassen", "", "", "ERIC_TALK", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TALK", skill }, { "DEST", true } }));
//            }
//            if (skill >= 2) {
//                menu.addMenuItem(new ClickMenuItem("Alkohol verkaufen", "", "", "ERIC_SELL", MenuItemStyle.normal).withData(new Dictionary<string, dynamic> { { "TALK", skill } }));
//            }
//            if (skill >= 2) {
//                menu.addMenuItem(new ClickMenuItem("Nach neuen Rezepten fragen", "", "", "ERIC_TALK", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "ASK", true }, { "TALK", skill } }));
//            }
//            player.showMenu(menu);
//            return true;
//        }

//        #region helper

//        private int getSkillLevel(IPlayer player) {
//            var skill = skilllist.FirstOrDefault(x => x.CharID == player.getCharacterId());
//            if (skill != null) {
//                return skill.Level;
//            }
//            return 0;
//        }

//        private int getProgressLevel(IPlayer player) {
//            var skill = skilllist.FirstOrDefault(x => x.CharID == player.getCharacterId());
//            if (skill != null) {
//                return skill.progress;
//            }
//            return 0;
//        }

//        private void addSkillLevel(IPlayer player, bool first, int progress, bool justprogress) {
//            //using (var db = new ChoiceVDb()) {
//            //    if (first == true) {

//            //        var skill = new moonshineskill {
//            //            charID = player.getCharacterId(),
//            //            Level = 1,
//            //            progress = progress,
//            //        };

//            //        db.Add(skill);
//            //        player.sendBlockNotification("CREATE", "");
//            //    } else if (justprogress == false) {

//            //        var check = db.moonshineskill.FirstOrDefault(x => x.charID == player.getCharacterId());
//            //        if (check != null) {
//            //            check.Level++;
//            //            check.progress = check.progress + progress;
//            //        }
//            //        db.Update(check);
//            //    } else {
//            //        var check = db.moonshineskill.FirstOrDefault(x => x.charID == player.getCharacterId());
//            //        if (check != null) {
//            //            check.progress = check.progress + progress;
//            //        }
//            //        db.Update(check);
//            //    }
//            //    db.SaveChanges();
//            //    db.DisposeAsync();
//            //    skilllist.Clear();
//            //    loadSkillLevel();
//            //}
//        }

//        private void loadSkillLevel() {
//            //using (var db = new ChoiceVDb()) {
//            //    foreach (var skill in db.moonshineskill) {
//            //        var x = new SkillLevel {
//            //            CharID = skill.charID,
//            //            Level = skill.Level,
//            //            progress = skill.progress,
//            //        };
//            //        skilllist.Add(x);
//            //    }
//            //    db.DisposeAsync();
//            //}
//        }

//        private void generateScrap(IPlayer player) {
//            int itemnr = generateScrap();
//            try {
//                configitems configItem = InventoryController.AllConfigItems[itemnr];
//                var item = new Item(configItem, -1);
//                var name = item.Name;
//                var itemcheck = player.getInventory().addItem(item);
//                if (itemcheck == false) {
//                    player.sendNotification(NotifactionTypes.Info, "Du hast keinen Platz für den folgenden Gegenstand: " + name, "Kein Platz");
//                } else {
//                    player.sendNotification(NotifactionTypes.Info, "Du hast folgenden Gegenstand gefunden: " + name, name + " gefunden");
//                }

//            } catch (Exception e) {
//                Logger.logException(e);
//            }
//        }

//        private int generateScrap() {
//            Random random = new Random();
//            int randomtime = random.Next(0, 30);
//            if (randomtime > 30) {
//                //other items
//            }
//            if (randomtime >= 0 && randomtime <= 9) {
//                return 66;
//            }
//            if (randomtime >= 10 && randomtime <= 19) {
//                return 67;
//            }
//            if (randomtime >= 20 && randomtime <= 24) {
//                return 68;
//            }
//            if (randomtime >= 25 && randomtime <= 29) {
//                return 69;
//            }
//            return 0;
//        }

//        #endregion
//    }

//    public class SkillLevel {
//        public int CharID { get; set; }
//        public int Level { get; set; }
//        public int progress { get; set; }

//    }
//}
