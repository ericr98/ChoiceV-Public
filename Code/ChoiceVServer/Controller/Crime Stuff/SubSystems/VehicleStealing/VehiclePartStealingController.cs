using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Crime_Stuff {
    public class VehiclePartStealingController : ChoiceVScript {
        public VehiclePartStealingController() {
            InteractionController.addVehicleInteractionElement(
                new ConditionalGeneratedPlayerInteractionMenuElement(
                    "Tuningteile stehlen",
                    getTuningPartStealMenu,
                    t => t is IPlayer player && player.hasCrimeFlag(),
                    v => v is ChoiceVVehicle veh && veh.IsRandomlySpawned && veh.VehicleTuning.anyStealableParts()
                )
            );
            EventController.addMenuEvent("PLAYER_STEAL_VEHICLE_TUNING_PART", onPlayerStealPartMenu);
        }

        private Menu getTuningPartStealMenu(IEntity sender, IEntity target) {
            var player = (IPlayer)sender;
            var veh = target as ChoiceVVehicle;
            var parts = veh.VehicleTuning.getStealableParts();

            var menu = new Menu("Tuningteile stehlen", "Welches Teil möchtest du abmontieren?");
            
            foreach(var part in parts) {
                var name = Constants.VehicleModTypeToName[part.Type];
                
                menu.addMenuItem(new ClickMenuItem($"{name} abbauen", "Montiere das Teil ab. Dies ist eine illegale Aktion!", "", "PLAYER_STEAL_VEHICLE_TUNING_PART")
                    .withData(new Dictionary<string, dynamic> { { "Vehicle", veh }, { "Part", part } }).needsConfirmation("Tuningteil abmontieren?", "Dies ist eine illegale Aktion!"));
            }

            return menu;
        }

        private bool onPlayerStealPartMenu(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var veh = (ChoiceVVehicle)data["Vehicle"];
            var part = (VehicleMods)data["Part"];

            var rot = player.getRotationTowardsPosition(veh.Position);
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var toolCfg = InventoryController.getConfigItemForType<VehicleTuningItem>(c => VehicleTuningItem.getModTypeForConfigItem(c) == part.Type);
                var tuningFlag = VehicleTuningItem.getToolFlagForConfigItem(toolCfg);

                var toolItem = player.getInventory().getItem<ToolItem>(t => t.Flag.HasFlag(tuningFlag));
                if(toolItem == null) {
                    player.sendBlockNotification("Du hast mit allen Mitteln versucht das Teil abzuschrauben aber die fehlt wohl das richtige Werkzeug!", "Fehlendes Werkzeug", Constants.NotifactionImages.Car);
                    return; 
                }

                if(veh.VehicleTuning.getMod((int)part.Type) == -1) {
                    player.sendBlockNotification("Das Tuningteil konnte nicht abgebaut werden!", "Abbauen unmöglich", Constants.NotifactionImages.Car);
                    return;
                }

                using(var db = new ChoiceVDb()) {
                    var dbMod = db.configvehiclemods.Include(c => c.configvehiclemodtypes).FirstOrDefault(m => m.configvehiclemodels_id == veh.DbModel.id && m.configvehiclemodtypes.ModTypeIndex == ((int)part.Type) && m.ModIndex == part.Level);
                    if(dbMod != null) {
                        var cfg = InventoryController.getConfigItemForType<VehicleTuningItem>(c => VehicleTuningItem.getModTypeForConfigItem(c) == part.Type);
                        var tuningItem = new VehicleTuningItem(cfg, veh.VehicleClassId, dbMod.DisplayName, veh.DbModel.id, true);

                        if(player.getInventory().addItem(tuningItem)) {
                            toolItem.use(player);
                            player.sendNotification(Constants.NotifactionTypes.Success, "Tuningteil erfolgreich abmontiert und eingepackt!", "Tuningteil gestohlen", Constants.NotifactionImages.Car);
                            CrimeNetworkController.OnPlayerCrimeActionDelegate(player, CrimeAction.StealVehiclePart, 1, new Dictionary<string, dynamic> {
                                { "Vehicle", veh },
                                { "Part", part },
                                { "Item", tuningItem },
                            });

                            if(new Random().NextDouble() < 0.33) {
                                ControlCenterController.createDispatch(DispatchType.NpcCallDispatch, "Fahrzeugteil gestohlen", $"Es wurde gemeldet, dass ein: {tuningItem.ModName} gestohlen wurde", veh.Position);
                            }
                        } else {
                            player.sendBlockNotification("Es war kein Platz im Inventar. Das Tuningteil konnte nicht eingepackt werden! Du hast es beschämt wieder angeschraubt!", "Kein Platz", Constants.NotifactionImages.Car);
                            return;
                        }
                    } else {
                        player.sendBlockNotification("Es ist ein unerwarteter Fehler aufgetreten. Bitte melde dich im Support. Code: IMPROVER", "Code: IMPROVER");
                        return;
                    }
                }

                switch(part.Type) {
                    case AltV.Net.Enums.VehicleModType.FrontBumper:
                        //TODO Remove Front Bumper
                        break;
                    case AltV.Net.Enums.VehicleModType.RearBumper:
                        //TODO Remove Back Bumper
                        break;
                }

                veh.VehicleTuning.setMod(part.Type, -1);
                veh.applyVehicleTuning(veh.VehicleTuning);

            }, rot, true, 1, TimeSpan.FromSeconds(30));

            return true;
        }
    }
}
