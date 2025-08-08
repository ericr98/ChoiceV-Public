using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using static ChoiceVServer.Controller.MiniJobController;
using static ChoiceVServer.Controller.Minijobs.Model.Minijob;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Minijobs.Model {
    public class MeetMiniJobTaskController : ChoiceVScript {
        public MeetMiniJobTaskController() {
            EventController.addMenuEvent("ON_MEET_MINIJOB_ACCEPT", onMeetMinijobAccept);

            #region Creation

            addMinijobTaskCreator("Treff-Minijob", startMeetMinijobCreation);

            EventController.addMenuEvent("ADMIN_MEET_MINIJOBTASK_CREATION", onAdminCreateMeetMinijob);

            #endregion
        }

        private bool onMeetMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (MeetMinijobTask)data["Task"];
            var instance = (MinijobInstance)data["Instance"];

            task.onFinish(instance);

            return true;
        }

        #region Creation

        protected static void startMeetMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
            var menu = new Menu("MeetMinijobTask", "Trage die Infos ein");
            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_MEET_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

            player.showMenu(menu);
        }

        private bool onAdminCreateMeetMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();

            var settings = new Dictionary<string, string>();
            settings.Add("Name", nameEvt.input);
            settings.Add("Description", descriptionEvt.input);


            callback.Invoke("MeetMinijobTask", settings);

            return true;
        }

        #endregion
    }

    public class MeetMinijobTask : MinijobTask {
        private string Name;
        private string Description;

        public MeetMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
            Name = Settings.ContainsKey("Name") ? Settings["Name"] : "";
            Description = Settings.ContainsKey("Description") ? Settings["Description"] : "";
        }

        public override void onInteractionStep(IPlayer player, MinijobInstance instance) {
            var menu = new Menu(Name, "Was möchstest du tun?");
            menu.addMenuItem(new StaticMenuItem("Information", Description, ""));
            menu.addMenuItem(new StaticMenuItem("Task-Bearbeitung", $"Gehe zur auf der Karte markierten Position und fahre mit dem Minijob fort", "Siehe Beschreibung"));
            menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_MEET_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
            player.showMenu(menu);
        }

        protected override void onShowInfoStep(MinijobInstance instance, IPlayer player) { }

        protected override void prepareStep() { }

        protected override void resetStep() { }

        public override void preprepare() { }


        public override void start(MinijobInstance instance) { }

        protected override void finishStep(MinijobInstance instance) { }

        protected override void finishForPlayerStep(MinijobInstance instance, IPlayer player) { }

        public override bool isMultiViable() {
            return true;
        }
    }
}
