//using AltV.Net.Data;
//using AltV.Net.Elements.Entities;
//using ChoiceVServer.Admin.Tools;
//using ChoiceVServer.Base;
//using ChoiceVServer.EventSystem;
//using ChoiceVServer.Model;
//using ChoiceVServer.Model.Menu;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static ChoiceVServer.Controller.MiniJobController;
//using static ChoiceVServer.Model.Menu.InputMenuItem;
//using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

//namespace ChoiceVServer.Controller {
//    public class AdjustMiniJobTaskController : ChoiceVScript {
//        public AdjustMiniJobTaskController() {
//            EventController.addMenuEvent("ON_ADJUST_MINIJOB_ACCEPT", onAdjustMinijobAccept);

//            EventController.addMenuEvent("ADJUST_MINIJOB_ADJUST_SENDER", onAdjustMinijobTaskSender);

//            #region Creation

//            addMinijobTaskCreator("Einstell-Minijob", startAdjustMinijobCreation);

//            EventController.addMenuEvent("ADMIN_ADJUST_MINIJOBTASK_CREATION", onAdminCreateAdjustMinijob);

//            #endregion
//        }

//        private bool onAdjustMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var task = (AdjustMinijobTask)data["Task"];
//            task.onFinish();

//            return true;
//        }

//        private bool onAdjustMinijobTaskSender(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            //var task = (AdjustMinijobTaskSendParameter)data["Task"];



//            return true;
//        }

//        #region Creation

//        protected static void startAdjustMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
//            var menu = new Menu("AdjustMinijobTask", "Trage die Infos ein");
//            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
//            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
//            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_ADJUST_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

//            player.showMenu(menu);
//        }

//        private bool onAdminCreateAdjustMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
//            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

//            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

//            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
//            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

//            var settings = new Dictionary<string, string>();
//            settings.Add("Name", nameEvt.input);
//            settings.Add("Description", descriptionEvt.input);


//            callback.Invoke("AdjustMinijobTask", settings);

//            return true;
//        }

//        #endregion
//    }

//    public class AdjustMinijobTask : MinijobTask {
//        private class AdjustMinijobTaskSpot {
//            public AdjustMinijobTask Master;
//            private string Name;
//            private CollisionShape Shape;

//            private List<AdjustMinijobTaskParameter> ParameterList;

//            public AdjustMinijobTaskSpot(AdjustMinijobTask master, string name, CollisionShape shape, List<AdjustMinijobTaskParameter> parameters) {
//                Master = master;
//                Name = name;
//                Shape = shape;

//                Shape.OnCollisionShapeInteraction += onInteract;
//                ParameterList = parameters;
//            }

//            private void onInteract(IPlayer player) {
//                var menu = new Menu(Name, "Was möchtest du tun?");

//                foreach(var parameter in ParameterList) {
//                    menu.addMenuItem(parameter.getMenuItem(player, Master.AllParameters));
//                }

//                player.showMenu(menu);
//            }
//        }

//        private abstract class AdjustMinijobTaskParameter {
//            protected AdjustMinijobTask Master;
//            protected string Name;
//            public string Group;

//            public AdjustMinijobTaskParameter(AdjustMinijobTask master, string name, string group) {
//                Master = master;
//                Name = name;
//                Group = group;
//            }

//            public abstract MenuItem getMenuItem(IPlayer player, List<AdjustMinijobTaskParameter> allParameters);
//        }

//        private class AdjustMinijobTaskSendParameter : AdjustMinijobTaskParameter {
//            public List<string> Options;
//            public int SweetSpot;

//            public AdjustMinijobTaskSendParameter(AdjustMinijobTask master, string name, string group, List<string> options) : base(master, name, group) {
//                Options = options;
//                SweetSpot = new Random().Next(0, Options.Count);
//            }

//            public override MenuItem getMenuItem(IPlayer player, List<AdjustMinijobTaskParameter> allParameters) {
//                return new ListMenuItem(Name, $"Gehört zur Gruppe: {Group}. Stelle den richtigen Wert ein. Die Richtigkeit kann an einem anderen Punkt überprüft werden", Options.ToArray(), "ADJUST_MINIJOB_ADJUST_SENDER", MenuItemStyle.normal, true, true).withData(new Dictionary<string, dynamic> { { "Paramter", this } });
//            }
//        }

//        private class AdjustMinijobTaskReceiveParameter : AdjustMinijobTaskParameter {
//            private int Noise;

//            public AdjustMinijobTaskReceiveParameter(AdjustMinijobTask master, string name, string group, int noise) : base(master, name, group) {
//                Noise = noise;
//            }


//            public override MenuItem getMenuItem(IPlayer player, List<AdjustMinijobTaskParameter> allParameters) {
//                var sameGroup = allParameters.Where(p => p.Group == Group && p is AdjustMinijobTaskSendParameter).Cast<AdjustMinijobTaskSendParameter>().ToList();
//                var totalDistance = 0f;

//                foreach(var sender in sameGroup) {
//                    totalDistance += Math.Abs(sender.Options.Count - sender.SweetSpot);
//                }

//                var str = "";
//                var style = MenuItemStyle.normal;
//                if(totalDistance < Noise) {
//                    str = "perfekt";
//                    style = MenuItemStyle.green;
//                } else {
//                    var value = (totalDistance - Noise) / sameGroup.Count;
//                    if(value < 1) {
//                        str = "fast perfekt";
//                        style = MenuItemStyle.normal;
//                    } else if(value < 3) {
//                        str = "annehmbar";
//                        style = MenuItemStyle.yellow;
//                    } else {
//                        str = "falsch";
//                        style = MenuItemStyle.red;
//                    }
//                }

//                return new StaticMenuItem(Name, $"Gehört zur Gruppe: {Group}. Der Wert ist {str} eingestellt. Schraube an den Parametern dieser Gruppe herum", str, style);
//            }
//        }

//        private string Name;
//        private string Description;

//        private List<AdjustMinijobTaskParameter> AllParameters;
//        private List<AdjustMinijobTaskSpot> AllSpots;

//        public AdjustMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
//            Name = Settings["Name"];
//            Description = Settings["Description"];

//            AllParameters = new();
//            AllSpots = new();

//            var sender = new AdjustMinijobTaskSendParameter(this, "Frequenz", "Radio-Frequenz", new List<string> { "1Ghz", "2Ghz", "3Ghz", "4Ghz", "5Ghz", "6Ghz", "7Ghz", "8Ghz" });
//            var receiver = new AdjustMinijobTaskReceiveParameter(this, "Radiogerät", "Radio-Frequenz", 0);

//            AllParameters.Add(sender);
//            AllParameters.Add(receiver);

//            var spot = new AdjustMinijobTaskSpot(this, "Tisch", CollisionShape.Create(new Position(-77.916756f, -444.15527f, 35.921883f), 4, 4, 0, true, false, true), AllParameters);
//            AllSpots.Add(spot);
//        }

//        public override void onInteractionStep(IPlayer player) {
//            var menu = new Menu(Name, "Was möchstest du tun?");
//            menu.addMenuItem(new StaticMenuItem("Information", Description, ""));
//            menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_ADJUST_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this } }));
//            player.showMenu(menu);
//        }

//        public override void onShowInfoStep(IPlayer player) { }

//        public override void onFinish() {

//            base.onFinish();
//        }

//        protected override void prepareStep() { }

//        protected override void resetStep() { }

//        public override void preprepare() { }

//        public override bool isMultiUseViable() {
//            return false;
//        }
//    }
//}
