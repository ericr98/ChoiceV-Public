using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Phone {
    internal class PhoneContactController : ChoiceVScript {
        public PhoneContactController() {
            EventController.addCefEvent("SMARTPHONE_CHANGE_CONTACT", onSmartphoneChangeContact);
            EventController.addCefEvent("SMARTPHONE_DELETE_CONTACT", onSmartphoneDeleteContact);
        }

        #region ContactChange

        internal class PhoneChangeContactChangeCefEvent {
            public int itemId;

            public int id;
            public bool favorit;
            public long number;
            public string name;
            public string note;
            public string email;
        }

        private class PhoneChangeContactIdCefEvent : IPlayerCefEvent {
            public string Event { get; private set; }
            public int newId;

            public PhoneChangeContactIdCefEvent(int newId) {
                Event = "PHONE_CONTACT_SET_ID";
                this.newId = newId;
            }
        }

        private void onSmartphoneChangeContact(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneChangeContactChangeCefEvent();
            cefData.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<Smartphone>(i => i.Id == cefData.itemId);
            if(item != null) {
                using(var db = new ChoiceVDb()) {
                    var contact = db.phonecontacts.Find(cefData.id);
                    if(contact != null) {
                        contact.favorit = cefData.favorit ? 1 : 0;
                        contact.number = cefData.number;
                        contact.name = cefData.name;
                        contact.note = cefData.note;
                        contact.email = cefData.email;
                    } else {
                        //CREATE NEW CONTACT
                        if(cefData.id == -1) {
                            var newContact = new phonecontact {
                                ownerNumber = item.CurrentNumber,
                                favorit = cefData.favorit ? 1 : 0,
                                name = cefData.name,
                                note = cefData.note,
                                email = cefData.email,
                                number = cefData.number,
                            };

                            db.phonecontacts.Add(newContact);
                            db.SaveChanges();

                            player.emitCefEventNoBlock(new PhoneChangeContactIdCefEvent(newContact.id));
                        } else {
                            Logger.logError($"onSmartphoneChangeContact: contact not found: charId: {cefData.id}, data:{evt.Data}",
                                $"Fehler im Smartphone Kontakt ändern: Kontakt nicht gefunden", player);
                        }
                    }

                    db.SaveChanges();
                }
            } else {
                Logger.logError($"onSmartphoneChangeContact: item not found: charId: {player.getCharacterId()}, data: {evt.Data}",
                    $"Fehler im Smartphone Kontakt ändern: Smartphone nicht gefunden", player);
            }
        }

        #endregion

        #region DeleteContact

        private class PhoneDeleteContactCefEvent {
            public int id;
        }

        private void onSmartphoneDeleteContact(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneDeleteContactCefEvent();
            cefData.PopulateJson(evt.Data);

            using(var db = new ChoiceVDb()) {
                var contact = db.phonecontacts.Find(cefData.id);

                if(contact != null) {
                    db.phonecontacts.Remove(contact);

                    db.SaveChanges();
                }
            }
        }

        #endregion
    }
}
