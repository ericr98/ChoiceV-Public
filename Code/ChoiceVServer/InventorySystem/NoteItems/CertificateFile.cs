using AltV.Net.Elements.Entities;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;

namespace ChoiceVServer.InventorySystem {
    public class CertificateFile : FileItem {
        public CertificateFile(item item) : base(item) {
            EventName = "OPEN_CERTIFICATE";
        }

        public CertificateFile(configitem cfgItem, string title, string name, string text, string signDate, string signName) : base(cfgItem) {
            EventName = "OPEN_CERTIFICATE";

            setData(new List<FileItemElement> {
                new FileItemElement("title", title),
                new FileItemElement("name", name),
                new FileItemElement("text", text),
                new FileItemElement("signDate", signDate),
                new FileItemElement("signName", signName),
            });

            updateDescription();
        }

        public CertificateFile(FileItem item) : base(item) {
            EventName = "OPEN_CERTIFICATE";
        }

        public override Item getCopy() {
            return new CertificateFile(this);
        }

        public override List<FileItemElement> getInitialElements() {
            return new List<FileItemElement> { };
        }

        public override int getNextFileId() {
            return 0;
        }

        public override string getSignature(IPlayer player) {
            return "";
        }

        public override void updateDescription() {
            Description = $"Zertifiziert: {Data["title"]} für {Data["name"]}";

            base.updateDescription();
        }
    }
}
