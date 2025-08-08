//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Base;
//using ChoiceVServer.Controller;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model.Database;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ChoiceVServer.InventorySystem {
//    class Jerrycan : Item {
//        public Jerrycan(items item) : base(item) { }

//        public Jerrycan(configitems configItem) : base(configItem) {

//        }

//        public override void use(IPlayer player) {

//        }
//    } 
//    public class JerrycanController : ChoiceVScript {
//        public JerrycanController() {
//            EventController.addMenuEvent("JERRYCAN_GIVE_FUEL", onFillFuel);

//            InteractionController.addVehicleInteractionElement(
//                new ConditionalPlayerInteractionMenuElement(
//                    new ClickMenuItem("Fahrzeug tanken", "Tankt das Fahrzeug", "", "JERRYCAN_GIVE_FUEL"),
//                    t => t is ChoiceVVehicle,
//                    p => p is IPlayer && (p as IPlayer).getInventory().hasItem(i => i is Jerrycan)
//                 )
//             );
//        }

//        private bool onFillFuel(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var vehicleId = (int)data["InteractionTargetId"];
//            var vehicle = ChoiceVAPI.FindVehicleById(vehicleId);

//            if(vehicle != null) {
//                var vehObj = vehicle.getObject();
//                var item = player.getInventory().getItem(x => x is Jerrycan);
//                if(item != null) {
//                    var anim = AnimationController.getAnimationByName("KNEEL_DOWN");
//                    AnimationController.animationTask(player, anim, () => {
//                        vehObj.setFuel(5);
//                        player.getInventory().removeItem(item);
//                        player.sendNotification(Constants.NotifactionTypes.Success, $"Fahrzeug wurde betankt", "Fahrzeug betankt");
//                    });
//                }

//            }
//            return true;
//        }
//    }
//}
