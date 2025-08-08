using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;

namespace ChoiceVServer.Controller.Crime_Stuff.CrimeModules.Shops;

public class BaseTraderModule : CrimeNetworkModule {
    private string ShopName;

    public BaseTraderModule(ChoiceVPed ped, CrimeNetworkPillar pillar) : base(ped, pillar, "Ausrüster/Ankäufer", "BASE_SHOP") {

    }

    public BaseTraderModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, CrimeNetworkController.getPillarById((int?)settings["BaseShop_CrimePillar"] ?? -1), "Basis-Ausrüster", "BASE_SHOP") {
       if(settings.ContainsKey("BASE_TRADER_SHOPNAME")) {
            ShopName = settings["BASE_TRADER_SHOPNAME"];
       } 
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Ausrüster/Ankäufer", $"Fügt die Funktionalität Ankäufers/Ausrüsters hinzu der {Pillar?.Name ?? "Keiner"} Crime Säule hinzu", "");
    }

    public override List<MenuItem> getCrimeMenuItems(IPlayer player) {
        var list = new List<MenuItem>();

        var reputation = player.getCrimeReputation();
        var menu = new Menu("Basis-Ausrüster", "Wähle eine Kategorie");

        //Buying
        var items = CrimeBaseTraderController.getItemsForShop(ShopName);
        var buyMenu = new Menu("Waren kaufen", "Was möchtest du verkaufen?");
        var sellMenu = new Menu("Waren verkaufen", "Was möchtest du verkaufen?");

        var buyMoreOptions = false;
        var sellMoreOptions = false;
        foreach(var item in items) {
            if (item.ReputationRequirement > reputation.getPillarReputation(Pillar)) {
                buyMoreOptions = item.SellNotBuy || buyMoreOptions;
                sellMoreOptions = !item.SellNotBuy || sellMoreOptions;
                continue;
            }

            var priceStr = $"${item.Price}";
            if(item.PriceIsFavors) {
                priceStr = $"{item.Price} Gefallen";
            }

            var itemMenu = new Menu(item.Item.name, "Wieviel möchtest du verkaufen?");
            itemMenu.addMenuItem(new StaticMenuItem("Preis", $"Der Preis der Ware ist: {priceStr}", priceStr));
            itemMenu.addMenuItem(new StaticMenuItem("Max. Anzahl pro Anfrage", $"Du kannst nur maximal {item.MaxPerRequest} Einheiten dieser Ware auf einmal anfragen.", item.MaxPerRequest.ToString()));

            var sellBuyStr = "Verkaufe";
            if(item.SellNotBuy) {
                sellBuyStr = "Kaufe";
            }

            itemMenu.addMenuItem(new InputMenuItem($"{sellBuyStr} {item.Item.name}", "Mache eine Anfrage die angegebene Waren zu kaufen/verkaufen. Dies kann einige Zeit dauern", "", InputMenuItemTypes.number, ""));
            itemMenu.addMenuItem(new CheckBoxMenuItem("Updates über Telefon", "Gib dem Ausrüster deine Handynummer damit er Updates dahin senden kann. Wenn du dies nicht möchtest kannst du periodisch zu Ausrüstern gehen um dir Updates zu holen", false, ""));
            itemMenu.addMenuItem(new MenuStatsMenuItem($"{sellBuyStr} {item.Item.name}", "Mache eine Anfrage die angegebene Waren zu kaufen/verkaufen. Dies kann einige Zeit dauern", "", "ON_PLAYER_BUY_SELL_CRIME_TRADER_ITEM", MenuItemStyle.green)
                .withData(new Dictionary<string, dynamic> {{"Item", item}, {"Pillar", Pillar}})
                .needsConfirmation($"{sellBuyStr} {item.Item.name}", "Anfrage wirklich stellen?"));

            var menuMenuItem = new MenuMenuItem(item.Item.name, itemMenu, $"Untermenü für {item.Item.name}", priceStr);
            if(item.SellNotBuy) {
                buyMenu.addMenuItem(menuMenuItem);
            } else {
                sellMenu.addMenuItem(menuMenuItem);
            }
        }

        if(buyMoreOptions) {
            buyMenu.addMenuItem(new StaticMenuItem("Weitere Waren zurückgehalten..", "Du merkst, dass der Ausrüster noch einige Waren zurückhält, da ihm deine Reputation noch nicht ausreicht", ""));
        }
        
        if(sellMoreOptions) {
            sellMenu.addMenuItem(new StaticMenuItem("Weitere Waren zurückgehalten..", "Du merkst, dass der Ausrüster noch einige Waren zurückhält, da ihm deine Reputation noch nicht ausreicht", ""));
        }

        if(buyMenu.getMenuItemCount() > 0) {
            menu.addMenuItem(new MenuMenuItem(buyMenu.Name, buyMenu));
        }

        if(sellMenu.getMenuItemCount() > 0) {
            menu.addMenuItem(new MenuMenuItem(sellMenu.Name, sellMenu));
        }


        //Current requests

        var requests = CrimeBaseTraderController.getCrimeTradeRequestsForPlayer(player);

        if(requests.Count > 0) {
            var currentRequestMenu = new Menu("Aktuelle Anträge", "Siehe dir die Anträge an");

            foreach(var request in requests) {
                var buySellStr = request.buyNotSell ? "Kauf" : "Verkauf";
                var reqMenu = new Menu($"{buySellStr} {request.amount}x {request.configCrimeTraderItem.configItem.name}", "Was möchtest du tun?");

                reqMenu.addMenuItem(new StaticMenuItem("Kauf oder Verkauf:", "Ob die waren von DIR geKAUFt oder VERKAUFt wird.", buySellStr));
                reqMenu.addMenuItem(new StaticMenuItem("Ware:", $"Die Ware, die in diesem Antrag angefragt/verkauft wird", request.configCrimeTraderItem.configItem.name));
                reqMenu.addMenuItem(new StaticMenuItem("Anzahl:", "Die Anzahl an Waren die angefragt/geliefert werden sollen", request.amount.ToString()));

                if(request.lockerDrawerId != null) {
                    reqMenu.addMenuItem(new StaticMenuItem("Schließfach", $"Das Schließfach für diesen Antrag ist das Schließfach Nr. {request.lockerDrawer.displayNumber} am Ort {request.lockerDrawer.locker.name}", $"Nr. {request.lockerDrawer.displayNumber} {request.lockerDrawer.locker.name}"));
                    reqMenu.addMenuItem(new StaticMenuItem("Schließfach Kombination", $"Die Kombination vom Schließfach", request.lockerDrawer.combination));
                } else {
                    reqMenu.addMenuItem(new StaticMenuItem("Abgabeort noch nicht erörtert", "Die Anfrage wurde noch nicht bearbeitet. Komme später wieder oder achte auf dein Handy!", "", MenuItemStyle.yellow));
                }

                if(!request.buyNotSell) {
                    if(request.canBeCollected) {
                        reqMenu.addMenuItem(new ClickMenuItem("Verkaufspreis annehmen", "Nimm den Verkaufspreis für diesen Auftrag an.", "", "ON_PLAYER_COLLECT_CRIME_TRADER_REQUEST", MenuItemStyle.green)
                            .withData(new Dictionary<string, dynamic> {{"Request", request}, {"Pillar", Pillar}}));
                    } else {
                        reqMenu.addMenuItem(new StaticMenuItem("Antrag noch nicht erfüllt", "Der Antrag ist noch nicht erfüllt. Liefere die Waren ab!", "", MenuItemStyle.yellow));
                    }
                }

                currentRequestMenu.addMenuItem(new MenuMenuItem(reqMenu.Name, reqMenu));
            } 

            menu.addMenuItem(new MenuMenuItem(currentRequestMenu.Name, currentRequestMenu));
        }


        if(menu.getMenuItemCount() > 0) {
            list.Add(new MenuMenuItem(menu.Name, menu));
        }

        return list;
    }

    public override void onRemove() { }
}
