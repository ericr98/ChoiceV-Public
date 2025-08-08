using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Farming;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.Constants;
using static ChoiceVServer.Controller.CallbackController;

namespace ChoiceVServer.Controller.PlaceableObjects {
    public class Marihuana : PlaceableObject, IFertilizeObject {
        private int ConfigId { get => (int)Data["ConfigId"]; set { Data["ConfigId"] = value; } }
        private float FertilizeLevel { get => (float)Data["FertilizeLevel"]; set { Data["FertilizeLevel"] = value; } }
        public float WaterLevel { get => (float)Data["WaterLevel"]; set { Data["WaterLevel"] = value; } }
        private float StartZ { get => (float)Data["StartZ"]; set { Data["StartZ"] = value; } }
        private DateTime NextGrowth { get => (DateTime)Data["NextGrowth"]; set { Data["NextGrowth"] = value; } }
        private float Growth { get => (float)Data["Growth"]; set { Data["Growth"] = value; } }
        private int DestroyCounter { get => (int)Data["DestroyCounter"]; set { Data["DestroyCounter"] = value; } }
        private int CurrentQuality { get => (int)Data["CurrentQuality"]; set { Data["CurrentQuality"] = value; } }

        public Marihuana(Position position, Rotation rotation, float width, float height, bool trackVehicles, Dictionary<string, dynamic> data) : base(position, rotation, width, height, trackVehicles, data) {
            IntervalPlaceable = true;
        }

        public Marihuana(string model, Item placeableItem, IPlayer player, Position playerPosition, Rotation playerRotation) : base(playerPosition, playerRotation, 3f, 3f, true, new Dictionary<string, dynamic>()) {
            ConfigId = placeableItem.ConfigId;

            FertilizeLevel = 0.4f;
            WaterLevel = 0.4f; //0.4f

            Position = playerPosition;
            StartZ = Position.Z - 1.64f;
            NextGrowth = DateTime.Now + TimeSpan.FromSeconds(10); //Minutes
            Growth = 0;
            DestroyCounter = 0;
            //Maximal Quality. Will reduce when requirements are not met
            CurrentQuality = 3;
            IntervalPlaceable = true;
        }

        public override void initialize(bool register = true) {
            var player = ChoiceVAPI.GetAllPlayers().FirstOrDefault(p => p.Position.Distance(Position) <= 10);

            if(player != null) {
                getPlayerMaterialStandOn(player, (p, mat) => {
                    if(!(mat == Materials.Grass || mat == Materials.GrassLong || mat == Materials.GrassShort)) {
                        player.sendBlockNotification("Die Pflanze wird hier nicht wachsen!", "Pflanze blockiert!", NotifactionImages.Marihuana);
                        onRemove();
                        var configItem = InventoryController.getConfigById(ConfigId);
                        player.getInventory().addItem(new PlaceableObjectItem(configItem));
                        return;
                    } else {
                        //Position given is player position 0.97
                        var propPos = Position;
                        propPos.Z = StartZ;
                        Object = ObjectController.createObject("prop_weed_01", propPos, new DegreeRotation(0, 0, 0), 200, true);
                        base.initialize(register);
                    }
                });
            } else {
                var newZ = StartZ + 0.5f * (Growth > 1 ? 1 : Growth);
                Object = ObjectController.createObject("prop_weed_01", new Position(Position.X, Position.Y, newZ), new DegreeRotation(0, 0, 0), 200, true);
                base.initialize(register);
            }
        }

        public override Menu onInteractionMenu(IPlayer player) {
            var menu = new Menu("Marihuana", "Was möchtest du tun?");
            var infoMenu = new Menu("Planzen-Infos", "Infos über den Dünger");
            infoMenu.addMenuItem(new StaticMenuItem("Größe", Growth >= 1 ? "Die Pflanze sieht ausgewachsen aus." : "Die Pflanze ist noch nicht ganz ausgewachsen.", ""));
            infoMenu.addMenuItem(new StaticMenuItem("Wasserstand", getWaterString(WaterLevel), ""));
            infoMenu.addMenuItem(new StaticMenuItem("Düngstand", getFertilizerString(FertilizeLevel), ""));
            infoMenu.addMenuItem(new ClickMenuItem("Pflanze gießen", "Gießt die Pflanze", "", "WEEDPLANT_WATERING", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "WeedPlant", this } }));

            menu.addMenuItem(new MenuMenuItem(infoMenu.Name, infoMenu));
            var pick = (ClickMenuItem)new ClickMenuItem("Ernten", "Ernte die Pflanze ab", "", "PICK_UP_PLACABLE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "placeable", this } });
            pick.needsConfirmation("Wirklich ernten?", "Möchtest du diese Pflanze wirklich ernten?");
            menu.addMenuItem(pick);

            var destroy = (ClickMenuItem)new ClickMenuItem("Zerstören", "Zerstöre die Pflanze", "", "DESTROY_PLACABLE", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "placeable", this } });
            destroy.needsConfirmation("Wirklich zerstören?", "Möchtest du diese Pflanze wirklich zerstören?");
            menu.addMenuItem(destroy);

            return menu;
        }

        public override void onInterval(TimeSpan tickLength) {
            if(NextGrowth <= DateTime.Now) {
                Growth += 0.005f;
                WaterLevel -= 0.004f;
                FertilizeLevel -= 0.004f;

                //If Fertilizing under a certain Level
                if((WaterLevel + FertilizeLevel) / 2 < 0.3f) {
                    DestroyCounter++;
                    if(DestroyCounter >= 20) {
                        onRemove();
                    }
                    //If Fertilizing over a certain Level
                } else if(WaterLevel >= 1.25 || FertilizeLevel >= 1.25) {
                    DestroyCounter++;
                    if(DestroyCounter >= 20) {
                        onRemove();
                    }
                    //Else Remove Destroy counter
                } else {
                    //If Fertilizing falls under a certain level, reduce Quality
                    if((WaterLevel + FertilizeLevel) / 2 < 0.4f) {
                        if(CurrentQuality > 1) {
                            CurrentQuality = 1;
                        }

                        //If Fertilizing falls under a certain level, reduce Quality
                    } else if((WaterLevel + FertilizeLevel) / 2 < 0.66f) {
                        if(CurrentQuality > 2) {
                            CurrentQuality = 2;
                        }
                    }

                    if(DestroyCounter > 0) {
                        DestroyCounter--;
                    }

                    var newZ = StartZ + (0.5f * Growth > 1 ? 1 : Growth);
                    if(newZ <= StartZ + 1.67f) {
                        ObjectController.moveObject(Object, new Position(Position.X, Position.Y, newZ), false);
                    }
                }

                NextGrowth = DateTime.Now + TimeSpan.FromSeconds(10); //Minutes
            }
        }

        public override bool onPickUp(IPlayer player, ref NotifactionImages img) {
            var r = new Random();
            var cuttlingCfg = InventoryController.getConfigItem(i => i.codeItem == typeof(PlaceableObjectItem).Name && i.additionalInfo == typeof(Marihuana).Name);

            var weedCfg = InventoryController.getConfigItemForType<MarihuanaItem>();
            if(Growth >= 1) {
                //player geta a cuttling and some weed
                var cuttling = new PlaceableObjectItem(cuttlingCfg);
                player.getInventory().addItem(cuttling);
                //He can get another cuttling
                if(r.Next() >= 0.15) {
                    var cuttling2 = new PlaceableObjectItem(cuttlingCfg);
                    player.getInventory().addItem(cuttling2);
                }

                var weedItem = new MarihuanaItem(weedCfg, r.Next(4, 6), CurrentQuality);
                player.getInventory().addItem(weedItem);

                player.sendNotification(NotifactionTypes.Success, "Du hast die Pflanze abgeerntet.", "Pflanze aufgehoben", NotifactionImages.Marihuana);
                return true;
            } else if(Growth >= 0.65) {
                var weedItem = new MarihuanaItem(weedCfg, r.Next(1, 3), CurrentQuality);
                player.getInventory().addItem(weedItem);

                player.sendNotification(NotifactionTypes.Warning, "Du hast die Pflanze abgeerntet. Sie war noch nicht ganz ausgewachsen!", "Pflanze aufgehoben!", NotifactionImages.Marihuana);
                return true;
            } else {
                player.sendNotification(NotifactionTypes.Danger, "Du hast die Pflanze abgeerntet. Sie war noch lange nicht ganz ausgewachsen!", "Pflanze aufgehoben!", NotifactionImages.Marihuana);
            }

            return false;
        }

        public string onFertilize(float level) {
            FertilizeLevel = FertilizeLevel + level;

            return "Du hast die Marihuana-Pflanze gedüngt";
        }

        private static string getWaterString(float waterLevel) {
            if(waterLevel > 1.0) {
                //> 100%
                return "Sieht aus als wäre die Pflanze überwässert!";
            } else if(waterLevel >= 0.8) {
                //80 - 100%
                return "Die Planze sieht gut bewässert aus.";
            } else if(waterLevel >= 0.5) {
                //50 - 80%
                return "Die Pflanze sieht etwas durstig aus.";
            } else if(waterLevel >= 0.2) {
                //20 - 50%
                return "Die Pflanze sieht sehr durstig aus.";
            } else {
                //0% - 20%
                return "Die Pflanze ist kurz vor dem Eingehen.";
            }
        }

        private static string getFertilizerString(float fertLevel) {
            if(fertLevel > 1.0) {
                //> 100%
                return "Sieht aus als wäre die Pflanze mit Dünger übersättigt!";
            } else if(fertLevel >= 0.7) {
                //80 - 100%
                return "Die Planze sieht gut gedüngt aus.";
            } else if(fertLevel >= 0.5) {
                //50 - 80%
                return "Die Pflanze könnte etwas Dünger gebrauchen.";
            } else if(fertLevel >= 0.2) {
                //20 - 50%
                return "Die Pflanze braucht unbedingt Dünger.";
            } else {
                //0% - 20%
                return "Die Pflanze ist kurz vor dem Eingehen.";
            }
        }
    }
    public class MarihuanaController : ChoiceVScript {
        public MarihuanaController() {
            EventController.addMenuEvent("WEEDPLANT_WATERING", onWateringPlant);
        }

        private bool onWateringPlant(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(data.ContainsKey("WeedPlant")) {
                var plant = (Marihuana)data["WeedPlant"];
                if(player.getInventory().hasItem(x => x.Name == "Wasser")) {
                    var item = player.getInventory().getItem(x => x.Name == "Wasser");
                    var anim = AnimationController.getAnimationByName(KNEEL_DOWN_ANIMATION);
                    AnimationController.animationTask(player, anim, () => {
                        player.getInventory().removeItem(item);
                        plant.WaterLevel += 0.3f;
                        player.sendNotification(NotifactionTypes.Success, "Erfolgreich die Pflanze gegossen", "Pflanze gegossen");
                    });
                } else {
                    player.sendNotification(NotifactionTypes.Warning, "Du hast kein Wasser!", "Kein Wasser");
                }
            }
            return true;
        }
    }
}
