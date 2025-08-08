using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Controller.Phone;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.InventorySystem {
    public class Smartphone : EquipItem, InventoryAuraItem {
        public int Version { get => (int)Data["Version"]; set => Data["Version"] = value; }
        public Inventory SIMCardInventory { get => InventoryController.loadInventory((int)Data["InventoryId"]); set => Data["InventoryId"] = value.Id; }
        public Inventory SecondarySIMCardInventory { get => InventoryController.loadInventory((int)Data["SecondaryInventoryId"]); set => Data["SecondaryInventoryId"] = value.Id; }
        public int BackgroundId { get => (int)Data["BackgroundId"]; set => Data["BackgroundId"] = value; }
        public int RingtoneId { get => (int)Data["RingtoneId"]; set => Data["RingtoneId"] = value; }

        public bool Selected { get => (bool)Data["Selected"]; set => Data["Selected"] = value; }

        public long CurrentNumber { get => getSimCardNumber(); }
        public long SecondaryNumber { get => getSecondarySimCardNumber(); }

        private long getSimCardNumber() {
            var sim = getSIMCard();
            if(SIMCardInventory.getAllItems().Count != 0 && !sim.Settings.flyMode) {
                return sim.Number;
            } else {
                return -1L;
            }
        }

        private long getSecondarySimCardNumber() {
            if(SecondarySIMCardInventory.getAllItems().Count != 0) {
                return getSecondarySIMCard().Number;
            } else {
                return -1L;
            }
        }
        public Smartphone(item item) : base(item) {
            init();
        }


        //Constructor for generic generation
        //TODO make that this method always get the newest version! (e.g. in config)
        public Smartphone(configitem configItem, int amount, int quality) : base(configItem) {
            Version = 1;
            BackgroundId = 1;
            RingtoneId = 1;
            SIMCardInventory = InventoryController.createInventory(Id ?? -1, 100, InventoryTypes.SIMCardSlot);
            SecondarySIMCardInventory = InventoryController.createInventory(Id ?? -1, 100, InventoryTypes.SIMCardSlot);
            Selected = false;
            init();

            updateDescription();
        }

        public Smartphone(configitem configItem, int version) : base(configItem) {
            Version = version;
            BackgroundId = 1;
            RingtoneId = 1;
            SIMCardInventory = InventoryController.createInventory(Id ?? -1, 100, InventoryTypes.SIMCardSlot);
            SecondarySIMCardInventory = InventoryController.createInventory(Id ?? -1, 100, InventoryTypes.SIMCardSlot);
            Selected = false;
            init();

            updateDescription();
        }

        public void init() {
            SIMCardInventory.BlockStatements.Add(
                new InventoryAddBlockStatement(
                    this,
                    i => !(i is SIMCard)
                )
            );

            SecondarySIMCardInventory.BlockStatements.Add(
                new InventoryAddBlockStatement(
                    this,
                    i => !(i is SIMCard)
                )
            );
        }

        public override void use(IPlayer player) {
            var menu = new Menu("Smartphone", "Was möchtest du tun?");

            var other = player.getInventory().getItem<Smartphone>(i => i.IsEquipped && i != this);
            if(other == null) {
                if(IsEquipped) {
                    menu.addMenuItem(new ClickMenuItem("Smartphone wegpacken", "Entferne dieses Smartphone für die Benutzung", "", "UNEQUIP_SMARTPHONE").withData(new Dictionary<string, dynamic> { { "Item", this } }));
                } else {
                    menu.addMenuItem(new ClickMenuItem("Smartphone auswählen", "Wähle dieses Smartphone für die Benutzung", "", "EQUIP_SMARTPHONE").withData(new Dictionary<string, dynamic> { { "Item", this } }));
                }
            } else {
                player.sendBlockNotification("Du hast schon ein Smartphone ausgerüstet", "Smartphone blockiert", Constants.NotifactionImages.System);
            }

            if(!IsEquipped) {
                var simCards = player.getInventory().getItems<SIMCard>(i => true);

                if(SIMCardInventory.getCount() > 0) {
                    var sim = SIMCardInventory.getItem<SIMCard>(i => true);
                    menu.addMenuItem(new ClickMenuItem("SIM-Karte entnehmen", $"Entnimm die Karte mit der Nummer {formatPhoneNumber(sim.Number)}", "", "UNEQUIP_SIM_CARD", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Item", this }, { "SIMCard", sim }, { "Inventory", SIMCardInventory } }));
                } else {
                    var cardsMenu = new Menu("SIM-Karten Auswahl", "Wähle eine Sim Karte aus");
                    foreach(var sim in simCards) {
                        cardsMenu.addMenuItem(new ClickMenuItem(formatPhoneNumber(sim.Number), "Wähle diese Karte aus", "", SIMCardInventory.getAllItems().Count == 0 ? "EQUIP_SIM_CARD" : "", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this }, { "SIMCard", sim }, { "Inventory", SIMCardInventory } }));
                    }

                    menu.addMenuItem(new MenuMenuItem(cardsMenu.Name, cardsMenu));
                }

                if(SecondarySIMCardInventory.getCount() > 0) {
                    var sim = SecondarySIMCardInventory.getItem<SIMCard>(i => true);
                    menu.addMenuItem(new ClickMenuItem("Sekundär SIM-Karte entnehmen", $"Entnimm die Karte mit der Nummer {formatPhoneNumber(sim.Number)}", "", "UNEQUIP_SIM_CARD", MenuItemStyle.red).withData(new Dictionary<string, dynamic> { { "Item", this }, { "SIMCard", sim }, { "Inventory", SecondarySIMCardInventory } }));
                } else {
                    var secCardsMenu = new Menu("Sekundär SIM-Karten Auswahl", "Wähle eine Sim Karte aus");
                    foreach(var sim in simCards) {
                        secCardsMenu.addMenuItem(new ClickMenuItem(formatPhoneNumber(sim.Number), "Wähle diese Karte aus", "", SecondarySIMCardInventory.getAllItems().Count == 0 ? "EQUIP_SIM_CARD" : "", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Item", this }, { "SIMCard", sim }, { "Inventory", SecondarySIMCardInventory } }));
                    }

                    menu.addMenuItem(new MenuMenuItem(secCardsMenu.Name, secCardsMenu));
                }
            }

            player.showMenu(menu);
        }

        public bool hasNumber(long number) {
            return (CurrentNumber == number || SecondaryNumber == number) && !getSIMCard().Settings.flyMode;
        }

        public SIMCard getSIMCard() {
            return SIMCardInventory.getItem<SIMCard>(i => true);
        }

        public SIMCard getSecondarySIMCard() {
            return SecondarySIMCardInventory.getItem<SIMCard>(i => true);
        }

        private class PlayerEquipPhoneEvent : IPlayerCefEvent {
            private class Contact {
                public int id;
                public bool favorit;
                public long number;
                public string name;
                public string note;
                public string email;

                public Contact(int id, bool favorit, long number, string name, string note, string email) {
                    this.id = id;
                    this.favorit = favorit;
                    this.number = number;
                    this.name = name;
                    this.note = note;
                    this.email = email;
                }
            }

            public string Event { get; private set; }

            public int itemId;
            public int version;
            public long number;
            public int background;
            public int ringtone;
            public string[] contacts;
            public string settings;

            public PlayerEquipPhoneEvent(int itemId, int version, long number, int background, int ringtone, phonecontact[] contacts, SIMCardSettings settings) {
                Event = "EQUIP_PHONE";
                this.itemId = itemId;
                this.version = version;
                this.number = number;
                this.background = background;
                this.ringtone = ringtone;
                this.contacts = contacts.Select(c => new Contact(c.id, c.favorit == 1, c.number, c.name, c.note, c.email).ToJson()).ToArray();
                this.settings = settings.ToJson();
            }
        }

        public override void equip(IPlayer player) {
            base.equip(player);

            Selected = true;
            var number = -1L;
            var sim = getSIMCard();
            if(sim != null) {
                number = sim.Number;
            }

            phonecontact[] allContacts;
            using(var db = new ChoiceVDb()) {
                allContacts = db.phonecontacts.Where(c => c.ownerNumber == number).ToArray();
            }

            player.emitCefEventNoBlock(new PlayerEquipPhoneEvent(Id ?? -1, Version, number, BackgroundId, RingtoneId, allContacts, sim != null ? sim.Settings : new SIMCardSettings()));

            handleMessenger(player, number);

            PhoneController.sendCallListToPlayer(player, number);
        }

        public override void fastEquip(IPlayer player) {
            equip(player);
        }

        private class PhoneAnswerMessengerChats : PhoneAnswerEvent {
            public string[] chats;

            public PhoneAnswerMessengerChats(string[] chats) : base("PHONE_ANSWER_MESSENGER_CHATS") {
                this.chats = chats;
            }
        }

        private class Chat {
            public int id;
            public long number;
            public int missed;
            public string lastD;
            public string lastM;

            public Chat(int id, long number, int missed, DateTime lastD, string lastM) {
                this.id = id;
                this.number = number;
                this.missed = missed;
                this.lastD = lastD.ToString("yyyy-MM-ddTHH:mm:ss");
                this.lastM = lastM;
            }
        }

        private void handleMessenger(IPlayer player, long number) {
            var chatList = new List<string>();

            using(var db = new ChoiceVDb()) {
                var chats = db.phonechats.Include(c => c.phonechatmessages).Where(c => c.number1 == number || c.number2 == number).ToList();
                var chatIdList = chats.Select(c => c.id).ToList();
                foreach(var chat in chats) {
                    var num = chat.number1;
                    if(number == num) {
                        num = chat.number2;
                    }

                    var max = DateTime.MinValue;
                    phonechatmessage maxMessage = null;
                    var notReadCount = 0;
                    foreach(var message in chat.phonechatmessages) {
                        if(message.read == 0 && message.from != number) {
                            notReadCount++;
                        }

                        if(message.sendDate > max) {
                            max = message.sendDate;
                            maxMessage = message;
                        }
                    }

                    var model = new Chat(chat.id, num, notReadCount, maxMessage != null ? maxMessage.sendDate : DateTime.MinValue, maxMessage != null ? new string(maxMessage.text.Take(35).ToArray()) : "");

                    chatList.Add(model.ToJson());
                }

                player.emitCefEventNoBlock(new PhoneAnswerMessengerChats(chatList.ToArray()));
            }
        }

        public override void unequip(IPlayer player) {
            base.unequip(player);

            Selected = false;

            player.emitCefEventNoBlock(new OnlyEventCefEvent("UNEQUIP_PHONE"));
        }

        public override void fastUnequip(IPlayer player) {
            unequip(player);
        }

        public void onEnterInventory(Inventory inventory) {
            if(Selected) {
                var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == inventory.OwnerId);
                if(player != null) {
                    equip(player);
                } else {
                    Selected = false;
                }
            }
        }

        public void onExitInventory(Inventory inventory, bool becauseOfUnload) {
            if(Selected) {
                var player = ChoiceVAPI.GetAllPlayers().Find(p => p.getCharacterId() == inventory.OwnerId);
                if(player != null) {
                    unequip(player);
                }

                Selected = false;
            }
        }

        public override void updateDescription() {
            var number = CurrentNumber;
            var secNumb = SecondaryNumber;
            var str = "";

            if(number != -1) {
                str += $", SIM-1: {formatPhoneNumber(number)}";
            }

            if(secNumb != -1) {
                str += $", SIM-2: {formatPhoneNumber(secNumb)}";
            }

            if(number == -1 && secNumb == -1) {
                str += ", keine SIM eingelegt!";
            }

            Description = $"Version: {Version}{str}";
            base.updateDescription();
        }

        public override void destroy(bool toDb = true) {
            InventoryController.destroyInventory(SIMCardInventory);
            InventoryController.destroyInventory(SecondarySIMCardInventory);
            base.destroy(toDb);
        }
    }
}
