using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AltV.Net.Elements.Entities;
using AltV.Net.Events;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Clothing;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.InventorySpot;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Player;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Shared;
using ChoiceVSharedApiModels.SupportKeyInfo.DatabaseJsonModels.SubInformation.Vehicle;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Support;

public class SupportKeyConroller : ChoiceVScript {
    public SupportKeyConroller() {
        EventController.addKeyEvent("SUPPORT_NOTIFY", ConsoleKey.F10, "Support Meldung senden", onSendSupportMessage, true, true);

        EventController.addMenuEvent("ON_SEND_SUPPORT_KEY", onSubmitSupportMessage);
    }

    private bool onSendSupportMessage(IPlayer player, ConsoleKey key, string eventName) {
        if(player.hasData("LastSupportKeyPress") && ((DateTime)player.getData("LastSupportKeyPress")) + TimeSpan.FromSeconds(20) > DateTime.Now) {
            player.sendBlockNotification("Bitte nur einzelne Support Meldungen senden! Ein vermehrtes senden von Support Meldung kann zu Sanktionen führen!", "");
            return false;
        }

        var pos = player.Position;

        player.sendNotification(Constants.NotifactionTypes.Info, "Alle Daten zu umlegenden Ereignissen (Spieler, Fahrzeuge, Inventare, etc.) wurden gespeichert. Wenn du dieses Menü abschickst werden diese Daten automatisch an den Support geschickt", "Daten gespeichert");

        var surroundingPlayers = ChoiceVAPI.FindNearbyPlayers(pos, 50);

        var surPlayerList = surroundingPlayers.Select(p => new SurroundingPlayerInfo(
            p.getCharacterId(),
            new Position(p.Position.X, p.Position.Y, p.Position.Z),
            p.Position.Distance(pos),
            p.Health,
            getInventoryInfo(player.getInventory()),
            getClothingInfo(player.getClothing())
        )).ToList();

        var surroundingVehicles = ChoiceVAPI.FindNearbyVehicles(player, 50);
        var surVehiclesList = surroundingVehicles.Select(v => new SurroundingVehicleInfo(
            v.VehicleId,
            v.DbModel.id,
            new Position(v.Position.X, v.Position.Y, v.Position.Z),
            v.Position.Distance(pos),
            v.PassengerList.Select(p => new PassengerInfo(p.Value.getCharacterId(), p.Key)).ToList(),
            getInventoryInfo(v.Inventory)
        )).ToList();


        var spots = InventorySpot.getListByPredicate(i => pos.Distance(i.CollisionShape?.Position ?? new System.Numerics.Vector3(100000, 100000, 0)) < 50);
        var surrSpotList = spots.Select(s => new SurroundingInventorySpot(
            s.Id,
            new Position(s.CollisionShape.Position.X, s.CollisionShape.Position.Y, s.CollisionShape.Position.Z),
            getInventoryInfo(s.Inventory)
        )).ToList();


        var menu = new Menu("Support Meldung", "Gib eine kurze Beschreibung an");
        menu.addMenuItem(new InputMenuItem("Beschreibung", "Gib eine kurze Beschreibung an", "", ""));
        menu.addMenuItem(new MenuStatsMenuItem("Abschicken", "Schicke die Meldung ab", "ON_SEND_SUPPORT_KEY", MenuItemStyle.green)
            .needsConfirmation("Meldung abschicken?", "Wirklich abschicken?")
            .withData(new Dictionary<string, dynamic> {
                { "Info", new SupportKeySurroundingInfo(surPlayerList, surVehiclesList, surrSpotList) }
            }));

        player.showMenu(menu);

        return true;
    }

    private bool onSubmitSupportMessage(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var info = data["Info"] as SupportKeySurroundingInfo;
        var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

        var desc = evt.elements[0].FromJson<InputMenuItemEvent>().input;

        using(var db = new ChoiceVDb()) {
            var dbInfo = new supportkeyinfo {
                sender = player.getCharacterId(),
                message = desc,
                surroundingData = info.ToJson(),
                createdDate = DateTime.Now
            };

            db.supportkeyinfos.Add(dbInfo);

            db.SaveChanges();

            player.sendNotification(Constants.NotifactionTypes.Success, $"Support Meldung wurde abgeschickt! Melde dich bitte zeitnah im Support. Die Id der Nachricht ist: {dbInfo.id}", "Meldung abgeschickt");
        }


        return true;
    }

    private static InventoryInfo getInventoryInfo(Inventory inventory) {
        return new InventoryInfo(inventory?.Id ?? -1, inventory?.getAllItems().Select(i => new ItemInfo(
            i.Id ?? 0,
            i.ConfigId,
            i.Name,
            i.Description
        )).ToList());
    }

    private static ClothingInfo getClothingInfo(ClothingPlayer clothing) {
        return new ClothingInfo(clothing.getComponentList().Select(c => new ClothingSlotInfo(c.isProp, c.slotId, c.component.Drawable, c.component.Texture, c.component.Dlc)).ToList());
    }
}