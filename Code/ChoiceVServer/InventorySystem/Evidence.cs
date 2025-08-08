using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.InventorySystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static ChoiceVServer.Base.BaseExtensions;
using static ChoiceVServer.Base.Constants;

namespace ChoiceVServer.InventorySystem {
    public class Evidence : Item {
        public class EvidenceData {
            public string Name;
            public string Info;

            public bool AfterAnalyze;
            public bool Hidden;

            public EvidenceData(string name, string info, bool afterAnalyze, bool hidden = false) {
                Name = name;
                Info = info;
                AfterAnalyze = afterAnalyze;
                Hidden = hidden;
            }
        }

        public EvidenceType EvidenceType { get => (EvidenceType)Data["Type"]; set { Data["Type"] = value; } }
        public bool Analyzed { get => (bool)Data["Analyzed"]; set { Data["Analyzed"] = value; } }
        public List<EvidenceData> EvidenceDataList { get => JsonConvert.DeserializeObject<List<EvidenceData>>(Data["EvidenceData"]); set => Data["EvidenceData"] = value.ToJson(); }
        public int EvidenceId { get => (int)Data["Id"]; set { Data["Id"] = value; } }


        public Evidence(item item) : base(item) { }

        public Evidence(configitem configItem, EvidenceType type, List<EvidenceData> evidenceData) : base(configItem) {
            EvidenceType = type;
            Analyzed = false;

            EvidenceDataList = evidenceData;
            EvidenceId = getNextEvidenceId();
            Description = Constants.EvidenceTypeToDescription[type] + " Beweis-Index: " + EvidenceId;
        }


        public override void use(IPlayer player) {
            base.use(player);
            var menu = createEvidenceMenu();

            player.showMenu(menu);
        }

        public Menu createEvidenceMenu(bool inBox = false, EvidenceBox box = null) {
            var s = "";
            if(Analyzed) {
                s = "Dieser Beweis ist komplett analysiert!";
            } else {
                s = "Dieser Beweis wurde noch nicht analysiert!";
            }

            var menu = new Menu(Constants.EvidenceTypeToString[EvidenceType] + " - Beweis " + EvidenceId, s);

            if(inBox) {
                menu.addMenuItem(new ClickMenuItem("Aus Box nehmen", "Nimm das Beweistück aus der Box", "", "RETRIEVE_EVIDENCE", MenuItemStyle.green).withData(new Dictionary<string, dynamic> { { "Evidence", this }, { "EvidenceBox", box } }));
            }

            foreach(var data in EvidenceDataList) {
                if((Analyzed || !data.AfterAnalyze) && !data.Hidden) {
                    var item = new StaticMenuItem(data.Name, data.Info, "");
                    menu.addMenuItem(item);
                }
            }

            if(menu.getMenuItemCount() == 0) {
                menu.addMenuItem(new StaticMenuItem("Keine Details verfügbar", "Für diesen Beweistypen gibt es im aktuellen Status keine weiteren Informationen", "", MenuItemStyle.yellow));
            }

            return menu;
        }

        public static int getNextEvidenceId() {
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(i => i.codeItem == typeof(Evidence).Name);
                var cItem = InventoryController.getConfigItemForType<Evidence>();

                var split = cItem.additionalInfo.Split('#');
                var value = int.Parse(split[0]);
                value += 1;

                cItem.additionalInfo = value + "#" + split[1];
                dItem.additionalInfo = value + "#" + split[1];

                db.SaveChanges();

                return value;
            }
        }
    }
}
