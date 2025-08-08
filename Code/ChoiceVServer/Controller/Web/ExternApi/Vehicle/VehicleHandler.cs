#nullable enable
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChoiceVExternApi;
using ChoiceVExternApi.Shared;
using ChoiceVExternApi.Shared.Attributes;
using ChoiceVServer.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace ChoiceVServer.Controller.Web.ExternApi.Vehicle;

public class VehicleHandler : IBasicExternApiHandler {
    private const string Prefix = "vehicle";

    public void registerRoutes(RouteHandler routeHandler) {
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetAllRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByVehicleIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCharacterIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix, handleGetByCompanyIdRoute);
        routeHandler.registerRoute(HttpMethod.Get, Prefix + "/configs", handleGetConfigsRoute);
    }

    /// <summary>
    /// Handles the route to get all vehicle configurations.
    /// No query parameters are required.
    /// Route: GET /vehicle/configs
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetConfigsRoute(HttpListenerContext context, string[] segments) {
        var configVehicleModels = VehicleController.getAllVehicleModels();
        await context.sendResponseAsync(configVehicleModels.convertToApiModel()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get a vehicle by vehicle ID.
    /// Requires query parameter "vehicleId".
    /// Route: GET /vehicle?vehicleId={vehicleId}
    /// </summary>
    [RequiredQueryParams("vehicleId")]
    private static async Task handleGetByVehicleIdRoute(HttpListenerContext context, string[] segments) {
        int? vehicleId = context.getQueryParameter<int>("vehicleId");

        if (!vehicleId.HasValue) {
            await context.sendResponseAsync("Invalid vehicleId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        await using var db = new ChoiceVDb();
        var vehicle = await VehicleHelper.getAllDbVehicles(db)
            .FirstOrDefaultAsync(x => x.id == vehicleId.Value).ConfigureAwait(false);

        await context.sendResponseAsync(vehicle.convertToApiModel()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get vehicles by character ID.
    /// Requires query parameter "characterId".
    /// Route: GET /vehicle?characterId={characterId}
    /// </summary>
    [RequiredQueryParams("characterId")]
    private static async Task handleGetByCharacterIdRoute(HttpListenerContext context, string[] segments) {
        int? characterId = context.getQueryParameter<int>("characterId");

        if (!characterId.HasValue) {
            await context.sendResponseAsync("Invalid characterId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        await using var db = new ChoiceVDb();
        var charVehicles = await VehicleHelper.getAllDbVehicles(db)
            .Where(x => x.vehiclesregistrations.Any(y => y.end == null && y.owner.id == characterId.Value))
            .ToListAsync()
            .ConfigureAwait(false);

        await context.sendResponseAsync(charVehicles.convertToApiModel()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get vehicles by company ID.
    /// Requires query parameter "companyId".
    /// Route: GET /vehicle?companyId={companyId}
    /// </summary>
    [RequiredQueryParams("companyId")]
    private static async Task handleGetByCompanyIdRoute(HttpListenerContext context, string[] segments) {
        int? companyId = context.getQueryParameter<int>("companyId");

        if (!companyId.HasValue) {
            await context.sendResponseAsync("Invalid companyId.", HttpStatusCode.BadRequest).ConfigureAwait(false);
            return;
        }

        await using var db = new ChoiceVDb();
        var charVehicles = await VehicleHelper.getAllDbVehicles(db)
            .Where(x => x.vehiclesregistrations.Any(y => y.companyOwner.id == companyId.Value))
            .ToListAsync().ConfigureAwait(false);

        await context.sendResponseAsync(charVehicles.convertToApiModel()).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the route to get all vehicles.
    /// No query parameters are required.
    /// Route: GET /vehicle
    /// </summary>
    [NoQueryParams]
    private static async Task handleGetAllRoute(HttpListenerContext context, string[] segments) {
        await using var db = new ChoiceVDb();
        var vehicles = await VehicleHelper.getAllDbVehicles(db).ToListAsync();
        await context.sendResponseAsync(vehicles.convertToApiModel()).ConfigureAwait(false);
    }
}
