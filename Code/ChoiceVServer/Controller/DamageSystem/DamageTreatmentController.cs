using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.DamageSystem.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.DamageSystem {
    public class DamageTreatmentController : ChoiceVScript {
        public DamageTreatmentController() {
            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Person aufhelfen", "Hilf der Person wieder auf die Beine!", "", "HELP_UP_PLAYER", MenuItemStyle.green),
                    p => p is IPlayer,
                    t => t is IPlayer tar && !tar.IsInVehicle && tar.hasState(Constants.PlayerStates.Dead) && !tar.getCharacterData().CharacterDamage.hasVerySevereInjury() && tar.getCharacterData().CharacterDamage.getWastedPain() < 100
                )
            );

            EventController.addMenuEvent("HELP_UP_PLAYER", onHelpUpPlayer);

            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new StaticMenuItem("Aufhelfen nicht möglich!", "Der Person kann nicht aufgeholfen werden. Informiere das Fachpersonal", "", MenuItemStyle.yellow),
                    p => p is IPlayer,
                    t => t is IPlayer tar && !tar.IsInVehicle && tar.hasState(Constants.PlayerStates.Dead) && !tar.hasState(Constants.PlayerStates.PermaDeath) && (tar.getCharacterData().CharacterDamage.hasVerySevereInjury() || tar.getCharacterData().CharacterDamage.getWastedPain() >= 100)
                )
            );

            InteractionController.addPlayerInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new StaticMenuItem("Aufhelfen unmöglich!", "Die Person befindet sich in einem komatösen Zustand. Niemand kann im aktuell aufhelfen. Informiere das Fachpersonal", "", MenuItemStyle.red),
                    p => p is IPlayer,
                    t => t is IPlayer tar && !tar.IsInVehicle && tar.hasState(Constants.PlayerStates.PermaDeath)
                )
            );

            InteractionController.addVehicleInteractionElement(
                new ConditionalPlayerInteractionMenuElement(
                    () => new ClickMenuItem("Aus Fahrzeug ziehen", "Ziehe eine verletzte Person aus dem Fahrzeug, und lege ihn zu deinen Füßen ab", "", "PULL_PLAYER_OUT_OF_CAR"),
                    p => p is IPlayer,
                    t => t is ChoiceVVehicle tar && tar.PassengerList.Values.Any(p => p.hasState(Constants.PlayerStates.Dead))
                )
            );
            
            InteractionController.addPlayerInteractionElement(new ConditionalPlayerInteractionMenuElement(
                () => new ClickMenuItem("Verletzungen ansehen", "Sieh dir die Verletzungen der Person an", "", "START_ANALYSE"),
                sender => canAnalyse(sender as IPlayer),
                target => true
            ));

            EventController.addMenuEvent("PULL_PLAYER_OUT_OF_CAR", onPullPlayerOutOfCar);

            EventController.addMenuEvent("SHOW_OPEN_INJURIES", onShowOpenInjury);

            EventController.addCefEvent("INJURY_CLICK", onInjurySelect);

            EventController.addMenuEvent("START_ANALYSE", onStartAnalyse);
            EventController.addMenuEvent("HEAL_INJURY", onInjuryHeal);
            EventController.addMenuEvent("DIAGNOSE_INJURY", onDiagnoseInjury);

            #region Support

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.DamageSystem,
                    "Verletzungseditor",
                    supportMenuGenerator
                )
             );

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.DamageSystem,
                    "Behandlungseditor",
                    supportTreatmentMenuGenerator
                )
            );

            EventController.addMenuEvent("SUPPORT_CREATE_NEW_INJURY", onSupportCreateNewInjury);
            EventController.addMenuEvent("SUPPORT_SELECT_TREATMENT_FOR_INJURY", onSupportSelectTreatmentForInjury);
            EventController.addMenuEvent("SUPPORT_DELETE_INJURY", onSupportDeleteInjury);

            EventController.addMenuEvent("SUPPORT_CREATE_NEW_INJURY_TREATMENT_STEP", onSupportCreateNewInjuryStep);
            EventController.addMenuEvent("SUPPORT_DELETE_INJURY_TREATMENT_STEP", onSupportDeleteInjuryStep);
            EventController.addMenuEvent("SUPPORT_CREATE_NEW_INJURY_TREATMENT", onSupportCreateNewInjuryTreatment);
            EventController.addMenuEvent("SUPPORT_DELETE_TREATMENT", onSupportDeleteTreatment);

            #endregion
        }
       
        //Maybe require 
        public static bool canAnalyse(IPlayer player) {
            return true;
        }

        public static bool canDiagnose(IPlayer player) {
            return true;
        }

        private bool onHelpUpPlayer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            var anim = AnimationController.getAnimationByName("KNEEL_DOWN");

            AnimationController.animationTask(player, anim, () => {
                DamageController.revivePlayer(target);
            });

            return true;
        }

        private bool onPullPlayerOutOfCar(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (ChoiceVVehicle)data["InteractionTarget"];
            var injuredTarget = target.PassengerList.Values.FirstOrDefault(p => p.hasState(Constants.PlayerStates.Dead));

            if(injuredTarget != null) {
                var anim = AnimationController.getAnimationByName("WORK_FRONT");

                AnimationController.animationTask(player, anim, () => {
                    injuredTarget.Position = player.Position;
                    injuredTarget.forceAnimation("dead", "dead_b", -1, 1);

                    player.sendNotification(NotifactionTypes.Success, "Person erfolgreich aus dem Fahrzeug gezogen", "Person geborgen", NotifactionImages.Car);
                    injuredTarget.sendNotification(NotifactionTypes.Warning, "Du wurdest aus dem Fahrzeug geborgen", "Geborgen worden", NotifactionImages.Car);
                });
            }

            return true;
        }

        private bool onShowOpenInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (IPlayer)data["InteractionTarget"];

            var shownTypes = new List<DamageType> { DamageType.Shot, DamageType.Sting, DamageType.Burning };

            var bodyPartInjuries = target.getCharacterData().CharacterDamage.AllInjuries.Where(i => shownTypes.Contains(i.Type)).GroupBy(i => i.BodyPart);

            var menu = new Menu("Gesundheitszustand", "Die Verletzungen der Person");
            foreach(var bp in bodyPartInjuries) {
                var subMenu = new Menu(CharacterBodyPartToString[bp.First().BodyPart], $"Verletzungen im {CharacterBodyPartToString[bp.First().BodyPart]}");

                foreach(var inj in bp) {
                    var type = "";
                    var strength = "";
                    inj.getMessage(ref type, ref strength);

                    subMenu.addMenuItem(new StaticMenuItem(type, $"Eine {strength} {type}", strength));
                }

                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));
            }

            player.showMenu(menu);

            return true;
        }

        private class MedicalAnalyseDataElement {
            public int charId;
            public int injuryId;
        }

        private void onInjurySelect(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var el = new MedicalAnalyseDataElement();
            JsonConvert.PopulateObject(evt.Data, el);

            if(Math.Sign(el.charId) == -1) {
                if (MedicScreenCallbacks.ContainsKey(el.charId)) {
                    MedicScreenCallbacks[el.charId].Invoke(el.injuryId);
                    MedicScreenCallbacks.Remove(el.charId);
                }
                return;
            }

            var target = ChoiceVAPI.FindPlayerByCharId(el.charId);
            if(target != null) {
                var injs = target.getCharacterData().CharacterDamage.findInjuryById(el.injuryId);
                var injStr = "Unbk. Verletzung";
                if(injs.DiagnosedInjury != null) {
                    injStr = injs.DiagnosedInjury.name;
                }

                var menu = new Menu(injStr, "Was möchtest du tun?");

                menu.addMenuItem(new ClickMenuItem("Verletzung ansehen", "Erhalte spezifischere Informationen über die Verletzung. Eventuell benötigst du medizinisches Equipment", "", "DIAGNOSE_INJURY").withData(new Dictionary<string, dynamic> { { "InjuryCefElement", el } }));

                var subMenu = new Menu("Verletzung behandeln", "Welches Werkzeug möchtest du benutzen?");
                var medItems = player.getInventory().getItems<MedicItem>(i => true);

                foreach(var item in medItems) {
                    var menuItem = item.getMedicalAnalyseMenuItem("HEAL_INJURY").withData(new Dictionary<string, dynamic> { { "Item", item }, { "InjuryCharId", el.charId }, { "InjuryInjuryId", el.injuryId } });
                    subMenu.addMenuItem(menuItem);
                }

                menu.addMenuItem(new MenuMenuItem(subMenu.Name, subMenu));

                player.showMenu(menu);
            }
        }

        private bool onDiagnoseInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var element = (MedicalAnalyseDataElement)data["InjuryCefElement"];
            var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == element.charId);

            if(target != null) {
                var inj = target.getCharacterData().CharacterDamage.findInjuryById(element.injuryId);
                if(inj != null) {
                    if(inj.BodyPart != CharacterBodyPart.None) {
                        lookAtInjury(player, inj);
                    } else {
                        Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"analyseInjury: the bodypart was not found, injuryId: {element.injuryId}");
                    }
                } else {
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"analyseInjury: the injury was not found, injuryId: {element.injuryId}");
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"analyseInjury: character not found during medical analyse");
            }
            return true;
        }

        private void lookAtInjury(IPlayer player, Injury injury) {
            var anim = AnimationController.getAnimationByName("INSPECT_MEDIC");
            AnimationController.animationTask(player, anim, () => {
                var strength = "";
                var type = "";
                var treated = "";

                if(injury.IsTreated) {
                    treated = "sie wurde bereits mit etwas behandelt";
                } else if(injury.IsHealing) {
                    treated = "sie ist im Heilprozess";
                }

                injury.getMessage(ref type, ref strength);
                player.sendNotification(NotifactionTypes.Info, $"Es handelt sich um eine {strength} {type} {treated}", $"{strength} {type}", NotifactionImages.Bone);
                var str = injury.diagnoseInjury(!injury.needsToolDiagnose() || player.isInSpecificCollisionShape(c => (string)c.Owner == "DIAGNOSE_SPOT"), out var shortStr);
                player.sendNotification(NotifactionTypes.Info, str, shortStr, NotifactionImages.Bone);
            });
        }

        private bool onInjuryHeal(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var charId = data["InjuryCharId"];
            var injuryId = data["InjuryInjuryId"];
            var item = (MedicItem)data["Item"];
            var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == charId);

            var anim = AnimationController.getAnimationByName("INSPECT_MEDIC");

            AnimationController.animationTask(player, anim, () => {
                if(target != null) {
                    var inj = (Injury)target.getCharacterData().CharacterDamage.findInjuryById(injuryId);
                    if(inj != null) {
                        if(item != null) {
                            if(item.treatInjury(inj)) {
                                Logger.logDebug(LogCategory.System, LogActionType.Updated, player, $"onInjuryHeal: Injury successfully treated. treaterId: {player.getCharacterId()}, patientId: {target.getCharacterId()}, injury: {inj.ToJson()}");
                            } else {
                                if(inj.IsHealing) {
                                    Logger.logDebug(LogCategory.System, LogActionType.Updated, player, $"onInjuryHeal: Injury treated, that is already healing. treaterId: {player.getCharacterId()}, patientId: {target.getCharacterId()}, injury: {inj.ToJson()}");
                                } else {
                                    Logger.logDebug(LogCategory.System, LogActionType.Updated, player, $"onInjuryHeal: Injury wrongly/not finishly treated! treaterId: {player.getCharacterId()}, patientId: {target.getCharacterId()}, injury: {inj.ToJson()}");
                                }
                            }
                            item.use(player);
                            showMedicScreen(player, target);
                        } else {
                            player.sendBlockNotification("Item nicht gefunden!", "Item weg", NotifactionImages.Bone);
                            Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onInjuryHeal: item not found, injuryId: {injuryId}");
                        }
                    } else {
                        player.sendBlockNotification("Verletzung nicht gefunden!", "Verletzung weg", NotifactionImages.Bone);
                        Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onInjuryHeal: injury not found, injuryId: {injuryId}");
                    }
                } else {
                    player.sendBlockNotification("Spieler nicht gefunden!", "Spieler weg", NotifactionImages.Bone);
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onInjuryHeal: player not found");
                }
            }, null, true);

            return true;
        }

        private class MedicAnalyseCefEvent : IPlayerCefEvent {
            public string Event { get; set; }
            public string[] injuries;
            public int charId;
            public MedicAnalyseCefEvent(int charId, string[] injs) {
                Event = "MEDICAL_ANALYSE";
                injuries = injs;
                this.charId = charId;
            }
        }

        private class InjuryCefObject {
            public int injuryId;
            public int severness;
            public string bodyPart;
            public int seed;

            public InjuryCefObject(int injuryId, int severness, CharacterBodyPart bodyPart, int seed) {
                this.injuryId = injuryId;
                this.severness = severness;
                this.bodyPart = CharacterBodyPartToCef[bodyPart];
                this.seed = seed;
            }
        }

        private bool onStartAnalyse(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var id = data["InteractionTargetId"];
            var target = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.getCharacterId() == id);
            showMedicScreen(player, target);
            return true;
        }

        public static void showMedicScreen(IPlayer player, IPlayer target) {
            if(target != null) {
                var list = new List<string>();
                var dmg = target.getCharacterData().CharacterDamage;
                foreach(var inj in dmg.AllInjuries) {
                    list.Add(new InjuryCefObject(inj.Id, inj.getShowSevernessLevel(), inj.BodyPart, inj.Seed).ToJson());
                }

                player.emitCefEventWithBlock(new MedicAnalyseCefEvent(target.getCharacterId(), list.ToArray()), "MEDIC_ANALYSIS");
            } else {
                player.sendBlockNotification("Der Spieler wurde nicht gefunden?", "Analyse fehlerhaft", NotifactionImages.Bone);
            }
        }

        private static Dictionary<int, Action<int>> MedicScreenCallbacks = new Dictionary<int, Action<int>>();

        public static void showMedicScreen(IPlayer player, int id, List<Injury> injuries, Action<int> callback) {
            var list = new List<string>();
            foreach(var inj in injuries) {
                list.Add(new InjuryCefObject(inj.Id, inj.getShowSevernessLevel(), inj.BodyPart, inj.Seed).ToJson());
            }

            if(Math.Sign(id) != -1) {
                id = -id;
            }

            MedicScreenCallbacks[id] = callback;
            player.emitCefEventWithBlock(new MedicAnalyseCefEvent(id, list.ToArray()), "MEDIC_ANALYSIS");
        }

        #region SupportStuff

        private Menu supportMenuGenerator(IPlayer player) {
            var menu = new Menu("Verletzungseditor", "Editiere mögliche Verletzungen");
            using(var db = new ChoiceVDb()) {
                var createMenu = new Menu("Neue Verletzung", "Füge eine neue Verletzung hinzu");
                createMenu.addMenuItem(new InputMenuItem("Name", "Der Name der Verletzung: z.B. Steifschuss", "", ""));
                var bodyPartList = Enum.GetNames<CharacterBodyPartCategories>().Concat(Enum.GetNames(typeof(CharacterBodyPart))).ToArray();
                createMenu.addMenuItem(new ListMenuItem("Körperteil", "Das Körperteil auf dem die Verletzung passieren kann", bodyPartList, ""));
                var damageTypeList = Enum.GetNames(typeof(DamageType));
                createMenu.addMenuItem(new ListMenuItem("Schadenstyp", "Der Schadenstyp bei dem die Verletzung auftritt", damageTypeList, ""));
                var severnessList = new string[] { "1", "2", "3", "4", "5", "6" };
                createMenu.addMenuItem(new ListMenuItem("Min. Schmerzlevel", "Das minimale Schmerzlevel bei dem die Verlzung auftritt", severnessList, ""));
                createMenu.addMenuItem(new ListMenuItem("Max. Schmerzlevel", "Das maximale Schmerzlevel bei dem die Verlzung auftritt", severnessList, ""));
                createMenu.addMenuItem(new InputMenuItem("Behandlungskategorie", "Die Kategorie der Behandlung", "", "").withOptions(db.configinjurytreatments.Select(t => t.identifier).ToArray()));
                createMenu.addMenuItem(new MenuStatsMenuItem("Verletzung erstellen", "Erstelle die Verletzung", "SUPPORT_CREATE_NEW_INJURY", MenuItemStyle.green).needsConfirmation("Verletzung erstellen?", "Verletzung wirklich erstellen?"));
                menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

                var medicItemList = InventoryController.getConfigItems(i => i.codeItem == typeof(MedicItem).Name).Select(i => i.name).ToArray();

                foreach(var bodyPart in bodyPartList) {
                    var partMenu = new Menu(bodyPart, $"Alle Verletzungen im/am {bodyPart}");
                    foreach(var dbInj in db.configinjuries
                        .Include(i => i.treatmentCategoryNavigation)
                        .ThenInclude(t => t.configinjurytreatmentssteps)
                        .ThenInclude(i => i.item).Where(i => i.bodyPart == bodyPart).OrderBy(i => i.minSeverness).ToList()) {
                        var injData = new Dictionary<string, dynamic> { { "DBId", dbInj.id } };

                        var injMenu = new Menu(dbInj.name, "Editiere die Verletzung");
                        injMenu.addMenuItem(new StaticMenuItem("Schadenstyp", "Der Schadenstyp der Verletzung", dbInj.damageType));
                        injMenu.addMenuItem(new StaticMenuItem("Min. Schmerzlevel", "Das minimale Schmerzlevel bei dem die Verlzung auftritt", dbInj.minSeverness.ToString()));
                        injMenu.addMenuItem(new StaticMenuItem("Max. Schmerzlevel", "Das maximale Schmerzlevel bei dem die Verlzung auftritt", dbInj.maxSeverness.ToString()));

                        if(dbInj.treatmentCategoryNavigation != null) {
                            var injuryTreatmentMenu = supportGetTreatmeantMenu(dbInj.treatmentCategoryNavigation);
                            injMenu.addMenuItem(new MenuMenuItem(injuryTreatmentMenu.Name, injuryTreatmentMenu));
                        } else {
                            var allTreatments = db.configinjurytreatments.ToList();

                            injMenu.addMenuItem(new InputMenuItem("Behandlung wählen", "Wähle die Behandlung. Einsehbar sind die Behandlungen im Behandlungsmenü", "", "SUPPORT_SELECT_TREATMENT_FOR_INJURY")
                                .withOptions(allTreatments.Select(t => t.identifier).ToArray())
                                .withData(new Dictionary<string, dynamic> { { "InjuryId", dbInj.id } }));
                        }

                        injMenu.addMenuItem(new ClickMenuItem("Verletzung löschen", "Verletzung wird gelöscht", "", "SUPPORT_DELETE_INJURY", MenuItemStyle.red).needsConfirmation("Verletzung löschen?", $"{dbInj} im {bodyPart} löschen?").withData(injData));
                        partMenu.addMenuItem(new MenuMenuItem(injMenu.Name, injMenu));
                    }

                    menu.addMenuItem(new MenuMenuItem(partMenu.Name, partMenu));
                }
            }

            return menu;
        }

        private Menu supportTreatmentMenuGenerator(IPlayer player) {
            var treatmentMenu = new Menu("Behandlungen", "Editiere mögliche Behandlungen");
            treatmentMenu.addMenuItem(new InputMenuItem("Neue Behandlung", "Füge eine neue Behandlung hinzu. Wähle einen eindeutigen und beschreibenden Identifer", "", "SUPPORT_CREATE_NEW_INJURY_TREATMENT", MenuItemStyle.green)
                .needsConfirmation("Behandlung erstellen?", "Behandlung wirklich erstellen?"));

            SupportController.setCurrentSupportFastAction(player, () => player.showMenu(supportTreatmentMenuGenerator(player)));

            using(var db = new ChoiceVDb()) {
                foreach(var treatment in db.configinjurytreatments
                    .Include(t => t.configinjurytreatmentssteps)
                    .ThenInclude(s => s.item)
                    .ToList()) {

                    var virtualTreatmentMenu = new VirtualMenu(treatment.identifier, () => {
                        var treatmentMenuMenu = supportGetTreatmeantMenu(treatment);
                        treatmentMenu.addMenuItem(new MenuMenuItem(treatmentMenuMenu.Name, treatmentMenuMenu));

                        return treatmentMenuMenu;
                    });

                    treatmentMenu.addMenuItem(new MenuMenuItem(virtualTreatmentMenu.Name, virtualTreatmentMenu));
                }
            }

            return treatmentMenu;
        }

        private Menu supportGetTreatmeantMenu(configinjurytreatment treatment) {
            var menu = new Menu(treatment.identifier, "Editiere die Behandlung");

            var createStepMenu = new Menu("Schritt hinzufügen", "Füge einen Behandlungsschritt hinzu");
            createStepMenu.addMenuItem(new InputMenuItem("Schritt", "Schritt in der Operation. Fängt bei 0 an!", "", ""));
            var medicItemList = InventoryController.getConfigItems(i => i.codeItem == typeof(MedicItem).Name).Select(i => i.name).ToArray();
            createStepMenu.addMenuItem(new ListMenuItem("Behandlungsitem", "Das MedicItem für die Behandlung", medicItemList, ""));
            createStepMenu.addMenuItem(new MenuStatsMenuItem("Schritt hinzufügen", "Füge den neuen Schritt hinzu", "SUPPORT_CREATE_NEW_INJURY_TREATMENT_STEP", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> { { "Identifier", treatment.identifier } })
                .needsConfirmation("Schritt hinzufügen?", "Schritt wirklich hinzufügen?"));
            menu.addMenuItem(new MenuMenuItem(createStepMenu.Name, createStepMenu, MenuItemStyle.green));

            foreach(var step in treatment.configinjurytreatmentssteps.OrderBy(i => i.order)) {
                var stepMenu = new Menu($"Schritt: {step.order}", $"Behandlung mit {step.item.name}");
                stepMenu.addMenuItem(new StaticMenuItem("Behandlungsitem", "Gibt an welches Item in der Behandlung benötigt wird", step.item.name));
                stepMenu.addMenuItem(new ClickMenuItem("Schritt löschen", "Lösche den Schritt aus der Operation", "", "SUPPORT_DELETE_INJURY_TREATMENT_STEP", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Identifier", step.identifier }, { "Order", step.order } })
                    .needsConfirmation($"Schritt {step.order} löschen?", $"Schritt {step.order} wirklich löschen?"));
                menu.addMenuItem(new MenuMenuItem(stepMenu.Name, stepMenu));
            }


            menu.addMenuItem(new ClickMenuItem("Behandlung löschen", "Lösche diese Behandlung", "", "SUPPORT_DELETE_TREATMENT", MenuItemStyle.red)
                .withData(new Dictionary<string, dynamic> { { "Identifier", treatment.identifier } })
                .needsConfirmation("Behandlung löschen?", "Behandlung wirklich hinzufügen?"));
            return menu;
        }

        private bool onSupportCreateNewInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = (MenuStatsMenuItemEvent)data["PreviousCefEvent"];

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var bodyPartEvt = evt.elements[1].FromJson<ListMenuItemEvent>();
            var damageTypeEvt = evt.elements[2].FromJson<ListMenuItemEvent>();
            var minSevernessEvt = evt.elements[3].FromJson<ListMenuItemEvent>();
            var maxSevernessEvt = evt.elements[4].FromJson<ListMenuItemEvent>();
            var treatmentCategoryEvt = evt.elements[5].FromJson<InputMenuItemEvent>();

            try {
                var name = nameEvt.input;
                var bodyPart = bodyPartEvt.currentElement;
                var damageType = damageTypeEvt.currentElement;
                var min = int.Parse(minSevernessEvt.currentElement);
                var max = int.Parse(maxSevernessEvt.currentElement);

                using(var db = new ChoiceVDb()) {
                    var newDbInj = new configinjury {
                        name = name,
                        bodyPart = bodyPart,
                        damageType = damageType,
                        minSeverness = min,
                        maxSeverness = max,
                        treatmentCategory = treatmentCategoryEvt.input,
                    };

                    db.configinjuries.Add(newDbInj);
                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, $"Verletzung {name} erfolgreich hinzugefügt", "");
                }
            } catch(Exception) {
                player.sendBlockNotification("Eine Eingabe war ungültig!", "");
            }

            return true;
        }

        private bool onSupportSelectTreatmentForInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var injId = (int)data["InjuryId"];
            var evt = menuItemCefEvent as InputMenuItemEvent;


            using(var db = new ChoiceVDb()) {
                var dbInjury = db.configinjuries.FirstOrDefault(i => i.id == injId);

                if(dbInjury != null) {
                    dbInjury.treatmentCategory = evt.input;
                    db.SaveChanges();

                    player.sendNotification(NotifactionTypes.Success, $"Behandlung für {dbInjury.name} erfolgreich gesetzt", "");
                } else {
                    player.sendBlockNotification("Verletzung nicht gefunden. Vielleicht wurde sie bereits gelöscht?", "");
                }
            }

            return true;
        }

        private bool onSupportDeleteInjury(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var injId = (int)data["DBId"];

            using(var db = new ChoiceVDb()) {
                var dbInjury = db.configinjuries.Find(injId);

                if(dbInjury != null) {
                    db.configinjuries.Remove(dbInjury);
                    db.SaveChanges();
                    player.sendNotification(NotifactionTypes.Warning, $"Verletzung {dbInjury.name} vom Typ {dbInjury.damageType} im/am {dbInjury.bodyPart}", "");
                } else {
                    player.sendBlockNotification("Verletzung nicht gefunden. Vielleicht wurde sie bereits gelöscht?", "");
                }
            }

            return true;
        }

        private bool onSupportCreateNewInjuryStep(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var identifier = (string)data["Identifier"];

            var evt = (MenuStatsMenuItemEvent)data["PreviousCefEvent"];
            var orderEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var itemEvt = evt.elements[1].FromJson<ListMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var order = int.Parse(orderEvt.input);
                var cfgItem = InventoryController.getConfigItem(i => i.name == itemEvt.currentElement);
                var dbTreatment = db.configinjurytreatments.Include(i => i.configinjurytreatmentssteps).FirstOrDefault(i => i.identifier == identifier);

                if(dbTreatment != null) {
                    if(cfgItem != null) {
                        var newStep = new configinjurytreatmentsstep {
                            identifier = identifier,
                            order = order,
                            itemId = cfgItem.configItemId,
                        };

                        if(order > dbTreatment.configinjurytreatmentssteps.Count) {
                            newStep.order = dbTreatment.configinjurytreatmentssteps.Count;
                            player.sendNotification(NotifactionTypes.Warning, "Schrittzahl war zu groß. Sie wurde angepasst", "");
                        } else if(order < dbTreatment.configinjurytreatmentssteps.Count) {
                            foreach(var already in dbTreatment.configinjurytreatmentssteps.OrderByDescending(i => i.order)) {
                                if(already.order >= order) {
                                    //Has to be done this way because order is a primary key
                                    db.configinjurytreatmentssteps.Remove(already);
                                    db.SaveChanges();

                                    already.order += 1;

                                    db.configinjurytreatmentssteps.Add(already);
                                    db.SaveChanges();
                                }
                            }

                        }

                        db.configinjurytreatmentssteps.Add(newStep);

                        db.SaveChanges();
                        player.sendNotification(NotifactionTypes.Success, $"Behandlungsschritt {order} erfolgreich hinzugefügt", "");
                    } else {
                        player.sendBlockNotification("Item nicht gefunden. Vielleicht wurde es gelöscht?", "");
                    }
                } else {
                    player.sendBlockNotification("Verletzung nicht gefunden. Vielleicht wurde sie bereits gelöscht?", "");
                }
            }

            return true;
        }

        private bool onSupportCreateNewInjuryTreatment(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var newTreatment = new configinjurytreatment {
                    identifier = evt.input,
                };

                db.configinjurytreatments.Add(newTreatment);
                db.SaveChanges();

                player.sendNotification(NotifactionTypes.Success, $"Behandlung {evt.input} erfolgreich hinzugefügt", "");
            }

            return true;
        }

        private bool onSupportDeleteTreatment(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var injIdentifier = (string)data["Identifier"];

            using(var db = new ChoiceVDb()) {
                var dbTreatment = db.configinjurytreatments.FirstOrDefault(ci => ci.identifier == injIdentifier);

                if(dbTreatment != null) {
                    db.configinjurytreatments.Remove(dbTreatment);
                    db.SaveChanges();
                    player.sendNotification(NotifactionTypes.Warning, $"Behandlung {injIdentifier} entfernt", "");
                } else {
                    player.sendBlockNotification("Behandlung nicht gefunden. Vielleicht wurde sie bereits gelöscht?", "");
                }
            }

            return true;
        }

        private bool onSupportDeleteInjuryStep(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var injIdentifier = (string)data["Identifier"];
            var order = (int)data["Order"];

            using(var db = new ChoiceVDb()) {
                var dbInjuryStep = db.configinjurytreatmentssteps.Include(ci => ci.identifierNavigation).ThenInclude(i => i.configinjurytreatmentssteps).FirstOrDefault(ci => ci.identifier == injIdentifier && ci.order == order);

                if(dbInjuryStep != null) {
                    db.configinjurytreatmentssteps.Remove(dbInjuryStep);

                    foreach(var other in dbInjuryStep.identifierNavigation.configinjurytreatmentssteps.OrderBy(ci => ci.order)) {
                        if(other.order > dbInjuryStep.order) {
                            //Has to be done this way because order is a primary key
                            db.configinjurytreatmentssteps.Remove(other);
                            db.SaveChanges();

                            other.order -= 1;

                            db.configinjurytreatmentssteps.Add(other);
                            db.SaveChanges();
                        }
                    }
                    db.SaveChanges();
                    player.sendNotification(NotifactionTypes.Warning, $"Behandlungschritt {order} entfernt", "");
                } else {
                    player.sendBlockNotification("Verletzung nicht gefunden. Vielleicht wurde sie bereits gelöscht?", "");
                }
            }

            return true;
        }

        #endregion
    }
}
