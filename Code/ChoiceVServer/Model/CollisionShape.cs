using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Character;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace ChoiceVServer.Model;

public class CollisionShapeController : ChoiceVScript {
    public CollisionShapeController() {
        InvokeController.AddTimedInvoke("COLLISION_SHAPE_CHECKER", onCollisionShapeChecker, TimeSpan.FromSeconds(15), true);
    }

    private static void onCollisionShapeChecker(IInvoke obj) {
        foreach(var player in ChoiceVAPI.GetAllPlayers()) {
            foreach(var shape in player.getCurrentCollisionShapes()) {
                shape.updateShapeForEntity(player);
            }
        } 
    }
}

public class CollisionShape : IDisposable {
    public static List<CollisionShape> AllShapes = [];
    public static int ShapeId = 0;

    private bool _TrackPlayers;

    private bool _TrackVehicles;

    public HashSet<IEntity> AllEntities = [];
    public Animation Animation;
    public string EventName;

    public bool HasNoHeight;
    public float Height;
    public bool IgnoreShape;


    public static float StandardZDiv = 1.5f;
    public float ZDiv = StandardZDiv;

    public bool Interactable;
    public bool InteractableOnBusy;

    public Dictionary<string, dynamic> InteractData = new();

    public CollisionShapeInteractionDelegate OnCollisionShapeInteraction;

    public CollisionShapeDelegate OnEntityEnterShape;
    public CollisionShapeDelegate OnEntityExitShape;

    public CollisionShapeDelegate OnPlayerMoveInShape;

    //In case something needs to be searched
    public object Owner;
    private Vector3 PosA = Vector3.Zero, PosB = Vector3.Zero, PosC = Vector3.Zero, PosD = Vector3.Zero;
    public float Rotation;
    public bool TrackPlayersInVehicles = true;
    public float Width;

    public float Size => Width * Height;

    public WorldGrid WorldGrid;

    public List<CharacterType> InteractableTypes = [CharacterType.Player];

    private CollisionShape(Vector3 pos, float width, float height, float rotation, bool interactable, string eventName, bool trackplayers = true, bool trackvehicles = true, Dictionary<string, dynamic> data = null) {
        WorldGrid = WorldController.getWorldGrid(pos);
        Position = pos;
        Width = width;
        Height = height;
        Rotation = (360 + rotation) % 360;

        Interactable = interactable;
        EventName = eventName;
        TrackPlayers = trackplayers;

        TrackVehicles = trackvehicles;

        InteractData = data;

        RestrictSpecificActions = false;
    }


    public int Id { get; private set; }
    public Vector3 Position { get; } = Vector3.Zero;

    public bool RestrictSpecificActions { get; private set; }

    public bool TrackVehicles {
        get => _TrackVehicles;
        set {
            if(value && !_TrackVehicles) {
                //EventController.VehicleMovedDelegate += OnVehicleMoved;
                EventController.addOnVehicleMoveCallback(WorldGrid, OnVehicleMoved);
            }
            if(!value && _TrackVehicles) {
                //EventController.VehicleMovedDelegate -= OnVehicleMoved;
                EventController.removeOnVehicleMoveCallback(WorldGrid, OnVehicleMoved);
            }
            _TrackVehicles = value;
        }
    }

    public bool TrackPlayers {
        get => _TrackPlayers;
        set {
            if(value && !_TrackPlayers) {
                //EventController.PlayerMovedDelegate += OnPlayerMoved;
                EventController.addOnPlayerMoveCallback(WorldGrid, OnPlayerMoved);
            }
            if(!value && _TrackPlayers) {
                //EventController.PlayerMovedDelegate -= OnPlayerMoved;
                EventController.removeOnPlayerMoveCallback(WorldGrid, OnPlayerMoved);
            }
            _TrackPlayers = value;
        }
    }

    public void Dispose() {
        lock(AllShapes) {
            IgnoreShape = true;
            TrackPlayers = false;
            TrackVehicles = false;
            Owner = null;

            foreach(var entity in AllEntities) {
                if(entity.Type == BaseObjectType.Player) {
                    var p = entity as IPlayer;
                    p.removeCollisionShape(this);
                } else if(entity.Type == BaseObjectType.Vehicle) {
                    var p = entity as ChoiceVVehicle;
                    p.removeCollisionShape(this);
                }
            }

            AllShapes.Remove(this);
            AllEntities.Clear();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static CollisionShape Create(Vector3 pos, float width, float height, float rotation, bool trackPlayers, bool trackVehicles, bool interactable = false, string eventname = "", Dictionary<string, dynamic> data = null) {
        lock(AllShapes) {
            var colShape = AllShapes.FirstOrDefault(c => c.Position == pos && c.Width == width && c.Height == height && c.Rotation == rotation);

            if(colShape == null) {
                colShape = new CollisionShape(pos, width, height, rotation, interactable, eventname, trackPlayers, trackVehicles, data);
                colShape.Id = ShapeId++;

                AllShapes.Add(colShape);
            }

            Logger.logDebug(LogCategory.System, LogActionType.Created, $"Collisionshape created: Position: {pos.ToJson()}, Width: {width}, Height: {height}, Eventname: {eventname}.");

            return colShape;
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static CollisionShape Create(string shortString) {
        var data = shortString.Split('#');

        var pos = data[0].FromJson<Position>();
        var width = data[1].FromJson<float>();
        var height = data[2].FromJson<float>();
        var rot = data[3].FromJson<float>();
        var trackPlayers = data[4].FromJson<bool>();
        var trackVehicles = data[5].FromJson<bool>();
        var interactable = data[6].FromJson<bool>();
        var evtName = "";
        if(data.Count() > 7) {
            evtName = data[7].FromJson<string>();
        }

        return Create(pos, width, height, rot, trackPlayers, trackVehicles, interactable, evtName);
    }

    /// <summary>
    ///     CollisionShapes with Restricted Actions dont allow e.g. throwing trash on ground, etc.
    /// </summary>
    public CollisionShape withRestrictedActions() {
        RestrictSpecificActions = true;
        return this;
    }

    public static CollisionShape Create(configgaragespawnspot dbGaragePosition) {
        return Create(dbGaragePosition.position.FromJson<Position>(), dbGaragePosition.width, dbGaragePosition.height, dbGaragePosition.rotation, false, true);
    }

    public void updateShapeForEntity(IEntity entity) {
        if(entity is IPlayer) {
            OnPlayerMoved(this, entity as IPlayer, entity.Position, 0);
        } else if(entity is ChoiceVVehicle) {
            OnVehicleMoved(entity, entity as ChoiceVVehicle, entity.Position, entity.Position, 0);
        }
    }

    private void OnPlayerMoved(object sender, IPlayer player, Position moveToPosition, float distance) {
        if(IsInShape(moveToPosition)) {
            if(AllEntities.Add(player)) {
                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"onEntityEnterColShape {Position} {moveToPosition} {getAllEntities().Count()} {Id}");

                if(TrackPlayersInVehicles || player.Vehicle == null) {
                    player.addCurrentCollisionShape(this);
                    OnEntityEnterShape?.Invoke(this, player);
                }
            }
            OnPlayerMoveInShape?.Invoke(this, player);
        } else {
            if(AllEntities.Remove(player)) {
                Logger.logDebug(LogCategory.Player, LogActionType.Updated, player, $"onEntityExitColShape {moveToPosition} {getAllEntities().Count()} {Id}");

                //Remove Colshape from EntityList
                player.removeCollisionShape(this);
                OnEntityExitShape?.Invoke(this, player);
            }
        }
    }

    private void OnVehicleMoved(object sender, ChoiceVVehicle v, Position previousPos, Position newPos, float distance) {
        if(v.Exists()) {
            if(IsInShape(newPos) || IsInShape(new Position((previousPos.X + newPos.X) / 2, (previousPos.Y + newPos.Y) / 2, (previousPos.Z + newPos.Z) / 2))) {
                if(AllEntities.Add(v)) {
                    Logger.logDebug(LogCategory.Vehicle, LogActionType.Updated, v, $"onEntityEnterColShape {Position} {getAllEntities().Count()} {Id}");
                    v.addCurrentCollisionShape(this);
                    OnEntityEnterShape?.Invoke(this, v);
                }
            } else {
                if(AllEntities.Remove(v)) {
                    Logger.logDebug(LogCategory.Vehicle, LogActionType.Updated, v, $"onEntityExitColShape {Position} {getAllEntities().Count()} {Id}");
                    v.removeCollisionShape(this);
                    OnEntityExitShape?.Invoke(this, v);
                }
            }
        }
    }

    private float Radians(float degrees) {
        return (float)(degrees * Math.PI / 180.0);
    }

    public Vector3 Rotate(Vector3 point, float xOffset, float yOffset) {
        return new Vector3(
            (float)(point.X + xOffset * Math.Cos(Radians(Rotation)) - yOffset * Math.Sin(Radians(Rotation))),
            (float)(point.Y + xOffset * Math.Sin(Radians(Rotation)) + yOffset * Math.Cos(Radians(Rotation))),
            point.Z);
    }

    public void Draw(IPlayer player, int span = 5000) {
        if(PosA.Equals(Vector3.Zero)) {
            PosB = Rotate(Position, -Width / 2, Height / 2);
            PosC = Rotate(Position, Width / 2, Height / 2);
            PosA = Rotate(Position, -Width / 2, -Height / 2);
            PosD = Rotate(Position, Width / 2, -Height / 2);
        }

        //player.emitClientEvent("DRAW_SHAPE", PosA.X, PosA.Y, PosA.Z, PosB.X, PosB.Y, PosB.Z, PosC.X, PosCs.Y, PosC.Z, PosD.X, PosD.Y, PosD.Z);
        player.emitClientEvent("SPOT_ADD", Position.X, Position.Y, Width, Height, 1, Rotation);

        // CalloutController.AddCallout("StopCollsionShape: " + Id, null, (ic, o) => player.emitClientEvent("SPOT_END"), null, TimeSpan.FromMilliseconds(span), false);

        // TODO
        // API.shared.delay(span, true, () => player.TriggerEvent("OP_STOP"));
    }

    public bool IsInShape(Vector3 point) {
        if(IgnoreShape) {
            return false;
        }

        if(point == Vector3.Zero || Position == Vector3.Zero) {
            return false;
        }

        if(Height == 0) {
            return Vector3.Distance(point, Position) <= Width;
        }

        if(PosA.Equals(Vector3.Zero)) {
            PosB = Rotate(Position, -Width / 2, Height / 2);
            PosC = Rotate(Position, Width / 2, Height / 2);
            PosA = Rotate(Position, -Width / 2, -Height / 2);
            PosD = Rotate(Position, Width / 2, -Height / 2);
        }

        var res = isPointInside2DRectangle(point.X, point.Y, PosA, PosB, PosC, PosD);

        return res && (Position.Z == 0 || Math.Abs(point.Z - Position.Z) < ZDiv || HasNoHeight);
    }

    private bool isPointInside2DRectangle(float pointX, float pointY, Vector3 PosA, Vector3 PosB, Vector3 PosC, Vector3 PosD) {
        Vector3 AB = PosB - PosA;
        Vector3 AM = new Vector3(pointX, pointY, PosA.Z) - PosA;

        Vector3 BC = PosC - PosB;
        Vector3 BM = new Vector3(pointX, pointY, PosB.Z) - PosB;

        Vector3 CD = PosD - PosC;
        Vector3 CM = new Vector3(pointX, pointY, PosC.Z) - PosC;

        Vector3 DA = PosA - PosD;
        Vector3 DM = new Vector3(pointX, pointY, PosD.Z) - PosD;

        float crossABAM = CrossProduct(AB, AM);
        float crossBCBM = CrossProduct(BC, BM);
        float crossCDCM = CrossProduct(CD, CM);
        float crossDADM = CrossProduct(DA, DM);

        if(crossABAM >= 0 && crossBCBM >= 0 && crossCDCM >= 0 && crossDADM >= 0) {
            return true;
        } else if(crossABAM <= 0 && crossBCBM <= 0 && crossCDCM <= 0 && crossDADM <= 0) {
            return true;
        } else {
            return false;
        }
    }

    private float CrossProduct(Vector3 a, Vector3 b) {
        return a.X * b.Y - a.Y * b.X;
    }

    //private bool isPointInside2DRectangle(float pointX, float pointY, Vector3 PosA, Vector3 PosB, Vector3 PosC, Vector3 PosD) {
    //    double x1 = PosA.X;
    //    double x2 = PosB.X;
    //    double x3 = PosC.X;
    //    double x4 = PosD.X;

    //    double y1 = PosA.Y;
    //    double y2 = PosB.Y;
    //    double y3 = PosC.Y;
    //    double y4 = PosD.Y;

    //    double a1 = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    //    double a2 = Math.Sqrt((x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3));
    //    double a3 = Math.Sqrt((x3 - x4) * (x3 - x4) + (y3 - y4) * (y3 - y4));
    //    double a4 = Math.Sqrt((x4 - x1) * (x4 - x1) + (y4 - y1) * (y4 - y1));

    //    double b1 = Math.Sqrt((x1 - pointX) * (x1 - pointX) + (y1 - pointY) * (y1 - pointY));
    //    double b2 = Math.Sqrt((x2 - pointX) * (x2 - pointX) + (y2 - pointY) * (y2 - pointY));
    //    double b3 = Math.Sqrt((x3 - pointX) * (x3 - pointX) + (y3 - pointY) * (y3 - pointY));
    //    double b4 = Math.Sqrt((x4 - pointX) * (x4 - pointX) + (y4 - pointY) * (y4 - pointY));

    //    double u1 = (a1 + b1 + b2) / 2;
    //    double u2 = (a2 + b2 + b3) / 2;
    //    double u3 = (a3 + b3 + b4) / 2;
    //    double u4 = (a4 + b4 + b1) / 2;

    //    double A1 = Math.Sqrt(u1 * (u1 - a1) * (u1 - b1) * (u1 - b2));
    //    double A2 = Math.Sqrt(u2 * (u2 - a2) * (u2 - b2) * (u2 - b3));
    //    double A3 = Math.Sqrt(u3 * (u3 - a3) * (u3 - b3) * (u3 - b4));
    //    double A4 = Math.Sqrt(u4 * (u4 - a4) * (u4 - b4) * (u4 - b1));

    //    double difference = A1 + A2 + A3 + A4 - a1 * a2;
    //    return difference < 1;
    //}

    public IEntity[] getAllEntities() {
        AllEntities = AllEntities.Where(e => e.Exists()).ToHashSet();
        return AllEntities.ToArray();
    }

    public List<IEntity> getAllEntitiesList() {
        AllEntities = AllEntities.Where(e => e.Exists()).ToHashSet();

        return AllEntities.ToList();
    }

    public bool IsOccupied() {
        return AllEntities.Count > 0;
    }

    public bool Interaction(IPlayer player) {
        if(OnCollisionShapeInteraction != null) {
            return OnCollisionShapeInteraction.Invoke(player);
        } else {
            EventController.triggerCollisionShapeEvent(player, this, InteractData);
            return true;
        }
    }

    public string toShortSave() {
        return $"{Position.ToJson()}#{Width.ToJson()}#{Height.ToJson()}#{Rotation.ToJson()}#{TrackPlayers.ToJson()}#{TrackVehicles.ToJson()}#{Interactable.ToJson()}";
    }

    public static string getShortSaveForData(Vector3 pos, float width, float height, float rotation, bool trackPlayers, bool trackVehicles, bool interactable = false) {
        return $"{pos.ToJson()}#{width.ToJson()}#{height.ToJson()}#{rotation.ToJson()}#{trackPlayers.ToJson()}#{trackVehicles.ToJson()}#{interactable.ToJson()}";
    }
}