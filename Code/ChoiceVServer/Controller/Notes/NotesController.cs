using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;
using File = ChoiceVServer.InventorySystem.File;

namespace ChoiceVServer.Controller {
    public enum FileLogoType {
        City,
    }

    #region Company Module

    public class AvailableNotesCompanyFunctionality : CompanyFunctionality {
        public AvailableNotesCompanyFunctionality() : base() { }

        public AvailableNotesCompanyFunctionality(Company company) : base(company) { }

        public override string getIdentifier() {
            return "COMPANY_DOCUMENTS";
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Druckbare Dokumente", "Füge die Funktion für das Unternehmen hinzu bestimmte Dokumente zu drucken");
        }

        public override void onLoad() {
            Company.registerCompanyAdminElement("DOCUMENT", documentGenerator, documentGenerator);
        }

        private MenuElement documentGenerator(IPlayer player) {
            var documents = Company.getSettings("PRINTABLE_DOCUMENTS");
            var menu = new Menu("Dokumente", "Was möchtest du tun?");

            string[] list;
            using(var db = new ChoiceVDb()) {
                list = db.configvariablefiles.Select(f => f.identifer).ToArray();
            }
            menu.addMenuItem(new InputMenuItem("Dokument hinzufügen", "Füge das Dokument mittels des Identifiers hinzu!", "", "ADD_DOCUMENT")
                .withOptions(list));
            
            foreach(var setting in documents) {
                menu.addMenuItem(new ClickMenuItem(setting.settingsValue, "Entferne diese Dokumentenvorlage vom Drucker der Firma", "", "REMOVE_DOCUMENT", MenuItemStyle.red)
                    .needsConfirmation($"{setting.settingsValue} entfernen?", "Die Firma kann das Dokument dann nicht mehr drucken")
                    .withData(new Dictionary<string, dynamic> { { "Identifier", setting.settingsValue } }));
            }

            return menu;
        }

        private void documentGenerator(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            switch(subEvent) {
                case "ADD_DOCUMENT":
                    var evt = menuItemCefEvent as InputMenuItemEvent;

                    var exists = false;
                    using(var db = new ChoiceVDb()) {
                        var configFile = db.configvariablefiles.Find(evt.input);
                        exists = configFile != null;
                    }

                    if(exists) {
                        Company.setSetting(evt.input, evt.input, "PRINTABLE_DOCUMENTS");
                        player.sendNotification(Constants.NotifactionTypes.Success, $"{evt.input} wurde erfolgreich als Dokument hinzugefügt", "");
                    } else {
                        player.sendBlockNotification($"Das Dokument mit Identifier {evt.input} existiert nicht!", "");
                    }
                    break;
                case "REMOVE_DOCUMENT":
                    var identifier = (string)data["Identifier"];
                    Company.deleteSetting(identifier, "PRINTABLE_DOCUMENTS");
                    break;
            }
        }

        public override void onRemove() {
            Company.deleteSettings("PRINTABLE_DOCUMENTS");
        }

        public List<string> getAvailableDocuments() {
            return Company.getSettings("PRINTABLE_DOCUMENTS").Select(s => s.settingsValue).ToList();
        }
    }

    #endregion

    
    public delegate void OnFlipbookDeletedDelegate(string identifier);
    public class NoteController : ChoiceVScript {
        private static Dictionary<string, string> FlipBookIdentifiersToUrlNames = new Dictionary<string, string>();
        
        public static OnFlipbookDeletedDelegate FlipbookDeletedDelegate;
            
        public NoteController() {
            using(var db = new ChoiceVDb()) {
                foreach(var flipbook in db.flipbooks) {
                    FlipBookIdentifiersToUrlNames.Add(flipbook.identifier, flipbook.urlName);
                }
            }
                
            EventController.addCefEvent("SAVE_NOTE", onNoteSave);
            EventController.addCefEvent("SAVE_INVOICE_FILE", onInvoiceSave);

            EventController.addCefEvent("SAVE_FILE_ITEM", onFileItemSave);

            InteractionController.addVehicleInteractionElement (
                new ConditionalPlayerInteractionMenuElement (
                    () => new ClickMenuItem("Zettel anbringen/abnehmen", "Bringe einen Zettel an oder nimm einen ab", "", "NOTES_ADD_VEHICLE_NOTE"),
                    p => (p as IPlayer).getInventory().hasItem(i => i is File || i is Note),
                    v => {
                        //Check if vehicle has file inventory, and if yes => check if it has a item in it
                        if(v.hasData("FILE_INVENTORY")) {
                            var inv = InventoryController.loadInventory(int.Parse((string)v.getData("FILE_INVENTORY")));
                            if(inv != null) {
                                return inv.getAllItems().Count > 0;
                            }
                        }
                        return false;
                    },
                    false,
                    true
                )
            );
            EventController.addMenuEvent("NOTES_ADD_VEHICLE_NOTE", onNotesAddVehicleNode);

            EventController.PlayerEnterVehicleDelegate += onPlayerEnterVehicle;

            VehicleController.addVehicleSpawnDataSetCallback("FILE_INVENTORY", onVehicleSpawnWithFileInventory);

            EventController.addCefEvent("SAVE_VARIABLE_FILE", onSaveVariableFile);
            EventController.addCefEvent("SIGN_VARIABLE_FILE", onSignVariableFile);

            #region Support (Variable File)

            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Misc,
                    "Erstellte Dokumente",
                    variableFileGenerator
                )
            );
            EventController.addMenuEvent("SUPPORT_CREATE_DOCUMENT", onSupportCreateDocument);
            EventController.addMenuEvent("SUPPORT_DELETE_DOCUMENT", onSupportDeleteDocument);
            EventController.addMenuEvent("SUPPORT_CHANGE_DOCUMENT_INFO", onSupportChangeDocumentInfo);
            EventController.addMenuEvent("SUPPORT_CHANGE_FIELD_INFO", onSupportChangeFieldInfo);
            EventController.addMenuEvent("SUPPORT_DOCUMENT_ADD_FIELD", onSupportDocumentAddField);
            EventController.addMenuEvent("SUPPORT_DOCUMENT_DELETE_FIELD", onSupportDocumentDeleteField);

            #endregion
                
            #region Support (FlipBook)
                
            SupportController.addSupportMenuElement(
                new GeneratedSupportMenuElement(
                    3,
                    SupportMenuCategories.Misc,
                    "FlipBooks erstellen",
                    flipBookGenerator
                )
            );
            EventController.addMenuEvent("SUPPORT_CREATE_FLIPBOOK", onSupportCreateFlipBook); 
            EventController.addMenuEvent("SUPPORT_SHOW_FLIPBOOK", onSupportShowFlipBook);
            EventController.addMenuEvent("SUPPORT_DELETE_FLIPBOOK", onSupportDeleteFlipBook);
            
            EventController.addMenuEvent("SUPPORT_CREATE_NEWSPAPER_ITEM", onSupportCreateNewspaperItem);
                
            #endregion
        }


        public static string createFlipbookFile(string identifier, string name, string owner, string description, string fileInformation, string additionalInfo) {
            var guid = Guid.NewGuid().ToString();
            byte[] bytes;
            if(fileInformation.StartsWith("data:application/pdf;base64,")) {
                bytes = Convert.FromBase64String(fileInformation.Substring("data:application/pdf;base64,".Length));
            } else {
                Logger.logError("Could not create flipbook file. Invalid file information");
                return null;
            } 
                
            var stream = new FileStream(@$"./temp/{guid}.pdf", FileMode.OpenOrCreate);
            var writer = new BinaryWriter(stream);
            writer.Write(bytes);
            writer.Close();

            var worked = false;
            using(var sftpClient = new SftpClient(Config.CefResourcesAddress, 22, Config.CefResourcesUser, Config.CefResourcesPassword)) {
                sftpClient.Connect();
                if(sftpClient.IsConnected) {
                    sftpClient.UploadFile(System.IO.File.OpenRead($"./temp/{guid}.pdf"), $"src/cef/flipbooks/{guid}.pdf");
                    worked = true;
                } 
            }

            if(!worked) {
                Logger.logError("Could not upload flipbook file to server");
                return null;    
            }
                
            using(var db = new ChoiceVDb()) {
                var flipbook = new flipbook {
                    identifier = identifier,
                    urlName = guid,
                    name = name,
                    owner = owner,
                    description = description,
                    additionalData = additionalInfo,
                }; 
                   
                db.flipbooks.Add(flipbook);
                db.SaveChanges();
            }
                
            FlipBookIdentifiersToUrlNames[identifier] =  guid;
                
            return guid; 
        }
            
        public static string getFlipbookUrl(string identifier) {
            return FlipBookIdentifiersToUrlNames.ContainsKey(identifier) ? FlipBookIdentifiersToUrlNames[identifier] : null;
        }

        #region Support (Variable File)

        private Menu variableFileGenerator(IPlayer player) {
            var menu = new Menu("Erstellte Dokumente", "Was möchtest du tun?");

            var createFileMenu = new Menu("Dokument erstellen", "Gib die Daten ein");
            createFileMenu.addMenuItem(new InputMenuItem("Identifier", "Gib einen eindeutigen Identfier für das Dokument ein.", "", ""));
            createFileMenu.addMenuItem(new InputMenuItem("Bild-Url", "Gib die URL des Bildes ein.", "", ""));
            createFileMenu.addMenuItem(new InputMenuItem("Höhe", "Gib die Höhe des Dokumentes in vh an", "", ""));
            createFileMenu.addMenuItem(new InputMenuItem("Breite", "Gib die Breite des Dokumentes in vh an", "", ""));
            createFileMenu.addMenuItem(new MenuStatsMenuItem("Dokument erstellen", "Erstelle das Dokument", "SUPPORT_CREATE_DOCUMENT", MenuItemStyle.green));

            menu.addMenuItem(new MenuMenuItem(createFileMenu.Name, createFileMenu, MenuItemStyle.green));

            SupportController.setCurrentSupportFastAction(player, () => player.showMenu(variableFileGenerator(player)));

            using(var db = new ChoiceVDb()) {
                foreach(var file in db.configvariablefiles.Include(f => f.configvariablefilesfields).OrderBy(f => f.identifer)) {
                    var virtSubMenu = new VirtualMenu(file.identifer, () => {
                        configvariablefile newFile;
                        using(var db = new ChoiceVDb()) {
                            newFile = db.configvariablefiles.Include(f => f.configvariablefilesfields).FirstOrDefault(i => i.identifer == file.identifer);
                        }

                        VariableFile.showVariableFileEvent(player, newFile, true);

                        var subMenu = new Menu(file.identifer, "Was möchtest du tun?");

                        var virtInfoMenu = new VirtualMenu("Dokumentendaten", () => {
                            var infoMenu = new Menu("Dokumentendaten", "Ändere die Daten des Dokuments");
                            infoMenu.addMenuItem(new InputMenuItem("Bild-Url", "Gib die URL des Bildes ein.", "", "SUPPORT_CHANGE_DOCUMENT_INFO")
                                .withStartValue(newFile.backgroundImage).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "Identifier", file.identifer }, { "Type", "URL" } }));
                            infoMenu.addMenuItem(new InputMenuItem("Höhe", "Gib die Höhe des Dokumentes in vh an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_DOCUMENT_INFO")
                                .withStartValue(newFile.height.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "Identifier", file.identifer }, { "Type", "Height" } }));
                            infoMenu.addMenuItem(new InputMenuItem("Breite", "Gib die Breite des Dokumentes in vh an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_DOCUMENT_INFO")
                                .withStartValue(newFile.width.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "Identifier", file.identifer }, { "Type", "Width" } }));


                            return infoMenu;
                        });

                        subMenu.addMenuItem(new MenuMenuItem(virtInfoMenu.Name, virtInfoMenu, MenuItemStyle.normal, null, true));

                        //Fields
                        var virtFieldsMenu = new VirtualMenu("Felder", () => {
                            var fieldsMenu = new Menu("Felder", "Was möchtest du tun?");
                            fieldsMenu.addMenuItem(new InputMenuItem("Feld erstellen", "Erstelle ein Feld. Gib den Identifier ein. Er sollte eindeutig und SINNVOLL sein. z.B. NAME, DATE, DEFENDANT", "", "SUPPORT_DOCUMENT_ADD_FIELD", MenuItemStyle.green)
                                .withNoCloseOnEnter().withData(new Dictionary<string, dynamic> { { "FileIdentifier", file.identifer }, { "Type", "VARIABLE" } }));

                            configvariablefile newFile;
                            using(var db = new ChoiceVDb()) {
                                newFile = db.configvariablefiles.Include(f => f.configvariablefilesfields).FirstOrDefault(i => i.identifer == file.identifer);
                            }

                            foreach(var field in newFile.configvariablefilesfields.Where(f => f.type == "VARIABLE")) {
                                var virtFieldMenu = new VirtualMenu(field.identifier, () => {
                                    configvariablefilesfield newField;
                                    using(var db = new ChoiceVDb()) {
                                        newField = db.configvariablefilesfields.FirstOrDefault(i => i.variableFileIdentifier == file.identifer && i.identifier == field.identifier);
                                    }

                                    var fieldMenu = new Menu(field.identifier, "Ändere die Daten");
                                    fieldMenu.addMenuItem(new InputMenuItem("Abstand links", "Gib den Abstand von links in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.x.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier }, { "Type", "X" } }));
                                    fieldMenu.addMenuItem(new InputMenuItem("Abstand oben", "Gib den Abstand von oben in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.y.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier }, { "Type", "Y" } }));
                                    fieldMenu.addMenuItem(new InputMenuItem("Schriftgröße", "Gib die Schriftgröße in vh an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.fontSize.ToString()).withEventOnAnyUpdate().withEnterDisabled().withNumberStep(0.1f)
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier }, { "Type", "FontSize" } }));
                                    fieldMenu.addMenuItem(new InputMenuItem("Breite", "Gib die Breite in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.width.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier }, { "Type", "Width" } }));
                                    fieldMenu.addMenuItem(new InputMenuItem("Höhe", "Gib die Höhe in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.height.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newField.identifier }, { "Type", "Height" } }));
                                    fieldMenu.addMenuItem(new InputMenuItem("Placeholder", "Gib den Placeholder für das Feld an (ausgegrauter Text im Hintergrund)", "", "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newField.placeholder).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier }, { "Type", "Placeholder" } }));

                                    fieldMenu.addMenuItem(new ClickMenuItem("Feld löschen", "Lösche das Feld", "", "SUPPORT_DOCUMENT_DELETE_FIELD", MenuItemStyle.red)
                                        .needsConfirmation("Feld löschen?", "Das Feld wirklich löschen?")
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", field.identifier } }));

                                    return fieldMenu;
                                });

                                fieldsMenu.addMenuItem(new MenuMenuItem(virtFieldMenu.Name, virtFieldMenu, MenuItemStyle.normal, null, true));
                            }

                            return fieldsMenu;
                        });

                        subMenu.addMenuItem(new MenuMenuItem(virtFieldsMenu.Name, virtFieldsMenu));


                        //Signatures
                        var virtSignaturesMenu = new VirtualMenu("Signaturen", () => {
                            var signaturesMenu = new Menu("Signaturen", "Was möchtest du tun?");
                            signaturesMenu.addMenuItem(new InputMenuItem("Signature erstellen", "Erstelle ein Feld. Gib den Identifier ein. Er sollte eindeutig und SINNVOLL sein. z.B. NAME, DATE, DEFENDANT", "", "SUPPORT_DOCUMENT_ADD_FIELD", MenuItemStyle.green)
                                .withNoCloseOnEnter().withData(new Dictionary<string, dynamic> { { "FileIdentifier", file.identifer }, { "Type", "SIGNATURE" } }));

                            configvariablefile newFile;
                            using(var db = new ChoiceVDb()) {
                                newFile = db.configvariablefiles.Include(f => f.configvariablefilesfields).FirstOrDefault(i => i.identifer == file.identifer);
                            }


                            foreach(var signature in newFile.configvariablefilesfields.Where(f => f.type == "SIGNATURE")) {
                                var virtSignatureMenu = new VirtualMenu(signature.identifier, () => {
                                    configvariablefilesfield newSignature;
                                    using(var db = new ChoiceVDb()) {
                                        newSignature = db.configvariablefilesfields.FirstOrDefault(i => i.variableFileIdentifier == file.identifer && i.identifier == signature.identifier);
                                    }

                                    var signatureMenu = new Menu(signature.identifier, "Ändere die Daten");
                                    signatureMenu.addMenuItem(new InputMenuItem("Abstand links", "Gib den Abstand von links in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newSignature.x.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier }, { "Type", "X" } }));
                                    signatureMenu.addMenuItem(new InputMenuItem("Abstand oben", "Gib den Abstand von oben in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newSignature.y.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier }, { "Type", "Y" } }));
                                    signatureMenu.addMenuItem(new InputMenuItem("Schriftgröße", "Gib die Schriftgröße in vh an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newSignature.fontSize.ToString()).withEventOnAnyUpdate().withEnterDisabled().withNumberStep(0.1f)
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier }, { "Type", "FontSize" } }));
                                    signatureMenu.addMenuItem(new InputMenuItem("Breite", "Gib die Breite in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newSignature.width.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier }, { "Type", "Width" } }));
                                    signatureMenu.addMenuItem(new InputMenuItem("Beschreibung", "Gib die Beschriftung des Buttons an. Wenn dieses Feld leer ist dann wird eine Test-Unterschrift gesetzt. DER BUTTON SOLLTE ABER EINEN NAMEN HABEN!", "", "SUPPORT_CHANGE_FIELD_INFO")
                                        .withStartValue(newSignature.placeholder).withEventOnAnyUpdate().withEnterDisabled()
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier }, { "Type", "Placeholder" } }));

                                    signatureMenu.addMenuItem(new ClickMenuItem("Feld löschen", "Lösche das Feld", "", "SUPPORT_DOCUMENT_DELETE_FIELD", MenuItemStyle.red)
                                        .needsConfirmation("Feld löschen?", "Das Feld wirklich löschen?")
                                        .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", newSignature.identifier } }));

                                    return signatureMenu;
                                });

                                signaturesMenu.addMenuItem(new MenuMenuItem(virtSignatureMenu.Name, virtSignatureMenu, MenuItemStyle.normal, null, true));
                            }

                            return signaturesMenu;
                        });

                        subMenu.addMenuItem(new MenuMenuItem(virtSignaturesMenu.Name, virtSignaturesMenu));

                        //Save Button
                        var virtSaveButtonMenu = new VirtualMenu("Speicher-Button", () => {
                            configvariablefilesfield saveButton;
                            using(var db = new ChoiceVDb()) {
                                saveButton = db.configvariablefilesfields.FirstOrDefault(i => i.variableFileIdentifier == file.identifer && i.identifier == "SAVE_BUTTON" && i.type == "SAVE_BUTTON");
                            }

                            var buttonMenu = new Menu("Speicher-Button", "Ändere die Daten");
                            buttonMenu.addMenuItem(new InputMenuItem("Abstand links", "Gib den Abstand von links in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                .withStartValue(saveButton.x.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", saveButton.identifier }, { "Type", "X" } }));
                            buttonMenu.addMenuItem(new InputMenuItem("Abstand oben", "Gib den Abstand von oben in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                .withStartValue(saveButton.y.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", saveButton.identifier }, { "Type", "Y" } }));
                            buttonMenu.addMenuItem(new InputMenuItem("Schriftgröße", "Gib die Schriftgröße in vh an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                .withStartValue(saveButton.fontSize.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", saveButton.identifier }, { "Type", "FontSize" } }));
                            buttonMenu.addMenuItem(new InputMenuItem("Breite", "Gib die Breite in % an", "", InputMenuItemTypes.number, "SUPPORT_CHANGE_FIELD_INFO")
                                .withStartValue(saveButton.width.ToString()).withEventOnAnyUpdate().withEnterDisabled()
                                .withData(new Dictionary<string, dynamic> { { "FileIdentifer", file.identifer }, { "FieldIdentifer", saveButton.identifier }, { "Type", "Width" } }));

                            return buttonMenu;
                        });
                        subMenu.addMenuItem(new MenuMenuItem(virtSaveButtonMenu.Name, virtSaveButtonMenu, MenuItemStyle.normal, null, true));

                        subMenu.addMenuItem(new ClickMenuItem("Dokument löschen", "Lösche das ausgewählte Dokument", "", "SUPPORT_DELETE_DOCUMENT", MenuItemStyle.red)
                            .needsConfirmation("Dokument löschen?", "Dokument wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Identifier", file.identifer } }));
                        return subMenu;
                    });
                    menu.addMenuItem(new MenuMenuItem(virtSubMenu.Name, virtSubMenu, MenuItemStyle.normal, null, true));
                }
            }

            return menu;
        }

        private bool onSupportCreateDocument(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var identifierEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var urlEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var heightEvt = evt.elements[2].FromJson<InputMenuItemEvent>();
            var widthEvt = evt.elements[3].FromJson<InputMenuItemEvent>();

            using(var db = new ChoiceVDb()) {
                var newDbFile = new configvariablefile {
                    identifer = identifierEvt.input,
                    backgroundImage = urlEvt.input,
                    height = float.Parse(heightEvt.input),
                    width = float.Parse(widthEvt.input),
                };

                db.configvariablefiles.Add(newDbFile);
                db.SaveChanges();

                db.configvariablefilesfields.Add(new configvariablefilesfield {
                    variableFileIdentifier = identifierEvt.input,
                    type = "SAVE_BUTTON",
                    identifier = "SAVE_BUTTON",
                    x = 35,
                    y = 95,
                    fontSize = 1.15f,
                    width = 50,
                    height = 0,
                    placeholder = "",
                });

                db.SaveChanges();

                player.sendNotification(Constants.NotifactionTypes.Info, "Das Dokument wurde erfolgreich erstellt! Drücke F6 um direkt wieder das Mneü zu öffnen.", "");
                Logger.logInfo(LogCategory.Support, LogActionType.Created, player, $"Added Document with identifier: {identifierEvt.input}");
            }

            return true;
        }


        private bool onSupportDeleteDocument(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var identifier = (string)data["Identifier"];

            using(var db = new ChoiceVDb()) {
                var file = db.configvariablefiles.FirstOrDefault(f => f.identifer == identifier);

                db.configvariablefiles.Remove(file);

                db.SaveChanges();

                player.sendNotification(Constants.NotifactionTypes.Warning, $"Das Dokument mit Identifier: {identifier} wurde erfolgreich gelöscht!", "");
                Logger.logInfo(LogCategory.Support, LogActionType.Removed, player, $"Deleted Document with identifier: {identifier}");
            }

            return true;
        }

        private bool onSupportChangeDocumentInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var identifier = (string)data["Identifier"];
            var type = (string)data["Type"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var file = db.configvariablefiles.Include(f => f.configvariablefilesfields).FirstOrDefault(f => f.identifer == identifier);

                switch(type) {
                    case "Identifier":
                        file.identifer = evt.input;
                        break;
                    case "URL":
                        file.backgroundImage = evt.input;
                        break;
                    case "Height":
                        file.height = float.Parse(evt.input);
                        break;
                    case "Width":
                        file.width = float.Parse(evt.input);
                        break;
                }

                db.SaveChanges();

                Logger.logInfo(LogCategory.Support, LogActionType.Updated, player, $"Updated Document with identifier: {file.identifer}");
                VariableFile.showVariableFileEvent(player, file, true);
            }

            return true;
        }

        private bool onSupportDocumentAddField(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var fileIdentifier = data["FileIdentifier"];
            var type = (string)data["Type"];

            using(var db = new ChoiceVDb()) {
                var newField = new configvariablefilesfield {
                    variableFileIdentifier = fileIdentifier,
                    identifier = evt.input,
                    x = 0,
                    y = 0,
                    width = 10,
                    height = 10,
                    fontSize = 1,
                    type = type,
                    placeholder = "",
                };

                db.configvariablefilesfields.Add(newField);

                db.SaveChanges();
                Logger.logInfo(LogCategory.Support, LogActionType.Created, player, $"Added field with identifier {evt.input} to file with identifier: {fileIdentifier}");
                player.sendNotification(Constants.NotifactionTypes.Info, "Das Feld wurde hinzugefügt. Gehe ein Menü-Ebene höher und öffne dieses Menü wieder. Danach ist das Feld sichtbar!", "");
            }

            return true;
        }


        private bool onSupportDocumentDeleteField(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var fileIdentifier = (string)data["FileIdentifer"];
            var fieldIdentifier = (string)data["FieldIdentifer"];

            using(var db = new ChoiceVDb()) {
                var field = db.configvariablefilesfields.FirstOrDefault(f => f.variableFileIdentifier == fileIdentifier && f.identifier == fieldIdentifier);

                db.configvariablefilesfields.Remove(field);

                db.SaveChanges();
                Logger.logInfo(LogCategory.Support, LogActionType.Removed, player, $"Deleted field with identifier {fieldIdentifier} from file with identifier: {fileIdentifier}");

                player.sendNotification(Constants.NotifactionTypes.Warning, "Das Feld wurde erfolgreich gelöscht!", "");
            }

            return true;
        }

        private bool onSupportChangeFieldInfo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var fileIdentifier = (string)data["FileIdentifer"];
            var fieldIdentifier = (string)data["FieldIdentifer"];
            var type = (string)data["Type"];

            var evt = menuItemCefEvent as InputMenuItemEvent;

            using(var db = new ChoiceVDb()) {
                var field = db.configvariablefilesfields.Include(f => f.variableFileIdentifierNavigation).ThenInclude(f => f.configvariablefilesfields).FirstOrDefault(f => f.variableFileIdentifier == fileIdentifier && f.identifier == fieldIdentifier);

                switch(type) {
                    case "X":
                        field.x = float.Parse(evt.input);
                        break;
                    case "Y":
                        field.y = float.Parse(evt.input);
                        break;
                    case "FontSize":
                        field.fontSize = float.Parse(evt.input);
                        break;
                    case "Height":
                        field.height = float.Parse(evt.input);
                        break;
                    case "Width":
                        field.width = float.Parse(evt.input);
                        break;
                    case "Placeholder":
                        field.placeholder = evt.input;
                        break;
                }

                db.SaveChanges();
                Logger.logTrace(LogCategory.Support, LogActionType.Updated, player, $"Updated field with identifier {fieldIdentifier} of file with identifier: {fileIdentifier}");

                VariableFile.showVariableFileEvent(player, field.variableFileIdentifierNavigation, true);
            }

            return true;
        }

        #endregion

        #region Support (FlipBook)
            
        private Menu flipBookGenerator(IPlayer player) {
            var menu = new Menu("FlipBooks erstellen", "Was möchtest du tun?");

            var createFlipBookMenu = new Menu("FlipBook erstellen", "Gib die Daten ein");
            createFlipBookMenu.addMenuItem(new InputMenuItem("Identifier", "Gib einen eindeutigen Identfier für das FlipBook ein.", "", ""));
            createFlipBookMenu.addMenuItem(new InputMenuItem("Name", "Gib den Namen des FlipBooks ein.", "", ""));
            createFlipBookMenu.addMenuItem(new InputMenuItem("Beschreibung", "Gib eine Beschreibung für das FlipBook ein.", "", ""));
            createFlipBookMenu.addMenuItem(new FileMenuItem("PDF-Datei", "Lade die PDF-Datei hoch", "", ""));
            createFlipBookMenu.addMenuItem(new MenuStatsMenuItem("FlipBook erstellen", "Erstelle das FlipBook", "SUPPORT_CREATE_FLIPBOOK", MenuItemStyle.green));
            menu.addMenuItem(new MenuMenuItem(createFlipBookMenu.Name, createFlipBookMenu, MenuItemStyle.green));

            using(var db = new ChoiceVDb()) {
                foreach(var flipbook in db.flipbooks) {
                    var flipbookMenu = new Menu(flipbook.name, "Was möchtest du tun?");
                        
                    flipbookMenu.addMenuItem(new StaticMenuItem("Identifier", "Der Identifier des Flipbooks", flipbook.identifier));
                    flipbookMenu.addMenuItem(new StaticMenuItem("Name", "Der Name des Flipbooks", flipbook.name));
                    flipbookMenu.addMenuItem(new StaticMenuItem("Url-Name", "Der Name der PDF-Datei", $"{flipbook.urlName}.pdf"));
                    flipbookMenu.addMenuItem(new StaticMenuItem("Beschreibung", "Die Beschreibung des Flipbooks", flipbook.description));
                        
                    flipbookMenu.addMenuItem(new ClickMenuItem("FlipBook anzeigen", "Zeige das FlipBook an", "", "SUPPORT_SHOW_FLIPBOOK")
                        .withData(new Dictionary<string, dynamic> { { "Identifier", flipbook.identifier } }));
                    flipbookMenu.addMenuItem(new InputMenuItem("Item erstellen", "Erstelle ein Item mit dem FlipBook", "Item-Beschreibung", "SUPPORT_CREATE_NEWSPAPER_ITEM")
                        .withData(new Dictionary<string, dynamic> { { "Identifier", flipbook.identifier } }));
                    flipbookMenu.addMenuItem(new ClickMenuItem("FlipBook löschen", "Lösche das ausgewählte FlipBook", "", "SUPPORT_DELETE_FLIPBOOK", MenuItemStyle.red)
                        .needsConfirmation("FlipBook löschen?", "FlipBook wirklich löschen?").withData(new Dictionary<string, dynamic> { { "Identifier", flipbook.identifier } }));
                        
                    menu.addMenuItem(new MenuMenuItem(flipbookMenu.Name, flipbookMenu)); 
                }
            } 
                
            return menu;
        }
            
        private bool onSupportCreateFlipBook(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var evt = menuitemcefevent as MenuStatsMenuItemEvent;
                
            var identifier = evt.elements[0].FromJson<InputMenuItemEvent>().input;
            var name = evt.elements[1].FromJson<InputMenuItemEvent>().input;
            var description = evt.elements[2].FromJson<InputMenuItemEvent>().input;
            var fileInformation = evt.elements[3].FromJson<FileMenuItem.FileMenuItemEvent>();
                
            createFlipbookFile(identifier, name, "SYSTEM", description, fileInformation.fileData, "");
                
            player.sendNotification(Constants.NotifactionTypes.Success, "Das Flipbook wurde erfolgreich erstellt!", "");
            return true;
        }

        private bool onSupportShowFlipBook(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var identifier = (string)data["Identifier"];
            player.emitCefEventWithBlock(new FlipBookEvent($"{getFlipbookUrl(identifier)}.pdf"), "FLIPBOOK");
            return true; 
        }
        
        
        private bool onSupportCreateNewspaperItem(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var identifier = (string)data["Identifier"];
            
            var evt = menuitemcefevent as InputMenuItemEvent;
            
            var cfg = InventoryController.getConfigItemByCodeIdentifier("NEWSPAPER");
            var item = new FlipBook(cfg, identifier, evt.input);

            player.getInventory().addItem(item, true);
            
            player.sendNotification(Constants.NotifactionTypes.Success, "Das Item wurde erfolgreich erstellt!", "");
            
            return true;
        }
            
        private bool onSupportDeleteFlipBook(IPlayer player, string itemevent, int menuitemid, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) {
            var identifier = (string)data["Identifier"];
                
            using(var sftpClient = new SftpClient(Config.CefResourcesAddress, 22, Config.CefResourcesUser, Config.CefResourcesPassword)) {
                sftpClient.Connect();
                if(sftpClient.IsConnected) {
                    sftpClient.DeleteFile($"src/cef/flipbooks/{getFlipbookUrl(identifier)}.pdf");
                }
            }
                
            using(var db = new ChoiceVDb()) {
                var flipbook = db.flipbooks.FirstOrDefault(f => f.identifier == identifier);
                db.flipbooks.Remove(flipbook);
                db.SaveChanges();
            }
                
            FlipbookDeletedDelegate?.Invoke(identifier);
            
            player.sendNotification(Constants.NotifactionTypes.Warning, $"Das Flipbook mit Identifier: {identifier} wurde erfolgreich gelöscht!", "");
            return true;
        }
            
        #endregion
            
        public record VariableFileData(string identifier, string text);

        public class VariableFileSaveCefEvent {
            public VariableFileData[] data;
            public int id;
        }

        private void onSaveVariableFile(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var fileEvt = new VariableFileSaveCefEvent();
            fileEvt.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<VariableFile>(n => n.Id == fileEvt.id);
            if(item != null) {
                foreach(var data in fileEvt.data) {
                    if(!(item.Data.hasKey(data.identifier) && data.text == "")) {
                        item.Data[data.identifier] = data.text;
                    }
                }

                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Player saved Variable-File-Item with Id: {fileEvt.id}");
            }
        }

        public class VariableFileSignCefEvent {
            public string identifier;
            public VariableFileData[] data;
            public int id;
        }

        private void onSignVariableFile(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var fileEvt = new VariableFileSignCefEvent();
            fileEvt.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<VariableFile>(n => n.Id == fileEvt.id);
            if(item != null) {
                foreach(var data in fileEvt.data) {
                    if(!(item.Data.hasKey(data.identifier) && data.text == "")) {
                        item.Data[data.identifier] = data.text;
                    }
                }

                if(!item.Data.hasKey(fileEvt.identifier)) {
                    item.Data[fileEvt.identifier] = $"{DateTime.Now.ToString("d MMM")}, {player.getCharacterShortName()}";
                }
                item.Finalized = true;
                Logger.logTrace(LogCategory.Player, LogActionType.Updated, player, $"Player signed Variable-File-Item with Id: {fileEvt.id}");
            }
        }

        private bool onNotesAddVehicleNode(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var target = (ChoiceVVehicle)data["InteractionTarget"];

            if(target.hasData("FILE_INVENTORY")) {
                var inv = InventoryController.loadInventory(int.Parse((string)target.getData("FILE_INVENTORY")));
                if(inv != null) {
                    inv.addBlockStatement(new InventoryAddBlockStatement(this, i => !((i is Note) || (i is File))));
                    InventoryController.showMoveInventory(player, player.getInventory(), inv, (_1, _2, _3, _4, _5) => {
                        var anim = AnimationController.getAnimationByName("TAKE_STUFF");
                        player.playAnimation(anim);

                        return true;
                    });
                } else {
                    player.sendBlockNotification("Dev-Code: NoteRat, melde dich beim Dev Team!", "Dev-Code: NoteRat");
                }
            } else {
                var inv = InventoryController.createInventory(target.VehicleId, 5, InventoryTypes.Vehicle);
                inv.addBlockStatement(new InventoryAddBlockStatement(this, i => !((i is Note) || (i is File))));
                target.setPermanentData("FILE_INVENTORY", inv.Id.ToString());
                InventoryController.showMoveInventory(player, player.getInventory(), inv, (_1, _2, _3, _4, _5) => {
                    var anim = AnimationController.getAnimationByName("TAKE_STUFF");
                    player.playAnimation(anim);

                    return true;
                });
            }

            return true;
        }

        private void onPlayerEnterVehicle(IPlayer player, ChoiceVVehicle vehicle, byte seatId) {
            if(vehicle.hasData("FILE_INVENTORY")) {
                var inv = InventoryController.loadInventory(int.Parse((string)vehicle.getData("FILE_INVENTORY")));
                if(inv != null && inv.getAllItems().Count > 0) {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Ein Zettel steckt an der Windschutzscheibe!", "Zettel an Windschutzscheibe", Constants.NotifactionImages.Car);
                }
            }
        }

        private void onVehicleSpawnWithFileInventory(ChoiceVVehicle vehicle, vehiclesdatum data) {
            var inv = InventoryController.loadInventory(int.Parse(data.value));
            if(inv == null) {
                vehicle.resetPermantData("FILE_INVENTORY");
            } else if(inv.getAllItems().Count <= 0) {
                InventoryController.destroyInventory(inv);
                vehicle.resetPermantData("FILE_INVENTORY");
            }
        }

        private void onNoteSave(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var e = evt.Data.FromJson<NoteCefEvent>();
            var title = e.title;
            var text = e.text;
            var finalized = e.readOnly;
            var id = e.id;

            var item = player.getInventory().getItem<Note>(n => n.Id == id);

            if(item != null) {
                if(item.Title != title) {
                    item.Title = title;
                }

                if(item.Text != text) {
                    item.Text = text;
                }

                item.Finalized = finalized;
                item.updateDescription();
            } else {
                player.sendBlockNotification("Ein Fehler ist aufgetreten. Ist das Notizitem noch in deinem Inventar?", "Notiz weg?");
            }
        }

        private void onInvoiceSave(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var e = new InvoiceFileCefEvent();
            e.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<InvoiceFile>(n => n.Id == e.id);

            if(item != null) {
                e.populateItem(item, player);
                item.updateDescription();
            } else {
                player.sendBlockNotification("Ein Fehler ist aufgetreten. Ist das Notizitem noch in deinem Inventar?", "Notiz weg?");
            }
        }

        private void onFileItemSave(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var e = evt.Data.FromJson<FileItemCefEvent>();

            var item = player.getInventory().getItem<FileItem>(n => n.Id == e.id);
            item.fillFromCef(e, player);
        }
    }
}

