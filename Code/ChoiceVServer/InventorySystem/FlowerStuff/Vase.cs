using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Shared.Elements.Data;
using BenchmarkDotNet.Attributes;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.PlaceableObjects;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.InventorySystem {
    public class VaseController: ChoiceVScript {
        public VaseController() {
            EventController.addMenuEvent("PLAYER_PUT_VASE", onPlayerPutVase);
            EventController.addMenuEvent("PLAYER_EMPTY_VASE", onPlayerEmptyVase);
            EventController.addMenuEvent("PLAYER_CREATE_VASE", onPlayerCreateVase);
        }

        private bool onPlayerPutVase(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var vase = (Vase)data["Item"];

            ObjectController.startObjectPlacerMode(player, vase.Model, 0, (p, pos, heading) => {
                var anim = AnimationController.getAnimationByName(Constants.KNEEL_DOWN_ANIMATION);
                AnimationController.animationTask(player, anim, () => {
                    var placeable = new VasePlaceable(pos, new DegreeRotation(0, 0, heading), vase.Model);
                    placeable.initialize();

                    vase.destroy();
                });
            });

            return true;
        }

        private bool onPlayerEmptyVase(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var vase = (Vase)data["Item"];
                var variation = Vase.getVaseVariation(vase.Model);

                var list = new List<Item>();
                foreach(var flower in variation.FlowerModels) {
                    var cfg = InventoryController.getConfigItemForType<StaticFlower>(i => i.additionalInfo == flower.Model);
                    var items = InventoryController.createItems(cfg, flower.Amount);
                    list.AddRange(items);
                }

                if(player.getInventory().addItems(list)) {
                    vase.Data.remove("Model");
                    vase.updateDescription();
                    player.sendNotification(Constants.NotifactionTypes.Info, "Die Vase wurde geleert. Du hast alle enthaltenen Blumen erhalten", "Vase geleert");
                } else {
                    player.sendBlockNotification("Du hast nicht genug Platz im Inventar!", "Inventar voll");
                }
            });
            return true;
        }

        private bool onPlayerCreateVase(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var vase = (Vase)data["Item"];
                var variation = (VaseVariation)data["Variation"];

                var flowers = player.getInventory().getItems<StaticFlower>(i => variation.FlowerModels.Any(f => f.Model == i.FlowerModel));

                foreach(var flower in variation.FlowerModels) {
                    var flowerItem = flowers.FirstOrDefault(f => f.FlowerModel == flower.Model);
                    if(flowerItem != null) {
                        if(!player.getInventory().removeItem(flowerItem, flower.Amount)) {
                            player.sendBlockNotification("Eine der Blumen war nicht in deinem Inventar! Melde dich im Support mit Code: FlowerTrack", "SupportCode: FlowerTrack");
                            return;
                        }
                    }
                }

                vase.Model = variation.Model;
                vase.updateDescription();
                player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast die Vasenvariation {variation.Name} erfolgreich gebaut!", "Vase gebaut");
            });

            return true;
        }
    }

    public class VaseVariation {
        public string Model;
        public string Name;
        public string Description;
        public List<VaseFlower> FlowerModels;

        public VaseVariation(string model, string name, string description, List<VaseFlower> flowerModels) {
            Model = model;
            Name = name;
            Description = description;
            FlowerModels = flowerModels;
        }
    }

    public class VaseFlower {
        public string Model;
        public int Amount;
    }

    public class Vase : Item {
        private static List<VaseVariation> AllAvailableVaseVariations = new List<VaseVariation> {
            new VaseVariation("apa_mp_h_acc_vase_flowers_03", "4 prop_flower1 (Rund)", "4 prop_flower1 in einer runden Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower1", Amount = 4 }
                }),
            new VaseVariation("vw_prop_flowers_vase_03a", "3 prop_flower1 (klein)", "3 prop_flower1 in einer kleinen Vase. Ungefähr 0.75m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower1", Amount = 3 }
                }),
             new VaseVariation("hei_heist_acc_flowers_01", "5 prop_flower1 (Zylinder)", "5 prop_flower1 in einer zylinderförmigen Vase. Ungefähr 0.75m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower1", Amount = 5 }
                }),
            new VaseVariation("v_ret_flowers", "Pinkweißes Bouquet", "7 prop_flower1, 9 prop_flower2, in einer rechteckigen Vase. Ungefähr 1.5m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower1", Amount = 7 },
                    new VaseFlower { Model = "prop_flower2", Amount = 9 },
                }),
             new VaseVariation("v_ret_j_flowerdisp", "Großes rote-Rosenbouquet", "12 rote Rosen, in einer Vase. Ungefähr 1.5m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower3", Amount = 12 },
                }),
             new VaseVariation("v_ret_j_flowerdisp_white", "Großes weiße-Rosenbouquet", "12 weiße Rosen, in einer Vase. Ungefähr 1.5m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower4", Amount = 12 },
                }),
            new VaseVariation("vw_prop_flowers_vase_01a", "Weiße-Rosenbouquet", "20 weiße Rosen, in einer rechteckige Vase. Ungefähr 0.75m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower4", Amount = 2 },
                }),
            new VaseVariation("v_ret_ps_flowers_01", "prop_flower5 Bouquet", "7 prop_flower5, in einer zylinderförmigen Vase. Ungefähr 1.25m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower5", Amount = 7 },
                }),
            new VaseVariation("vw_prop_flowers_vase_02a", "2 prop_flower2 (klein)", "2 prop_flower2, in einer zylinderförmigen Vase. Ungefähr 0.5m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower2", Amount = 2 },
                }),
            new VaseVariation("v_ret_ps_flowers_02", "Pink-violettes Bouquet", "9 prop_flower2, und 10 prop_flower6 in einer zylinderförmigen Vase. Ungefähr 2m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower2", Amount = 9 },
                    new VaseFlower { Model = "prop_flower6", Amount = 10 },
                }),
            new VaseVariation("apa_mp_h_acc_vase_flowers_01", "prop_flower8 Bouquet", "13 prop_flower8, und Blattgewächs in einer rechteckigen Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower8", Amount = 13 },
                }),
            new VaseVariation("apa_mp_h_acc_vase_flowers_02", "Buschig-violettes Bouquet", "12 prop_flower9, in einer zylinderförmigen Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower9", Amount = 12 },
                }),
            new VaseVariation("apa_mp_h_acc_vase_flowers_02", " Bouquet", "12 prop_flower9, in einer zylinderförmigen Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower9", Amount = 12 },
                }),
            new VaseVariation("apa_mp_h_acc_vase_flowers_04", "buschig-violettes Bouquet", "12 prop_flower9, in einer zylinderförmigen Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower9", Amount = 12 },
                }),
            new VaseVariation("vw_prop_flowers_potted_02a", "prop_flower12 Bouquet", "2 prop_flower12, in einer rechteckige Vase. Ungefähr 1m hoch",
                new List<VaseFlower> {
                    new VaseFlower { Model = "prop_flower12", Amount = 2 },
                }),
        };

        public string Model { get => (string)Data["Model"]; set { Data["Model"] = value; } }
     
        public Vase(item item) : base(item) { }

        public Vase(configitem configItem, int amount, int quality) : base(configItem) {}

        public Vase(configitem configItem, string modelName) : base(configItem) {
            Model = modelName;
        }


        public override void use(IPlayer player) {
            base.use(player);

            var flowers = player.getInventory().getItems<StaticFlower>(i => true);

            Menu menu = null;
            if(Data.hasKey("Model")) {
                menu = new Menu("Blumenvase", "Was möchtest du tun?");
                menu.addMenuItem(new ClickMenuItem("Vase platzieren", "Platziere die Vase auf den Boden", "", "PLAYER_PUT_VASE", MenuItemStyle.green)
                    .withData(new Dictionary<string, dynamic> { { "Item", this } }));
                menu.addMenuItem(new ClickMenuItem("Vase leeren", "Leere die Vase. Die Blumen werden in dein Inventar gelegt", "", "PLAYER_EMPTY_VASE", MenuItemStyle.red)
                    .withData(new Dictionary<string, dynamic> { { "Item", this } }));
            } else {
                menu = new Menu("Vase befüllen", "Welche Variation?");
                foreach(var variation in AllAvailableVaseVariations) {
                    var available = true;
                    foreach(var flower in variation.FlowerModels) {
                        if(!flowers.Any(f => f.FlowerModel == flower.Model && f.StackAmount >= flower.Amount)) {
                            available = false;
                            break;
                        }
                    }

                    if(available) {
                        menu.addMenuItem(new ClickMenuItem(variation.Name, variation.Description, "", "PLAYER_CREATE_VASE", MenuItemStyle.green)
                                                       .withData(new Dictionary<string, dynamic> { { "Item", this }, { "Variation", variation } }));
                    } else {
                        menu.addMenuItem(new StaticMenuItem(variation.Name, $"Du kannst diese Variation nicht bauen: {variation.Description}", "Nicht verfügbar", MenuItemStyle.yellow));
                    }
                }
            }

            player.showMenu(menu);
        }

        public static VaseVariation getVaseVariation(string model) {
            return AllAvailableVaseVariations.FirstOrDefault(v => v.Model == model);
        }

        public override void updateDescription() {
            if(Data.hasKey("Model")) {
                Description = AllAvailableVaseVariations.FirstOrDefault(v => v.Model == Model).Description;
            } else {
                Description = "Sie ist nicht mit Worten zu beschreiben. Sehr flexibles Design.";
            }
            base.updateDescription();
        }
    }
}
