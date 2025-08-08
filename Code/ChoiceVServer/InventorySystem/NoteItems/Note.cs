using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;

namespace ChoiceVServer.InventorySystem {
    public class NoteCefEvent : IPlayerCefEvent {
        public string Event { get; }
        public string title;
        public string text;
        public bool readOnly;
        public int id;

        public NoteCefEvent(string title, string text, bool readOnly, int id) {
            Event = "OPEN_NOTE";
            this.title = title;
            this.text = text;
            this.readOnly = readOnly;
            this.id = id;
        }
    }

    //TODO Öffnet Inventar, Fernglas, In Deckung, etc. wenn man die Tasten drücken
    public class Note : Item {
        public string Title { get => Data.hasKey("Title") ? (string)Data["Title"] : ""; set { Data["Title"] = value; } }
        public string Text { get => Data.hasKey("Text") ? (string)Data["Text"] : ""; set { Data["Text"] = value; } }
        public bool Finalized { get => Data.hasKey("Finalized") ? (bool)Data["Finalized"] : false; set { Data["Finalized"] = value; } }

        public Note(item item) : base(item) {
            updateDescription();
        }

        public Note(configitem configItem, int amount, int quality) : base(configItem) {
            updateDescription();
        }

        public Note(configitem configItem, string title, string text, bool finalized) : base(configItem) {
            Title = title;
            Text = text;
            Finalized = finalized;

            updateDescription();
        }

        public override void use(IPlayer player) {
            base.use(player);

            var evt = new NoteCefEvent(Title, Text, Finalized, Id ?? -1);
            player.emitCefEventWithBlock(evt, "CEF_FILE");
        }

        public static void showNoteEvent(IPlayer player, string title, string text, bool finalized) {
            var evt = new NoteCefEvent(title, text, finalized, -1);
            player.emitCefEventWithBlock(evt, "CEF_FILE");
        }


        public override void updateDescription() {
            if(Title == "") {
                Description = "Die Notiz ist unbeschrieben";
            } else {
                Description = "Titel: " + Title;
            }
        }

        public Item getCopy() {
            var cfg = InventoryController.getConfigById(ConfigId);

            return new Note(cfg, Title, Text, Finalized);
        }
    }
}
