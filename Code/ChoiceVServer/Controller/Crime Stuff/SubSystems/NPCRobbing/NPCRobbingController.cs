using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;



namespace ChoiceVServer.Controller.Crime_Stuff;

public delegate string RobbingNPCCallback(IPlayer player, ChoiceVPed ped, ref decimal currentMonetaryValue);
public class NPCRobbingController : ChoiceVScript {
    private static List<(Predicate<ChoiceVPed>, RobbingNPCCallback)> AllRobbingPredicates;

    public NPCRobbingController() {
        AllRobbingPredicates = [];
        EventController.addMenuEvent("ON_ROB_NPC", onRobNPC);

        addRobbingPredicate(p => true, onRobPedForMoney);
        //addRobbingPredicate(p => true, onRobPedForFenceGoods);

        PedController.addNPCModuleGenerator("NPC-Ausraubbar-Modul", robbableNpcModuleGenerator, robbablleNpcModuleCallback);
    }

    public static void addRobbingPredicate(Predicate<ChoiceVPed> predicate, RobbingNPCCallback callback) {
        AllRobbingPredicates.Add((predicate, callback));
    }

    private string onRobPedForMoney(IPlayer player, ChoiceVPed ped, ref decimal currentMonetaryValue) {
        var random  = new Random();
        var moneyToSteal = random.Next(Convert.ToInt32(currentMonetaryValue * 0.5m), Convert.ToInt32(currentMonetaryValue));

        if(moneyToSteal <= 0) {
            return null;
        }

        currentMonetaryValue -= moneyToSteal;
        return "$" + moneyToSteal;
    }

    private string onRobPedForFenceGoods(IPlayer player, ChoiceVPed ped, ref decimal currentMonetaryValue) {
       var random = new Random(); 

        //TODO Implement the generation of FenceGoods with the monetary value based on the minimum price

       return null;
    }

    private bool onRobNPC(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var module = data["Module"] as NPCRobbingModule;

        module.getPed().playAnimation("random@mugging1", "return_wallet_positive_a_player", 49, 4000, 0, 2);
        InvokeController.AddTimedInvoke("RobbingPed", (i) => {
            var monetaryValue = module.getCurrentMonetaryValue();
            var startingValue = monetaryValue;
            var gottenStuff = new List<string>();

            foreach (var (predicate, callback) in AllRobbingPredicates.Shuffle()) {
                if (predicate(module.getPed())) {
                    var received = callback(player, module.getPed(), ref monetaryValue);

                    if (received != null) {
                        gottenStuff.Add(received);
                    }

                    if (monetaryValue <= 0) {
                        break;
                    }
                }
            }

            CrimeNetworkController.OnPlayerCrimeActionDelegate(player, CrimeAction.RobNPC, Convert.ToSingle(startingValue - monetaryValue), new Dictionary<string, dynamic>{
                { "Position", player.Position }
            });

            var stuffStr = gottenStuff.Count > 0 ? $"Du hast {string.Join(", ", gottenStuff)} erbeutet." : "Du hast nichts erbeutet.";
            player.sendNotification(Constants.NotifactionTypes.Info, stuffStr, "Person ausgeraubt");
            module.pedWasJustRobbed();

        }, TimeSpan.FromSeconds(4), false);
        
        return true;
    }

    private void robbablleNpcModuleCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
        var maxValue = evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>().input; 
        var refillTime = evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>().input;


        creationFinishedCallback(new Dictionary<string, dynamic> {
            { "MaximalMonetaryValue", Convert.ToDecimal(maxValue) },
            { "MaximalMonetaryRegainInHours", Convert.ToInt32(refillTime) }
        });
    }

    private List<MenuItem> robbableNpcModuleGenerator(ref Type codeType) {
        codeType = typeof(NPCRobbingModule);

        return [
            new InputMenuItem("Maximaler Ausraubwert", "Maximaler Wert, den der NPC haben kann", "", InputMenuItemTypes.number, ""),
            new InputMenuItem("Zeit bis zur Wiederherstellung", "Maximale Zeit in Stunden, bis der Wert des NPCs wiederhergestellt wird", "", InputMenuItemTypes.number, "")
        ];
    }
}