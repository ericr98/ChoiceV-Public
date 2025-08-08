using System;
using System.Collections.Generic;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.Controller.Crime_Stuff;
using ChoiceVServer.Model.Menu;

public abstract class CrimeNetworkMissionTrigger {
        public int Id;
        public CrimeAction Type;
        public CrimeNetworkPillar Pillar;
        public float Amount;
        public TimeSpan TimeConstraint;
        public Position Position;
        public float Radius;

        public decimal CashReward;
        public float ReputationReward;
        public float FavorReward;

        protected Dictionary<string, string> Settings;

        public CrimeNetworkMissionTrigger(int id, CrimeAction type, CrimeNetworkPillar pillar, float amount, TimeSpan timeConstraint, Position location, float radius) {
            Id = id;
            Type = type;
            Pillar = pillar;
            Amount = amount;
            TimeConstraint = timeConstraint;
            Position = location;
            Radius = radius;
        }

        public void setSettings(Dictionary<string, string> settings) {
            Settings = settings;
            afterSetSettings();
        }

        protected virtual void afterSetSettings() { }

        public void setReward(decimal cashReward, float reputationReward, float favorReward) {
            CashReward = cashReward;
            ReputationReward = reputationReward;
            FavorReward = favorReward;
        }

        public bool onTriggerProgress(IPlayer player, string name, CrimeAction action, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress) {
            if(!(action != Type || currentProgress.Status is CrimeMissionProgressStatus.Completed or CrimeMissionProgressStatus.CompletedLate || (Position != Position.Zero && Position.Distance(player.Position) > Radius))) {
                return onTriggerProgressStep(player, name, amount, data, currentProgress);
            } else {
                return false;
            }
        }

        public abstract bool onTriggerProgressStep(IPlayer player, string name, float amount, Dictionary<string, dynamic> data, CrimeMissionProgress currentProgress);

        public Menu getMenuRepresentative(IPlayer player, DateTime startTime, CrimeMissionProgress currentProgress, bool crimeSpree = false) {
            var menu = new Menu(getName(), "Was möchtest du tun?");

            if(Position != Position.Zero) {
                menu.addMenuItem(new ClickMenuItem("Position anzeigen", $"Zeige dir das Areal in dem die Beabeitung stattfinden muss an.", $"", "CRIME_SHOW_MISSION_AREA").withData(new Dictionary<string, dynamic> { { "Mission", this } }));
            }

            if(!crimeSpree) {
                if(TimeConstraint != TimeSpan.Zero) {
                    if(startTime + TimeConstraint > DateTime.Now) {
                        var restTime = Math.Round((TimeConstraint - (DateTime.Now - startTime)).TotalMinutes);
                        menu.addMenuItem(new StaticMenuItem("Übrige Zeit", $"Du hast noch {restTime}min Zeit um die volle Belohnung zu erhalten!", $"{restTime}min übrig"));
                    } else {
                        menu.addMenuItem(new StaticMenuItem("Zeit abgelaufen!", "Du hast die Zeit für die Bearbeitung überschritten! Du wirst beim Abschluss nicht die volle Belohnung erhalten!", "", MenuItemStyle.yellow));
                    }
                }
            }

            foreach(var item in getMenuItemInfo(player, currentProgress)) {
                menu.addMenuItem(item);
            }

            return menu;
        }

        protected abstract List<MenuItem> getMenuItemInfo(IPlayer player, CrimeMissionProgress currentProgress);

        public abstract string getName();

        public abstract string getPillarReputationName();

        public void sendPlayerSelectNotification(IPlayer player) {
            var posStr = "";
            if(Position != Position.Zero) {
                posStr = $"Es gibt örtliche Einschränkung! Siehe auf die Karte!";
                showPositionBlip(player);
            }

            var timeStr = "";
            if(TimeConstraint != TimeSpan.Zero) {
                timeStr = $"Du hast {TimeConstraint.TotalMinutes}min Zeit für den Auftrag!";
            }

            player.sendNotification(Constants.NotifactionTypes.Info, $"{getPlayerSelectNotificationMessage(player)} {timeStr} {posStr}", "Kriminellen Auftrag erhalten", Constants.NotifactionImages.Thief);
        }

        protected abstract string getPlayerSelectNotificationMessage(IPlayer player);

        public void showPositionBlip(IPlayer player) {
            if(Position != Position.Zero) {
                var centerBlip = BlipController.createPointBlip(player, $"Auftragsareal: {getName()}", Position, 3, 433, 200);
                var radiusBlip = BlipController.createRadiusBlip(player, Position, Radius, 3, 100);

                InvokeController.AddTimedInvoke("Blip-Remover", (i) => {
                    BlipController.destroyBlipByName(player, centerBlip);
                    BlipController.destroyBlipByName(player, radiusBlip);
                }, TimeSpan.FromMinutes(1), false);
            }
        }

        #region Admin Stuff

        public virtual List<MenuItem> getCreateListMenuItems() { return new List<MenuItem>(); }

        public virtual Dictionary<string, dynamic> getSettingsFromMenuStats(MenuStatsMenuItem.MenuStatsMenuItemEvent evt) { return new Dictionary<string, dynamic>(); }

        #endregion
    }