using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.InventorySystem;
using System.Linq;

namespace ChoiceVServer.Model.AnalyseSpots {
    public class CarPaintSpot : AnalyseSpot {

        public CarPaintSpot(CollisionShape shape, int inventoryId) : base(shape, inventoryId) {

        }

        public override void onInteraction(IPlayer player) {
            base.onInteraction(player);

            var menu = generateEvidenceSpotMenu(player, Constants.EvidenceType.CarPaint, "Auto");

            player.showMenu(menu);
        }

        public override void onAlignWithEvidence(IPlayer player, Evidence evidence) {
            base.onAlignWithEvidence(player, evidence);

            var vehicle = (ChoiceVVehicle)CollisionShape.AllEntities.FirstOrDefault(v => v.Type == BaseObjectType.Vehicle);

            if(vehicle != null) {
                var vehicleId = int.Parse(evidence.EvidenceDataList.FirstOrDefault(e => e.Hidden = true).Info);
                if(vehicle.VehicleId == vehicleId) {
                    var list = evidence.EvidenceDataList;
                    if(!list.Any(d => d.Name == "Fahrzeugabgleich")) {
                        list.Add(new Evidence.EvidenceData("Fahrzeugabgleich", $"Wurde erfolgreich abgeglichen mit Fahrzeug mit Chassisnummer: {vehicle.ChassisNumber}", true));
                        evidence.EvidenceDataList = list;
                    }
                    player.sendNotification(Constants.NotifactionTypes.Success, "Der Beweis passt eindeutig zu dem Fahrzeug!", "Beweis zugeordnet", Constants.NotifactionImages.MagnifyingGlass);
                } else {
                    player.sendBlockNotification("Der Beweis kam von einem anderen Fahrzeug!", "Beweis nicht zugeordnet", Constants.NotifactionImages.MagnifyingGlass);
                }
            } else {
                player.sendBlockNotification("Es gibt kein Auto zum Gegenprüfen!", "Auto nicht gefunden", Constants.NotifactionImages.MagnifyingGlass);
            }
        }
    }
}
