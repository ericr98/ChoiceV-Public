using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public class FoodCombinedItem : FoodItem {
        public FoodCombinedItem(item item) : base(item) { }

        public FoodCombinedItem(configitem configItem, int amount, int quality) : base(configItem, quality) {
            
        }
        
        private void setDescription(List<Item> originCombinedItems) {
            if(originCombinedItems == null || originCombinedItems.Count == 0) {
                Description = "Ohne weitere Zutaten";
            } else {
                Description = "Mit: ";
                foreach(var item in originCombinedItems.OrderBy(i => i.Name)) {
                    Description += item.ConfigItem.name + ", ";
                }

                Description = Description.Substring(0, Description.Length - 2);
            }
            
            updateDescription();
        }

        public override void setCreateData(Dictionary<string, dynamic> data) {
            setDescription(data["CombineItems"] as List<Item>);
        }

        public override object Clone() {
            return MemberwiseClone();
        }
    }
}
