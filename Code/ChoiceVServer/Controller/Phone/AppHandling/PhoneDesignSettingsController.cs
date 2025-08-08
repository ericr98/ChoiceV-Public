using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.InventorySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.Phone {
    internal class PhoneDesignSettingsController : ChoiceVScript {
        public PhoneDesignSettingsController() {
            EventController.addCefEvent("SMARTPHONE_DESIGN_CHANGE", onSmartphoneDesignChange);
            EventController.addCefEvent("PHONE_CHANGE_SETTING", onSmartphoneSettingsChange);
        }

        #region DesignChange

        private class PhoneDesignChangeCefEvent {
            public int itemId;
            public int backgroundId;
            public int ringtoneId;
        }

        private void onSmartphoneDesignChange(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = new PhoneDesignChangeCefEvent();
            cefData.PopulateJson(evt.Data);

            var item = player.getInventory().getItem<Smartphone>(i => i.Id == cefData.itemId);
            if(item != null) {
                item.BackgroundId = cefData.backgroundId;
                item.RingtoneId = cefData.ringtoneId;
            }
        }

        #endregion

        #region SettingsChange

        private class AdditionalSettingsChangeEvent {
            public int itemId;
        }

        private void onSmartphoneSettingsChange(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            var cefData = evt.Data.FromJson<SIMCardSettings>();

            var addCefData = new AdditionalSettingsChangeEvent();
            addCefData.PopulateJson(evt.Data);

            var phone = player.getInventory().getItem<Smartphone>(i => i.Id == addCefData.itemId);
            if(phone != null) {
                var sim = phone.getSIMCard();
                if(sim != null) {
                    sim.Settings = cefData;
                } else {
                    Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onSmartphoneSettingsChange: no sim in found phone: data: {evt.Data}");
                }
            } else {
                Logger.logWarning(LogCategory.Player, LogActionType.Blocked, player, $"onSmartphoneSettingsChange: phone not found in player inventory: data: {evt.Data}");
            }
        }

        #endregion
    }
}
