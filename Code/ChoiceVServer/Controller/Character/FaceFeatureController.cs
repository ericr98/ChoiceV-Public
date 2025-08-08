using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.SoundSystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Color;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller {
    public class FaceFeatureController : ChoiceVScript {
        private static TimeSpan HAIR_UPDATE_TIME = TimeSpan.FromDays(7);
        private static TimeSpan HAIR_UPDATE_MIN_OFFLINE_TIME = TimeSpan.FromHours(5);
        private static decimal HAIR_TRIMMING_PRICE = 25;
        private static decimal HAIR_SAME_LEVEL_CUT_PRICE = 35;
        private static decimal HAIR_DIFFERENT_LEVEL_CUT_PRICE = 60;
        private static decimal HAIR_MAKEOVER_PRICE = 15;
        private static decimal HAIR_COLOR_PRICE = 110;
        private static decimal HAIR_HIGHLIGHTS_PRICE = 55;
        private static decimal HAIR_OVERLAY_PRICE = 45;

        private static TimeSpan BEARD_UPDATE_TIME = TimeSpan.FromDays(2);
        private static TimeSpan BEARD_UPDATE_MIN_OFFLINE_TIME = TimeSpan.FromHours(5);
        private static decimal BEARD_TRIMMING_PRICE = 20;
        private static decimal BEARD_SAME_LEVEL_CUT_PRICE = 30;
        private static decimal BEARD_DIFFERENT_LEVEL_CUT_PRICE = 40;
        private static decimal BEARD_COLOR_PRICE = 85;

        public static List<confighairstyle> AllHairStyles { get; private set; }
        public static List<confighairoverlay> AllHairOverlays { get; private set; }
        public static List<configbeardstyle> AllBeardStyles { get; private set; }


        private static List<string> BarberChairModels = new List<string> {
            "v_serv_bs_barbchair", "v_serv_bs_barbchair2", "v_serv_bs_barbchair3", "v_serv_bs_barbchair5", "v_ilev_hd_chair"
        };

        public FaceFeatureController() {
            AllHairStyles = new List<confighairstyle>();
            AllHairOverlays = new List<confighairoverlay>();
            AllBeardStyles = new List<configbeardstyle>();

            loadHair();

            EventController.PlayerPastSuccessfullConnectionDelegate += onPlayerPastConnect;
            CharacterController.addPlayerConnectDataSetCallback("HAIR_UPDATE", onPlayerUpdateHair);
            CharacterController.addPlayerConnectDataSetCallback("BEARD_UPDATE", onPlayerUpdateBeard);
            CharacterController.addPlayerConnectDataSetCallback("HAIR_OVERLAY", onPlayerSetOverlay);
            CharacterController.PlayerCharacterCreatedDelegate += onCharacterCreated;

            EventController.addMenuEvent("HAIR_SALON_CUT_HAIR", onPlayerCutHair);
            EventController.addMenuEvent("HAIR_SALON_TRIM_HAIR", onPlayerTrimHair);
            EventController.addMenuEvent("HAIR_SALON_MAKEOVER_HAIR", onPlayerMakeoverHair);
            EventController.addMenuEvent("HAIR_SALON_COLOR_HAIR", onPlayerColorHair);
            EventController.addMenuEvent("HAIR_SALON_COLOR_HAIR_CONFIRM", onPlayerConfirmHairColor);
            EventController.addMenuEvent("HAIR_SALON_COLOR_HIGHLIGHTS", onPlayerColorHighlights);
            EventController.addMenuEvent("HAIR_SALON_COLOR_HIGHLIGHTS_CONFIRM", onPlayerConfirmHighlights);
            EventController.addMenuEvent("HAIR_SALON_OVERLAY", onPlayerOverlay);

            EventController.addMenuEvent("HAIR_SALON_CUT_BEARD", onPlayerCutBeard);
            EventController.addMenuEvent("HAIR_SALON_TRIM_BEARD", onPlayerTrimBeard);
            EventController.addMenuEvent("HAIR_SALON_COLOR_BEARD", onPlayerColorBeard);
            EventController.addMenuEvent("HAIR_SALON_COLOR_BEARD_CONFIRM", onPlayerConfirmBeardColor);

            foreach(var model in BarberChairModels) {
                SittingController.addChairSittingCallback(ChoiceVAPI.Hash(model).ToString(), onBarberChairSitting);
            }

            InteractionController.addKeyInteractionCallback(new ConditionalKeyInteractionCallback((p) => onBarberChairSitting(p, true), (p) => {
                foreach(var model in BarberChairModels) {
                    if(SittingController.isPlayerSittingOnChair(p, ChoiceVAPI.Hash(model).ToString())) {
                        return true;
                    }
                }

                return false;
            }));

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.PlayerStyle,
                    "Haare einstellen",
                    generateSupportHairMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_HAIRSTYLE", createNewHairStyle);
            EventController.addMenuEvent("SUPPORT_DELETE_HAIRSTYLE", deleteHairStyle);
            EventController.addMenuEvent("SUPPORT_ADD_HAIRSTYLE_SUCCESSOR", addHairStyleSuccessor);
            EventController.addMenuEvent("SUPPORT_DELETE_HAIRSTLYE_SUCCESOR", deleteHairStyleSuccessor);

            EventController.addMenuEvent("SUPPORT_ADD_HAIRSTYLE_MAKEOVER", onSupportAddHairstyleMakeover);

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.PlayerStyle,
                    "Bärte einstellen",
                    generateBeardMenu
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_BEARDSTYLE", createNewBeardStyle);
            EventController.addMenuEvent("SUPPORT_DELETE_BEARDSTYLE", deleteBeardStyle);
            EventController.addMenuEvent("SUPPORT_ADD_BEARDSTYLE_SUCCESSOR", addBeardStyleSuccessor);
            EventController.addMenuEvent("SUPPORT_DELETE_BEARDSTLYE_SUCCESOR", deleteBeardStyleSuccessor);

            EventController.addMenuEvent("HYGIENE_DRINK_WATER", onHygieneDrinkWater);
        }

        public static void openHygineMenu(IPlayer player) {
            var data = player.getCharacterData();

            var menu = new Menu("Hygieneoptionen", "Was möchtest du tun?", resetPlayerStyle);

            if(data.Gender == 'M') {
                var beardMenu = getBeardStyleMenu(player, false);
                menu.addMenuItem(new MenuMenuItem(beardMenu.Name, beardMenu));
            } else {
                var makeoverMenu = getMakoverMenu(player, false);
                if(makeoverMenu != null) {
                    menu.addMenuItem(new MenuMenuItem(makeoverMenu.Name, makeoverMenu));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Umstylen nicht möglich", "Du kannst deine Haare nicht umstylen, für diese Frisur gibt es keine Alternativen!", "Keine Alternativen", MenuItemStyle.yellow));
                }
            }

            menu.addMenuItem(new ClickMenuItem("Wasser trinken", "Trinke Wasser aus dem Waschbecken", "", "HYGIENE_DRINK_WATER"));

            player.showMenu(menu);
        }

        public static confighairstyle getHelmetReplacementHair(IPlayer player) {
            var hair = player.getCharacterData().Style.hairStyle;
            var gender = player.getCharacterData().Gender.ToString();
            return AllHairStyles.FirstOrDefault(h => h.gender == gender && h.gtaId == hair)?.helmetReplacementNavigation;
        }

        private bool onHygieneDrinkWater(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("DRINK_WATER");

            AnimationController.animationTask(player, anim, () => {
                var plData = player.getCharacterData();
                plData.Thirst = 100;
                player.updateHud();
            });
            return true;
        }

        private void loadHair() {
            using(var db = new ChoiceVDb()) {
                AllHairStyles = db.confighairstyles.Include(h => h.successors).ThenInclude(s => s.successors).Include(h => h.confighair2s).Include(h => h.confighair1s).Include(h => h.helmetReplacementNavigation).ToList();
                AllHairOverlays = db.confighairoverlays.OrderBy(c => c.displayName).ToList();
                AllBeardStyles = db.configbeardstyles.Include(h => h.successors).ThenInclude(s => s.successors).ToList();
            }
        }

        //Spieler hat Glatze/Haartransplant und es wachsen keine Haare
        public static void makePlayerNoHairGrowth(IPlayer player) {
            player.setPermanentData("NO_HAIR_GROWTH", "true");
        }

        private void onBarberChairSitting(IPlayer player, bool sittingDown) {
            if(sittingDown) {
                var menu = new Menu("Friseurauswahl", "Wähle die gewünschte Dienstleistung aus", resetPlayerStyle);
                var hairMenu = getHairStyleMenu(player);
                menu.addMenuItem(new MenuMenuItem(hairMenu.Name, hairMenu));
                if(player.getCharacterData().Gender == 'M') {
                    var beardMenu = getBeardStyleMenu(player, true);
                    menu.addMenuItem(new MenuMenuItem(beardMenu.Name, beardMenu));
                }
                player.showMenu(menu, false, true);
            } else {
                player.closeMenu();
            }
        }

        private static Menu getHairStyleMenu(IPlayer player) {
            var data = player.getCharacterData();
            var update = ((string)player.getData("HAIR_UPDATE")).FromJson<DateTime>();
            var gender = data.Gender;
            var cash = player.getCash();

            var menu = new Menu("Haare ändern", "Was möchtest du tun?");

            if(!player.hasData("NO_HAIR_GROWTH") && !player.hasState(PlayerStates.InTerminal)) {
                var hairLengthPercent = 0f;
                var lengthTemp = update - DateTime.Now;
                if(lengthTemp <= TimeSpan.Zero) {
                    hairLengthPercent = 100;
                } else {
                    hairLengthPercent = Math.Abs((float)Math.Round((100 - ((float)(lengthTemp / HAIR_UPDATE_TIME)) * 100f), 0));
                }

                menu.addMenuItem(new StaticMenuItem("Aktuelle Haarlänge", "Bei 100% werden deine Haar sichtbar länger!", $"{hairLengthPercent}%"));
                if(cash > HAIR_TRIMMING_PRICE) {
                    menu.addMenuItem(new ClickMenuItem("Auf gleiche Länge trimmen", "Lasse deine Haare wieder auf dieselbe Frisur wie gerade trimmen", $"${HAIR_TRIMMING_PRICE}", "HAIR_SALON_TRIM_HAIR").needsConfirmation("Haare trimmen?", $"Haare wirklich für ${HAIR_TRIMMING_PRICE} trimmen?"));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Auf gleiche Länge trimmen", "Lasse deine Haare wieder auf dieselbe Frisur wie gerade trimmen", $"${HAIR_TRIMMING_PRICE} (zu teuer)", MenuItemStyle.red));
                }

                var makeOverMenu = getMakoverMenu(player, true);
                if(makeOverMenu != null) {
                    menu.addMenuItem(new MenuMenuItem(makeOverMenu.Name, makeOverMenu));
                }

                var styleMenu = new Menu("Neue Frisur wählen", "Wähle eine neue Frisur aus");
                var currentHairCut = getConfigHairStyleByGtaId(data.Style.hairStyle, gender);
                var viableHairCuts = AllHairStyles.Where(h => h.level <= currentHairCut.level && h != currentHairCut && h.gender == gender.ToString()).OrderByDescending(h => h.level).ToList();
                styleMenu.addMenuItem(new StaticMenuItem("Verfügbare Frisuren", $"Aufgrund deiner Haarlänge hast du {viableHairCuts.Count} Frisuren zur Auswahl", $"{viableHairCuts.Count}"));
                foreach(var hairCut in viableHairCuts) {
                    var price = HAIR_SAME_LEVEL_CUT_PRICE;
                    if(hairCut.level != currentHairCut.level) {
                        price = HAIR_DIFFERENT_LEVEL_CUT_PRICE;
                    }

                    if(cash >= price) {
                        styleMenu.addMenuItem(new ClickMenuItem(hairCut.name, $"Die Frisur {hairCut.name} für ${price} schneiden lassen", $"${price}", "HAIR_SALON_CUT_HAIR", MenuItemStyle.normal, true).needsConfirmation($"{hairCut.name} schneiden?", $"{hairCut.name} für ${price} schneiden lassen?").withData(new Dictionary<string, dynamic> { { "HairStyle", hairCut }, { "Price", price } }));
                    } else {
                        styleMenu.addMenuItem(new StaticMenuItem(hairCut.name, $"Die Frisur {hairCut.name} für ${price} schneiden lassen", $"${price} (zu teuer)", MenuItemStyle.red));
                    }
                }

                menu.addMenuItem(new MenuMenuItem(styleMenu.Name, styleMenu));
            }

            if(cash >= HAIR_COLOR_PRICE) {
                menu.addMenuItem(new ClickMenuItem("Haare färben", "Lasse deine Haare färben", $"${HAIR_COLOR_PRICE}", "HAIR_SALON_COLOR_HAIR"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Haare färben", "Lasse deine Haare färben", $"${HAIR_COLOR_PRICE} (zu teuer)", MenuItemStyle.red));
            }

            if(cash >= HAIR_HIGHLIGHTS_PRICE) {
                menu.addMenuItem(new ClickMenuItem("Haarhighlights färben", "Lasse dir Haarhighlights setzen", $"${HAIR_HIGHLIGHTS_PRICE}", "HAIR_SALON_COLOR_HIGHLIGHTS"));
            } else {
                menu.addMenuItem(new StaticMenuItem("Haarhighlights färben", "Lasse dir Haarhighlights setzen", $"${HAIR_HIGHLIGHTS_PRICE} (zu teuer)", MenuItemStyle.red));
            }

            if(cash >= HAIR_OVERLAY_PRICE) {
                var sprayMenu = new Menu("Haaransatz anbringen", "Lasse dir ein Haaransatz anbringen");
                if(player.hasData("HAIR_OVERLAY")) {
                    sprayMenu.addMenuItem(new ClickMenuItem("Entfernen", $"Entferne den aktuellen Haaransatz", $"${HAIR_OVERLAY_PRICE}", "HAIR_SALON_OVERLAY", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Overlay", null } }));
                }
                foreach(var overlay in AllHairOverlays) {
                    sprayMenu.addMenuItem(new ClickMenuItem(overlay.displayName, $"Bringe einen {overlay.displayName} an", $"${HAIR_OVERLAY_PRICE}", "HAIR_SALON_OVERLAY", MenuItemStyle.normal, true).withData(new Dictionary<string, dynamic> { { "Overlay", overlay } }));
                }
                menu.addMenuItem(new MenuMenuItem(sprayMenu.Name, sprayMenu));
            } else {
                menu.addMenuItem(new StaticMenuItem("Haaransatz anbringen", "Lasse dir ein Haaransatz anbringen", $"${HAIR_OVERLAY_PRICE} (zu teuer)", MenuItemStyle.red));
            }

            return menu;
        }

        private static Menu getMakoverMenu(IPlayer player, bool buy) {
            var style = player.getCharacterData().Style;
            var hairStyle = AllHairStyles.FirstOrDefault(h => h.gender == style.gender && h.gtaId == style.hairStyle);

            var makeOvers = getMakeOverList(hairStyle);

            if(makeOvers.Count > 0) {
                var makeoverMenu = new Menu("Frisur umstylen", "Style die Frisur um", resetPlayerStyle);
                foreach(var makeover in makeOvers) {
                    var data = new Dictionary<string, dynamic> { { "Makeover", makeover }, { "Buy", buy } };
                    if(buy) {
                        if(player.getCharacterData().Cash > HAIR_MAKEOVER_PRICE) {
                            makeoverMenu.addMenuItem(new ClickMenuItem(makeover.name, $"Style deine Haare um, um eine {makeover.name} Frisur zu erhalten", $"${HAIR_MAKEOVER_PRICE}", "HAIR_SALON_MAKEOVER_HAIR", MenuItemStyle.normal, true).withData(data));
                        } else {
                            makeoverMenu.addMenuItem(new StaticMenuItem(makeover.name, $"Style deine Haare um, umeine {makeover.name} Frisur zu erhalten", $"${HAIR_MAKEOVER_PRICE} (zu teuer)", MenuItemStyle.yellow));
                        }
                    } else {
                        makeoverMenu.addMenuItem(new ClickMenuItem(makeover.name, $"Style deine Haare um, um eine {makeover.name} Frisur zu erhalten", "", "HAIR_SALON_MAKEOVER_HAIR", MenuItemStyle.normal, true).withData(data));
                    }
                }
                return makeoverMenu;
            } else {
                return null;
            }
        }

        private static List<confighairstyle> getMakeOverList(confighairstyle hairStyle) {
            return getMakeOverListRec(hairStyle.confighair1s.Concat(hairStyle.confighair2s).ToList(), new List<confighairstyle> { hairStyle }).Distinct().ToList();


            static List<confighairstyle> getMakeOverListRec(List<confighairstyle> makeOverStyles, List<confighairstyle> already) {
                var list = new List<confighairstyle>();
                foreach(var makeover in makeOverStyles.Except(already)) {
                    already.Add(makeover);
                    list.Add(makeover);

                    list = list.Concat(getMakeOverListRec(makeover.confighair1s.Concat(makeover.confighair2s).ToList(), already)).ToList();
                }

                return list;
            }
        }

        private static Menu getBeardStyleMenu(IPlayer player, bool buy) {
            var update = ((string)player.getData("BEARD_UPDATE")).FromJson<DateTime>();
            var cash = player.getCash();
            var data = player.getCharacterData();

            var menData = new Dictionary<string, dynamic> { { "Buy", buy } };

            var menu = new Menu("Bart ändern", "Was möchtest du tun?");

            var hairLengthPercent = 0f;
            var lengthTemp = update - DateTime.Now;
            if(lengthTemp <= TimeSpan.Zero) {
                hairLengthPercent = 100;
            } else {
                hairLengthPercent = (float)Math.Round((100 - ((float)(lengthTemp / BEARD_UPDATE_TIME)) * 100f), 0);
            }

            menu.addMenuItem(new StaticMenuItem("Aktuelle Bartlänge", "Bei 100% wird dein Bart sichtbar länger!", $"{hairLengthPercent}%"));
            if(buy) {
                if(cash > HAIR_TRIMMING_PRICE) {
                    menu.addMenuItem(new ClickMenuItem("Auf gleiche Länge trimmen", "Lasse deinen Bart wieder auf den selben Stil wie gerade trimmen", $"${BEARD_TRIMMING_PRICE}", "HAIR_SALON_TRIM_BEARD").withData(menData).needsConfirmation("Bart trimmen?", $"Bart wirklich für ${HAIR_TRIMMING_PRICE} trimmen?"));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Auf gleiche Länge trimmen", "Lasse deinen Bart wieder auf den selben Stil wie gerade trimmen", $"${BEARD_TRIMMING_PRICE} (zu teuer)", MenuItemStyle.red));
                }
            } else {
                menu.addMenuItem(new ClickMenuItem("Auf gleiche Länge trimmen", "Trimme deinen Bart wieder auf den selben Stil wie gerade", "", "HAIR_SALON_TRIM_BEARD").withData(menData).needsConfirmation("Bart trimmen?", $"Bart wirklich trimmen?"));
            }

            var styleMenu = new Menu("Neuen Bartstil wählen", "Wähle einen neuen Bartstil aus");
            var currentBeard = getConfigBeardStyleByGtaId(data.Style.overlay_1);
            var viableBeardsCuts = AllBeardStyles.Where(h => h.level < currentBeard.level && h != currentBeard).OrderByDescending(h => h.level).ToList();
            styleMenu.addMenuItem(new StaticMenuItem("Verfügbare Bärte", $"Aufgrund deiner Bartlänge hast du {viableBeardsCuts.Count}/{AllBeardStyles.Count} Bartstile zur Auswahl", $"{viableBeardsCuts.Count}/{AllBeardStyles.Count}"));
            foreach(var beard in viableBeardsCuts) {
                if(buy) {
                    var price = BEARD_SAME_LEVEL_CUT_PRICE;
                    if(beard.level != currentBeard.level) {
                        price = BEARD_DIFFERENT_LEVEL_CUT_PRICE;
                    }

                    if(cash >= price) {
                        styleMenu.addMenuItem(new ClickMenuItem(beard.name, $"Den Bartstil {beard.name} für ${price} schneiden lassen", $"${price}", "HAIR_SALON_CUT_BEARD", MenuItemStyle.normal, true).needsConfirmation($"{beard.name} schneiden?", $"{beard.name} für ${price} schneiden lassen?").withData(new Dictionary<string, dynamic> { { "BeardStyle", beard }, { "Price", price }, { "Buy", buy } }));
                    } else {
                        styleMenu.addMenuItem(new StaticMenuItem(beard.name, $"Den Bartstil {beard.name} für ${price} schneiden lassen", $"${price} (zu teuer)", MenuItemStyle.red));
                    }
                } else {
                    styleMenu.addMenuItem(new ClickMenuItem(beard.name, $"Der Bartstil {beard.name} schneiden", "", "HAIR_SALON_CUT_BEARD", MenuItemStyle.normal, true).needsConfirmation($"{beard.name} schneiden?", $"{beard.name} wirklich schneiden?").withData(new Dictionary<string, dynamic> { { "BeardStyle", beard }, { "Price", 0 }, { "Buy", buy } }));
                }
            }

            menu.addMenuItem(new MenuMenuItem(styleMenu.Name, styleMenu));

            if(buy) {
                if(cash > HAIR_COLOR_PRICE) {
                    menu.addMenuItem(new ClickMenuItem("Bart färben", "Lasse deinen Bart färben", $"${BEARD_COLOR_PRICE}", "HAIR_SALON_COLOR_BEARD"));
                } else {
                    menu.addMenuItem(new StaticMenuItem("Bart färben", "Lasse deinen Bart färben", $"${BEARD_COLOR_PRICE} (zu teuer)", MenuItemStyle.red));
                }
            }

            return menu;
        }

        public static void resetPlayerStyle(IPlayer player) {
            player.setStyle(player.getCharacterData().Style);

            resetPlayerOverlay(player);
            Logger.logTrace(LogCategory.Player, LogActionType.Created, player, $"reset player style");
        }

        private void onCharacterCreated(IPlayer player, character character) {
            resetHairLength(player, false);
        }

        private void onPlayerPastConnect(IPlayer player, character character) {
            if(!player.hasData("HAIR_UPDATE") && !player.hasData("NO_HAIR_GROWTH")) {
                resetHairLength(player);
            }

            if(player.getCharacterData().Gender == 'M' && !player.hasData("BEARD_UPDATE")) {
                resetBeardLength(player);
            }
        }

        #region Beard

        private static void resetBeardLength(IPlayer player) {
            player.setPermanentData("BEARD_UPDATE", (DateTime.Now + BEARD_UPDATE_TIME).ToJson());
        }

        private void onPlayerUpdateBeard(IPlayer player, character character, characterdatum data) {
            if(DateTime.Now - character.lastLogout > BEARD_UPDATE_MIN_OFFLINE_TIME && data.value.FromJson<DateTime>() < DateTime.Now) {
                var charData = player.getCharacterData();
                var gender = charData.Gender;
                var gtaId = charData.Style.overlay_1;

                var configBeardStyle = getConfigBeardStyleByGtaId(gtaId);
                if(configBeardStyle != null && configBeardStyle.successors.Count > 0) {
                    var randomId = new Random().Next(configBeardStyle.successors.Count);
                    var newCfg = configBeardStyle.successors.ToList()[randomId];

                    var style = player.getCharacterData().Style;
                    style.overlay_1 = newCfg.gtaId;
                    player.setStyle(style);
                    using(var db = new ChoiceVDb()) {
                        var charId = player.getCharacterId();
                        var dbStyle = db.characterstyles.Find(charId);
                        if(dbStyle != null) {
                            dbStyle.overlay_1 = newCfg.gtaId;
                            db.SaveChanges();
                        }
                    }
                    resetBeardLength(player);
                    player.sendNotification(NotifactionTypes.Warning, "Dein Bart ist sichtbar länger geworden!", "Bart länger geworden!", NotifactionImages.Scissors);

                    Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"players beard grew from gtaId: {configBeardStyle.gtaId} to gtaId: {newCfg.gtaId}");
                }
            }
        }

        private bool onPlayerCutBeard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(menuItemCefEvent.action == "changed") {
                var configStyle = (configbeardstyle)data["BeardStyle"];
                var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
                style.overlay_1 = configStyle.gtaId;
                player.setStyle(style);
            } else {
                var configStyle = (configbeardstyle)data["BeardStyle"];
                var price = (decimal)data["Price"];
                var buy = (bool)data["Buy"];

                //Works, because second operant is not eveluated if first one is true
                if(!buy || player.removeCash(price)) {
                    var style = player.getCharacterData().Style;
                    style.overlay_1 = configStyle.gtaId;
                    player.setStyle(style);
                    using(var db = new ChoiceVDb()) {
                        var charId = player.getCharacterId();
                        var dbStyle = db.characterstyles.Find(charId);
                        if(dbStyle != null) {
                            dbStyle.overlay_1 = configStyle.gtaId;
                            db.SaveChanges();
                        }
                    }

                    if(buy) {
                        player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich eine/einen {configStyle.name} schneiden lassen!", "Bart geändert", NotifactionImages.Scissors);
                        SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.ManualShaving, 0.4f, "mp3");

                        Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"players bought new beard with gtaId {configStyle.gtaId}");
                    } else {
                        SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.ManualShaving, 0.4f, "mp3");
                        var anim = AnimationController.getAnimationByName("SHAVE_BEARD");
                        AnimationController.animationTask(player, anim, () => {
                            var rand = new Random();
                            if(rand.NextDouble() <= 0.075) {
                                player.getCharacterData().CharacterDamage.addInjury(player, DamageType.Sting, CharacterBodyPart.Head, rand.Next(1, 5));
                                player.sendNotification(NotifactionTypes.Warning, "Du hast dich beim Rasieren geschnitten!", "Geschnitten!", NotifactionImages.Scissors);
                            }

                            player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich eine/einen {configStyle.name} geschnitten!", "Bart geändert", NotifactionImages.Scissors);

                            Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"players shaved own beard to gtaId {configStyle.gtaId}");
                        });
                    }
                }
            }

            return true;
        }

        private bool onPlayerTrimBeard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var buy = data["Buy"];
            if(player.removeCash(BEARD_TRIMMING_PRICE)) {
                if(buy) {
                    resetBeardLength(player);
                    SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.TrimmerCut, 0.3f, "mp3");
                    player.sendNotification(NotifactionTypes.Success, "Dein Bart wurde erfolgreich getrimmt!", "Bart getrimmt!", NotifactionImages.Scissors);
                } else {
                    SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.ManualShaving, 0.4f, "mp3");
                    var anim = AnimationController.getAnimationByName("SHAVE_BEARD");
                    AnimationController.animationTask(player, anim, () => {
                        resetBeardLength(player);

                        var rand = new Random();
                        if(rand.NextDouble() <= 0.075) {
                            player.getCharacterData().CharacterDamage.addInjury(player, DamageType.Sting, CharacterBodyPart.Head, rand.Next(1, 5));
                            player.sendNotification(NotifactionTypes.Warning, "Du hast dich beim Rasieren geschnitten!", "Geschnitten!", NotifactionImages.Scissors);
                        }

                        player.sendNotification(NotifactionTypes.Success, "Dein Bart wurde erfolgreich getrimmt!", "Bart getrimmt!", NotifactionImages.Scissors);
                    });
                }
            }

            return true;
        }

        private bool onPlayerColorBeard(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.closeMenu();
            player.showColorPicker(new ColorPicker(ColorPickerType.GithubPicker, onColorPickerBeardCallback, PlayerHairColors[0], PlayerHairColors.ToArray()).withData(data));
            return true;
        }

        private void setPlayerBeardColor(IPlayer player, int beardColor) {
            var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
            style.overlaycolor_1 = beardColor;
            player.setStyle(style);
        }

        private bool onColorPickerBeardCallback(IPlayer player, Dictionary<string, dynamic> data, Rgba colorRgb, string colorHex, ColorPickerActions action) {
            var color = PlayerHairColors.IndexOf(colorHex);
            if(action == ColorPickerActions.Select) {
                setPlayerBeardColor(player, color);
            } else {
                player.showMenu(MenuController.getConfirmationMenu("Aktuelle Farbe kaufen?", $"Aktuelle Farbe für ${BEARD_COLOR_PRICE} kaufen?", "HAIR_SALON_COLOR_BEARD_CONFIRM", new Dictionary<string, dynamic> { { "Color", color } }, resetPlayerStyle));
            }

            return true;
        }

        private bool onPlayerConfirmBeardColor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var color = data["Color"];

            if(player.removeCash(BEARD_COLOR_PRICE)) {
                var style = player.getCharacterData().Style;
                style.overlaycolor_1 = color;
                player.setStyle(style);
                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var dbStyle = db.characterstyles.Find(charId);
                    if(dbStyle != null) {
                        dbStyle.overlaycolor_1 = color;
                        db.SaveChanges();
                    }
                }

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"players changed beard color to color {color}");
                player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich deinen Bart färben lassen!", "Bart gefärbt", NotifactionImages.Scissors);
            }

            return true;
        }

        private static configbeardstyle getConfigBeardStyleByGtaId(int gtaId) {
            return AllBeardStyles.FirstOrDefault(b => b.gtaId == gtaId);
        }

        #endregion

        #region HairStyle

        private void onPlayerUpdateHair(IPlayer player, character character, characterdatum data) {
            if(player.hasData("NO_HAIR_GROWTH")) {
                return;
            }

            if(DateTime.Now - character.lastLogout > HAIR_UPDATE_MIN_OFFLINE_TIME && data.value.FromJson<DateTime>() < DateTime.Now) {
                var charData = player.getCharacterData();
                var gender = charData.Gender;
                var gtaId = charData.Style.hairStyle;

                var configHairStyle = getConfigHairStyleByGtaId(gtaId, gender);
                if(configHairStyle != null && configHairStyle.successors.Count > 0) {
                    var rnd = new Random();
                    var count = configHairStyle.successors.Count;
                    var newCfg = configHairStyle.successors.ToList()[rnd.Next(0, count)];

                    if(newCfg != null) {
                        var style = player.getCharacterData().Style;
                        style.hairStyle = newCfg.gtaId;
                        player.setStyle(style);
                        using(var db = new ChoiceVDb()) {
                            var charId = player.getCharacterId();
                            var dbStyle = db.characterstyles.Find(charId);
                            if(dbStyle != null) {
                                dbStyle.hairStyle = newCfg.gtaId;
                                db.SaveChanges();
                            }
                        }
                        resetHairLength(player, false);

                        player.sendNotification(NotifactionTypes.Warning, "Deine Haare sind sichtbar länger geworden!", "Haare länger geworden!", NotifactionImages.Scissors);

                        Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player hair grew from gtaId {gtaId} to gtaId {newCfg.gtaId}");
                    }
                }
            }
        }

        private bool onPlayerCutHair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(menuItemCefEvent.action == "changed") {
                var configStyle = (confighairstyle)data["HairStyle"];
                var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
                style.hairStyle = configStyle.gtaId;
                player.setStyle(style);
            } else {
                var configStyle = (confighairstyle)data["HairStyle"];
                var price = (decimal)data["Price"];

                if(player.removeCash(price)) {
                    var style = player.getCharacterData().Style;
                    style.hairStyle = configStyle.gtaId;
                    player.setStyle(style);
                    using(var db = new ChoiceVDb()) {
                        var charId = player.getCharacterId();
                        var dbStyle = db.characterstyles.Find(charId);
                        if(dbStyle != null) {
                            dbStyle.hairStyle = configStyle.gtaId;
                            db.SaveChanges();
                        }
                    }

                    resetHairLength(player);

                    player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich eine/einen {configStyle.name} schneiden lassen!", "Frisur geändert", NotifactionImages.Scissors);
                    Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player cut hair to gtaId {configStyle.gtaId}");
                }
            }

            return true;
        }

        private bool onPlayerTrimHair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.removeCash(HAIR_TRIMMING_PRICE)) {
                resetHairLength(player);
                player.sendNotification(NotifactionTypes.Success, $"Du hast dir deine Haare trimmen lassen!", "Haare getrimmt", NotifactionImages.Scissors);
            }

            return true;
        }

        private bool onPlayerMakeoverHair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var buy = (bool)data["Buy"];
            var makeover = (confighairstyle)data["Makeover"];

            if(menuItemCefEvent.action == "changed") {
                var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
                style.hairStyle = makeover.gtaId;
                player.setStyle(style);
            } else {
                var style = player.getCharacterData().Style;
                if(buy) {
                    if(player.removeCash(HAIR_MAKEOVER_PRICE)) {
                        style.hairStyle = makeover.gtaId;
                        player.setStyle(style);
                        SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.ScissorsCut, 1, "mp3");

                        player.sendNotification(NotifactionTypes.Success, $"Du hast deine Haare umstylen lassen!", "Haare umgestylt", NotifactionImages.Scissors);
                        Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player bought hair makeover to gtaId {makeover.gtaId}");
                    }
                } else {
                    style.hairStyle = makeover.gtaId;
                    player.setStyle(style);

                    var anim = AnimationController.getAnimationByName("EQUIP_HAT");
                    player.playAnimation(anim);
                    player.sendNotification(NotifactionTypes.Success, $"Du hast deine Haare umgestylt!", "Haare umgestylt", NotifactionImages.Scissors);

                    Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player restyled own hair makeover to gtaId {makeover.gtaId}");
                }

                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var dbStyle = db.characterstyles.Find(charId);
                    if(dbStyle != null) {
                        dbStyle.hairStyle = makeover.gtaId;
                        db.SaveChanges();
                    }
                }
            }

            return true;
        }


        private static confighairstyle getConfigHairStyleByGtaId(int gtaId, char gender) {
            return AllHairStyles.FirstOrDefault(h => h.gtaId == gtaId && h.gender == gender.ToString());
        }

        public static void resetHairLength(IPlayer player, bool sound = true) {
            player.setPermanentData("HAIR_UPDATE", (DateTime.Now + HAIR_UPDATE_TIME).ToJson());
            if(sound) {
                SoundController.playSoundAtCoords(player.Position, 2, SoundController.Sounds.ScissorsCut, 1, "mp3");
            }
        }

        #endregion

        #region Hair Coloring

        private void setPlayerHairColor(IPlayer player, int hairColor) {
            var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
            style.hairColor = hairColor;
            player.setStyle(style);

            resetPlayerOverlay(player);
        }

        private bool onPlayerColorHair(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.closeMenu();
            player.showColorPicker(new ColorPicker(ColorPickerType.GithubPicker, onColorHairPickerCallback, PlayerHairColors[0], PlayerHairColors.ToArray()).withData(data));
            return true;
        }

        private bool onColorHairPickerCallback(IPlayer player, Dictionary<string, dynamic> data, Rgba colorRgb, string colorHex, ColorPickerActions action) {
            var color = PlayerHairColors.IndexOf(colorHex);
            if(action == ColorPickerActions.Select) {
                setPlayerHairColor(player, color);
            } else {
                player.showMenu(MenuController.getConfirmationMenu("Aktuelle Farbe kaufen?", $"Aktuelle Farbe für ${HAIR_COLOR_PRICE} kaufen?", "HAIR_SALON_COLOR_HAIR_CONFIRM", new Dictionary<string, dynamic> { { "Color", color } }, resetPlayerStyle));
            }

            return true;
        }

        private bool onPlayerConfirmHairColor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var color = data["Color"];

            if(player.removeCash(HAIR_COLOR_PRICE)) {
                var style = player.getCharacterData().Style;
                style.hairColor = color;
                style.hairHighlight = color;
                player.setStyle(style);

                resetPlayerOverlay(player);
                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var dbStyle = db.characterstyles.Find(charId);
                    if(dbStyle != null) {
                        dbStyle.hairColor = color;
                        dbStyle.hairHighlight = color;
                        db.SaveChanges();
                    }
                }
                player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich deine Haare färben lassen!", "Frisur gefärbt", NotifactionImages.Scissors);

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player colored hair to color {color} and highlightcolor {color}");
            }

            return true;
        }

        private void setPlayerHighlightColor(IPlayer player, int hairHighlight) {
            var style = player.getCharacterData().Style.ToJson().FromJson<characterstyle>();
            style.hairHighlight = hairHighlight;
            player.setStyle(style);
        }


        private bool onPlayerColorHighlights(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            player.closeMenu();
            player.showColorPicker(new ColorPicker(ColorPickerType.GithubPicker, onColorPickerHighlightCallback, PlayerHairColors[0], PlayerHairColors.ToArray()).withData(data));
            return true;
        }

        private bool onColorPickerHighlightCallback(IPlayer player, Dictionary<string, dynamic> data, Rgba colorRgb, string colorHex, ColorPickerActions action) {
            var color = PlayerHairColors.IndexOf(colorHex);
            if(action == ColorPickerActions.Select) {
                setPlayerHighlightColor(player, color);
            } else {
                player.showMenu(MenuController.getConfirmationMenu("Aktuelles Highlight kaufen?", $"Aktuelles Highlight für ${HAIR_HIGHLIGHTS_PRICE} kaufen?", "HAIR_SALON_COLOR_HIGHLIGHTS_CONFIRM", new Dictionary<string, dynamic> { { "Color", color } }, resetPlayerStyle));
            }

            return true;
        }

        private bool onPlayerConfirmHighlights(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var color = data["Color"];

            if(player.removeCash(HAIR_HIGHLIGHTS_PRICE)) {
                var style = player.getCharacterData().Style;
                style.hairHighlight = color;
                player.setStyle(style);
                using(var db = new ChoiceVDb()) {
                    var charId = player.getCharacterId();
                    var dbStyle = db.characterstyles.Find(charId);
                    if(dbStyle != null) {
                        dbStyle.hairHighlight = color;
                        db.SaveChanges();
                    }
                }

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player colored hair makeover to color {color} and highlightcolor {color}");
                player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich deine Haare färben lassen!", "Frisur gefärbt", NotifactionImages.Scissors);
            }

            return true;
        }

        #endregion

        #region Hair Overlay

        private static void resetPlayerOverlay(IPlayer player) {
            if(player.hasData("HAIR_OVERLAY")) {
                var data = (string)player.getData("HAIR_OVERLAY");
                var arr = data.Split("#");
                player.resetOverlayType("hair_overlay");
                player.setOverlay("hair_overlay", arr[0], arr[1]);
            } else {
                player.resetOverlayType("hair_overlay");
            }
        }

        private bool onPlayerOverlay(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var configOverlay = (confighairoverlay)data["Overlay"];
            if(menuItemCefEvent.action == "changed") {
                player.resetOverlayType("hair_overlay");
                if(configOverlay != null) {
                    player.setOverlay("hair_overlay", configOverlay.collection, configOverlay.hash);
                }
            } else {
                if(player.removeCash(HAIR_OVERLAY_PRICE)) {
                    player.resetOverlayType("hair_overlay");
                    if(configOverlay != null) {
                        savePlayerHairOverlay(player, configOverlay);
                        player.setOverlay("hair_overlay", configOverlay.collection, configOverlay.hash);
                        SoundController.playSoundAtCoords(player.Position, 2.5f, SoundController.Sounds.Spray, 0.6f, "mp3");
                        player.sendNotification(NotifactionTypes.Success, $"Dein neuer Ansatz: {configOverlay.displayName} wurde erfolgreich aufgesprüht!", "Haaransatz geändert", NotifactionImages.Scissors);

                        Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player bought hair overlay with id {configOverlay.id}");
                    } else {
                        player.resetPermantData("HAIR_OVERLAY");
                        player.sendNotification(NotifactionTypes.Success, $"Du hast dir erfolgreich deinen Haaransatz entfernen lassen!", "Haaransatz geändert", NotifactionImages.Scissors);

                        Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"player removed hair overlay");
                    }
                }
            }

            return true;
        }

        public static void savePlayerHairOverlay(IPlayer player, confighairoverlay configOverlay) {
            player.setPermanentData("HAIR_OVERLAY", configOverlay.collection + "#" + configOverlay.hash);
            player.setOverlay("hair_overlay", configOverlay.collection, configOverlay.hash);
        }

        private void onPlayerSetOverlay(IPlayer player, character character, characterdatum data) {
            var arr = data.value.Split("#");
            player.resetOverlayType("hair_overlay");
            player.setOverlay("hair_overlay", arr[0], arr[1]);
        }

        #endregion

        #region Support

        private Menu generateSupportHairMenu(IPlayer player) {
            var menu = new Menu("Haare einstellen", "Stelle Haare und ihre Verbindung ein");

            var genderList = new char[] { 'M', 'F' };

            foreach(var gender in genderList) {
                var data = new Dictionary<string, dynamic> { { "Gender", gender } };
                var subMenu = new Menu(gender == 'M' ? "Männliche Frisuren" : "Weibliche Frisuren", "Frisuren editieren");
                var createMenu = new Menu("Hairstyle erstellen", "Erstelle einen neuen Hairstyle");
                createMenu.addMenuItem(new InputMenuItem("Name", "Der Anzeigename der Frisur", "", ""));
                createMenu.addMenuItem(new InputMenuItem("GTA-ID", "Die GTA Id der Frisur", "", ""));
                createMenu.addMenuItem(new InputMenuItem("Level", "Die Haarlänge der Frisur", "", ""));
                createMenu.addMenuItem(new MenuStatsMenuItem("Frisur erstellen", "Erstelle die Frisur", "SUPPORT_CREATE_HAIRSTYLE", MenuItemStyle.green).withData(data).needsConfirmation("Frisur erstellen?", "Frisur wirklich erstellen?"));
                subMenu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

                foreach(var already in AllHairStyles.Where(h => h.gender == gender.ToString()).OrderBy(h => h.level).ToList()) {
                    var subSubMenu = new Menu($"{already.gtaId}: {already.name}", "Editiere den Haarstyle");
                    subSubMenu.addMenuItem(new StaticMenuItem("GTA-ID", $"Die GTA-ID der Frisur ist: {already.gtaId}", $"{already.gtaId}"));
                    subSubMenu.addMenuItem(new StaticMenuItem("Level", $"Das Level der Frisur ist: {already.level}", $"{already.level}"));

                    var addData = new Dictionary<string, dynamic> { { "Gender", gender }, { "Predecessor", already } };
                    var successorMenu = new Menu("Nachfolger", "Editiere die Nachfolger");
                    successorMenu.addMenuItem(new InputMenuItem("Nachfolger hinzufügen", "Füge einen Nachfolger mit der GTA ID hinzu", "GTA-ID", "SUPPORT_ADD_HAIRSTYLE_SUCCESSOR", MenuItemStyle.green).withData(addData).needsConfirmation("Nachfolger erstellen?", "Nachfolger wirklich erstellen?"));
                    foreach(var succ in already.successors) {
                        var delData = new Dictionary<string, dynamic> { { "Successor", succ }, { "Predecessor", already } };
                        successorMenu.addMenuItem(new ClickMenuItem($"{succ.name} löschen", "Diesen Nachfolger löschen", "", "SUPPORT_DELETE_HAIRSTLYE_SUCCESOR", MenuItemStyle.red).withData(delData).needsConfirmation("Nachfolger löschen?", "Nachfolger wirklich löschen?"));
                    }
                    subSubMenu.addMenuItem(new MenuMenuItem(successorMenu.Name, successorMenu));

                    var makeOverData = new Dictionary<string, dynamic> { { "Gender", gender }, { "From", already } };
                    var makeoverMenu = new Menu("Umstylungen hinzufügen", "Editiere die Umstylungen");
                    makeoverMenu.addMenuItem(new InputMenuItem("Umstylungen hinzufügen", "Füge einen Nachfolger mit der GTA ID hinzu", "GTA-ID", "SUPPORT_ADD_HAIRSTYLE_MAKEOVER", MenuItemStyle.green).withData(addData).needsConfirmation("Umstylung erstellen?", "Umstylung wirklich erstellen?"));
                    foreach(var makeover in already.confighair2s.Concat(already.confighair1s)) {
                        var delData = new Dictionary<string, dynamic> { { "From", already }, { "To", makeover } };
                        makeoverMenu.addMenuItem(new ClickMenuItem($"{makeover.name} löschen", "Diese Umstylvariante löschen", "", "SUPPORT_DELETE_HAIRSTLYE_MAKEOVER", MenuItemStyle.red).withData(delData).needsConfirmation("Umstylung löschen?", "Umstylung wirklich löschen?"));
                    }
                    subSubMenu.addMenuItem(new MenuMenuItem(makeoverMenu.Name, makeoverMenu));

                    var remData = new Dictionary<string, dynamic> { { "HairStyle", already } };
                    subSubMenu.addMenuItem(new ClickMenuItem("Frisur löschen", "Lösche die Frisur", "", "SUPPORT_DELETE_HAIRSTYLE", MenuItemStyle.red).withData(remData).needsConfirmation("Frisur löschen?", "Frisur wirklich löschen?"));

                    subMenu.addMenuItem(new MenuMenuItem(subSubMenu.Name, subSubMenu));
                }

                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }

            return menu;
        }

        private bool createNewHairStyle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var gender = (char)data["Gender"];
            var evt = (MenuStatsMenuItemEvent)data["PreviousCefEvent"];
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var idEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var levelEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var id = int.Parse(idEvt.input);
                var lvl = int.Parse(levelEvt.input);
                var newHair = new confighairstyle {
                    gender = gender.ToString(),
                    gtaId = id,
                    level = lvl,
                    name = nameEvt.input,
                };

                db.confighairstyles.Add(newHair);
                db.SaveChanges();
            }

            loadHair();
            player.sendNotification(NotifactionTypes.Success, "Neue Frisur erfolgreich erstellt!", "");
            return true;
        }

        private bool deleteHairStyle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var hairStyle = (confighairstyle)data["HairStyle"];

            using(var db = new ChoiceVDb()) {
                var dbHair = db.confighairstyles.FirstOrDefault(h => h.id == hairStyle.id);

                if(dbHair != null) {
                    db.confighairstyles.Remove(dbHair);
                    db.SaveChanges();
                }
            }

            player.sendNotification(NotifactionTypes.Warning, "Frisur erfolgreich gelöscht!", "");
            loadHair();
            return true;
        }

        private bool addHairStyleSuccessor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;

            var gender = (char)data["Gender"];
            var pre = (confighairstyle)data["Predecessor"];
            try {
                var gtaIds = evt.input.Split(',');

                var gtaId = int.Parse(evt.input);
                var suc = AllHairStyles.FirstOrDefault(h => h.gender == gender.ToString() && h.gtaId == gtaId);
                if(suc != null) {
                    using(var db = new ChoiceVDb()) {
                        var succ = db.confighairstyles.Find(suc.id);
                        var pred = db.confighairstyles.Find(pre.id);
                        pred.successors.Add(succ);
                        //var newEdge = new confighairstylesgraph {
                        //    predecessor = pre.id,
                        //    successor = suc.id,
                        //};

                        //db.confighairstylesgraph.Add(newEdge);
                        db.SaveChanges();
                    }

                    player.sendNotification(NotifactionTypes.Success, "Nachfolger erfolgreich hinzugefügt!", "");
                    loadHair();
                } else {
                    player.sendBlockNotification("Die angegebene Frisur gibt es noch nicht!", "");
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig!", "");
            }

            return true;
        }

        private bool deleteHairStyleSuccessor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var pre = (confighairstyle)data["Predecessor"];
            var suc = (confighairstyle)data["Successor"];

            using(var db = new ChoiceVDb()) {
                var pred = db.confighairstyles.Include(c => c.successors).FirstOrDefault(p => p.id == pre.id);
                var succ = db.confighairstyles.Find(suc.id);
                pred.successors.Remove(succ);
                db.SaveChanges();
            }

            loadHair();
            player.sendNotification(NotifactionTypes.Warning, "Nachfolger erfolgreich gelöscht!", "");
            return true;
        }

        private Menu generateBeardMenu(IPlayer player) {
            var menu = new Menu("Bärte einstellen", "Stelle Bärte und ihre Verbindung ein");

            var subMenu = new Menu("Bärte", "Bärte editieren");
            var createMenu = new Menu("Bärte erstellen", "Erstelle einen neuen Bärtestil");
            createMenu.addMenuItem(new InputMenuItem("Name", "Der Anzeigename des Bartes", "", ""));
            createMenu.addMenuItem(new InputMenuItem("GtaID", "Die GTA Id des Bartes", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Level", "Die Haarlänge des Bartes", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Bart erstellen", "Erstelle die Bart", "SUPPORT_CREATE_BEARDSTYLE", MenuItemStyle.green).needsConfirmation("Bart erstellen?", "Bart wirklich erstellen?"));
            subMenu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            foreach(var already in AllBeardStyles.OrderBy(h => h.level).ToList()) {
                var subSubMenu = new Menu(already.name, "Editiere den Bart");
                subSubMenu.addMenuItem(new StaticMenuItem("GTA-ID", $"Die GTA-ID des Bartes ist: {already.gtaId}", $"{already.gtaId}"));
                subSubMenu.addMenuItem(new StaticMenuItem("Level", $"Das Level des Bartes ist: {already.level}", $"{already.level}"));

                var addData = new Dictionary<string, dynamic> { { "Predecessor", already } };
                var successorMenu = new Menu("Nachfolger", "Editiere die Nachfolger");
                successorMenu.addMenuItem(new InputMenuItem("Nachfolger hinzufügen", "Füge einen Nachfolger mit der GTA ID hinzu", "GTA-ID", "SUPPORT_ADD_BEARDSTYLE_SUCCESSOR", MenuItemStyle.green).withData(addData).needsConfirmation("Nachfolger erstellen?", "Nachfolger wirklich erstellen?"));
                foreach(var edge in already.predecessors) {
                    var successor = edge.successors.First();
                    var delData = new Dictionary<string, dynamic> { { "Successor", successor }, { "Predecessor", edge } };
                    successorMenu.addMenuItem(new ClickMenuItem($"{successor.name} löschen", "Diesen Nachfolger löschen", "", "SUPPORT_DELETE_BEARDSTLYE_SUCCESOR", MenuItemStyle.red).withData(delData).needsConfirmation("Nachfolger löschen?", "Nachfolger wirklich löschen?"));
                }
                subSubMenu.addMenuItem(new MenuMenuItem(successorMenu.Name, successorMenu));
                var remData = new Dictionary<string, dynamic> { { "BeardStyle", already } };
                subSubMenu.addMenuItem(new ClickMenuItem("Frisur löschen", "Lösche die Frisur", "", "SUPPORT_DELETE_BEARDSTYLE", MenuItemStyle.red).withData(remData).needsConfirmation("Bart löschen?", "Bart wirklich löschen?"));

                subMenu.addMenuItem(new MenuMenuItem(subSubMenu.Name, subSubMenu));
            }

            menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));

            return menu;
        }


        private bool createNewBeardStyle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = (MenuStatsMenuItemEvent)data["PreviousCefEvent"];
            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var idEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var levelEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var id = int.Parse(idEvt.input);
                var lvl = int.Parse(levelEvt.input);
                var newBeard = new configbeardstyle {
                    gtaId = id,
                    level = lvl,
                    name = nameEvt.input,
                };

                db.configbeardstyles.Add(newBeard);
                db.SaveChanges();
            }

            loadHair();
            player.sendNotification(NotifactionTypes.Success, "Neuen Bart erfolgreich erstellt!", "");
            return true;
        }

        private bool deleteBeardStyle(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var beardStyle = (configbeardstyle)data["BeardStyle"];

            using(var db = new ChoiceVDb()) {
                var dbBeard = db.configbeardstyles.FirstOrDefault(b => b.id == beardStyle.id);

                if(dbBeard != null) {
                    db.configbeardstyles.Remove(dbBeard);
                    db.SaveChanges();
                }
            }

            player.sendNotification(NotifactionTypes.Warning, "Bart erfolgreich gelöscht!", "");
            loadHair();
            return true;
        }

        private bool addBeardStyleSuccessor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;

            var pre = (configbeardstyle)data["Predecessor"];
            try {
                var gtaId = int.Parse(evt.input);
                var suc = AllBeardStyles.FirstOrDefault(b => b.gtaId == gtaId);
                if(suc != null) {
                    using(var db = new ChoiceVDb()) {
                        var dbPred = db.configbeardstyles.Find(pre.id);
                        dbPred.successors.Add(suc);
                        //var newEdge = new configbeardstylesgraph {
                        //    predecessor = pre.id,
                        //    successor = suc.id,
                        //};

                        //db.configbeardstylesgraph.Add(newEdge);
                        db.SaveChanges();
                    }

                    player.sendNotification(NotifactionTypes.Success, "Nachfolger erfolgreich hinzugefügt!", "");
                    loadHair();
                } else {
                    player.sendBlockNotification("Den angegebenen Bart gibt es noch nicht!", "");
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig!", "");
            }

            return true;
        }

        private bool deleteBeardStyleSuccessor(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var pre = (configbeardstyle)data["Predecessor"];
            var suc = (configbeardstyle)data["Successor"];

            using(var db = new ChoiceVDb()) {
                var pred = db.configbeardstyles.Find(pre.id);
                pred.successors.Remove(suc);
                db.SaveChanges();
            }

            loadHair();
            player.sendNotification(NotifactionTypes.Warning, "Nachfolger erfolgreich gelöscht!", "");
            return true;
        }

        private bool onSupportAddHairstyleMakeover(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = data["PreviousCefEvent"] as InputMenuItemEvent;

            var gender = (char)data["Gender"];
            var from = (confighairstyle)data["Predecessor"];
            try {
                var gtaId = int.Parse(evt.input);
                var makeover = AllHairStyles.FirstOrDefault(b => b.gtaId == gtaId && b.gender == gender.ToString());
                if(makeover != null) {
                    using(var db = new ChoiceVDb()) {
                        var dbMakeOver = db.confighairstyles.Find(makeover.id);
                        var dbAlready = db.confighairstyles.Find(from.id);
                        dbAlready.confighair2s.Add(dbMakeOver);
                        db.SaveChanges();
                    }

                    player.sendNotification(NotifactionTypes.Success, "Nachfolger erfolgreich hinzugefügt!", "");
                    loadHair();
                } else {
                    player.sendBlockNotification("Die angegebene Frisur gibt es noch nicht!", "");
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig!", "");
            }

            return true;
        }

        #endregion
    }
}
