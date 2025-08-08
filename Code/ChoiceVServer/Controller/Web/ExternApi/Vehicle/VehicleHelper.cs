using System;
using System.Collections.Generic;
using System.Linq;
using ChoiceVServer.Model.Database;
using ChoiceVSharedApiModels.Vehicles;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Vehicle;

public static class VehicleHelper {

    public static List<ConfigVehicleApiModel> convertToApiModel(this IEnumerable<configvehiclesmodel> models) {
        if(models == null) {
            throw new ArgumentNullException(nameof(models), "Model list cannot be null");
        }

        return models.Select(model => model.convertToApiModel()).ToList();
    }

    public static ConfigVehicleApiModel convertToApiModel(this configvehiclesmodel model) {
        if(model == null) {
            throw new ArgumentNullException(nameof(model), "Model cannot be null");
        }

        return new ConfigVehicleApiModel {
            Id = model.id,
            CreateDate = model.createDate,
            Model = model.Model,
            ModelName = model.ModelName,
            DisplayName = model.DisplayName,
            ClassId = model.classId,
            SpecialFlag = model.specialFlag,
            Seats = model.Seats,
            InventorySize = model.InventorySize,
            FuelMax = model.FuelMax,
            FuelType = model.FuelType,
            StartPoint = model.StartPoint,
            EndPoint = model.EndPoint,
            PowerMultiplier = model.PowerMultiplier,
            Extras = model.Extras,
            WindowCount = model.windowCount,
            TyreCount = model.tyreCount,
            DoorCount = model.doorCount,
            Price = model.price,
            TrunkInBack = model.trunkInBack,
            Useable = model.useable,
            ProducerName = model.producerName,
            NeedsRecheck = model.needsRecheck,
            HasNumberplate = model.hasNumberplate
        };
    }


    public static List<VehicleApiModel> convertToApiModel(this IEnumerable<vehicle> models) {
        if(models == null) {
            throw new ArgumentNullException(nameof(models), "Model list cannot be null");
        }

        return models.Select(model => model.convertToApiModel()).ToList();
    }

    public static VehicleApiModel convertToApiModel(this vehicle model) {
        if(model == null) {
            throw new ArgumentNullException(nameof(model), "Vehicle model cannot be null");
        }

        return new VehicleApiModel {
            Id = model.id,
            ModelId = model.modelId,
            ChassisNumber = model.chassisNumber,
            Position = model.position,
            Rotation = model.rotation,
            GarageId = model.garageId,
            RegisteredCompanyId = model.registeredCompanyId,
            Dimension = model.dimension,
            LastMoved = model.lastMoved,
            NumberPlate = model.numberPlate,
            CreateDate = model.createDate,
            Fuel = model.fuel,
            DrivenDistance = model.drivenDistance,
            KeyLockVersion = model.keyLockVersion,
            DirtLevel = model.dirtLevel,
            RandomlySpawnedDate = model.randomlySpawnedDate,
            Config = model.model?.convertToApiModel(),
            Coloring = model.vehiclescoloring?.convertToApiModel(),
            Registrations = model.vehiclesregistrations?.Select(r => r.convertToApiModel()).ToList() ?? []
        };
    }

    public static VehicleColoringApiModel convertToApiModel(this vehiclescoloring model) {
        if(model == null) {
            throw new ArgumentNullException(nameof(model), "VehiclesColoring model cannot be null");
        }

        return new VehicleColoringApiModel {
            VehicleId = model.vehicleId,
            PrimaryColor = model.primaryColor,
            SecondaryColor = model.secondaryColor,
            PrimaryColorRGB = model.primaryColorRGB,
            SecondaryColorRGB = model.secondaryColorRGB,
            PearlColor = model.pearlColor
        };
    }

    public static VehicleRegistrationApiModel convertToApiModel(this vehiclesregistration model) {
        if(model == null) {
            throw new ArgumentNullException(nameof(model), "VehiclesRegistration model cannot be null");
        }

        return new VehicleRegistrationApiModel {
            Id = model.id,
            VehicleId = model.vehicleId,
            OwnerId = model.ownerId,
            CompanyOwnerId = model.companyOwnerId,
            NumberPlate = model.numberPlate,
            Start = model.start,
            End = model.end
        };
    }

    public static IQueryable<vehicle> getAllDbVehicles(ChoiceVDb db) {
        return db.vehicles
            .AsNoTracking()
            .Include(v => v.model)
            .Include(v => v.model)
            .Include(v => v.vehiclesregistrations)
            .Include(v => v.vehiclescoloring)
            .Include(v => v.vehiclesdata)
            .AsSplitQuery();
    }
}
