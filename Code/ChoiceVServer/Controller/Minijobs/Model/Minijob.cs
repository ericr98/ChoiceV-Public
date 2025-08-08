using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Minijobs.Modules;
using ChoiceVServer.Model;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Controller.MiniJobController;

namespace ChoiceVServer.Controller.Minijobs.Model;

public record RewardOption(decimal CashReward, List<int> Items, List<int> ItemsAmount, string RankName, int rankExp);
public record MadeJobs(int Id, int Amount);

public class Minijob {
    public class MinijobInstance {
        public int StarterId;
        public List<int> ConnectedPlayersIds;
        public List<IPlayer> ConnectedPlayers;
        public DateTime StartTime;
        public int CurrentTask;

        public MinijobInstance(IPlayer starter, int startTask) {
            StarterId = starter.getCharacterId();
            ConnectedPlayersIds = new List<int>() { starter.getCharacterId() };
            ConnectedPlayers = new List<IPlayer>() { starter };
            StartTime = DateTime.Now;
            CurrentTask = startTask;
        }

        public void addPlayer(IPlayer player) {
            if (!ConnectedPlayersIds.Contains(player.getCharacterId())) {
                ConnectedPlayersIds.Add(player.getCharacterId());
                ConnectedPlayers.Add(player);
            } else if (!ConnectedPlayers.Contains(player)) {
                ConnectedPlayers.Add(player);
            }
        }
    }

    //ConfigInfo
    public int Id;
    public MiniJobTypes Type;
    public string Name;
    public string Description { get; private set; }
    public string Requirement { get; private set; }
    public string Information { get; private set; }
    public TimeSpan WorkTime { get; private set; }

    public List<MinijobTask> AllTasks;
    public List<RewardOption> RewardOptions;

    public List<ChoiceVPed> AllNoInteractNPCs;
    public List<ChoiceVPed> AllOnlyOngoingVisible;

    public int MaxUses { get; private set; }

    private List<MinijobInstance> AllInstances;

    public bool Ongoing { get; private set; }

    public bool BlockMultiUse { get; private set; }

    public Minijob(int id, MiniJobTypes type, string name, string description, string requirement, string information, float maxWorkHours, int maxUses, List<RewardOption> rewardOptions, bool blockMultiUse) {
        Id = id;
        Type = type;
        Name = name;
        Description = description;
        Requirement = requirement;
        Information = information;
        WorkTime = TimeSpan.FromHours(maxWorkHours);
        MaxUses = maxUses;
        AllTasks = new();
        RewardOptions = rewardOptions;
        AllNoInteractNPCs = new();
        AllOnlyOngoingVisible = new();

        AllInstances = new();

        BlockMultiUse = blockMultiUse;
    }

    public void addMinijobTask(MinijobTask task, bool addPeds = true) {
        AllTasks.Add(task);
        AllTasks = AllTasks.OrderBy(t => t.Order).ToList();

        if (addPeds) {
            var testColShape = CollisionShape.Create(task.CollisionShapeString);
            var connectedPeds = PedController.findPeds(p => testColShape.IsInShape(p.Position)).ToList();
            testColShape.Dispose();

            foreach (var ped in connectedPeds) {
                if (!AllNoInteractNPCs.Contains(ped)) {
                    AllNoInteractNPCs.Add(ped);
                    ped.addModule(new NPCMinijobModule(ped, this));
                }
            }
        }
    }

    public void startMiniJob(IPlayer player, int startTask = 0) {
        if (!Ongoing || isMultiViable()) {
            if (!Ongoing) {
                AllOnlyOngoingVisible.ForEach(p => p.getNPCModulesByType<NPCOnlyVisibleInMinijobModule>().First().onMinijobStart());
            }

            var nextTask = AllTasks[startTask];

            if (!Ongoing) {
                foreach (var task in AllTasks) {
                    task.preprepare();
                }
            }

            if (!AllInstances.Any(i => i.CurrentTask == startTask)) {
                nextTask.prepare();
            }

            var inst = new MinijobInstance(player, startTask);
            AllInstances.Add(inst);

            Ongoing = true;

            nextTask.start(inst);

            nextTask.onShowInfo(inst, player);

            player.setData("CURRENT_MINIJOB", this);
        }
    }

    public void addPlayerToMinijob(MinijobInstance instance, IPlayer player) {
        if (Ongoing) {
            if (instance != null) {
                instance.addPlayer(player);
                var nextTask = AllTasks[instance.CurrentTask];
                nextTask.onShowInfo(instance, player);
                player.setData("CURRENT_MINIJOB", this);
            }
        }
    }

    public void removePlayerFromMinijob(IPlayer player) {
        var instance = AllInstances.FirstOrDefault(i => i.ConnectedPlayersIds.Contains(player.getCharacterId()));
        if (instance != null) {
            if (instance.ConnectedPlayersIds.Contains(player.getCharacterId())) {
                instance.ConnectedPlayersIds.Remove(player.getCharacterId());
            }

            if (instance.ConnectedPlayers.Contains(player)) {
                instance.ConnectedPlayers.Remove(player);

                var nextTask = AllTasks[instance.CurrentTask];
                nextTask.resetForPlayer(instance, player);

                player.resetPermantData("ADD_TO_MINIJOB");
                player.resetData("CURRENT_MINIJOB");
            }

            if (instance.ConnectedPlayers.Count <= 0) {
                finishMinijob(instance, false);
            }
        }
    }

    public void removePlayerForNow(IPlayer player) {
        var instance = AllInstances.FirstOrDefault(i => i.ConnectedPlayersIds.Contains(player.getCharacterId()));
        if (instance != null) {
            instance.ConnectedPlayers.Remove(player);
        }
    }

    public void advanceTask(MinijobInstance instance, MinijobTask currentTask) {
        if (!AllInstances.Any(i => i != instance && i.CurrentTask == AllTasks.IndexOf(currentTask))) {
            currentTask.reset();
        }

        if (instance.CurrentTask < AllTasks.Count - 1) {
            var nextTask = AllTasks[instance.CurrentTask + 1];

            if (!AllInstances.Any(i => i.CurrentTask == instance.CurrentTask + 1)) {
                nextTask.prepare();
            }
            nextTask.start(instance);

            instance.CurrentTask++;

            foreach (var player in instance.ConnectedPlayers) {
                nextTask.onShowInfo(instance, player);

                if (nextTask.Spot.IsInShape(player.Position)) {
                    nextTask.Spot.Interaction(player);
                }
            }
        } else {
            finishMinijob(instance, true);
        }
    }


    public void finishMinijob(MinijobInstance instance, bool finishedProperly) {
        if (AllInstances.Count == 1 && AllInstances.First() == instance) {
            foreach (var task in AllTasks) {
                task.postreset();
            }

            AllOnlyOngoingVisible.ForEach(p => p.getNPCModulesByType<NPCOnlyVisibleInMinijobModule>().First().onMinijobStop());
        }

        foreach (var player in instance.ConnectedPlayers) {
            player.resetData("CURRENT_MINIJOB");

            if (finishedProperly) {
                if (RewardOptions.Count > 0) {
                    if (RewardOptions.Count > 1) {
                        player.sendNotification(Constants.NotifactionTypes.Info, "Du kannst dir deine Belohnung im \"Selbst-Menu\" aussuchen. Standardtastenbelgung: M", "Belohnung auswählbar", Constants.NotifactionImages.MiniJob);
                        player.setPermanentData("MINIJOB_REWARD_OPTIONS", RewardOptions.ToJson());
                        player.setPermanentData("MINIJOB_REWARD_COUNT", instance.ConnectedPlayers.Count.ToString());
                    } else {
                        MiniJobController.givePlayerReward(player, RewardOptions.First(), instance.ConnectedPlayers.Count);
                    }
                } else {
                    player.sendNotification(Constants.NotifactionTypes.Info, "Für diesen Minijob gibt es keine Belohnung!", "Belohnung erhalten", Constants.NotifactionImages.MiniJob);
                }

                if (MaxUses != -1) {
                    var madeJobs = ((string)player.getData("MADE_JOBS_LIST")).FromJson<List<MadeJobs>>();

                    if (madeJobs != null) {
                        var already = madeJobs.FirstOrDefault(m => m.Id == Id);

                        if (already != null) {
                            already = new MadeJobs(Id, already.Amount + 1);
                        } else {
                            madeJobs.Add(new MadeJobs(Id, 1));
                        }
                    } else {
                        madeJobs = [new MadeJobs(Id, 1)];
                    }

                    player.setPermanentData("MADE_JOBS_LIST", madeJobs.ToJson());
                }
            } else {
                player.sendBlockNotification("Du hast für den Job zu lange benötigt. Er wurde abgebrochen!", "Job abgebrochen", Constants.NotifactionImages.MiniJob);
            }
        }

        foreach (var playerId in instance.ConnectedPlayersIds) {
            CharacterController.resetPlayerPermanentData(playerId, "ADD_TO_MINIJOB");
        }

        AllInstances.Remove(instance);

        if (AllInstances.Count <= 0) {
            Ongoing = false;
        }
    }

    public bool ifInJobInteract(IPlayer player, Action<MinijobInstance> onTrueCallback) {
        var instance = AllInstances.FirstOrDefault(i => i.ConnectedPlayers.Contains(player));
        if (instance != null) {
            onTrueCallback.Invoke(instance);
            return true;
        } else {
            player.sendBlockNotification("Tritt dem Minijob bei um hier etwas zu tun!", "Interaktion nicht möglich", Constants.NotifactionImages.MiniJob);
            return false;
        }
    }

    public Menu getRepresentative(IPlayer player = null, NPCMinijobDistributerModule givingModule = null) {
        var menu = new Menu(Name, "Was möchtest du tun?");

        menu.addMenuItem(new StaticMenuItem("Beschreibung", Description, ""));
        menu.addMenuItem(new StaticMenuItem("Informationen", $"Maximale Bearbeitungszeit: {WorkTime.TotalHours}h. {Information}", ""));
        menu.addMenuItem(new StaticMenuItem("Vorraussetzungen", Requirement, ""));

        var data = new Dictionary<string, dynamic> { { "Minijob", this }, { "Player", player } };
        if (givingModule != null) {
            data["GivingModule"] = givingModule;
        }

        menu.addMenuItem(new ClickMenuItem("Annehmen", "Nimm den Minijob an", "", "MINIJOB_TAKE_JOB", MenuItemStyle.green)
            .withData(data)
            .needsConfirmation($"{Name} annehmen?", "Job wirklich annehmen?"));

        return menu;
    }

    public bool canPlayerDoMinijob(IPlayer player) {
        if (MaxUses != -1) {
            if (!player.hasData("MADE_JOBS_LIST")) {
                return true;
            }

            var madeJobs = ((string)player.getData("MADE_JOBS_LIST")).FromJson<List<MadeJobs>>();
            var already = madeJobs.FirstOrDefault(m => m.Id == Id);

            if (already != null) {
                if (already.Amount >= MaxUses) {
                    return false;
                }
            }
        }
        return true;
    }

    public void update() {
        if (Ongoing) {
            foreach (var instance in AllInstances) {
                if (instance.StartTime + WorkTime < DateTime.Now) {
                    finishMinijob(instance, false);
                }
            }
        }
    }

    public bool isMultiViable() {
        return !BlockMultiUse && AllTasks.All(t => t.isMultiViable());
    }

    public bool isPlayerDoingMinijob(IPlayer player) {
        return AllInstances.Any(i => i.ConnectedPlayersIds.Contains(player.getCharacterId()));
    }

    public bool isPlayerByIdDoingMinijob(int playerId) {
        return AllInstances.Any(i => i.ConnectedPlayersIds.Contains(playerId));
    }

    public int getPlayerCurrentTask(IPlayer player) {
        var instance = AllInstances.FirstOrDefault(i => i.ConnectedPlayersIds.Contains(player.getCharacterId()));
        if (instance != null) {
            return instance.CurrentTask;
        } else {
            return -1;
        }
    }

    public MinijobInstance getPlayerInstance(IPlayer player) {
        return AllInstances.FirstOrDefault(i => i.ConnectedPlayersIds.Contains(player.getCharacterId()));
    }

    public MinijobInstance getPlayerByIdInstance(int playerId) {
        return AllInstances.FirstOrDefault(i => i.ConnectedPlayersIds.Contains(playerId));
    }
}
