using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class FlipBookEvent : IPlayerCefEvent {
        public string Event { get; }
        public string url { get; }

        public FlipBookEvent(string url) {
            Event = "OPEN_PDF_URL";
            this.url = url;
        }
    }

    public class FlipBook : Item {
        public string FlipBookIdentifier { get => (string)Data["FlipBookIdentifier"]; private set => Data["FlipBookIdentifier"] = value; }
        public FlipBook(item dbItem) : base(dbItem) { }

        public FlipBook(configitem configItem, int amount, int quality) : base(configItem, quality) { }

        public FlipBook(configitem configItem, string identifier, string description) : base(configItem) {
            FlipBookIdentifier = identifier;
            Description = description;

            updateDescription();
        }

        public override void use(IPlayer player) {
            base.use(player);

            player.emitCefEventWithBlock(new FlipBookEvent($"{NoteController.getFlipbookUrl(FlipBookIdentifier)}.pdf"), "FLIPBOOK");
        }

        public override void processAdditionalInfo(string info) {
            if(!string.IsNullOrEmpty(info)) {
                FlipBookIdentifier = info;
            }
        }

        public override void setCreateData(Dictionary<string, dynamic> data) {
            if(data.TryGetValue("VERSION", out var version)) {
                var newspaper = NewspaperController.getNewspaperByName((string)version);
                
                if(newspaper != null) {
                    FlipBookIdentifier = newspaper.Identifier;
                    Description = newspaper.Name;
                    updateDescription();
                    return;
                }
            }

            Description = "Unleserliche Zeitung. Enstanden durch einen Druckfehler.";
            updateDescription();
        }
    }
}
