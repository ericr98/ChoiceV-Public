using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.PrisonSystem;
using ChoiceVServer.Controller.PrisonSystem.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static ChoiceVServer.Model.Menu.CheckBoxMenuItem;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.ListMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Companies {
    public class CompanyPrisonFunctionality : CompanyFunctionality {
        private List<Prison> Prisons;

        public CompanyPrisonFunctionality() : base() { }

        public CompanyPrisonFunctionality(Company company) : base(company) {
            Company = company;
        }

        public bool hasPrison(Prison prison) {
            return Prisons.Contains(prison);
        }

        public override string getIdentifier() {
            return "PRISON_FUNCTIONALITY";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Gefängnisverwaltung", "Füge die Möglichkeit zu auf bestimmte Gefängnisse zuzugreifen");
        }

        public override List<string> getSinglePermissionsGranted() {
            return new List<string> { "PRISON_CONTROL" };
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement(
               "PRISON_CONFIGURATION",
               configurePrison,
               onPrisonConfigure
            );

            Company.registerCompanyInteractElement(
               "PRISON_REGISTER",
               getPrisonRegisterMenu,
               onPrisonRegister,
               "PRISON_CONTROL"
            );

            Company.registerCompanySelfElement(
                "PRISON_INMATE_CONTROL",
                getInmateControl,
                onInmateControl,
                "PRISON_CONTROL"

             );

            var prisonIds = Company.getSetting<List<int>>("PRISON_IDS");
            if(prisonIds == null) {
                prisonIds = new List<int>();
            }

            Prisons = PrisonController.getPrisons(p => prisonIds.Contains(p.Id));
        }

        public override void onRemove() {
            Company.unregisterCompanyElement("PRISON_CONFIGURATION");
            Company.unregisterCompanyElement("PRISON_REGISTER");
        }

        #region Admin Stuff

        private MenuElement configurePrison(IPlayer player) {
            var menu = new Menu("Gefängnis konfigurieren", "Konfiguriere die Gefängnisse");

            var prisons = PrisonController.getAllPrisons().Select(p => p.Name).ToArray();
            menu.addMenuItem(new ListMenuItem("Gefängnis hinzufügen", "Füge das ausgewählte Gefängnis hinzu", prisons, "ADD_PRISON"));

            foreach(var prison in Prisons) {
                menu.addMenuItem(new ClickMenuItem($"{prison.Name} löschen", "Entfernen das gewählte Gefängnis", "", "REMOVE_PRISON", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "PrisonId", prison.Id } })
                    .needsConfirmation("Gefängnis wirklich löschen?", "Gefängnis wirklich löschen?"));
            }

            return menu;
        }

        private void onPrisonConfigure(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "ADD_PRISON":
                    var evt = menuItemCefEvent as ListMenuItemEvent;
                    var addPrison = PrisonController.getPrisons(p => p.Name == evt.currentElement).FirstOrDefault();
                    if(addPrison != null) {
                        Prisons.Add(addPrison);
                        Company.setSetting("PRISON_IDS", Prisons.Select(p => p.Id).ToList().ToJson());
                    }
                    player.sendNotification(Constants.NotifactionTypes.Success, $"{addPrison.Name} wurde hinzugefügt!", "Gefängnis hinzugefügt");
                    break;
                case "REMOVE_PRISON":
                    int prisonId = data["PrisonId"];
                    var removePrison = Prisons.FirstOrDefault(p => p.Id == prisonId);
                    if(removePrison != null) {
                        Prisons.Remove(removePrison);
                        Company.setSetting("PRISON_IDS", Prisons.Select(p => p.Id).ToList().ToJson());
                    }
                    player.sendNotification(Constants.NotifactionTypes.Warning, $"{removePrison.Name} wurde entfernt!", "Gefängnis entfernt");
                    break;
            }
        }

        #endregion

        #region Interaction

        private MenuElement getPrisonRegisterMenu(IPlayer player, IPlayer target) {
            if(target.isInSpecificCollisionShape(c => Prisons.Any(p => p == c.Owner && PrisonController.hasPlayerAccessToPrison(player, p))) && !Prisons.Any(p => p.isPlayerInPrison(target))) {
                var prison = Prisons.FirstOrDefault(p => target.isInSpecificCollisionShape(c => c.Owner == p));

                var menu = new Menu("Person inhaftieren", "Inhaftiere die besagte Person");

                menu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des Insassen an", "", ""));
                menu.addMenuItem(new InputMenuItem("Dauer (Hafteinheiten)", "Gib die Dauer der Haft in Hafteinheiten an", "", InputMenuItemTypes.number, ""));

                menu.addMenuItem(new MenuStatsMenuItem("Person inhaftieren", "Inhaftiere die Person mit den Angaben", "IMPRISON_PLAYER", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Target", target }, { "Prison", prison } })
                    .needsConfirmation("Die Person inhaftieren?", "Die Person wirklich inhaftieren?"));

                return menu;
            } else if(Prisons.Any(p => p.isPlayerInPrison(target))) {
                var prison = Prisons.FirstOrDefault(p => p.isPlayerInPrison(target));
                var inmate = prison.getInmateForPlayer(target);

                return new MenuMenuItem("Insassen verwalten", getInmateControlMenu(inmate));
            }

            return null;
        }

        private void onPrisonRegister(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "IMPRISON_PLAYER") {
                var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
                var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
                var durationEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

                if(data["Target"] is IPlayer target && data["Prison"] is Prison prison) {
                    prison.imprisonPlayer(target, nameEvt.input, PrisonController.getTimeSpanFromImprisonUnits(float.Parse(durationEvt.input)));
                    player.sendNotification(Constants.NotifactionTypes.Success, $"{nameEvt.input} wurde in {prison.Name} für {durationEvt.input} HE inhaftiert!", "Person inhaftiert");
                }
            } else if(subEvent.StartsWith("INMATE_CONTROL")) {
                inmateControlMenuChange(player, subEvent, data["Inmate"], menuItemCefEvent);
            }
        }

        #endregion

        #region Self Menu

        private MenuElement getInmateControl(Company company, IPlayer player) {
            var menu = new Menu("Gefängniskontrolle", "Kontrolliere das Gefängnis");

            foreach(var prison in Prisons) {
                var prisonMenu = new Menu(prison.Name, "Kontrolliere das Gefängnis");

                var inmatesMenu = new Menu("Insassenverwaltung", "Welchen Insasses verwalten?");

                var oldMenu = new Menu("Älter als 2 Wochen", "Welchen Insasses verwalten");
                foreach(var inmate in prison.getInmates().OrderBy(p => p.Name)) {
                    var virtInmateMenu = new VirtualMenu(inmate.Name, () => getInmateControlMenu(inmate));

                    if(inmate.CreatedDate < DateTime.Now.AddDays(-14)) {
                        oldMenu.addMenuItem(new MenuMenuItem(virtInmateMenu.Name, virtInmateMenu));
                    } else {
                        inmatesMenu.addMenuItem(new MenuMenuItem(virtInmateMenu.Name, virtInmateMenu));
                    }
                }

                if(oldMenu.getMenuItems().Count > 0) {
                    inmatesMenu.addMenuItem(new MenuMenuItem(oldMenu.Name, oldMenu));
                }
                prisonMenu.addMenuItem(new MenuMenuItem(inmatesMenu.Name, inmatesMenu));
                
                var inPrisonMenu = new VirtualMenu("Aktuell im Gefängnis", () => {
                    var menu = new Menu("Aktuell im Gefängnis", "Welchen Insassen verwalten?");

                    using(var db = new ChoiceVDb()) {
                        var inmates = db.prisoninmates
                            .Include(i => i._char)
                            .Where(p => p.prisonId == prison.Id && p.releasedDate == null).ToList();
                        
                        foreach(var inmate in inmates) {
                            var str = prison.isPositionInPrison(inmate._char.position.FromJson()) ? "sichtbar" : "NICHT SICHTBAR";
                            
                            menu.addMenuItem(new StaticMenuItem(inmate.name, $"In der letzten Kameraaktualisierung ist der Insasse {str} im Gefängnis.", str));
                        }
                    }

                    return menu;
                });
                prisonMenu.addMenuItem(new MenuMenuItem(inPrisonMenu.Name, inPrisonMenu));

                var virtCellMenu = new VirtualMenu("Gefängniszellen", () => {
                    var cellsMenu = new Menu("Gefängniszellen", "Siehe dir die Zellen an");

                    foreach(var cell in prison.Cells) {
                        cellsMenu.addMenuItem(new StaticMenuItem(cell.Name, $"Zelle: {cell.Name} ist {(cell.Inmate != null ? "belegt" : "frei")}", cell.Inmate != null ? cell.Inmate.Name : "FREI"));
                    }
                    
                    return cellsMenu;
                });
                prisonMenu.addMenuItem(new MenuMenuItem(virtCellMenu.Name, virtCellMenu));
    
                if(Prisons.Count > 1) {
                    menu.addMenuItem(new MenuMenuItem(prisonMenu.Name, prisonMenu));
                } else {
                    prisonMenu.Name = "Gefängniskontrolle"; 
                    return prisonMenu;
                }
            }

            return menu;
        }

        private void onInmateControl(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent.StartsWith("INMATE_CONTROL")) {
                inmateControlMenuChange(player, subEvent, data["Inmate"], menuItemCefEvent);
            }
        }

        #endregion

        private Menu getInmateControlMenu(PrisonInmate inmate) {
            var inmateMenu = new Menu(inmate.Name, "Verwalte den Insassen", false);

            inmateMenu.addMenuItem(new InputMenuItem("Name", "Ändere den Namen des Insassen", "", "INMATE_CONTROL_CHANGE_NAME")
                .withStartValue(inmate.Name)
                .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                .needsConfirmation("Namen ändern?", "Namen wirklich ändern?"));

            var imprisonUnits = PrisonController.getImprisonUnitsFromTimeSpan(inmate.TimeLeftOnline, inmate.TimeLeftOffline);
            inmateMenu.addMenuItem(new InputMenuItem("Haftzeit übrig: ", "Passe die Haftzeit des Insassen an", "", InputMenuItemTypes.number, "INMATE_CONTROL_CHANGE_TIME")
                .withStartValue(imprisonUnits.ToString())
                .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                .needsConfirmation("Haftzeit ändern?", "Haftzeit wirklich ändern?"));

            var escapedPrefix = "Nicht geflohen";
            if(inmate.Escaped) {
                escapedPrefix = "GEFLOHEN!";
            }

            inmateMenu.addMenuItem(new ClickMenuItem("Entflohen", $"Der Insasse ist als \"{escapedPrefix}\" markiert. Klicke um den Status zurückzusetzen.", escapedPrefix, "INMATE_CONTROL_CHANGE_ESCAPED")
                .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                .needsConfirmation("Haftzeit ändern?", "Haftzeit wirklich ändern?"));

            inmateMenu.addMenuItem(new CheckBoxMenuItem("Für Ausgang freigeben", "Gib den Insassen für Ausgang frei. Er sendet keinen Alarm bei Verlassen des Gefängnisses", inmate.ClearedForExit, "INMATE_CONTROL_CHANGE_CLEARED_FOR_EXIT")
                .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                .needsConfirmation("Insassen Ausgang geben?", "Wirklich Ausgang geben?"));


            if(inmate.IsReleased) {
                inmateMenu.addMenuItem(new ClickMenuItem("Eintrag löschen", "Der Insasse ist bereits freigelassen, aber die Akte noch nicht entfernt. Versuche mit einem Klick die Akte zu entfernen", "(Bereits freigelassen)", "INMATE_CONTROL_DELETE_INMATE", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                    .needsConfirmation("Akte löschen?", "Akte zu löschen versuchen?"));
            } else {
                inmateMenu.addMenuItem(new ClickMenuItem("Freilassen", "Lasse den Gefangengen frei. Seine Insassenregistrierung wird gelöscht sobald möglich", "", "INMATE_CONTROL_RELEASE_INMATE", MenuItemStyle.yellow)
                    .withData(new Dictionary<string, dynamic> { { "Inmate", inmate } })
                    .needsConfirmation("Insassen freilassen?", "Insassen wirklich freilassen?"));
            }

            return inmateMenu;
        }

        private void inmateControlMenuChange(IPlayer initiator, string subEvent, PrisonInmate inmate, MenuItemCefEvent menuItemCefEvent) {
            if(subEvent == "INMATE_CONTROL_CHANGE_NAME") {
                var evt = menuItemCefEvent as InputMenuItemEvent;
                inmate.Name = evt.input;

                initiator.sendNotification(Constants.NotifactionTypes.Success, $"Insassen-Name auf \"{evt.input}\" gesetzt!", "Änderungen gespeichert", Constants.NotifactionImages.Prison);
            } else if(subEvent == "INMATE_CONTROL_CHANGE_TIME") {
                var evt = menuItemCefEvent as InputMenuItemEvent;
                var imprisonUnits = float.Parse(evt.input);
                var timeSpan = PrisonController.getTimeSpanFromImprisonUnits(imprisonUnits);
                inmate.TimeLeftOnline = (int)(timeSpan.TotalMinutes * PrisonController.TIMELEFT_ACTIVE_PERCENTAGE);
                inmate.TimeLeftOffline = (int)(timeSpan.TotalMinutes * (1 - PrisonController.TIMELEFT_ACTIVE_PERCENTAGE));

                initiator.sendNotification(Constants.NotifactionTypes.Success, $"Insassen-Haftzeit auf {imprisonUnits} gesetzt!", "Änderungen gespeichert", Constants.NotifactionImages.Prison);
            } else if(subEvent == "INMATE_CONTROL_CHANGE_CLEARED_FOR_EXIT") {
                var evt = menuItemCefEvent as CheckBoxMenuItemEvent;
                inmate.ClearedForExit = evt.check;

                initiator.sendNotification(Constants.NotifactionTypes.Success, $"Insasse für Ausgang freigegeben: {(inmate.ClearedForExit ? "Erlaubt" : "NICHT erlaubt")}", "Änderungen gespeichert", Constants.NotifactionImages.Prison);
            } else if(subEvent == "INMATE_CONTROL_RELEASE_INMATE") {
                inmate.Prison.releaseInmate(inmate);

                var (worked, message) = inmate.Prison.tryDeleteInmate(inmate);
                if(worked) {
                    initiator.sendNotification(Constants.NotifactionTypes.Success, "Insasse freigelassen. Die Kartei wurde gelöscht.", "Insasse freigelassen", Constants.NotifactionImages.Prison);
                } else {
                    initiator.sendNotification(Constants.NotifactionTypes.Warning, $"Insasse freigelassen. Es gibt aber noch mind. 1 Grund warum die Akte nicht gelöscht wird. Der erste ist: {message}", "Insasse freigelassen", Constants.NotifactionImages.Prison);
                }

                return;
            } else if(subEvent == "INMATE_CONTROL_DELETE_INMATE") {
                var (worked, message) = inmate.Prison.tryDeleteInmate(inmate);
                if(worked) {
                    initiator.sendNotification(Constants.NotifactionTypes.Success, "Insassenakte gelöscht.", "Insassenakte gelöscht", Constants.NotifactionImages.Prison);
                } else {
                    initiator.sendNotification(Constants.NotifactionTypes.Warning, $"Insassenakte konnte nicht gelöscht werden. Grund: {message}", "Insassenakte nicht gelöscht", Constants.NotifactionImages.Prison);
                }

                return;
            }   

            using(var db = new ChoiceVDb()) {
                var dbInmate = db.prisoninmates.Find(inmate.Id);
                dbInmate.name = inmate.Name;
                dbInmate.timeLeftOffline = inmate.TimeLeftOffline;
                dbInmate.timeLeftOnline = inmate.TimeLeftOnline;
                dbInmate.clearedForExit = inmate.ClearedForExit;
                db.SaveChanges();
            }
        }
    }
}
