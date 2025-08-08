using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class SocialSecurityCardCefEvent : IPlayerCefEvent {
        public string Event { get; private set; }

        public long number;
        public string name;

        public SocialSecurityCardCefEvent(long number, string name) {
            Event = "OPEN_SOCIAL_SECURITY_CARD";
            this.number = number;
            this.name = name;
        }
    }

    public class SocialSecurityCard : IdCardItem {
        public SocialSecurityCard(item item) : base(item) {
            EventName = "OPEN_SOCIAL_SECURITY_CARD";
        }

        public SocialSecurityCard(configitem configItem, IPlayer player) : base(configItem) {
            EventName = "OPEN_SOCIAL_SECURITY_CARD";
            var list = new List<IdCardItemElement>();

            var name = player.getCharacterShortenedName();
            var social = player.getSocialSecurityNumber();
            if(social.Length < 4) {
                var next = getNextSocialSecurityCardId(social);
                using(var db = new ChoiceVDb()) {
                    var dbChar = db.characters.Find(player.getCharacterId());
                    if(dbChar != null) {
                        dbChar.socialSecurityNumber = next;
                        db.SaveChanges();
                    } else {
                        Logger.logFatal($"SocialSecurityCard: Character not found! {player.getCharacterId()}");
                    }
                }

                list.Add(new IdCardItemElement("number", next.ToString()));
                social = next;
            } else {
                list.Add(new IdCardItemElement("number", social.ToString()));
            }

            list.Add(new IdCardItemElement("name", name));

            setData(list);

            Description = $"Social-Security Karte von {name} mit Nummer {formatSocialSecurityNumber(social)}";
            updateDescription();
        }

        public static string getNextSocialSecurityCardId(string prefix) {
            var rnd = new Random();
            var suffix = rnd.Next(0, 999999);
            var already = false;
            using(var db = new ChoiceVDb()) {
                already = db.characters.FirstOrDefault(c => c.socialSecurityNumber == prefix + suffix.ToString()) != null;
            }

            if(already) {
                return getNextSocialSecurityCardId(prefix);
            } else {
                return prefix + suffix.ToString();
            }
        }

        public override void use(IPlayer player) {
            base.use(player);

            player.emitCefEventWithBlock(getCefEvent(), "SOCIAL_SECURITY_CARD");
        }

        private static string formatSocialSecurityNumber(string number) {
            return $"{number.Substring(0, 3)}-{number.Substring(2, 2)}-{number.Substring(4)}";
        }
    }
}
