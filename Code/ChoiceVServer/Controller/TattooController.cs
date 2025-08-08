using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.DamageSystem;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;


namespace ChoiceVServer.Controller {
    public enum TattooCategory {
        Deactivated = 0,
        NotShown = 1,
        CanBeRemoved = 2,
        NPC = 3,
        Player = 4,
        Prison = 5,
        Tribal = 6,
    }

    public class TattooController : ChoiceVScript {
        public static List<configtattoo> AllShopTattoos;
        public static List<List<configtattoo>> AllTattooCategories;

        public TattooController() {
            loadConfigTattoos();
            EventController.PlayerSuccessfullConnectionDelegate += onPlayerConnect;

            EventController.addMenuEvent("PLAYER_SELECT_TATTOO", onPlayerSelectTattoo);
            EventController.addCollisionShapeEvent("SHIPPING_PED_INTERACT", onTattooPedInteract);

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.PlayerStyle,
                    "Tattoo-Menü",
                    tattooGenerator
                )
            );

            EventController.addMenuEvent("SUPPORT_TATTOO_CHANGE", onSupportTattooChange);

            EventController.addMenuEvent("TATTOO_MACHINE_INSPECT", onTattooMachineInspect);
            EventController.addMenuEvent("TATTOO_MACHINE_EDIT", onTattooMachineEdit);

            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Tattowiervorschau öffnen", "Zeige der Person Tattoos", "", "OPEN_PLAYER_TATTOO_SHOW_MENU"),
                    s => (s as IPlayer).getInventory().hasItem<StaticItem>(t => t.AdditionalInfo == "TattooShapes") && isTattooArtist(s as IPlayer),
                    t => true
                )
            );
            EventController.addMenuEvent("OPEN_PLAYER_TATTOO_SHOW_MENU", onPlayerOpenShowTattooMenu);

            EventController.addMenuEvent("TATTOO_MACHINE_PREPARE_ZONE", onTattooMachinePrepareZone);
            EventController.addMenuEvent("TATTOO_MACHINE_AFTER_ZONE", onTattooMachineAfterZone);
        }

        private bool isTattooArtist(IPlayer player) {
            //TODO
            return true;

            //var companies = CompanyController.getCompanies(player);
            //if(companies != null) {
            //    return companies.FirstOrDefault(c => c.Type == CompanyType.Tattoo) != null;
            //}

            //return false;
        }

        #region TattooMachine

        private bool onTattooMachineInspect(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var item = (TattooMachine)data["Item"];

            var freshNeedle = item.FreshNeedle;
            var freshColor = item.FreshColorSet;
            if(freshNeedle && freshColor) {
                player.sendNotification(NotifactionTypes.Info, "Die Maschine ist in astreinem Zustand", "Tattoomachine benutzbar", NotifactionImages.Tattoo);
            } else switch(freshColor) {
                    //TODO: Dieser Fall ist immer "true", da zuvor ja schon auf "true" und "true" geprüft wird.
                    case true when !freshNeedle:
                        player.sendNotification(NotifactionTypes.Warning, "Die Nadel der Maschine sieht benutzt aus!", "Tattoonadel benutzt", NotifactionImages.Tattoo);
                        break;
                    case false when freshNeedle:
                        player.sendNotification(NotifactionTypes.Warning, "Das Farbset in der Maschine ist leer!", "Tattoofarbe leer", NotifactionImages.Tattoo);
                        break;
                    default:
                        player.sendNotification(NotifactionTypes.Warning, "Das Farbset der Maschine ist leer und die Nadel benutzt!", "Tattoofarbe leer", NotifactionImages.Tattoo);
                        break;
                }

            return true;
        }

        private bool onTattooMachineEdit(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var type = data["Type"];
            var item = (TattooMachine)data["Item"];

            switch(type) {
                case "Needle":
                    var needleItem = player.getInventory().getItem<StaticItem>(n => n.AdditionalInfo == "TattooNeedle");
                    if(needleItem != null) {
                        player.getInventory().removeItem(needleItem);
                        item.FreshNeedle = true;
                        player.sendNotification(NotifactionTypes.Success, "Tattoonadel erfolgreich gewechselt!", "Tattoonadel gewechselt", NotifactionImages.Tattoo);
                    } else {
                        player.sendBlockNotification("Die Nadel konnte nicht gewechselt werden!", "Nadel nicht gewechselt!", NotifactionImages.Tattoo);
                    }
                    break;
                case "Color":
                    var colorItem = player.getInventory().getItem<StaticItem>(n => n.AdditionalInfo == "TattooColor");
                    if(colorItem != null) {
                        player.getInventory().removeItem(colorItem);
                        item.FreshColorSet = true;
                        player.sendNotification(NotifactionTypes.Success, "Tattoofarbset erfolgreich gewechselt!", "Tattoofarbset gewechselt", NotifactionImages.Tattoo);
                    } else {
                        player.sendBlockNotification("Das Tattoofarbset konnte nicht gewechselt werden!", "Tattoofarbset nicht gewechselt!", NotifactionImages.Tattoo);
                    }
                    break;
            }

            return true;
        }

        public static void playerOpenTatooMenu(IPlayer player, IPlayer target) {
            if(target != null) {
                var menu = new Menu("Tätowierungsset", "Was möchtest du tun?", true);
                var prepareMenu = new Menu("Körperteil vorbereiten", "Tättowierung vorbereiten", (p) => resetPlayerTattoos(target));
                for(var i = 0; i <= 5; i++) {
                    var prepData = new Dictionary<string, dynamic> { { "ZoneId", i }, { "Target", target } };
                    prepareMenu.addMenuItem(new ClickMenuItem($"{getZoneNameFromId(i)} vorbereiten", $"Bereite {getZoneNameFromId(i)} auf eine Tätowierung vor", "", "TATTOO_MACHINE_PREPARE_ZONE").withData(prepData).needsConfirmation($"{getZoneNameFromId(i)} vorbereiten", "Tätowierung wirklich vorbereiten?"));
                }
                menu.addMenuItem(new MenuMenuItem(prepareMenu.Name, prepareMenu));

                var tattooMenu = getTattooMenu(player, target, false, true);
                menu.addMenuItem(new MenuMenuItem(tattooMenu.Name, tattooMenu));

                var afterMenu = new Menu("Körperteil eincremen", "Welches Körperteil eincremen?");
                for(var i = 0; i <= 5; i++) {
                    var afterData = new Dictionary<string, dynamic> { { "ZoneId", i }, { "Target", target } };
                    afterMenu.addMenuItem(new ClickMenuItem($"{getZoneNameFromId(i)} eincremen", $"Creme {getZoneNameFromId(i)} ein um eine Entzündung zu verhindern", "", "TATTOO_MACHINE_AFTER_ZONE").withData(afterData).needsConfirmation($"{getZoneNameFromId(i)} eincremen", "Körperteil wirklich eincremen?"));
                }
                menu.addMenuItem(new MenuMenuItem(afterMenu.Name, afterMenu));

                player.showMenu(menu, false);
            } else {
                player.sendBlockNotification("Der andere Spieler konnte nicht gefunden werden!", "Spieler weg!", NotifactionImages.Tattoo);
            }
        }

        private bool onTattooMachinePrepareZone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["Target"];
            var zoneId = (int)data["ZoneId"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                if(target.Position.Distance(player.Position) < 3) {
                    target.setData("TATTOO_PREPARE", zoneId);
                    player.sendNotification(NotifactionTypes.Success, $"{getZoneNameFromId(zoneId)} erfolgreich auf Tätowierung vorbereitet!", $"{getZoneNameFromId(zoneId)} vorbereitet!", NotifactionImages.Tattoo);
                } else {
                    player.sendBlockNotification("Das Ziel hat sich zu weit entfernt!", "Ziel zu weit weg!", NotifactionImages.Tattoo);
                }
            });

            return true;
        }

        private bool onTattooMachineAfterZone(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["Target"];
            var zoneId = (int)data["ZoneId"];

            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                if(target.Position.Distance(player.Position) < 3) {
                    target.setData("TATTOO_AFTER", zoneId);
                    player.sendNotification(NotifactionTypes.Success, $"{getZoneNameFromId(zoneId)} erfolgreich nach eingecremt!", $"{getZoneNameFromId(zoneId)} eingecremt!", NotifactionImages.Tattoo);
                } else {
                    player.sendBlockNotification("Das Ziel hat sich zu weit entfernt!", "Ziel zu weit weg!", NotifactionImages.Tattoo);
                }
            });

            return true;
        }

        private bool onPlayerOpenShowTattooMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var targetId = (int)data["InteractionTargetId"];
            var target = ChoiceVAPI.FindPlayerByCharId(targetId);

            if(target != null) {
                player.showMenu(getTattooMenu(player, target, false));
            } else {
                player.sendBlockNotification("Der andere Spieler konnte nicht gefunden werden!", "Spieler weg!", NotifactionImages.Tattoo);
            }

            return true;
        }

        #endregion

        #region Support Stuff

        private Menu tattooGenerator(IPlayer player) {
            var menu = new Menu("Tattoo-Menü", "Tattoos editieren", resetPlayerTattoos);
            var gender = player.getCharacterData().Gender;
            var categoryList = ((TattooCategory[])Enum.GetValues(typeof(TattooCategory))).Select(t => t.ToString()).ToList();

            foreach(var bodyPartTattoos in AllTattooCategories) {
                var bodyPart = getZoneNameFromId(bodyPartTattoos.First().ZoneID);

                var partMenu = new Menu(bodyPart, "Körperteil editieren");
                foreach(var tattoo in bodyPartTattoos.Where(t => gender == 'M' && t.HashNameMale != "" || gender == 'F' && t.HashNameFemale != "")) {
                    var data = new Dictionary<string, dynamic> { { "Target", player }, { "Tattoo", tattoo }, { "Npc", true } };

                    var tatMenu = new Menu(tattoo.LocalizedName, tattoo.LocalizedName + " editeren", resetPlayerTattoos);
                    tatMenu.addMenuItem(new HoverMenuItem("Tattoo anzeigen (selekt.)", "Zeige das Tattoo an", "", "PLAYER_SELECT_TATTOO").withData(data));

                    var freeViewList = new[] { "Aus", "An" };
                    tatMenu.addMenuItem(new ListMenuItem("Freikamera Toggle", "Stelle ein ob du dich frei umschauen kannst", freeViewList, "SUPPORT_CLOTH_FREE_CAM", MenuItemStyle.normal, true, true));
                    tatMenu.addMenuItem(new ListMenuItem("Kategorie ändern", "Ändere die Kategorie des Tattos", categoryList.ShiftLeft(tattoo.Category).ToArray(), ""));
                    tatMenu.addMenuItem(new InputMenuItem("Preis ändern", "Ändere den Preis des Tattos", $"{tattoo.Price}", ""));
                    tatMenu.addMenuItem(new MenuStatsMenuItem("Änderungen speichern", "Speichere die Änderungen", "SUPPORT_TATTOO_CHANGE", MenuItemStyle.green).needsConfirmation("Änderungen speichern?", "Wirklich speichern?").withData(data));

                    partMenu.addMenuItem(new MenuMenuItem(tatMenu.Name, tatMenu));
                }

                menu.addMenuItem(new MenuMenuItem(partMenu.Name, partMenu));
            }

            return menu;
        }


        private bool onSupportTattooChange(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as MenuStatsMenuItemEvent;
            var tat = (configtattoo)data["Tattoo"];

            try {
                var catEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
                var priceEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

                using(var db = new ChoiceVDb()) {
                    var dbTat = db.configtattoos.Find(tat.id);

                    dbTat.Category = (int)Enum.Parse(typeof(TattooCategory), catEvt.currentElement);
                    if(priceEvt.input != null && priceEvt.input != "") {
                        dbTat.Price = decimal.Parse(priceEvt.input);
                    }

                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, $"Tattoo editiert: Preis: {dbTat.Price}, Category: {((TattooCategory)dbTat.Category).ToString()}", "");
                    loadConfigTattoos();
                }
            } catch(Exception) {
                player.sendBlockNotification("Etwas ist schiefgelaufen!", "");
            }

            return true;
        }

        #endregion

        private void loadConfigTattoos() {
            using(var db = new ChoiceVDb()) {
                AllShopTattoos = db.configtattoos.ToList();
                AllTattooCategories = AllShopTattoos.GroupBy(t => t.ZoneID).Select(t => t.OrderBy(t => t.LocalizedName).ToList()).ToList();
            }
        }

        private void onPlayerConnect(IPlayer player, character character) {
            setPlayerTattoos(player, character.tattoos.ToList());
        }

        private static void resetPlayerTattoos(IPlayer player) {
            var gender = player.getCharacterData().Gender;
            player.resetOverlayType("tattoo");
            foreach(var tattoo in player.getCharacterData().AllTattoos) {
                if(gender == 'F') {
                    player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameFemale);
                } else {
                    player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameMale);
                }
            }
        }

        public static Menu getTattooMenu(IPlayer player, IPlayer target, bool npc, bool tattooMachine = false, Func<bool> buyCallback = null) {
            var gender = target.getCharacterData().Gender;
            var menu = new Menu("Tattoo-Menü", "Zeige Tattoos und steche sie", (p) => resetPlayerTattoos(target));

            var cash = target.getCash();
            var playerTattoos = player.getCharacterData().AllTattoos;

            foreach(var bodyPartTattoos in AllTattooCategories) {
                var bodyPart = getZoneNameFromId(bodyPartTattoos.First().ZoneID);

                var partMenu = new Menu(bodyPart, "Steche Tattos auf das Körperteil", (p) => resetPlayerTattoos(target));
                IEnumerable<configtattoo> tattooList;
                if(npc && buyCallback == null) {
                    tattooList = bodyPartTattoos.Where(t => t.Category == (int)TattooCategory.NPC && (gender == 'M' && t.HashNameMale != "" || gender == 'F' && t.HashNameFemale != ""));
                } else {
                    tattooList = bodyPartTattoos.Where(t => (t.Category == (int)TattooCategory.NPC || t.Category == (int)TattooCategory.Player) && gender == 'M' && t.HashNameMale != "" || gender == 'F' && t.HashNameFemale != "");
                }

                foreach(var tattoo in tattooList) {
                    if(!playerTattoos.Contains(tattoo)) {
                        var data = new Dictionary<string, dynamic> { { "Target", target }, { "Tattoo", tattoo }, { "Npc", npc }, { "Action", buyCallback } };
                        if(npc) {
                            if(buyCallback != null || cash >= tattoo.Price) {
                                partMenu.addMenuItem(new ClickMenuItem(tattoo.LocalizedName, "Steche dir angegebene Tattoo", buyCallback == null ? $"${tattoo.Price}" : "", "PLAYER_SELECT_TATTOO", MenuItemStyle.normal, true).needsConfirmation(buyCallback == null ? $"Für ${tattoo.Price} stechen lassen?" : "Tattoo stechen lassen?", "Wirklich stechen lassen?").withData(data));
                            } else {
                                partMenu.addMenuItem(new HoverMenuItem(tattoo.LocalizedName, "Du hast nicht genug Geld", $"${tattoo.Price}", "PLAYER_SELECT_TATTOO", MenuItemStyle.red).withData(data));
                            }
                        } else {
                            if(tattooMachine) {
                                partMenu.addMenuItem(new ClickMenuItem(tattoo.LocalizedName, "Steche der Person das angegebene Tattoo", "", "PLAYER_SELECT_TATTOO", MenuItemStyle.normal, true).withData(data).needsConfirmation($"Person {tattoo.LocalizedName} stechen?", "Tattoo wirklich stechen?"));
                            } else {
                                partMenu.addMenuItem(new HoverMenuItem(tattoo.LocalizedName, "Zeige das angegebene Tattoo", "", "PLAYER_SELECT_TATTOO").withData(data));
                            }
                        }
                    } else {
                        partMenu.addMenuItem(new StaticMenuItem(tattoo.LocalizedName, "Du hast dieses Tattoo bereits", $"${tattoo.Price}", MenuItemStyle.yellow));
                    }
                }

                menu.addMenuItem(new MenuMenuItem(partMenu.Name, partMenu));
            }

            return menu;
        }

        public static List<configtattoo> getPlayerTattoos(IPlayer player) {
            return player.getCharacterData().AllTattoos;
        }

        public static bool removePlayerTattoo(IPlayer player, configtattoo tattoo) {
            var worked = false;
            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Include(c => c.tattoos).FirstOrDefault(c => c.id == player.getCharacterId());
                var el = dbChar.tattoos.FirstOrDefault(t => t.id == tattoo.id);
                worked = dbChar.tattoos.Remove(el);
                player.getCharacterData().AllTattoos.Remove(tattoo);

                db.SaveChanges();

                if(worked) {
                    resetPlayerTattoos(player);
                }
            }

            return worked;
        }

        private static string getZoneNameFromId(int id) {
            switch(id) {
                case 0:
                    return "Torso";
                case 1:
                    return "Kopf";
                case 2:
                    return "Linker Arm";
                case 3:
                    return "Rechter Arm";
                case 4:
                    return "Linkes Bein";
                case 5:
                    return "Rechtes Bein";
                default:
                    return "Unbekannt";
            }
        }

        private static void setPlayerTattoos(IPlayer player, List<configtattoo> tattooList) {
            var gender = player.getCharacterData().Gender;

            foreach(var item in tattooList) {
                if(gender == 'F') {
                    player.setOverlay("tattoo", item.CollectionName, item.HashNameFemale);
                    //player.emitClientEvent(PlayerSetDecoration, player, "tattoo", item.CollectionName, item.HashNameFemale);
                } else {
                    player.setOverlay("tattoo", item.CollectionName, item.HashNameMale);
                    //player.emitClientEvent(PlayerSetDecoration, player, "tattoo", item.CollectionName, item.HashNameMale);
                }
            }

            player.getCharacterData().AllTattoos = tattooList;
        }


        /// <summary>
        /// Sets a player Tattoo by collection and hash. Look them up in the configTattoo database table
        /// </summary>
        public static void setPlayerTattoo(IPlayer player, string collection, string hash, bool female) {
            configtattoo tattoo;
            using(var db = new ChoiceVDb()) {
                if(female) {
                    tattoo = db.configtattoos.FirstOrDefault(t => t.CollectionName == collection && t.HashNameFemale == hash);
                } else {
                    tattoo = db.configtattoos.FirstOrDefault(t => t.CollectionName == collection && t.HashNameMale == hash);
                }
            }

            setPlayerTattoo(player, tattoo, female);
        }

        /// <summary>
        /// Sets a player Tattoo by database row id. Look them up in the configTattoo database table
        /// </summary>
        public static void setPlayerTattoo(IPlayer player, int configTattooId, bool female) {
            configtattoo tattoo;
            using(var db = new ChoiceVDb()) {
                tattoo = db.configtattoos.FirstOrDefault(t => t.id == configTattooId);
            }

            setPlayerTattoo(player, tattoo, female);
        }

        /// <summary>
        /// Consider using setPlayerTattoo overloads! Sets a player Tattoo by database Row.
        /// </summary>
        public static void setPlayerTattoo(IPlayer player, configtattoo tattoo, bool female) {
            if(tattoo == null) {
                Logger.logError($"setPlayerTattoo: The tattoo to set for: {player.getCharacterId()} was null",
                    $"Fehler im Tattoo setzen: Tatto wurde nicht gefunden", player);
                return;
            }

            using(var db = new ChoiceVDb()) {
                var dbChar = db.characters.Find(player.getCharacterId());
                dbChar.tattoos.Add(tattoo);

                db.SaveChanges();
            }

            player.getCharacterData().AllTattoos.Add(tattoo);

            if(female) {
                player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameFemale);
            } else {
                player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameMale);
            }
        }

        /// <summary>
        /// Temporary set Player
        /// </summary>
        private static void setPlayerTempTattoo(IPlayer player, configtattoo tattoo, bool female) {
            if(tattoo == null) {
                Logger.logError($"setPlayerTattoo: The tattoo to set for: {player.getCharacterId()} was null",
                    $"Fehler im Tattoo setzen: Tatto wurde nicht gefunden", player);
                return;
            }

            if(female) {
                player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameFemale);
                //player.emitClientEvent(PlayerSetDecoration, player, "tattoo", tattoo.CollectionName, tattoo.HashNameFemale);
            } else {
                player.setOverlay("tattoo", tattoo.CollectionName, tattoo.HashNameMale);
                //player.emitClientEvent(PlayerSetDecoration, player, "tattoo", tattoo.CollectionName, tattoo.HashNameMale);
            }
        }

        private bool onPlayerSelectTattoo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var tattoo = (configtattoo)data["Tattoo"];
            var target = (IPlayer)data["Target"];
            var npc = (bool)data["Npc"];
            var buyAction = (Func<bool>)data["Action"];

            if(menuItemCefEvent.action == "changed") {
                if(tattoo != null && target != null) {
                    resetPlayerTattoos(target);
                    setPlayerTempTattoo(target, tattoo, player.getCharacterData().Gender == 'F');
                }
            } else {
                if(npc) {
                    buyAction ??= () => target.removeCash(tattoo.Price);

                    if(buyAction.Invoke()) {
                        SoundController.playSoundAtCoords(target.Position, 7.5f, SoundController.Sounds.TattooMachine, 0.75f, "mp3");
                        setPlayerTattoo(target, tattoo, player.getCharacterData().Gender == 'F');
                        target.sendNotification(NotifactionTypes.Success, "Tattoo wurde gestochen", "Tattoo gestochen", NotifactionImages.Tattoo);
                    } else {
                        target.sendBlockNotification("Du hast nicht genug Bargeld!", "Nicht genug Geld!", NotifactionImages.Tattoo);
                    }
                } else {
                    var anim = AnimationController.getAnimationByName("WORK_FRONT");
                    SoundController.playSoundAtCoords(player.Position, 7.5f, SoundController.Sounds.TattooMachine, 0.75f, "mp3");
                    AnimationController.animationTask(player, anim, () => {
                        if(target.Position.Distance(player.Position) < 2.5) {
                            var item = player.getInventory().getItem<TattooMachine>(t => true);
                            if(item != null && item.FreshColorSet) {
                                setPlayerTattoo(target, tattoo, player.getCharacterData().Gender == 'F');
                                var freshNeedle = item.FreshNeedle.ToJson().FromJson<bool>();
                                var zoneId = tattoo.ZoneID;
                                item.FreshColorSet = false;
                                item.FreshNeedle = false;
                                InvokeController.AddTimedInvoke("TATTOO_INJURY_CHECKER", i => {
                                    var zoneIdPrepared = player.hasData("TATTOO_PREPARE") ? (int)player.getData("TATTOO_PREPARE") : -1;
                                    var zoneIdAfter = player.hasData("TATTOO_AFTER") ? (int)player.getData("TATTOO_AFTER") : -1;

                                    //One or more things were missed during the tattooing. With some chance, an injury is formed 
                                    if(!freshNeedle || zoneIdPrepared != zoneId || zoneIdAfter != zoneId) {
                                        var rand = new Random();
                                        var injury = rand.NextDouble() <= 0.33;

                                        if(injury) {
                                            DamageController.addPlayerInjury(target, DamageType.Inflammation, getCharacterBodyPartFromZoneId(zoneId), rand.Next(10, 25));
                                        }
                                    }
                                }, TimeSpan.FromMinutes(7.5), false);
                            } else {
                                player.sendBlockNotification("Du hast das Tattoo gestochen doch es ist keine Farbe zu sehen!", "Farbe leer!", NotifactionImages.Tattoo);
                            }
                        } else {
                            player.sendBlockNotification("Das Ziel ist zu weit weg!", "Ziel zu weit weg!", NotifactionImages.Tattoo);
                        }
                    });
                }
            }

            return true;
        }
        private static CharacterBodyPart getCharacterBodyPartFromZoneId(int id) {
            switch(id) {
                case 0:
                    return CharacterBodyPart.Head;
                case 1:
                    return CharacterBodyPart.Torso;
                case 2:
                    return CharacterBodyPart.LeftArm;
                case 3:
                    return CharacterBodyPart.RightArm;
                case 4:
                    return CharacterBodyPart.LeftLeg;
                case 5:
                    return CharacterBodyPart.RightLeg;
                default:
                    return CharacterBodyPart.None;
            }
        }

        private bool onTattooPedInteract(IPlayer player, CollisionShape collisionShape, Dictionary<string, dynamic> data) {
            player.showMenu(getTattooMenu(player, player, true));
            return true;
        }
    }

    public class TattooFunctionality : CompanyFunctionality {
        private ChoiceVPed TattooPed;

        public TattooFunctionality() : base() { }

        public TattooFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "TATTOO_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Tattoos stechen", "Ermöglicht es der Firma Tattos zu stechen");
        }

        public override void onLoad() {
            setPed();

            Company.registerCompanyInteractElement(
                "TATTOO_OPEN_MENU",
                tattooMenuOpenGenerator,
                onOpenTatooMenu
            );
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("TATTOO_OPEN_MENU");
            Company.deleteSetting("TATTOO_PED_POSITION");
            Company.deleteSetting("TATTOO_PED_HEADING");
        }

        private MenuElement tattooMenuOpenGenerator(IPlayer player, IPlayer target) {
            if(!target.getBusy()) {
                if(player.getInventory().hasItem<TattooMachine>(t => true)) {
                    return new ClickMenuItem("Tattoo-Menü öffnen", "Öffne das Tattoo-Menü für diese Person", "", "TATTOO_OPEN_MENU").withData(new Dictionary<string, dynamic> { { "Target", target } });
                } else {
                    return new StaticMenuItem("Tattoo-Menü nicht verfügbar!", "Du hast kein Tattoowierungsset dabei und das Tattoo-Menü ist deshalb nicht verfügbar!", "", MenuItemStyle.yellow);
                }
            } else {
                return new StaticMenuItem("Tattoo-Menü nicht verfügbar!", "Person ist beschäftigt und das Tattoo-Menü ist deshalb nicht verfügbar!", "", MenuItemStyle.yellow);
            }
        }

        private void onOpenTatooMenu(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "TATTOO_OPEN_MENU") {
                var target = (IPlayer)data["Target"];
                TattooController.playerOpenTatooMenu(player, target);
            }
        }


        public override void onLastEmployeeLeaveDuty() {
            setPed();
        }

        public override void onFirstEmployeeEnterDuty() {

        }

        private void setPed() {
            if(TattooPed == null) {
                var posString = Company.getSetting("TATTOO_PED_POSITION");
                if(posString != null) {
                    var pos = posString.FromJson();
                    var heading = float.Parse(Company.getSetting("TATTOO_PED_HEADING"));
                    TattooPed = PedController.createPed("Tattoowierer", "u_m_y_tattoo_01", pos, heading);
                }
            }
        }
    }
}
