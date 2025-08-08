using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.Phone.PhoneContactController;
using static ChoiceVServer.Controller.Phone.PhoneController;

namespace ChoiceVServer.Controller.Phone {
    internal class PhoneMessageController : ChoiceVScript {
        public PhoneMessageController() {
            EventController.addCefEvent("PHONE_MESSENGER_SEND_MESSAGE", onSmartphoneSendMessage);
            EventController.addCefEvent("PHONE_MESSENGER_REQUEST_MESSAGES", onSmartphoneRequestMessage);
            EventController.addCefEvent("PHONE_MESSENGER_READ_MESSAGES", onSmartphoneReadMessages);
            EventController.addCefEvent("PHONE_MESSENGER_READ_SINGLE_MESSAGE", onSmartphoneReadSingleMessage);
        }

        internal static void sendSMSToNumber(long from, long to, string text) {
            using(var db = new ChoiceVDb()) {
                var chat = db.phonechats.FirstOrDefault(c => (c.number1 == from && c.number2 == to) || (c.number1 == to && c.number2 == from));
                if(chat == null) {
                    try {
                        chat = new phonechat {
                            number1 = from,
                            number2 = to,
                        };

                        db.phonechats.Add(chat);
                        db.SaveChanges();
                    } catch(Exception e) {
                        Logger.logException(e);
                        return;
                    }
                }

                var createDate = DateTime.Now;
                var newMessage = new phonechatmessage {
                    chatId = chat.id,
                    from = from,
                    sendDate = createDate,
                    text = text,
                    read = 0,
                };

                db.phonechatmessages.Add(newMessage);
                db.SaveChanges();

                foreach(var target in ChoiceVAPI.GetAllPlayers()) {
                    if(target.getInventory() != null) {
                        var smartphone = target.getInventory().getItem<Smartphone>(i => i.CurrentNumber == chat.number1 || i.CurrentNumber == chat.number2);
                        if(smartphone != null) {
                            target.emitCefEventNoBlock(new PhoneSendChatMessageCefEvent(chat.id, newMessage.id, newMessage.text, createDate, newMessage.from, false));
                        }
                    }
                }
            }
        } 

        #region ReceiveMessage

        private class PhoneReceiveChatMessageCefEvent {
            public int itemId;
            public int chatId;
            public long from;
            public long to;
            public string text;
        }

        private class PhoneSendChatMessageCefEvent : PhoneAnswerEvent {
            private class Message {
                public int id;
                public string text;
                public string date;
                public long from;
                public bool read;

                public Message(int id, string text, DateTime date, long from, bool read) {
                    this.id = id;
                    this.text = text;
                    this.date = date.ToString("yyyy-MM-ddTHH:mm:ss");
                    this.from = from;
                    this.read = read;
                }
            }

            public int chatId;
            public string message;

            public PhoneSendChatMessageCefEvent(int chatId, int id, string text, DateTime date, long from, bool read) : base("PHONE_MESSENGER_MESSAGE_RECEIVE") {
                this.chatId = chatId;
                this.message = new Message(id, text, date, from, read).ToJson();
            }
        }

        private void onSmartphoneSendMessage(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneReceiveChatMessageCefEvent();
            cefData.PopulateJson(evt.Data);

            if(cefData.chatId == -2) {
                PhoneController.sendPhoneNotificationToPlayer(player, "Diese Nummer ist nicht vergeben!");
                return;
            }

            using(var db = new ChoiceVDb()) {
                var chat = db.phonechats.FirstOrDefault(c => (c.number1 == cefData.from && c.number2 == cefData.to) || (c.number1 == cefData.to && c.number2 == cefData.from));
                if(chat == null) {
                    try {
                        chat = new phonechat {
                            number1 = cefData.from,
                            number2 = cefData.to,
                        };

                        db.phonechats.Add(chat);
                        db.SaveChanges();
                    } catch(Exception) {
                        PhoneController.sendPhoneNotificationToPlayer(player, "Diese Nummer ist nicht vergeben!");
                        return;
                    }
                }

                var createDate = DateTime.Now;
                var newMessage = new phonechatmessage {
                    chatId = chat.id,
                    from = cefData.from,
                    sendDate = createDate,
                    text = cefData.text,
                    read = 0,
                };

                db.phonechatmessages.Add(newMessage);
                db.SaveChanges();

                player.emitCefEventNoBlock(new PhoneSendChatMessageCefEvent(chat.id, newMessage.id, newMessage.text, createDate, newMessage.from, true));

                foreach(var target in ChoiceVAPI.GetAllPlayers()) {
                    if(target != player) {
                        var smartphone = target.getInventory().getItem<Smartphone>(i => i.CurrentNumber == chat.number1 || i.CurrentNumber == chat.number2);
                        if(smartphone != null) {
                            target.emitCefEventNoBlock(new PhoneSendChatMessageCefEvent(chat.id, newMessage.id, newMessage.text, createDate, newMessage.from, false));
                        }
                    }
                }
            }
        }

        #endregion

        #region RequestMessages

        private class PhoneRequestMessagesCefEvent {
            public int id;
            public bool currentlySelected;
        }

        private class Message {
            public int id;
            public string text;
            public string date;
            public long from;
            public bool read;

            public Message(int id, string text, DateTime date, long from, bool read) {
                this.id = id;
                this.text = text;
                this.date = date.ToString("yyyy-MM-ddTHH:mm:ss");
                this.from = from;
                this.read = read;
            }
        }

        private class PhoneAnswerMessagesCefEvent : PhoneAnswerEvent {

            public int id;
            public string[] messages;

            public PhoneAnswerMessagesCefEvent(int id, List<Message> allMessages) : base("PHONE_MESSENGER_ANSWER_MESSAGES") {
                this.id = id;
                this.messages = allMessages.Select(m => m.ToJson()).ToArray();
            }
        }

        private void onSmartphoneRequestMessage(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneRequestMessagesCefEvent();
            cefData.PopulateJson(evt.Data);

            using(var db = new ChoiceVDb()) {
                var dbChat = db.phonechats.Include(c => c.phonechatmessages).FirstOrDefault(c => c.id == cefData.id);

                if(dbChat != null) {
                    var messageList = dbChat.phonechatmessages.Select(m => new Message(m.id, m.text, m.sendDate, m.from, m.read == 1 || cefData.currentlySelected)).OrderByDescending(m => m.date).Take(200).ToList();

                    player.emitCefEventNoBlock(new PhoneAnswerMessagesCefEvent(dbChat.id, messageList));

                    if(cefData.currentlySelected) {
                        foreach(var message in dbChat.phonechatmessages.Take(200)) {
                            if(message.from != dbChat.number1) {
                                message.read = 1;
                            }
                        }

                        db.SaveChanges();
                    }
                }
            }

        }

        #endregion

        #region ReadMessages

        private class PhoneReadMessagesCefEvent {
            public int itemId;

            public int id;
        }

        internal void onSmartphoneReadMessages(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneReadMessagesCefEvent();
            cefData.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<Smartphone>(i => i.Id == cefData.itemId);
            if(item != null) {
                var number = item.CurrentNumber;

                using(var db = new ChoiceVDb()) {
                    var dbChat = db.phonechats.Include(c => c.phonechatmessages).FirstOrDefault(c => c.id == cefData.id);

                    if(dbChat != null) {
                        foreach(var message in dbChat.phonechatmessages.Take(200)) {
                            if(message.from != number) {
                                message.read = 1;
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }


        #endregion

        #region ReadSingleMessage

        private void onSmartphoneReadSingleMessage(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = evt.Data.FromJson<PhoneReadMessagesCefEvent>();

            //Only allow if user really has item
            var item = player.getInventory().getItem<Smartphone>(i => i.Id == cefData.itemId);
            if(item != null) {
                using(var db = new ChoiceVDb()) {
                    var dbMessage = db.phonechatmessages.FirstOrDefault(c => c.id == cefData.id);

                    if(dbMessage != null) {
                        dbMessage.read = 1;
                        db.SaveChanges();
                    }
                }
            }

        }

        #endregion
    }
}
