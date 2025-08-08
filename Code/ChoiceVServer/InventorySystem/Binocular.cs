using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class BinocularController : ChoiceVScript {
        public BinocularController() {
            EventController.addKeyEvent("BINOCULARS", ConsoleKey.U, "Fernglas benutzen", onPressBinocular, true);
        }

        private bool onPressBinocular(IPlayer player, ConsoleKey key, string eventName) {
            if(player.hasData("BINOCULARS_IN_INV") && !player.getBusy(new List<Constants.PlayerStates> { Constants.PlayerStates.InAnimation })) {
                if(!player.IsInVehicle || player.Vehicle.Driver != player) {
                    if(!player.hasData("BINOCULARS_OPEN") && !player.getBusy()) {
                        var list = (List<BinocularData>)player.getData("BINOCULARS_IN_INV");
                        var both = list.FirstOrDefault(b => b.ThermalVision && b.NightVision);
                        var anim = AnimationController.getAnimationByName("BINOCULAR");
                        player.playAnimation(anim);
                        if(both != null) {
                            player.emitClientEvent("BINOCULARS_TOGGLE", true, true);
                        } else {
                            var thermal = list.FirstOrDefault(b => b.NightVision);
                            if(thermal != null) {
                                player.emitClientEvent("BINOCULARS_TOGGLE", true, true, false);
                            } else {
                                var night = list.FirstOrDefault(b => b.ThermalVision);
                                if(night != null) {
                                    player.emitClientEvent("BINOCULARS_TOGGLE", true, false, true);
                                } else {
                                    player.emitClientEvent("BINOCULARS_TOGGLE", true, false, false);
                                }
                            }
                        }
                        player.setData("BINOCULARS_OPEN", true);
                    } else {
                        player.stopAnimation();

                        player.emitClientEvent("BINOCULARS_TOGGLE", false);
                        player.resetData("BINOCULARS_OPEN");
                    }
                    return true;
                } else {
                    player.sendBlockNotification("Du kannst keine Ferngläser in als Fahrer benutzen!", "Ferngläser blockiert", Constants.NotifactionImages.MagnifyingGlass);
                }
            }

            return false;
        }
    }

    public class BinocularData {
        public int ItemId;
        public bool NightVision;
        public bool ThermalVision;

        public BinocularData(int itemId, bool nightVision, bool thermalVision) {
            ItemId = itemId;
            NightVision = nightVision;
            ThermalVision = thermalVision;
        }
    }

    public class Binocular : Item, InventoryAuraItem {
        public bool ThermalVision;
        public bool NightVision;

        public Binocular(item item) : base(item) { }

        public Binocular(configitem configItem, int amount, int quality) : base(configItem) {
            processAdditionalInfo(configItem.additionalInfo);
        }

        public void onEnterInventory(Inventory inventory) {
            var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == inventory.OwnerId);
            if(player != null) {
                if(player.hasData("BINOCULARS_IN_INV")) {
                    var list = (List<BinocularData>)player.getData("BINOCULARS_IN_INV");
                    list.Add(new BinocularData(Id ?? 0, NightVision, ThermalVision));
                    player.setData("BINOCULARS_IN_INV", list);
                } else {
                    var list = new List<BinocularData> {
                        new BinocularData(Id ?? 0, NightVision, ThermalVision)
                    };
                    player.setData("BINOCULARS_IN_INV", list);
                }
            }
        }

        public void onExitInventory(Inventory inventory, bool becauseOfUnload) {
            var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == inventory.OwnerId);
            if(player != null) {
                if(player.hasData("BINOCULARS_IN_INV")) {
                    var list = (List<BinocularData>)player.getData("BINOCULARS_IN_INV");
                    var data = list.FirstOrDefault(b => b.ItemId == Id);
                    if(data != null) {
                        list.Remove(data);
                    }
                    player.setData("BINOCULARS_IN_INV", list);
                }
            }
        }

        public override void processAdditionalInfo(string info) {
            var split = info.Split('#');
            NightVision = int.Parse(split[0]) == 1;
            ThermalVision = int.Parse(split[1]) == 1;
        }
    }
}
