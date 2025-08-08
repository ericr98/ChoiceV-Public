using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller {
    public class GameComponentVector {
        public int x;
        public int y;

        public GameComponentVector(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    public abstract class MechanicalGameComponent {
        public int Id { get; private set; }
        public bool Identified { get; set; }
        public bool MovedToStash { get; set; }
        public int Depth { get; private set; }
        public string Image { get; private set; }
        public List<GameComponentVector> Positions { get; private set; }

        public bool WasBlockedByOtherPart { get; set; }

        public MechanicalGameComponent(int id, bool identified, bool movedToStash, int depth, string image, List<GameComponentVector> positions) {
            Id = id;
            Identified = identified;
            MovedToStash = movedToStash;
            Depth = depth;
            Image = image;
            Positions = positions;
        }

        public class GameComponentCefRepresentative {
            public int id;
            public bool iden;
            public int depth;
            public List<GameComponentVector> pos;
            public bool stash;
            public string img;

            public GameComponentCefRepresentative(int id, bool iden, bool movedToStash, int depth, List<GameComponentVector> pos, string img) {
                this.id = id;
                this.iden = iden;
                this.depth = depth;
                this.pos = pos;
                this.stash = movedToStash;
                this.img = img;
            }
        }

        public GameComponentCefRepresentative getCefRepresentative() {
            return new GameComponentCefRepresentative(Id, Identified, MovedToStash, Depth, Positions, Image);
        }

        public abstract Menu getSelectMenu(IPlayer player, bool blockedByOtherPart);

        public string getStringRepresentation() {
            return $"{Id}#{Identified}#{MovedToStash}#{Depth}#{Image}#{Positions.ToJson()}#{getStringRepresentationStep()}";
        }

        public abstract string getStringRepresentationStep();

        public static MechanicalGameComponent getComponentFromStringRepresentation(string representation) {
            var split = representation.Split('#');

            var id = int.Parse(split[0]);
            var identified = bool.Parse(split[1]);
            var movedToStash = bool.Parse(split[2]);
            var depth = int.Parse(split[3]);
            var image = split[4];
            var positions = split[5].FromJson<List<GameComponentVector>>();

            split = split.Skip(6).ToArray();

            switch(split[0]) {
                case "I":
                    return IdentifyMechanicalGameComponent.createFromRepresentation(id, identified, movedToStash, depth, image, positions, split);
                case "M":
                    return VehicleMechanicalGameComponent.createFromRepresentation(id, identified, movedToStash, depth, image, positions, split);
                default:
                    return null;
            }
        }
    }

    public delegate bool MechanicalGameComponentActionCallback(IPlayer player, MechanicalGame game, MechanicalGameComponent component, string action);


    public class MechanicalGame {
        private record GameDepth(int depth, GameComponentVector position);

        public int ColCount;
        public int RowCount;
        public List<MechanicalGameComponent> Components;

        private MechanicalGameComponentActionCallback ActionCallback;

        private List<GameDepth> AllDepths = new List<GameDepth>();

        public bool UpdateFlag { get; set; }

        public MechanicalGame(int colCount, int rowCount, List<MechanicalGameComponent> components, MechanicalGameComponentActionCallback actionCallback) {
            ColCount = colCount;
            RowCount = rowCount;
            Components = components;
            ActionCallback = actionCallback;
        }

        public MechanicalGame(string stringRepresentation, MechanicalGameComponentActionCallback actionCallback) {
            var split = stringRepresentation.Split('~');

            ColCount = int.Parse(split[0]);
            RowCount = int.Parse(split[1]);

            var components = new List<MechanicalGameComponent>();
            var componentSplits = split[2].Split('|');
            foreach(var componentSplit in componentSplits) {
                components.Add(MechanicalGameComponent.getComponentFromStringRepresentation(componentSplit));
            }
            Components = components;

            ActionCallback = actionCallback;
        }

        public void onClickComponentById(IPlayer player, int id, bool blockedByOtherPart) {
            var component = Components.FirstOrDefault(c => c.Id == id);
            component.WasBlockedByOtherPart = blockedByOtherPart;

            var menu = component.getSelectMenu(player, blockedByOtherPart);

            player.showMenu(menu);
        }

        public void onClickComponent(IPlayer player, MechanicalGameComponent component) {
            var menu = component.getSelectMenu(player, component.WasBlockedByOtherPart);

            player.showMenu(menu);
        }

        private class MechanicalGameShowCefEvent : IPlayerCefEvent {
            public string Event { get; set; }
            public int col;
            public int row;
            public int maxDepth;
            public string[] parts;
            public string[] depths;

            public MechanicalGameShowCefEvent(int col, int row, List<MechanicalGameComponent> allComponents, List<GameDepth> depths) {
                Event = "SHOW_MECHANIC_GAME";
                this.col = col;
                this.row = row;
                maxDepth = allComponents.Max(c => c.Depth);
                parts = allComponents.Select(c => c.getCefRepresentative().ToJson()).ToArray();
                this.depths = depths.Select(c => c.ToJson()).ToArray();
            }
        }

        public void showToPlayer(IPlayer player) {
            player.setData("MECHANICAL_GAME", this);
            player.emitCefEventWithBlock(new MechanicalGameShowCefEvent(ColCount, RowCount, Components, AllDepths), "MECHANICAL_GAME");
        }

        public bool anyStashedComponents() {
            return Components.Any(c => c.MovedToStash);
        }

        public void invokeAction(IPlayer player, MechanicalGameComponent component, string action) {
            ActionCallback.Invoke(player, this, component, action);
        }

        public string getStringRepresentation() {
            var comString = "";
            if(Components.Count > 0) {
                comString = Components.First().getStringRepresentation();
                foreach(var component in Components.Skip(1)) {
                    comString += "|" + component.getStringRepresentation();
                }
            }

            return $"{ColCount}~{RowCount}~{comString}";
        }

        public void onGameLvlUpdate(int depth, GameComponentVector position) {
            var already = AllDepths.FirstOrDefault(d => d.position.x == position.x && d.position.y == position.y);

            if(already != null) {
                if(already.depth != depth) {
                    AllDepths.Remove(already);
                    AllDepths.Add(new GameDepth(depth, position));
                }
            } else {
                AllDepths.Add(new GameDepth(depth, position));
            }
        }
    }

    public class MechanicalGameController : ChoiceVScript {
        public MechanicalGameController() {
            EventController.addCefEvent("MECHANICAL_GAME_CLOSED", onMechanicalGameClosed);
            EventController.addCefEvent("MECHANICAL_GAME_ON_CLICK", onMechanicalGameClick);
            EventController.addCefEvent("MECHANICAL_GAME_LVL_DOWN", onMechanicalGameLvlDown);
            EventController.addCefEvent("MECHANICAL_GAME_LVL_UP", onMechanicalGameLvlUp);

            EventController.addMenuEvent("MECHANICAL_GAME_IDENTIFY", onMechanicalGameIdentify);
            EventController.addMenuEvent("MECHANICAL_GAME_MOVE_TO_STASH", onMechanicalGameMoveToStash);

        }

        private void onMechanicalGameClosed(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            player.closeMenu();
            player.resetData("MECHANICAL_GAME");
        }
        
        private class MechanicalGameOnClickEvent {
            public int id;
            public bool blockedByOtherPart;
        }

        private void onMechanicalGameClick(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var mechEvt = new MechanicalGameOnClickEvent();
            mechEvt.PopulateJson(evt.Data);

            if(!player.hasState(Constants.PlayerStates.InAnimationTask)) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                game.onClickComponentById(player, mechEvt.id, mechEvt.blockedByOtherPart);
            } else {
                player.sendBlockNotification("Du bist gerade beschäftigt", "Gerade beschäftigt");
            }
        }

        private class MechanicalGameActionCefEvent : IPlayerCefEvent {
            public string Event { get; set; }
            public int partId;
            public string action;

            public MechanicalGameActionCefEvent(int partId, string action) {
                Event = "UPDATE_MECHANIC_GAME_PART";
                this.partId = partId;
                this.action = action;
            }
        }

        private bool onMechanicalGameIdentify(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                var component = (MechanicalGameComponent)data["Component"];
                var anim = AnimationController.getAnimationByName("WORK_FRONT");

                AnimationController.animationTask(player, anim, () => {
                    component.Identified = true;
                    player.emitCefEventNoBlock(new MechanicalGameActionCefEvent(component.Id, "IDENTIFY"));

                    game.onClickComponent(player, component);
                }, null, true, 1, TimeSpan.FromSeconds(10));

                game.UpdateFlag = true;
            }

            return true;
        }

        private bool onMechanicalGameMoveToStash(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");

                var component = (MechanicalGameComponent)data["Component"];
                var anim = AnimationController.getAnimationByName("WORK_FRONT");

                AnimationController.animationTask(player, anim, () => {
                    component.MovedToStash = true;
                    player.emitCefEventNoBlock(new MechanicalGameActionCefEvent(component.Id, "MOVE_TO_STASH"));

                    game.onClickComponent(player, component);
                }, null, true, 1, TimeSpan.FromSeconds(10));

                game.UpdateFlag = true;
            }

            return true;
        }

        public static void moveMechanicalGameToStash(IPlayer player, MechanicalGameComponent component) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                component.MovedToStash = true;
                player.emitCefEventNoBlock(new MechanicalGameActionCefEvent(component.Id, "MOVE_TO_STASH"));
                game.onClickComponent(player, component);

                game.UpdateFlag = true;
            }
        }

        public static void putMechanicalGameBackIn(IPlayer player, MechanicalGameComponent component) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                component.MovedToStash = false;
                player.emitCefEventNoBlock(new MechanicalGameActionCefEvent(component.Id, "MOVE_BACK_IN"));
                game.onClickComponent(player, component);

                game.UpdateFlag = true;
            }
        }

        public static bool getPositionIsAvailable(IPlayer player, int depth, List<GameComponentVector> positions) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");
                return !game.Components.Any((c) => !c.MovedToStash && c.Depth <= depth && c.Positions.Any(p => positions.Any(p2 => p2.x == p.x && p2.y == p.y)));
            } else {
                return false;
            }
        }

        public static void refreshMenu(IPlayer player, MechanicalGameComponent component) {
            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");

                game.onClickComponent(player, component);
            }
        }

        public static MechanicalGame startMechanicGame(IPlayer player, int colCount, int rowCount, List<MechanicalGameComponent> allComponents, MechanicalGameComponentActionCallback actionCallback) {
            var game = new MechanicalGame(colCount, rowCount, allComponents, actionCallback);
            game.showToPlayer(player);

            return game;
        }


        public static void makeWorkAndToolCheck(IPlayer player, IdentifyMechanicalGameComponent component, string toolName, Action<bool, ToolItem> callback) {
            var anim = AnimationController.getAnimationByName("WORK_FRONT");
            AnimationController.animationTask(player, anim, () => {
                var tool = player.getInventory().getItem<ToolItem>(t => t.Name == toolName);
                callback.Invoke(tool != null && tool.Flag == component.LosenToolFlag, tool);
            }, null, true, 1, TimeSpan.FromSeconds(10));
        }


        private class MechanicalGameLvlDownEvent {
            public int depth;
            public GameComponentVector position;
        }

        private void onMechanicalGameLvlDown(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var mechEvt = new MechanicalGameLvlDownEvent();
            mechEvt.PopulateJson(evt.Data);

            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");

                game.onGameLvlUpdate(mechEvt.depth, mechEvt.position);
            }
        }

        private void onMechanicalGameLvlUp(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var mechEvt = new MechanicalGameLvlDownEvent();
            mechEvt.PopulateJson(evt.Data);

            if(player.hasData("MECHANICAL_GAME")) {
                var game = (MechanicalGame)player.getData("MECHANICAL_GAME");

                game.onGameLvlUpdate(mechEvt.depth, mechEvt.position);
            }
        }

    }
}
