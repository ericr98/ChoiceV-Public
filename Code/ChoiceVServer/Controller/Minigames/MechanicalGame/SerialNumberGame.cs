using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Model.Menu.ListMenuItem;

namespace ChoiceVServer.Controller {
    public class IdentifyMechanicalGameComponent : MechanicalGameComponent {
        public string SerialNumberOrName;
        public bool Moveable;

        public SpecialToolFlag LosenToolFlag;

        public IdentifyMechanicalGameComponent(int id, string serialOrName, SpecialToolFlag neededToolFlag, bool identified, bool movedToStash, bool moveable, int depth, string image, List<GameComponentVector> positions) : base(id, identified, movedToStash, depth, image, positions) {
            SerialNumberOrName = serialOrName;
            LosenToolFlag = neededToolFlag;

            Moveable = moveable;
        }


        public override Menu getSelectMenu(IPlayer player, bool blockedByOtherComponent) {
            Menu menu;

            if(!Identified) {
                menu = new Menu("Unbekanntes Teil", "Was möchtest du tun?");
                menu.addMenuItem(new ClickMenuItem("Teil untersuchen", "Identifiziere den Typ und die Seriennummer des Teils", "", "MECHANICAL_GAME_IDENTIFY").withData(new Dictionary<string, dynamic> { { "Component", this } }));
            } else {
                menu = new Menu(SerialNumberOrName, "Was möchtest du tun?");

                if(Moveable) {
                    if(!MovedToStash) {
                        menu.addMenuItem(new ClickMenuItem("Teil beiseite legen", "Lege das Teil in die Ablage. Es kann von dort noch eingesteckt werden", "", "MECHANICAL_GAME_MOVE_TO_STASH").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil beiseite legen?", "Teil wirklich beiseite legen?"));
                    } else {
                        var tools = player.getInventory().getItems<ToolItem>(t => true);
                        if(tools.Count() == 0) {
                            menu.addMenuItem(new StaticMenuItem("Keine Werkzeuge dabei", "Du hast keine Werkzeuge dabei um das Teil wieder einzubauen", "", MenuItemStyle.yellow));
                        } else {
                            menu.addMenuItem(new ListMenuItem("Teil einbauen", "Baue das Teil von der Ablage wieder ein. Dies ist nur möglich, wenn kein anderes Teil dieses blockiert!", tools.Select(t => t.Name).ToArray(), "MECHANICAL_GAME_BUILD_IN").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil wieder einbauen?", "Mit ausgewähltem Werkzeug einbauen?"));

                        }
                    }

                    menu.addMenuItem(new ClickMenuItem("Teil herausnehmen", "Nimm das Teil aus der Maschine und stecke es ein", "", "MECHANICAL_GAME_TAKE").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil herausnehmen?", "Teil wirklich herausnehmen?"));
                } else {
                    var tools = player.getInventory().getItems<ToolItem>(t => true);
                    if(tools.Count() == 0) {
                        menu.addMenuItem(new StaticMenuItem("Keine Werkzeuge dabei", "Du hast keine Werkzeuge dabei um das Teil zu lösen", "", MenuItemStyle.yellow));
                    } else {
                        menu.addMenuItem(new ListMenuItem("Teil lösen", "Löse das Teil aus der Maschine", tools.Select(t => t.Name).ToArray(), "MECHANICAL_GAME_SERIAL_LOSEN").withData(new Dictionary<string, dynamic> { { "Component", this } }).needsConfirmation("Teil lösen?", "Teil mit ausgewähltem Werkzeug lösen?"));
                    }
                }
            }

            return menu;
        }

        public override string getStringRepresentationStep() {
            return $"I#{SerialNumberOrName}#{Moveable}#{(int)LosenToolFlag}";
        }

        public static IdentifyMechanicalGameComponent createFromRepresentation(int id, bool identified, bool movedToStash, int depth, string image, List<GameComponentVector> positions, string[] data) {
            var moveable = bool.Parse(data[2]);
            var component = new IdentifyMechanicalGameComponent(id, data[1], (SpecialToolFlag)int.Parse(data[3]), identified, movedToStash, moveable, depth, image, positions);


            return component;
        }
    }

    public class SerialNumberGameController : ChoiceVScript {
        public SerialNumberGameController() {
            EventController.addMenuEvent("MECHANICAL_GAME_SERIAL_LOSEN", onMechanicalGameSerialLosen);
            EventController.addMenuEvent("MECHANICAL_GAME_BUILD_IN", onMechanicalGameBuildIn);
        }

        private bool onMechanicalGameSerialLosen(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var component = (IdentifyMechanicalGameComponent)data["Component"];
            var evt = menuItemCefEvent as ListMenuItemEvent;

            var anim = AnimationController.getAnimationByName("WORK_FRONT_LONG");
            makeWorkAndToolCheck(player, component, evt.currentElement, (checkPassed, tool) => {
                if(checkPassed) {
                    tool.use(player);
                    component.Moveable = true;
                    MechanicalGameController.refreshMenu(player, component);

                    player.sendNotification(Constants.NotifactionTypes.Success, "Das Teil hat sich mit dem Werkzeug einfach lösen lassen. Es kann nun herausgenommen werden", "Teil gelöst");
                } else {
                    var charId = player.getCharacterId();
                    if(player.hasData("MECHANICAL_GAME")) {
                        var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                        game.invokeAction(player, component, "WRONGLY_LOSEND");
                    }
                }
            });

            return true;
        }

        private bool onMechanicalGameBuildIn(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var component = (IdentifyMechanicalGameComponent)data["Component"];
            var evt = menuItemCefEvent as ListMenuItemEvent;

            makeWorkAndToolCheck(player, component, evt.currentElement, (checkPassed, tool) => {
                if(checkPassed) {
                    tool.use(player);
                    if(MechanicalGameController.getPositionIsAvailable(player, component.Depth, component.Positions)) {
                        MechanicalGameController.putMechanicalGameBackIn(player, component);
                        MechanicalGameController.refreshMenu(player, component);

                        player.sendNotification(Constants.NotifactionTypes.Success, "Das Teil hat sich mit dem Werkzeug einfach einbauen lassen. Es ist jetzt wieder fest verbaut", "Teil eingebaut");

                        component.Moveable = false;
                    } else {
                        player.sendNotification(Constants.NotifactionTypes.Danger, "Das Teil konnte nicht eingebaut werden, weil es durch ein anderes Teil blockiert ist!", "Teil blockiert!");
                    }
                } else {
                    var charId = player.getCharacterId();
                    if(player.hasData("MECHANICAL_GAME")) {
                        var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                        game.invokeAction(player, component, "WRONGLY_PUT_IN");
                    }
                }
            });


            return true;
        }

        private void makeWorkAndToolCheck(IPlayer player, IdentifyMechanicalGameComponent component, string toolName, Action<bool, ToolItem> callback) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT_LONG");
            AnimationController.animationTask(player, anim, () => {
                var tool = player.getInventory().getItem<ToolItem>(t => t.Name == toolName);
                callback.Invoke(tool != null && tool.Flag == component.LosenToolFlag, tool);
            }, null, true, 1, TimeSpan.FromSeconds(10));
        }

    }
}
