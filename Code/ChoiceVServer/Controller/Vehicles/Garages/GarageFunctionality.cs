using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Controller.Garages;
using ChoiceVServer.Controller.Garages.Model;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller;

public class GarageFunctionality : CompanyFunctionality {
    private List<Garage> Garages = [];

    public GarageFunctionality() { }

    public GarageFunctionality(Company company) : base(company) { }

    public override string getIdentifier() {
        return "GARAGE_FUNCTIONALITY";
    }

    public override SelectionInfo getSelectionInfo() {
        return new SelectionInfo(getIdentifier(), "Garagenfunktionalität", "Erlaubt das erstellen und verwalten von Firmengaragen");
    }

    public override void onLoad() {
        Company.registerCompanyAdminElement(
            "EDIT_GARAGES",
            garageGenerator,
            garageCallback
        );


        Garages = GarageController.AllGarages.Values.Where(g => g.OwnerType == Base.Constants.GarageOwnerType.Company && g.OwnerId == Company.Id).ToList();
    }

    public void addGarage(Garage garage) { 
        Garages.Add(garage);
    }

    private MenuElement garageGenerator(IPlayer player) {
        var menu = new Menu("Garagenverwaltung", "Verwalte die Garagen deiner Firma");

        menu.addMenuItem(new ClickMenuItem("Garage erstellen", "Erstelle eine neue Garage", "", "CREATE"));

        foreach (var garage in Garages) {
            menu.addMenuItem(new ClickMenuItem(garage.Name, "Verwalte die Garage", "", "EDIT").withData(new Dictionary<string, dynamic>{{"Garage", garage}}));
        }

        return menu;
    }

    private void garageCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if (subEvent == "CREATE") {
            var menu = GarageController.getGarageGenerateMenu(player, Constants.GarageOwnerType.Company, company.Id);
            player.showMenu(menu);
        } else if (subEvent == "EDIT") {
            var garage = (Garage)data["Garage"];
            var menu = garage.getSupportMenu();
            player.showMenu(menu);
        }
    }

    public override void onRemove() {
        Garages.ForEach(g => GarageController.deleteGarage(g.Id));
        Garages = null;
        Company.unregisterCompanyElement("EDIT_GARAGES");
    }
}