using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.Controller.Phone {
    internal record PhoneBookEntry(string name, long number);
    internal class PhoneBookController : ChoiceVScript{
        private List<PhoneBookEntry> PhoneBookEntries = new List<PhoneBookEntry>();

        public PhoneBookController() {
            EventController.addCefEvent("PHONE_PHONE_BOOK_REQUEST", onPhoneBookRequestEntries);

            using(var db = new ChoiceVDb()) {
                PhoneBookEntries = db.configphonebookentries.Select(p => new PhoneBookEntry(p.name, p.number)).ToList();
            }
        }

        private class PhoneRequestPhoneBookEntriesCefEvent {
            public int id;
        }


        private class PhoneRequestPhoneBookCefEvent : PhoneAnswerEvent {
            public int id;
            public string[] entries;

            public PhoneRequestPhoneBookCefEvent(int id, List<PhoneBookEntry> allEntries) : base("PHONE_PHONE_BOOK_ANSWER") {
                this.id = id;
                this.entries = allEntries.Select(m => m.ToJson()).ToArray();
            }
        }

        private void onPhoneBookRequestEntries(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneRequestPhoneBookEntriesCefEvent();
            cefData.PopulateJson(evt.Data);

            player.emitCefEventNoBlock(new PhoneRequestPhoneBookCefEvent(cefData.id, PhoneBookEntries));
        }
    }
}
