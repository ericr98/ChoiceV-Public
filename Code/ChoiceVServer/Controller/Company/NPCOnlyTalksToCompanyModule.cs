using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;

public class NPCOnlyTalksToCompanyModule : NPCModule {
    private List<Company> Companies;

    public NPCOnlyTalksToCompanyModule(ChoiceVPed ped) : base(ped) {
        
    }

    public NPCOnlyTalksToCompanyModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        if(settings.ContainsKey("Companies")) {
            var list = ((string)settings["Companies"]).FromJson<List<int>>();
            Companies = list.ConvertAll(CompanyController.getCompanyById);
        }
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Spricht nur zu Firmen Modul", $"NPC spricht nur zu Firmen: {string.Join(", ", Companies.Select(c => c.Name))}", $"{Companies.Count} Firmen");
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        if(!CompanyController.getCompanies(player).Any(c => Companies.Contains(c))) {
            return [new StaticMenuItem("Die Person spricht nicht mit dir", "Diese Person spricht nur mit spezifischen Firmen", "", MenuItemStyle.yellow)];
        } else {
            return [];
        }
    }

    public override bool overridesOtherModulesOnInteraction(IPlayer player) {
        return !CompanyController.getCompanies(player).Any(c => Companies.Contains(c));
    }

    public override void onRemove() {
        Companies = null;
    }
}
