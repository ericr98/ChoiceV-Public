using AltV.Net.Data;
using ChoiceVServer.Base;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Garages.Model {
    public class GarageSpawnSpot {
        public int Id;
        public int GarageId;
        public float SpawnRotation;
        public Position SpawnPosition;
        public CollisionShape CollisionShape;

        public GarageSpawnSpot(int id, int garageId, Position pos, float rot, configgaragespawnspot dbSpot) {
            Id = id;
            GarageId = garageId;

            SpawnRotation = rot;
            SpawnPosition = pos;

            CollisionShape = CollisionShape.Create(dbSpot);

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"SpawnSpot created: id {id}, garageId {garageId}");
        }

        public bool isOccupied() {
            return CollisionShape.IsOccupied();
        }

        public ChoiceVVehicle getOccupyingVehicle() {
            return (ChoiceVVehicle)CollisionShape.getAllEntities().FirstOrDefault();
        }

        public Menu getSupportMenu() {
            var menu = new Menu($"{Id} Garage Spot", "Was möchtest du tun?");

            menu.addMenuItem(new ClickMenuItem("Teleportieren", "Teleportiere dich zum Spot", "", "SUPPORT_GARAGE_SPOT_TELEPORT", MenuItemStyle.green)
                                .withData(new Dictionary<string, dynamic> {{ "Spot", this }}));

            menu.addMenuItem(new MenuStatsMenuItem("Spot löschen", "Lösche die Garagespot", "", "SUPPORT_DELETE_GARAGE_SPOT", MenuItemStyle.red)
                .withData(new Dictionary<string, dynamic> {{ "Spot", this }}));

            return menu;
        }
    }
}
