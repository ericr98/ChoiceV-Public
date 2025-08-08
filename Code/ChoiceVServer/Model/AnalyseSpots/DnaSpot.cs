using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.InventorySystem;
using System;
using System.Linq;

namespace ChoiceVServer.Model.AnalyseSpots {
    public class DnaSpot : AnalyseSpot {

        public DnaSpot(CollisionShape shape, int inventoryId) : base(shape, inventoryId) {

        }

        public override void onInteraction(IPlayer player) {
            base.onInteraction(player);

            var menu = generateEvidenceSpotMenu(player, Constants.EvidenceType.Blood, "Blutprobe");

            player.showMenu(menu);
        }

        public override void onAlignWithEvidence(IPlayer player, Evidence evidence) {
            base.onAlignWithEvidence(player, evidence);

            var charId = evidence.EvidenceDataList.FirstOrDefault(e => e.Name == "charID");
            if(charId != null) {
                var bloodSyringe = player.getInventory().getItem<BloodSyringe>(i => i.ExpirationDate < DateTime.Now && i.CharacterId == int.Parse(charId.Info));
                if(bloodSyringe != null) {
                    bloodSyringe.setAsEvidence(PoliceController.getNextItemEvidenceId());

                    var list = evidence.EvidenceDataList;
                    if(!list.Any(d => d.Name == "Blutprobenabgleich")) {
                        list.Add(new Evidence.EvidenceData("Blutprobenabgleich", $"Wurde erfolgreich abgeglichen mit Blutprobe mit Beweisindex: {bloodSyringe.getEvidenceId()}", true));
                        evidence.EvidenceDataList = list;
                    }

                    player.sendNotification(Constants.NotifactionTypes.Success, "Der Beweis passt eindeutig zu einer Blutprobe in deinem Inventar!", "Beweis zugeordnet", Constants.NotifactionImages.MagnifyingGlass);
                } else {
                    player.sendBlockNotification("Der Beweis wurde von einer anderen Person hinterlassen", "Keine Übereinstimmung", Constants.NotifactionImages.MagnifyingGlass);
                }
            } else {
                player.sendBlockNotification("Der Beweis ist fehlerhaft und kann nicht analysiert werden.", "Beweis fehlerhaft", Constants.NotifactionImages.MagnifyingGlass);
            }
        }
    }
}
