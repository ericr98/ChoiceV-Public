using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Controller.Minijobs.Model.Minijob;

namespace ChoiceVServer.Controller.Minijobs.Model;

public abstract class MinijobTask {
    protected readonly Minijob Master;

    public readonly int Id;
    public readonly int Order;
    protected readonly Dictionary<string, string> Settings;

    public readonly string CollisionShapeString;
    public CollisionShape Spot;

    protected MinijobTask(Minijob master, int id, int order, string spotString, Dictionary<string, string> settings) {
        Master = master;

        Id = id;
        Order = order;

        Settings = settings;

        CollisionShapeString = spotString;
    }

    public virtual void preprepare() { }
    public virtual void postreset() { }

    public void prepare() {
        Spot = CollisionShape.Create(CollisionShapeString);
        Spot.OnCollisionShapeInteraction += onInteraction;

        prepareStep();
    }

    protected abstract void prepareStep();

    public void reset() {
        if (Spot != null) {
            Spot.Dispose();
            Spot = null;
        }

        resetStep();
    }

    public virtual void resetForPlayer(Minijob.MinijobInstance instance, IPlayer player) {
        BlipController.destroyBlipByName(player, $"MINIJOB_TASK_{Id}");
        finishForPlayerStep(instance, player);
    }

    protected abstract void finishForPlayerStep(MinijobInstance instance, IPlayer player);

    protected abstract void resetStep();

    public abstract void start(MinijobInstance instance);

    protected abstract void finishStep(MinijobInstance instance);

    public void onShowInfo(MinijobInstance instance, IPlayer player) {
        player.sendNotification(Constants.NotifactionTypes.Info, "Begib dich zu der auf der Karte beschriebenen Position", "Begib dich zur Position", Constants.NotifactionImages.MiniJob);
        BlipController.createPointBlip(player, $"Aufgabe: {Order}", Spot.Position, Order % 85, 78, 255, $"MINIJOB_TASK_{Id}");

        onShowInfoStep(instance, player);
    }

    /// <summary>
    /// Is called for EVERY player!
    /// </summary>
    protected abstract void onShowInfoStep(MinijobInstance instance, IPlayer player);

    public virtual void onFinish(MinijobInstance instance) {
        finishStep(instance);

        foreach (var player in instance.ConnectedPlayers) {
            BlipController.destroyBlipByName(player, $"MINIJOB_TASK_{Id}");
        }

        Master.advanceTask(instance, this);
    }

    public bool onInteraction(IPlayer player) {
        return Master.ifInJobInteract(player, (instance) => {
            onInteractionStep(player, instance);
        });
    }

    public abstract void onInteractionStep(IPlayer player, MinijobInstance instance);

    public abstract bool isMultiViable();

    #region Admin Stuff

    public virtual List<MenuItem> getAdminMenuElements() {
        return [];
    }

    #endregion
}
