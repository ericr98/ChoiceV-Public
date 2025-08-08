using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public class TrashCompanyFunctionality : CompanyFunctionality {
        public TrashCompanyFunctionality() : base() { }
        
        public TrashCompanyFunctionality(Company company) : base(company) {
            Company = company;
        }

        public override string getIdentifier() {
            return "TRASH_COMPANY_FUNCTIONALITY";
        }

        public override void onLoad() {
            Company.registerCompanySelfElement(
                "TRASH_COMPANY",
                trashZoneGenerator,
                _);
        }

        private MenuElement trashZoneGenerator(Company company, IPlayer player) {
            var menu = new Menu("Zonenfüllstand anzeigen", "Was möchtest du tun?");

            var zones = TrashController.getTrashZones();
            
            foreach(var zone in zones) {
                var fillLevel = Math.Round(zone.FillLevel * 100, 2);
                menu.addMenuItem(new StaticMenuItem(zone.Name, $"Die Zone {zone.Name} hat den Füllstand von {fillLevel}", $"Füllstand: {fillLevel}%")); 
            }

            return menu;
        }

        private void _(Company company, IPlayer player, string subevent, Dictionary<string, dynamic> data, MenuItemCefEvent menuitemcefevent) { }
        
        public override void onRemove() {
            Company.unregisterCompanyElement("TRASH_COMPANY");
        }

        public override SelectionInfo getSelectionInfo() {
            return new SelectionInfo(getIdentifier(), "Müllentsorgung", "Ermöglicht es den Befüllstand der Bezirke zu sehen");
        }
    }
}