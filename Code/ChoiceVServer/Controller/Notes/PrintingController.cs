using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller;

public class PrintingController : ChoiceVScript {
    public PrintingController() {
        InteractionController.addObjectInteractionCallback("ON_PRINTER_INTERACT", "Drucker-Interaktion", onPrinterInteraction);

        EventController.addMenuEvent("GET_EMPTY_INVOICE", getCompanyInvoice);

        EventController.addMenuEvent("COPY_DOCUMENT", onCompanyDocument);
        EventController.addMenuEvent("PRINT_AVAILABLE_DOCUMENT", onCompanyPrintDocument);
    }

    public static void onPrinterInteraction(IPlayer player, string modelName, string info, Position objectPosition, float objectHeading, bool isBroken, ref Menu menu) {
        var companies = CompanyController.getCompanies(player).Where(c => CompanyController.hasPlayerPermission(player, c, "ISSUE_RECEIPT")).ToList();
        if (info == "onlyBill") {
            if (companies.Count == 0) {
                player.sendBlockNotification("Du hast keine Berechtigung Dokumente auszudrucken!", "Keine Berechtigung", Constants.NotifactionImages.Printer);
                return;
            } else {
                player.showMenu(getBillMenu(companies));
            }
        } else {
            var printerMenu = new Menu("Drucker/Kopierer", "Was möchtest du tuen?");

            var billMenu = getBillMenu(companies);
            printerMenu.addMenuItem(new MenuMenuItem(billMenu.Name, billMenu));

            var docs = player.getInventory().getItems<File>(i => !i.IsCopy);
            var copyMenu = new Menu("Dokument kopieren", "Kopiere ein Dokument");
            foreach (Item doc in docs) {
                var data = new Dictionary<string, dynamic> { { "Item", doc } };

                copyMenu.addMenuItem(new ClickMenuItem($"{doc.Name}: {(doc as File).FileId}", doc.Description, "", "COPY_DOCUMENT").withData(data).needsConfirmation("Wirklich kopieren?", $"Kopiere Dokument: {(doc as File).FileId}"));
            }

            printerMenu.addMenuItem(new MenuMenuItem(copyMenu.Name, copyMenu));

            var list = new List<configitem>();
            var printCompanies = CompanyController.getCompanies(player).Where(c => CompanyController.hasPlayerPermission(player, c, "PRINT_DOCUMENTS")).ToList();

            foreach (var printCompany in printCompanies) {
                var funct = printCompany.getFunctionality<AvailableNotesCompanyFunctionality>();
                if (funct != null) {
                    var availDocs = funct.getAvailableDocuments();
                    foreach (var availDoc in availDocs) {
                        var cfg = InventoryController.getConfigItemForType<VariableFile>(i => availDoc == i.additionalInfo.Split("#")[0]);
                        list.Add(cfg);
                    }
                }
            }

            var availMenu = new Menu("Dokumentvorlagen ausdrucken", "Welches Dokument möchtest du drucken?");
            foreach (var item in list) {
                if (item != null) {
                    availMenu.addMenuItem(new InputMenuItem(item.name, $"Drucke X {item.name} aus. Wähle die Anzahl aus. Max 5. Leerlassen für 1.", "Anzahl", InputMenuItemTypes.number, "PRINT_AVAILABLE_DOCUMENT").withData(new Dictionary<string, dynamic> { { "Cfg", item } }));
                }
            }

            printerMenu.addMenuItem(new MenuMenuItem(availMenu.Name, availMenu));

            menu.addMenuItem(new MenuMenuItem(printerMenu.Name, printerMenu));
        }
    }

    private static Menu getBillMenu(List<Company> companies) {
        var menu = new Menu("Rechnung ausstellen", "Von welcher Firma?");
        foreach (var company in companies) {
            var data = new Dictionary<string, dynamic> { { "Company", company } };
            menu.addMenuItem(new ClickMenuItem(company.Name, $"Erhalte einen leeren Rechnungsbeleg von {company.Name}", "", "GET_EMPTY_INVOICE").withData(data));
        }

        return menu;
    }

    private static bool getCompanyInvoice(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var company = (Company)data["Company"];
        
        
        if(couldntRemovePaper(player, 1)) {
            player.sendBlockNotification("Rechnung konnte nicht ausgedruckt werden. Nicht genug Papier dabei!", "Kein Papier!", Constants.NotifactionImages.Printer);
            return true;
        }
        
        if (company != null) {
            var configItem = InventoryController.getConfigItemForType<InvoiceFile>();
            if (configItem != null) {
                player.getInventory().addItem(new InvoiceFile(configItem, company), true);
                player.sendNotification(Constants.NotifactionTypes.Success, $"Rechnung(en) für {company.Name} ausgedruckt", "Rechnung ausgedruckt", Constants.NotifactionImages.Printer);
            } else {
                Logger.logError($"getCompanyInvoice: InvoiceFile Item not found!",
                    $"Fehler beim Item Spawnen. Kein Config-Item für Rechnung!", player);
            }
        } else {
            Logger.logWarning(LogCategory.System, LogActionType.Blocked, "$getCompanyInvoice: Company was null");
        }

        return true;
    }

    private static bool onCompanyDocument(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var item = (File)data["Item"];
        
        if(couldntRemovePaper(player, 1)) {
            player.sendBlockNotification("Dokument konnte nicht kopiert werden. Nicht genug Papier dabei!", "Kein Papier!", Constants.NotifactionImages.Printer);
            return false;
        }

        if (player.getInventory().addItem(item.getCopy(), true)) {
            player.sendNotification(Constants.NotifactionTypes.Success, $"Das Dokument {item.FileId} wurde erfolgreich kopiert", "Dokument kopiert", Constants.NotifactionImages.Printer);
        } else {
            player.sendBlockNotification($"Das Dokument {item.FileId} konnte nicht blockiert werden", "Inventar voll", Constants.NotifactionImages.Printer);
        }

        return true;
    }

    private static bool onCompanyPrintDocument(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var cfg = (configitem)data["Cfg"];

        var evt = menuItemCefEvent as InputMenuItemEvent;
        try {
            var amount = 1;
            if (evt.input != "" && evt.input != null) {
                amount = Math.Max(1, Math.Min(5, int.Parse(evt.input)));
            }

            if(couldntRemovePaper(player, 1)) {
                player.sendBlockNotification("Dokument konnte nicht gedruckt werden. Nicht genug Papier dabei!", "Kein Papier!", Constants.NotifactionImages.Printer);
                return false;
            }


            var items = InventoryController.createItems(cfg, amount);
            player.getInventory().addItems(items, true);
            

            player.sendNotification(Constants.NotifactionTypes.Success, $"Es wurden erfolgreich {amount} {cfg.name} ausgedruckt!", "Dokumente gedruckt", Constants.NotifactionImages.Printer);
        } catch (Exception) {
            player.sendBlockNotification("Der Input war keine Zahl!", "");
        }
        return true;
    }

    private static bool couldntRemovePaper(IPlayer player, int amount) {
        var paperItem = player.getInventory().getItem(i => i.ConfigItem.codeIdentifier == "PAPER");
        return paperItem == null || !player.getInventory().removeItem(paperItem, amount);
    }
}