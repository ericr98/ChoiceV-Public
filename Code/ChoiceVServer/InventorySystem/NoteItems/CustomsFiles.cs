using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class USCustomsFile : FileItem {
        public USCustomsFile(item item) : base(item) {
            EventName = "OPEN_US_CUSTOMS_FILE";
        }

        public USCustomsFile(FileItem item) : base(item) {
            EventName = "OPEN_US_CUSTOMS_FILE";
        }

        public USCustomsFile(configitem cfg) : base(cfg) {
            EventName = "OPEN_US_CUSTOMS_FILE";
        }

        public override int getNextFileId() {
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(c => c.codeItem == typeof(USCustomsFile).Name);
                var cItem = InventoryController.getConfigItemForType<USCustomsFile>();

                var value = int.Parse(dItem.additionalInfo);

                cItem.additionalInfo = (value + 1).ToString();
                dItem.additionalInfo = (value + 1).ToString();

                db.SaveChanges();

                return value;
            }
        }

        public override List<FileItemElement> getInitialElements() {
            return new List<FileItemElement> {
                new FileItemElement("admission", ""),
                new FileItemElement("flightNumber", ""),
                new FileItemElement("enterDate", ""),
                new FileItemElement("fullName", ""),
                new FileItemElement("birthday", ""),
                new FileItemElement("citizenship", ""),
                new FileItemElement("number", ""),
                new FileItemElement("goverment", ""),
                new FileItemElement("drugs", null),
                new FileItemElement("crime", null),
                new FileItemElement("terror", null),
                new FileItemElement("work", null),
                new FileItemElement("child", null),
                new FileItemElement("visa", null),
                new FileItemElement("signature", ""),
            };
        }

        public override string getSignature(IPlayer player) {
            return $"{player.getCharacterShortName()}";
        }

        public override Item getCopy() {
            return new USCustomsFile(this);
        }
    }

    public class PericoCustomsFile : FileItem {
        public PericoCustomsFile(item item) : base(item) { EventName = "OPEN_PERICO_CUSTOMS_FILE"; }

        public PericoCustomsFile(FileItem item) : base(item) { EventName = "OPEN_PERICO_CUSTOMS_FILE"; }

        public PericoCustomsFile(configitem cfg) : base(cfg) { EventName = "OPEN_PERICO_CUSTOMS_FILE"; }

        public override int getNextFileId() {
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(c => c.codeItem == typeof(PericoCustomsFile).Name);
                var cItem = InventoryController.getConfigItemForType<PericoCustomsFile>();

                var value = int.Parse(dItem.additionalInfo);

                cItem.additionalInfo = (value + 1).ToString();
                dItem.additionalInfo = (value + 1).ToString();

                db.SaveChanges();

                return value;
            }
        }

        public override List<FileItemElement> getInitialElements() {
            return new List<FileItemElement> {
                new FileItemElement("visaNumber", ""),
                new FileItemElement("flightNumber", ""),
                new FileItemElement("enterDate", ""),
                new FileItemElement("fullName", ""),
                new FileItemElement("birthday", ""),
                new FileItemElement("citizenship", ""),
                new FileItemElement("number", ""),
                new FileItemElement("visaAmount", ""),

                new FileItemElement("time", ""),
                new FileItemElement("visit", ""),
                new FileItemElement("work", null),
                new FileItemElement("workMaybe", ""),
                new FileItemElement("food", null),
                new FileItemElement("foodMaybe", ""),
                new FileItemElement("money", null),
                new FileItemElement("moneyMaybe", ""),

                new FileItemElement("signature", ""),
            };
        }

        public override string getSignature(IPlayer player) {
            return $"{player.getCharacterName()}";
        }

        public override Item getCopy() {
            return new PericoCustomsFile(this);
        }
    }
}
