using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Shopsystem;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff.SubSystems.HoboSystem.HoboCrimeQuestions;

public class HoboCrimeItemBuyShopWithItemsQuestion : HoboCrimeQuestion {
    private List<int> ConfigItemIds;

    public HoboCrimeItemBuyShopWithItemsQuestion() { }

    public HoboCrimeItemBuyShopWithItemsQuestion(int id, CrimeNetworkPillar pillar, string name, List<string> labels, int requiredReputation, Dictionary<string, string> settings) : base(id, pillar, name, labels, requiredReputation) {
        ConfigItemIds = settings["ConfigItemIds"].FromJson<List<int>>();
    }

    public override Menu getQuestionMenu() {
        var menu = new Menu(Name, "Siehe dir die Position an");

        var peds = PedController.findPeds(p => p.hasModule<ShopBuyPedModule>(m => ConfigItemIds.Any(i => m.hasItem(i))));
        
        if(peds.Count == 0) {
            return new Menu("Fehler", "Es gibt aktuell keinen Händler, der die gewünschten Items kauft.");
        }

        menu.addMenuItem(new ClickMenuItem("Position anzeigen", $"Zeige die eine/einen Ankäufer an", "", "ON_PLAYER_SHOW_MODULE_POSITION").withData(new Dictionary<string, dynamic> {
            { "Position", peds.OrderBy(o => new Random().Next()).First().Position },
        }));

        return menu;
    }

    public override List<MenuItem> getSupportMenuInfo() {
        var items = ConfigItemIds.Select(i => InventoryController.getConfigById(i)).ToList();

        var menu = new Menu("Items", "Items nach denen gesucht wird");
        foreach(var item in items) {
            menu.addMenuItem(new StaticMenuItem(item.name, "", ""));
        }

        return [new MenuMenuItem(menu.Name, menu)];
        
    }

    public override List<MenuItem> onSupportCreateMenuItems() {
        var types = Assembly.GetExecutingAssembly().GetTypes().Where(i => i.IsSubclassOf(typeof(CrimeNetworkModule))).ToList();

        return [new InputMenuItem("Items", "Gib die Item-Ids ein, nach denen gesucht werden soll, mittels Komma getrennt", "", "")];
    }

    public override Dictionary<string, string> onSupportCreateSettings(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) {
        var typeName = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>();

        return new Dictionary<string, string> {
            { "ConfigItemIds", typeName.input.Split(",").Select(int.Parse).ToJson() }
        }; 
    }
}