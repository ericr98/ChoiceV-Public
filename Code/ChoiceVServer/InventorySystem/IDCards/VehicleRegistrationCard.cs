using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChoiceVServer.InventorySystem {
    public record VehicleRegistrationCardInfo(int VehicleId, int CharacterId, string ChassisNumber, string Owner, string NumberplateText, string VehicleName, bool hasNumberPlate);

    public enum VehicleOwnerType {
        Player,
        Company,
    }
    
    public class VehicleRegistrationCard : IdCardItem {
        public int VehicleId { get => (int)Data["VehicleId"]; set { Data["VehicleId"] = value; } }
        public int OwnerId { get => (int)Data["OwnerId"]; set { Data["OwnerId"] = value; } }
        public VehicleOwnerType OwnerType { get => (VehicleOwnerType)Data["OwnerType"]; set { Data["OwnerType"] = value; } }

        public VehicleRegistrationCard(item item) : base(item) {
            EventName = "OPEN_VEHICLE_REGISTRATION_CARD";
        }

        public VehicleRegistrationCard(configitem cfg, string owner, int ownerId, ChoiceVVehicle vehicle, string signature) : base(cfg) {
            EventName = "OPEN_VEHICLE_REGISTRATION_CARD";
            VehicleId = vehicle.VehicleId;
            OwnerId = ownerId;

            var list = new List<IdCardItemElement> {
                new("startDate", DateTime.Now.ToString("d")),
                new("expDate", (DateTime.Now + TimeSpan.FromDays(712)).ToString("d")),
                new("licNumber", getNextLicNumber().ToString()),
                new("owner", owner),
                new("chassisNumber", vehicle.ChassisNumber.ToString()),
                new("hasNumberplate", (vehicle.DbModel.hasNumberplate == 1).ToString()),
                new("numberPlate", vehicle.NumberplateText),
                new("vehicleName", vehicle.DbModel.DisplayName),
                new("vehicleProducer", vehicle.DbModel.producerName),
                new("dmvSignature", signature),
                new("ownerSignature", owner)
            };

            setData(list);

            Description = $"Fahrzeuginhaberkarte für ein {vehicle.DbModel.DisplayName} mit Kennzeichen {vehicle.NumberplateText}";
            updateDescription();
        }

        public int getNextLicNumber() {
            var rnd = new Random();
            var step = rnd.Next(0, 50);
            using(var db = new ChoiceVDb()) {
                var dItem = db.configitems.FirstOrDefault(c => c.codeItem == typeof(VehicleRegistrationCard).Name);
                var cItem = InventoryController.getConfigItemForType<VehicleRegistrationCard>();

                var value = int.Parse(dItem.additionalInfo);

                cItem.additionalInfo = (value + step).ToString();
                dItem.additionalInfo = (value + step).ToString();

                db.SaveChanges();

                return value;
            }
        }

        public void changeInfos(VehicleOwnerType ownerType, int ownerId, string owner, string numberPlate, string signature) {
            OwnerId = ownerId;
            OwnerType = ownerType;

            var data = getData();
            var chassisNumber = data.FirstOrDefault(c => c.Name == "chassisNumber")?.Data;
            var vehicleName = data.FirstOrDefault(c => c.Name == "vehicleName")?.Data;
            var vehicleProducer = data.FirstOrDefault(c => c.Name == "vehicleProducer")?.Data;
            var hasNumberplate = data.FirstOrDefault(c => c.Name == "hasNumberplate")?.Data;

            var list = new List<IdCardItemElement> {
                new("startDate", DateTime.Now.ToString("d")),
                new("expDate", (DateTime.Now + TimeSpan.FromDays(60)).ToString("d")),
                new("licNumber", getNextLicNumber().ToString()),
                new("owner", owner),
                new("chassisNumber", chassisNumber),
                new("hasNumberplate", hasNumberplate),
                new("numberPlate", numberPlate),
                new("vehicleName", vehicleName),
                new("vehicleProducer", vehicleProducer),
                new("dmvSignature", signature),
                new("ownerSignature", owner),
            };

            setData(list);

            Description = $"Fahrzeuginhaberkarte für ein {chassisNumber} mit Kennzeichen {numberPlate}";
            updateDescription();
        }

        public VehicleRegistrationCardInfo getRegistrationInfo() {
            var data = getData();
            var chassis = data.FirstOrDefault(d => d.Name == "chassisNumber")?.Data;
            var hasNumberPlate = data.FirstOrDefault(d => d.Name == "hasNumberplate")?.Data;
            var numberPlate = data.FirstOrDefault(d => d.Name == "numberPlate")?.Data;
            var owner = data.FirstOrDefault(d => d.Name == "owner")?.Data;
            var vehName = data.FirstOrDefault(d => d.Name == "vehicleName")?.Data;

            return new VehicleRegistrationCardInfo(VehicleId, OwnerId, chassis, owner, numberPlate, vehName, bool.Parse(hasNumberPlate));
        }
    }
}
