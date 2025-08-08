using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using System.Linq;

namespace ChoiceVServer.Model.AnalyseSpots {
    public class WeaponSpot : AnalyseSpot {

        public WeaponSpot(CollisionShape shape, int inventoryId) : base(shape, inventoryId) { }

        public override void onInteraction(IPlayer player) {
            base.onInteraction(player);

            var menu = generateEvidenceSpotMenu(player, Constants.EvidenceType.CartridgeCase, "Waffe");

            player.showMenu(menu);
        }

        public override void onAlignWithEvidence(IPlayer player, Evidence evidence) {
            base.onAlignWithEvidence(player, evidence);

            var weaponId = int.Parse(evidence.EvidenceDataList.FirstOrDefault(e => e.Hidden = true).Info);
            var weapon = player.getInventory().getItem(i => i.Id == weaponId);
            if(weapon == null) {
                player.sendBlockNotification("Konnte im Inventar keine passende Waffe zu dem Beweis finden!", "Beweis nicht zugeordnet!", Constants.NotifactionImages.MagnifyingGlass);
            } else {
                player.sendNotification(Constants.NotifactionTypes.Success, "Der Beweis passt eindeutig zu einer Waffe im Inventar!", "Beweis zugeordnet!", Constants.NotifactionImages.MagnifyingGlass);
                weapon.setAsEvidence(PoliceController.getNextItemEvidenceId());

                var list = evidence.EvidenceDataList;
                if(!list.Any(d => d.Name == "Waffenabgleich")) {
                    list.Add(new Evidence.EvidenceData("Waffenabgleich", $"Wurde erfolgreich abgeglichen mit Waffe mit Beweisindex: {weapon.getEvidenceId()}", true));
                    evidence.EvidenceDataList = list;
                }

                player.sendNotification(Constants.NotifactionTypes.Success, "Der Beweis passt eindeutig zu einer Waffe in deinem Inventar!", "Beweis zugeordnet", Constants.NotifactionImages.MagnifyingGlass);
            }
        }
    }
}
