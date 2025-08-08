using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.MiniJobController;
using static ChoiceVServer.Controller.Minijobs.Model.DeliverMinijobTask;
using static ChoiceVServer.Controller.Minijobs.Model.Minijob;
using static ChoiceVServer.Model.Menu.InputMenuItem;
using static ChoiceVServer.Model.Menu.MenuStatsMenuItem;

namespace ChoiceVServer.Controller.Minijobs.Model {
    public class DeliverMinijobTaskController : ChoiceVScript {
        public DeliverMinijobTaskController() {
            EventController.addMenuEvent("ON_DELIVER_MINIJOB_DELIVER", onDeliverMinijobDeliver);
            EventController.addMenuEvent("ON_DELIVER_MINIJOB_ACCEPT", onDeliverMinijobAccept);

            #region Creation

            addMinijobTaskCreator("Abliefer-Minijob", startDeliverMinijobCreation);

            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOBTASK_CREATION", onAdminCreateDeliverMinijob);

            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_CREATE_OPTION", onAdminCreateDeliverOptionCreate);
            EventController.addMenuEvent("ADMIN_DELIVER_MINIJOB_DELETE_OPTION", onAdminCreateDeliverOptionDelete);

            #endregion
        }

        private bool onDeliverMinijobDeliver(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (DeliverMinijobTask)data["Task"];
            var item = (configitem)data["Item"];

            task.onDeliverItems(player, item);

            return true;
        }

        private bool onDeliverMinijobAccept(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (DeliverMinijobTask)data["Task"];
            var instance = (MinijobInstance)data["Instance"];

            task.onFinish(instance);

            return true;
        }

        #region Creation

        private void startDeliverMinijobCreation(IPlayer player, Action<string, Dictionary<string, string>> finishCallback) {
            var menu = new Menu("MeetMinijobTask", "Trage die Infos ein");
            menu.addMenuItem(new InputMenuItem("Name", "Trage den Namen des Treffpunktes ein (z.B. NPC Name)", "", ""));
            menu.addMenuItem(new InputMenuItem("Beschreibung", "Trage ein was an diesem Punkt als Beschreibung steht (Konversation vom NPC)", "", ""));
            menu.addMenuItem(new InputMenuItem("Anzahl an Optionen", "Trage ein wieviele Items abgegeben werden müssen. Sie werden zufällig aus den hinzugefügten bestimmt", "", ""));
            menu.addMenuItem(new MenuStatsMenuItem("Fortfahren", "Fahre mit der Erstellung fort", "ADMIN_DELIVER_MINIJOBTASK_CREATION", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Callback", finishCallback } }));

            player.showMenu(menu);
        }


        private bool onAdminCreateDeliverMinijob(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var callback = (Action<string, Dictionary<string, string>>)data["Callback"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;

            var nameEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var descriptionEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var optionEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            var settings = new Dictionary<string, string>();
            settings.Add("Name", nameEvt.input);
            settings.Add("Description", descriptionEvt.input);
            settings.Add("SelectedAmount", int.Parse(optionEvt.input).ToString());
            settings.Add("AllOptions", new List<DeliverMinijobOption>().ToJson());

            callback.Invoke("DeliverMinijobTask", settings);

            return true;
        }

        private bool onAdminCreateDeliverOptionCreate(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (DeliverMinijobTask)data["Task"];

            var evt = menuItemCefEvent as MenuStatsMenuItemEvent;
            var idEvt = evt.elements[0].FromJson<InputMenuItemEvent>();
            var minEvt = evt.elements[1].FromJson<InputMenuItemEvent>();
            var maxEvt = evt.elements[2].FromJson<InputMenuItemEvent>();

            var cfgItem = InventoryController.getConfigById(int.Parse(idEvt.input));
            var record = new DeliverMinijobOption(cfgItem.configItemId, int.Parse(minEvt.input), int.Parse(maxEvt.input));

            task.AllOptions.Add(record);

            using(var db = new ChoiceVDb()) {
                var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllOptions");
                already.value = task.AllOptions.ToJson();

                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Success, $"{cfgItem.name} {minEvt.input} - {maxEvt.input} erfolgreich hinzugefügt", "");

            return true;
        }

        private bool onAdminCreateDeliverOptionDelete(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
            var task = (DeliverMinijobTask)data["Task"];
            var option = (DeliverMinijobOption)data["Option"];

            task.AllOptions.Remove(option);

            using(var db = new ChoiceVDb()) {
                var already = db.configminijobstaskssettings.FirstOrDefault(s => s.taskId == task.Id && s.name == "AllOptions");
                already.value = task.AllOptions.ToJson();

                db.SaveChanges();
            }

            player.sendNotification(Constants.NotifactionTypes.Success, $"{option.ItemId} {option.MinAmount} - {option.MaxAmount} erfolgreich entfernt", "");

            return true;
        }

        #endregion
    }

    public class DeliverMinijobTask : MinijobTask {
        public record DeliverMinijobOption(int ItemId, int MinAmount, int MaxAmount);

        private class InstanceInfo {
            public MinijobInstance Instance;
            public List<int> SelectedItems;
            public List<int> SelectedItemsAmount;

            public InstanceInfo(MinijobInstance instance, List<int> selectedItems, List<int> selectedItemsAmount) {
                Instance = instance;
                SelectedItems = selectedItems;
                SelectedItemsAmount = selectedItemsAmount;
            }
        }

        //Config
        private string Name;
        private string Description;
        private int SelectedAmount;
        public List<DeliverMinijobOption> AllOptions;

        private List<InstanceInfo> AllInstanceInfos;

        public DeliverMinijobTask(Minijob master, int id, int order, string spotStr, Dictionary<string, string> settings) : base(master, id, order, spotStr, settings) {
            Name = Settings["Name"];
            Description = Settings["Description"];
            SelectedAmount = int.Parse(Settings["SelectedAmount"]);
            AllOptions = Settings["AllOptions"].FromJson<List<DeliverMinijobOption>>();

            AllInstanceInfos = new();
        }

        public override void onInteractionStep(IPlayer player, MinijobInstance instance) {
            var menu = new Menu(Name, "Was möchstest du tun?");
            menu.addMenuItem(new StaticMenuItem("Information", Description, ""));
            menu.addMenuItem(new StaticMenuItem("Task-Bearbeitung", $"Besorge die unter \"Benötigte Sachen\" spezifizierten Items und gib die ab. Das machst du in dem du das Menuelement auswählst.", "Siehe Beschreibung"));

            var info = AllInstanceInfos.Find(i => i.Instance == instance);

            if(info.SelectedItemsAmount.Any(i => i != 0)) {
                var neededMenu = new Menu("Benötigte Sachen", "Welche Sachen abgeben?");

                for(var i = 0; i < info.SelectedItems.Count; i++) {
                    var itemId = info.SelectedItems[i];
                    var amount = info.SelectedItemsAmount[i];

                    var item = InventoryController.getConfigById(itemId);

                    if(amount <= 0) {
                        neededMenu.addMenuItem(new StaticMenuItem(item.name, $"Es werden noch {amount}x {item.name} benötigt", $"{amount}"));
                    } else {
                        var data = new Dictionary<string, dynamic> { { "Task", this }, { "Item", item } };
                        neededMenu.addMenuItem(new ClickMenuItem(item.name, $"Es werden noch {amount}x {item.name} benötigt. Klicken um abzugeben", $"{amount}", "ON_DELIVER_MINIJOB_DELIVER").withData(data).needsConfirmation($"{item.name} abgeben?", $"Verfügbare {item.name} abgeben?"));
                    }
                }

                menu.addMenuItem(new MenuMenuItem(neededMenu.Name, neededMenu));
            } else {
                menu.addMenuItem(new ClickMenuItem("Fortfahren", "Fahre fort", "", "ON_DELIVER_MINIJOB_ACCEPT", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Task", this }, { "Instance", instance } }));
            }

            //neededMenu.addMenuItem(new ClickMenuItem("Sachen abgeben", "Gib alle Sachen aus deinen Taschen ab, die passen", "", "ON_MEET_MINIJOB_DELIVER", MenuItemStyle.green).withData(data));

            player.showMenu(menu);
        }

        protected override void onShowInfoStep(MinijobInstance instance, IPlayer player) { }

        protected override void prepareStep() { }

        public override void start(MinijobInstance instance) {
            var already = new List<int>();
            var counter = 0;

            var rand = new Random();

            var selected = new List<DeliverMinijobOption>();

            while(already.Count < SelectedAmount && counter < 1000) {
                counter++;
                var next = rand.Next(0, AllOptions.Count);

                if(!already.Contains(next)) {
                    already.Add(next);
                    selected.Add(AllOptions[next]);
                }
            }

            var selectedItems = new List<int>();
            var selectedItemsAmount = new List<int>();

            foreach(var option in selected) {
                selectedItems.Add(option.ItemId);
                selectedItemsAmount.Add(rand.Next(option.MinAmount, option.MaxAmount + 1));
            }

            AllInstanceInfos.Add(new InstanceInfo(instance, selectedItems, selectedItemsAmount));
        }


        protected override void resetStep() {
            AllInstanceInfos.Clear();
        }

        public void onDeliverItems(IPlayer player, configitem dbItem) {
            lock(AllInstanceInfos) {
                var info = AllInstanceInfos.FirstOrDefault(i => i.Instance.ConnectedPlayersIds.Contains(player.getCharacterId()));
                if(info != null) {
                    var idx = info.SelectedItems.FindIndex(c => c == dbItem.configItemId);
                    var amount = info.SelectedItemsAmount[idx];

                    var items = player.getInventory().getItems(amount, i => i.ConfigId == dbItem.configItemId);

                    if(items.Count > 0) {
                        var first = items.First();

                        var possibleAmount = amount;
                        if(first.CanBeStacked) {
                            if(first.StackAmount < amount) {
                                possibleAmount = first.StackAmount ?? 1;
                            }

                        } else {
                            if(items.Count < amount) {
                                possibleAmount = items.Count;
                            }
                        }

                        if(player.getInventory().removeSimelarItems(first, possibleAmount)) {
                            info.SelectedItemsAmount[idx] -= possibleAmount;
                            player.sendNotification(Constants.NotifactionTypes.Success, $"Du hast {possibleAmount}x {dbItem.name} abgegeben", $"{amount - possibleAmount}x {dbItem.name}", Constants.NotifactionImages.MiniJob);
                        } else {
                            player.sendBlockNotification("Etwas ist schiefgelaufen. Code: DeliverMonkey. Gehe in den Support", "Fehlercode: DeliverMonkey");
                        }

                    }
                } else {
                    player.sendBlockNotification("Keine passenden Items dabei", "Nichts passendes", Constants.NotifactionImages.MiniJob);
                }
            }
        }

        public override void preprepare() { }

        protected override void finishStep(MinijobInstance instance) {
            AllInstanceInfos.RemoveAll(i => i.Instance == instance);
        }

        protected override void finishForPlayerStep(MinijobInstance instance, IPlayer player) { }

        public override bool isMultiViable() {
            return true;
        }

        #region Admin stuff

        public override List<MenuItem> getAdminMenuElements() {
            var list = new List<MenuItem>();

            list.Add(new StaticMenuItem("Anzahl an Optionen", "Trage ein wieviele Items abgegeben werden müssen. Sie werden zufällig aus den hinzugefügten bestimmt", $"{SelectedAmount}"));

            var alreadyMenu = new Menu("Item-Optionen", "Was möchtest du tun?");
            foreach(var option in AllOptions) {
                var item = InventoryController.getConfigById(option.ItemId);

                var name = "NICHT GEFUNDEN!";
                var configId = -1;
                if(item != null) {
                    name = item.name;
                    configId = item.configItemId;
                }

                var alreadyData = new Dictionary<string, dynamic> { { "Task", this }, { "Option", item } };
                var itemMenu = new Menu($"{name}", "Was möchtest du tun");
                itemMenu.addMenuItem(new StaticMenuItem("Item", $"Das Item ist {name} mit der ConfigId {configId}", $"{configId}"));
                itemMenu.addMenuItem(new StaticMenuItem("Min. Anzahl", $"Es werden mind. {option.MinAmount} gefordert", $"{option.MinAmount}"));
                itemMenu.addMenuItem(new StaticMenuItem("Max. Anzahl", $"Es werden mind. {option.MaxAmount} gefordert", $"{option.MaxAmount}"));
                itemMenu.addMenuItem(new ClickMenuItem("Option löschen", "Lösche die Option", "", "ADMIN_DELIVER_MINIJOB_DELETE_OPTION", MenuItemStyle.red).withData(alreadyData).needsConfirmation($"{name} löschen?", "Option wirklich löschen"));

                alreadyMenu.addMenuItem(new MenuMenuItem(itemMenu.Name, itemMenu));
            }

            list.Add(new MenuMenuItem(alreadyMenu.Name, alreadyMenu));

            var createData = new Dictionary<string, dynamic> { { "Task", this } };
            var createMenu = new Menu("Option erstellen", "Gib die Daten ein");
            createMenu.addMenuItem(new InputMenuItem("Item-Id", "Gib die Datenbank Id des Items ein", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Min. Anzahl", "Die ein wieviele Items mind. gefordert werden können", "", ""));
            createMenu.addMenuItem(new InputMenuItem("Max. Anzahl", "Die ein wieviele Items max. gefordert werden können", "", ""));
            createMenu.addMenuItem(new MenuStatsMenuItem("Erstellen", "Erstelle die angegebene Option", "ADMIN_DELIVER_MINIJOB_CREATE_OPTION", MenuItemStyle.green).withData(createData).needsConfirmation("Option erstellen?", "Option wirklich erstellen?"));

            list.Add(new MenuMenuItem(createMenu.Name, createMenu, MenuItemStyle.green));

            return list;
        }

        #endregion
    }
}
