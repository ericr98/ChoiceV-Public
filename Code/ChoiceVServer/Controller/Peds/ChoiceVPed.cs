using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Bogus;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller;
public class ChoiceVPed {
    public CollisionShapeInteractionDelegate OnPedInteraction {
        get => CollisionShape.OnCollisionShapeInteraction;
        set => CollisionShape.OnCollisionShapeInteraction = value;
    }

    public int Id;
    public string Name;
    public IPlayer Seer;
    public string Model;
    public Position Position;
    public float Heading;
    public CollisionShape CollisionShape;

    public Dictionary<string, dynamic> NonPersistentData;
    public ExtendedDictionary<string, dynamic> Data;

    public int HeadVariation;
    public int TorsoVariation;

    public string Scenario = null;

    public Animation StandardAnimation = null;

    public Animation Animation = null;
    public int CurrentAnimPriorityLevel = 0;

    public DateTime LastThreatendDate = DateTime.MinValue;

    public bool IsBeingThreatend { get => DateTime.Now - LastThreatendDate < TimeSpan.FromSeconds(7.5); }

    public bool HasReportedLastThreaten;
    public IInvoke ReportingInvoke = null;

    public DateTime AnimStart = DateTime.MinValue;

    private List<NPCModule> Modules;

    public bool PerformingThreatendIgnoreAction;

    public bool IsVisible = true;

    public int Alpha = 255;

    public ChoiceVPed(int id, string name, string model, Position positon, float heading, IPlayer seer, int headVariation = 0, int torsoVariation = 0, Dictionary<string, dynamic> data = null) {
        if (id == -1) {
            Id = PedController.NonDbPedsId--;
        } else {
            Id = id;
        }

        if (name != null) {
            Name = name;
        } else {
            Name = new Faker().Name.FullName();
        }

        Seer = seer;
        Model = model;
        Position = positon;
        Heading = heading;

        HeadVariation = headVariation;
        TorsoVariation = torsoVariation;

        CollisionShape = CollisionShape.Create(positon, 4, 4, heading, true, false, true);
        CollisionShape.OnCollisionShapeInteraction += onInteract;

        CollisionShape.OnEntityEnterShape += onEnterShape;

        NonPersistentData = new Dictionary<string, dynamic>();

        Data = new ExtendedDictionary<string, dynamic>(data == null ? new Dictionary<string, dynamic>() : data);
        if (id != -1) {
            Data.OnValueChanged += onDataChanged;
            Data.OnValueRemoved += onDataRemoved;
        }
        Modules = new List<NPCModule>();
    }

    public void setScenario(string scenario) {
        Scenario = scenario;

        ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "SCENARIO", scenario);
    }

    public void setStandardAnimation(string animDict, string animName, int animFlag, int animDuration, float animPercent) {
        StandardAnimation = new Animation(animDict, animName, animDuration == -1 ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(animDuration), animFlag, animPercent);
        Animation = new Animation(animDict, animName, animDuration == -1 ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(animDuration), animFlag, animPercent);

        AnimStart = DateTime.Now;

        ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "ANIMATION", Animation.Dictionary, Animation.Name, Animation.Flag, Animation.Duration == TimeSpan.MaxValue ? -1 : Animation.Duration.TotalMilliseconds, Animation.StartAtPercent);
    }

    public void returnToStandardAnimation() {
        if (StandardAnimation != null) {
            Animation = StandardAnimation;
            CurrentAnimPriorityLevel = 0;
            AnimStart = DateTime.Now;

            ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "ANIMATION", Animation.Dictionary, Animation.Name, Animation.Flag, Animation.Duration == TimeSpan.MaxValue ? -1 : Animation.Duration.TotalMilliseconds, Animation.StartAtPercent);
        } else {
            Animation = null;
            ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "STOP_ANIMATION");
        }
    }

    public void playAnimation(string animDict, string animName, int animFlag, int animDuration, float animPercent, int priorityLevel = 0) {
        if (Animation == null || (priorityLevel >= CurrentAnimPriorityLevel || AnimStart + Animation.Duration < DateTime.Now)) {
            CurrentAnimPriorityLevel = priorityLevel;
            Animation = new Animation(animDict, animName, TimeSpan.FromMilliseconds(animDuration), animFlag, animPercent);

            AnimStart = DateTime.Now;
            ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "ANIMATION", animDict, animName, animFlag, animDuration, animPercent);
        }
    }

    public void stopAnimation() {
        Animation = null;
        ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "STOP_ANIMATION");
    }

    public void setAlpha(int alpha) {
        Alpha = alpha;
        ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "ALPHA", alpha);
    }

    public void spawn(IPlayer player) {
        if (Animation != null) {
            double animDuration = -1;
            if (Animation.Duration != TimeSpan.MaxValue) {
                animDuration = Animation.Duration.TotalMilliseconds;

                if (AnimStart != DateTime.MinValue) {
                    animDuration -= ((int)(DateTime.Now - AnimStart).TotalMilliseconds);

                    if (animDuration < 0) {
                        Animation = null;
                    }
                }
            }

            player.emitClientEvent(Constants.PlayerSpawnStaticPed, Id, Model, Position.X, Position.Y, Position.Z, Heading, HeadVariation, TorsoVariation, Scenario, Animation.Dictionary, Animation.Name, Animation.Flag, animDuration, Animation.StartAtPercent, IsVisible);
        } else {
            player.emitClientEvent(Constants.PlayerSpawnStaticPed, Id, Model, Position.X, Position.Y, Position.Z, Heading, HeadVariation, TorsoVariation, Scenario, null, null, null, null, null, IsVisible);
        }

        ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "ALPHA", Alpha);
        Logger.logTrace(LogCategory.Player, LogActionType.Created, $"ped: {ToString()}");
    }

    public void delete(IPlayer player) {
        if (CollisionShape != null) {
            CollisionShape.Dispose();
            CollisionShape = null;
        }

        player.emitClientEvent(Constants.PlayerDestroyStaticPed, Id);
    }

    public void toggleVisible(bool visible) {
        var changed = false;
        if (IsVisible != visible) {
            changed = true;
        }

        IsVisible = visible;

        if (changed) {
            ChoiceVAPI.emitClientEventToAll("SET_STATIC_PED_DATA", Id, "VISIBLE", IsVisible);
        }
    }

    public void setAdminVisible(IPlayer player, bool visible) {
        player.emitClientEvent("SET_STATIC_PED_DATA", Id, "ADMIN_VISIBLE", visible);
    }

    public void addModule(NPCModule module) {
        Modules.Add(module);
    }

    public void removeModuleByType<T>() {
        Modules.RemoveAll(m => m is T);
    }

    public void removeModule(NPCModule module) {
        module.onRemove();
        Modules.Remove(module);
    }

    public void remove() {
        CollisionShape?.Dispose();

        Modules.ForEach(m => m.onRemove());
        Modules.Clear();
    }

    public List<NPCModule> getModules() {
        return Modules;
    }

    public bool hasModule<T>(Predicate<T> predicate) {
        return Modules.OfType<T>().Any(m => predicate(m));
    }

    public bool hasModule(Type type, Predicate<NPCModule> predicate) {
        return Modules.Any(m => m.GetType() == type && predicate(m));
    }

    public List<T> getNPCModulesByType<T>(Predicate<T> predicate = null) {
        return Modules.OfType<T>().Where(m => (predicate == null || predicate(m))).ToList();
    }


    internal bool onInteract(IPlayer player) {
        if (!IsVisible && !player.getCharacterData().AdminMode) {
            return false;
        }

        var menu = new Menu(Name, "Wie kann ich dir helfen?");

        var overrideModule = Modules.FirstOrDefault(m => m.overridesOtherModulesOnInteraction(player));
        if(overrideModule != null) {
            overrideModule.onInteraction(player);

            var items = overrideModule.getMenuItems(player);
            if(items == null) {
                return false;
            }

            foreach(var item in items) {
                menu.addMenuItem(item);
            }
        } else {
            Modules.ForEach(m => m.onInteraction(player));

            foreach (var module in Modules) {
                var items = module.getMenuItems(player);

                //NoInteractModule
                if (items == null) {
                    return false;
                }

                foreach (var item in items) {
                    menu.addMenuItem(item);
                }
            }
        }

        if (menu.getMenuItemCount() == 0) {
            menu.addMenuItem(new StaticMenuItem("Keine Interaktion möglich", "Die Person möchte nicht mir dir reden", "", MenuItemStyle.yellow));
            player.showMenu(menu);
        } else {
            player.showMenu(menu);
        }

        return true;
    }

    private void onDataChanged(string key, dynamic value) {
        PedController.onPedDataValueChanged(this, key, value);
    }

    private bool onDataRemoved(string key) {
        return PedController.onPedDataValueRemoved(this, key);
    }

    public override string ToString() {
        var ev = CollisionShape != null ? CollisionShape.EventName : "null";
        return $"{Model}, position: {Position.X}/{Position.Y}/{Position.Z}, interactEvent: {ev}";
    }

    public void onTick(TimeSpan TickTimer) {
        Modules.ForEach(m => m.onTick(TickTimer));
    }

    public void onShortTick() {
        if (LastThreatendDate != DateTime.MinValue && !IsBeingThreatend && ReportingInvoke == null && !PerformingThreatendIgnoreAction) {
            if(hasModule<NPCWillNotCallPoliceModule>(m => true) && new Random().NextDouble() < 0.9) {
                LastThreatendDate = DateTime.MinValue;
                returnToStandardAnimation();
                return;
            }

            playAnimation("amb@code_human_wander_texting@male@base", "static", 1, 3000, 0.5f, 1);
            ReportingInvoke = InvokeController.AddTimedInvoke("Ped-Report-invoke", (i) => {
                if (!IsBeingThreatend) {
                    ControlCenterController.createDispatch(DispatchType.NpcCallDispatch, "Person wurde bedroht", $"Die Person mit Namen: {Name} wurde bedroht.", Position);

                    returnToStandardAnimation();

                    LastThreatendDate = DateTime.MinValue;
                }

                ReportingInvoke = null;
            }, TimeSpan.FromSeconds(3), false);
        }
    }

    internal void onEnterShape(CollisionShape shape, IEntity entity) {
        Modules.ForEach(m => m.onEntityEnterPedShape(entity));
    }
}