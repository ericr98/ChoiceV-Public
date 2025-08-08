using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Minijobs.Model;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Controller.MiniJobController;

namespace ChoiceVServer.Controller.Minijobs.Modules;

public class NPCMinijobDistributerModule : NPCModule {
    private MiniJobTypes Type;
    private readonly int AmountOfJobs;

    internal List<Minijob> AvailableMinijobs = [];

    public NPCMinijobDistributerModule(ChoiceVPed ped, Dictionary<string, dynamic> settings) : base(ped, settings) {
        Type = (MiniJobTypes)settings["Type"];
        AmountOfJobs = (int)settings["AmountOfJobs"];
    }

    public override List<MenuItem> getMenuItems(IPlayer player) {
        AvailableMinijobs = AvailableMinijobs.Where(m => !m.Ongoing).ToList();

        if (AvailableMinijobs.Count < AmountOfJobs && AvailableMinijobs.Count <= MinijobCountPerType[Type]) {
            foreach (var available in MiniJobController.Minijobs.Shuffle().Where(m => m.Type == Type && !m.Ongoing)) {
                if (!AvailableMinijobs.Contains(available)) {
                    AvailableMinijobs.Add(available);

                    if (AvailableMinijobs.Count >= AmountOfJobs || AvailableMinijobs.Count >= MinijobCountPerType[Type]) {
                        break;
                    }
                }
            }
        }

        var menu = new Menu("Minijobauswahl", "Wähle einen Minijob aus");

        var blockedDate = MiniJobController.checkPlayerMinijobBlocked(player);
        if (blockedDate > DateTime.Now) {
            menu.addMenuItem(new StaticMenuItem("Blockiert", $"Da du einen Minijob abgebrochen hast bist du bist {blockedDate} blockiert!", $"bis {blockedDate}", MenuItemStyle.red));
        } else {
            if (player.hasData("CURRENT_MINIJOB")) {
                menu.addMenuItem(new StaticMenuItem("Aktuell in Minijob", "Du befindest dich aktuell in einem Minijob. Beende ihn erst!", "", MenuItemStyle.yellow));
            } else {
                foreach (var minijob in AvailableMinijobs) {
                    if (minijob.canPlayerDoMinijob(player)) {
                        var menuItem = minijob.getRepresentative(null, this);
                        menu.addMenuItem(new MenuMenuItem(menuItem.Name, menuItem));
                    } else {
                        menu.addMenuItem(new StaticMenuItem($"{minijob.Name}", "Dieser Minijob ist nicht nochmal für dich verfügbar. Du hast die maximale Anzahl an Bearbeitungen gemacht", "N/A", MenuItemStyle.yellow));
                    }
                }
            }
        }

        return [new MenuMenuItem(menu.Name, menu)];
    }

    public override StaticMenuItem getAdminMenuStaticRepresentative(IPlayer player) {
        return new StaticMenuItem("Minijob Geben Modul", $"Gibt Minijobs des Types: {Type} aus", "");
    }

    public override void onRemove() { }
}
