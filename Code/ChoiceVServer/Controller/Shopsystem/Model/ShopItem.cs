using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Shopsystem.Model {
    public class ShopItem {
        public configitem ConfigItem;
        public int Quality;
        public decimal Price;
        public ShopTypes ShopType;
        public string Category;

        public ShopItem(configitem configItem, int quality, decimal price, ShopTypes shopType, string category) {
            ConfigItem = configItem;
            Quality = quality;
            Price = price;
            ShopType = shopType;
            Category = category;
        }

        public string getName() {
            var qualityStr = "";
            if(Quality != -1) {
                qualityStr += " (";
                for(var i = 0; i < Quality; i++) {
                    qualityStr += "*";
                }
                qualityStr += ")";
            }

            return ConfigItem.name + qualityStr;
        }
    }
}
