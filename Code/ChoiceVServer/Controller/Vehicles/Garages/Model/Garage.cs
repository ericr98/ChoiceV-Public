using AltV.Net.Data;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.Controller.Garages.Model {
    public class Garage {
        public int Id;
        public string Name;
        public Constants.GarageType Type;
        public Constants.GarageOwnerType OwnerType;
        public int OwnerId;
        public int Slots;
        public int SlotsFree;
        public List<GarageSpawnSpot> SpawnPositions = new List<GarageSpawnSpot>();
        public ChoiceVPed Manager;

        public Garage(int id, string name, Constants.GarageType type, Constants.GarageOwnerType ownerType, int ownerId, int slots, int slotsFree, Position pos, float heading) {
            Id = id;
            Name = name;
            Type = type;
            OwnerType = ownerType;
            OwnerId = ownerId;
            Slots = slots;
            SlotsFree = slotsFree;

            if(pos != Position.Zero) {
                Manager = PedController.createPed("Garagenmanager", "s_m_m_dockwork_01", pos, heading);
                Manager.addModule(new NPCGarageModule(Manager, this));
            }

            Logger.logInfo(LogCategory.System, LogActionType.Created, $"Garage created: id {id}, name {name}");
        }

        public bool hasAFreeSpace() {
            foreach(var spot in SpawnPositions) {
                if(!spot.isOccupied()) {
                    return true;
                }
            }

            return false;
        }

        public void update() { 
            var occupiedGarageSpots = SpawnPositions.Where(s => s.isOccupied()).OrderBy(s => s.getOccupyingVehicle().LastMoved).ToList();
            
            //If less than 20% off slots are free, free them 
            while(((float)SpawnPositions.Count - (float)occupiedGarageSpots.Count) / (float)SpawnPositions.Count < 0.5f) {
                var oldest = occupiedGarageSpots.First();

                if(oldest.getOccupyingVehicle().Driver == null && oldest.getOccupyingVehicle().LastMoved < DateTime.Now - TimeSpan.FromMinutes(30)) {
                    GarageController.parkVehicleInGarage(oldest.getOccupyingVehicle(), Id);

                    occupiedGarageSpots.Remove(oldest);
                }
            }
        }

        public List<vehicle> getAllParkedVehicles() {
            using(var db = new ChoiceVDb()) {
                return db.vehicles.Where(v => v.garageId == Id).ToList();
            }
        }

        public Menu getSupportMenu() {
            var menu = new Menu(Name, "Was möchtest du tun?");

            menu.addMenuItem(new StaticMenuItem("Typ", "", Type.ToString()));

            menu.addMenuItem(new ClickMenuItem("Teleportieren", "Teleportiere dich zur Garage", "", "SUPPORT_GARAGE_TELEPORT", MenuItemStyle.green)
                                .withData(new Dictionary<string, dynamic> {{ "GarageId", this }}));


            var spotsMenu = new Menu("Spots", "Was möchtest du tun?");

            var spotCreateMenu = new Menu("Spot erstellen", "Erstelle einen neuen Spot");
            spotCreateMenu.addMenuItem(new ClickMenuItem("Erstellen", "Erstelle einen neuen Spot", "", "SUPPORT_CREATE_GARAGE_SPOT")
                .withData(new Dictionary<string, dynamic> {{ "Garage", this }}));
            spotsMenu.addMenuItem(new MenuMenuItem("Spot erstellen", spotCreateMenu, MenuItemStyle.green));

            foreach(var spot in SpawnPositions) {
                var spotMenu = spot.getSupportMenu();
                spotsMenu.addMenuItem(new MenuMenuItem(spotMenu.Name, spotMenu));
            }
            menu.addMenuItem(new MenuMenuItem("Spots", spotsMenu));


            menu.addMenuItem(new MenuStatsMenuItem("Garage löschen", "Lösche die Garage", "", "SUPPORT_DELETE_GARAGE", MenuItemStyle.red)
                .withData(new Dictionary<string, dynamic> {{ "GarageId", this }}));

            return menu;
        }

        internal IEnumerable<ChoiceVVehicle> getAllVehiclesInSpots() {
            return SpawnPositions.Select(p => p.getOccupyingVehicle());
        }
    }
}
