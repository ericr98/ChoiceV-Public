using AltV.Net.Elements.Entities;
using ChoiceVServer.Controller.PrisonSystem.Model;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using ChoiceVServer.Base;


namespace ChoiceVServer.Controller.PrisonSystem;

public class NPCPrisonFoodModule : NPCModule {
    private Prison Prison;
        
    public NPCPrisonFoodModule(ChoiceVPed ped) : base(ped) { }
    public NPCPrisonFoodModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        Prison = PrisonController.getPrisonById((int)settings["PrisonId"]);
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        if(!Prison.isPlayerInPrison(player)) {
            return [];
        }
            
        if(player.hasData("PRISON_LAST_MEAL")) {
            var lastMeal = (DateTime)player.getData("PRISON_LAST_MEAL");
            if(lastMeal + PrisonController.FOOD_HOW_OFTEN_TIMESPAN > DateTime.Now) {
                var minutes = Math.Round((PrisonController.FOOD_HOW_OFTEN_TIMESPAN - (DateTime.Now - lastMeal)).TotalMinutes, 0);
                return [new StaticMenuItem("Aktuell kein Essen abholbar", $"Du hast bereits in den letzten {PrisonController.FOOD_HOW_OFTEN_TIMESPAN.TotalMinutes} Minuten Essen bestellt. Komme in ca. {minutes} min wieder.", "Bereits Essen abgeholt", MenuItemStyle.yellow)];
            }
        }

        return [new ClickMenuItem("Essen abholen", "Hole dir dein 5 Sterne Gefängnisessen ab.", "", "PRISON_ORDER_FOOD", MenuItemStyle.green)
            .withData(new Dictionary<string, dynamic> { { "Prison", Prison } })];
    }

    public override void onRemove() {
        Prison = null;
    }
        
    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Gefängnisessensausgabe", $"Gibt Essen für Insassen des Gefängnisses {Prison.Name} aus", Prison.Name);
    }
}