using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AltV.Net.CApi.ClientEvents;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;

namespace ChoiceVServer.Controller.Crime_Stuff;

public class HoboController : ChoiceVScript {
    private static Dictionary<string, List<HoboCrimeQuestion>> CrimeQuestions = [];

    public const decimal HOBO_QUESTION_COST = 15;

    public HoboController() {
        loadQuestions();
        EventController.addMenuEvent("ON_PLAYER_ASK_HOBO", onPlayerAskHobo);
        EventController.addMenuEvent("ON_PLAYER_ASK_HOBO_ANSWER", onPlayerAskHoboAnswer);

        PedController.addNPCModuleGenerator("Obdachlosen-Modul", hoboModuleGenerator, hoboModuleCallback); 

        EventController.addMenuEvent("ON_PLAYER_SHOW_MODULE_POSITION", onPlayerShowModule);
        EventController.addMenuEvent( "ON_PLAYER_SHOW_RANDOM_VEHICLE_POSITION", onPlayerShowRandomVehiclePosition);

        #region Support Stuff

        SupportController.addSupportMenuElement(new GeneratedSupportMenuElement(4, SupportMenuCategories.Crime, supportQuestionGenerator));

        EventController.addMenuEvent("ON_SUPPORT_CREATE_QUESTION", onSupportCreateQuestion);
        EventController.addMenuEvent("ON_SUPPORT_CREATE_QUESTION_FINAL", onSupportCreateQuestionFinal);

        #endregion
    }

    private static void loadQuestions() {
        CrimeQuestions.Clear();

        using(var db = new ChoiceVDb()) {
            var questions = db.configcrimehoboquestions.ToList();

            foreach(var question in questions) {
                var questionType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == question.codeType);

                var pillar = CrimeNetworkController.getPillarById(question.pillarId ?? -1);
                var q = Activator.CreateInstance(questionType, question.id, pillar, question.name, question.labels.FromJson<List<string>>(), question.requiredReputation, question.settings.FromJson<Dictionary<string, string>>()) as HoboCrimeQuestion;

                foreach(var label in q.Labels) {
                    var keyword = label.ToLower();
                    if(!CrimeQuestions.ContainsKey(keyword)) {
                        CrimeQuestions[keyword] = [];
                    }

                    CrimeQuestions[keyword].Add(q);
                }
            }
        }
    }

    private bool onPlayerAskHobo(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as InputMenuItem.InputMenuItemEvent;
        var keyword = evt.input.ToLower();

        if(!player.removeCash(HOBO_QUESTION_COST)) {
            player.sendBlockNotification("Du hast nicht genug Geld, um den Bettler zu fragen.", "Nicht genug Geld");
            return false;
        }

        if(!CrimeQuestions.ContainsKey(keyword)) {
            player.sendBlockNotification("Der Bettler weiß nichts zu diesem Thema.", "Keine Antwort");
            return false;
        }

        var questions = CrimeQuestions[keyword];

        var menu = new Menu("Bettlerantworten", "Der Bettler antwortet dir auf deine Frage");

        var questionWithMissingReputation = false;
        var reputation = player.getCrimeReputation();
        foreach(var question in questions) {
            if(question.RequiredReputation > reputation.getPillarReputation(question.Pillar)) {
                questionWithMissingReputation = true;
                continue;
            }

            menu.addMenuItem(new ClickMenuItem(question.Name, $"Frage den Bettler über dieses Thema.", "", "ON_PLAYER_ASK_HOBO_ANSWER").withData(new Dictionary<string, dynamic> {
                { "Question", question }
            }));
        }

        if(questionWithMissingReputation) {
            menu.addMenuItem(new StaticMenuItem("Der Bettler weiß mehr über dieses Thema..", "Du merkst dass der Bettler mehr zu diesem Thema sagen könnte, aber deine Reputation reicht nicht aus.", "", MenuItemStyle.yellow));
        }

        player.showMenu(menu);

        return true;
    }

    private bool onPlayerAskHoboAnswer(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var question = data["Question"] as HoboCrimeQuestion;

        player.showMenu(question.getQuestionMenu());

        return true;
    }

    private List<MenuItem> hoboModuleGenerator(ref Type codeType) {
        codeType = typeof(HoboNPCModule);

        return [];
    }

    private void hoboModuleCallback(IPlayer player, MenuStatsMenuItem.MenuStatsMenuItemEvent evt, Action<Dictionary<string, dynamic>> creationFinishedCallback) {
        creationFinishedCallback.Invoke([]);
    }

    private bool onPlayerShowModule(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var position = (Position)data["Position"];
        
        BlipController.createTemporaryPointBlip(player, "Angefragte Person", position, 70, 458, 255, TimeSpan.FromMinutes(10));

        player.sendNotification(Constants.NotifactionTypes.Success, "Die Position wurde markiert. Sie bleibt markiert für etwa 10min!", "Position markiert");

        return true;
    }

    private bool onPlayerShowRandomVehiclePosition(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var vehicleClass = (int)data["VehicleClassId"];

        var vehicle = ChoiceVAPI.GetVehicles(v => v.IsRandomlySpawned && v.VehicleClassId == vehicleClass).OrderBy(v => new Random().Next()).FirstOrDefault();

        if(vehicle != null) {
            BlipController.createTemporaryPointBlip(player, "Angefragtes Fahrzeug", vehicle.Position, 70, 458, 255, TimeSpan.FromMinutes(5));

            player.sendNotification(Constants.NotifactionTypes.Success, "Die Position wurde markiert. Sie bleibt markiert für etwa 5min!", "Position markiert");
        } else {
            player.sendBlockNotification("Es konnte kein Fahrzeug dieser Klasse gefunden werden", "Kein Fahrzeug gefunden");
        }

        return true;
    }

    #region Support Stuff

    private static MenuItem supportQuestionGenerator(IPlayer player) {
        var menu = new Menu("Bettlerfragen", "Was möchtest du tun?");

        var createMenu = new Menu("Frage erstellen", "Erstelle eine neue Frage für den Bettler");
        createMenu.addMenuItem(new InputMenuItem("Schlüsselwörter", "Schlüsselwörter für die Frage. Getrennt durch Kommas", "", ""));
        createMenu.addMenuItem(new InputMenuItem("Name", "Name der Frage", "", ""));
        createMenu.addMenuItem(new ListMenuItem("Säule", "Die Säule, die die Frage betrifft", new List<string> {"Keine"}.Concat(CrimeNetworkController.getAllPillars().Select(p => p.Name)).ToArray(), ""));
        createMenu.addMenuItem(new InputMenuItem("Benötigte Reputation", "Die Reputation die benötigt wird, um die Frage zu stellen", "", InputMenuItemTypes.number, ""));
        createMenu.addMenuItem(new ListMenuItem("Typ", "Der Typ der Frage", Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(HoboCrimeQuestion))).Select(t => t.Name).ToArray(), ""));
        createMenu.addMenuItem(new MenuStatsMenuItem("Weiter zu Einstellungen", "Gehe zu den Einstellungen der Frage", "ON_SUPPORT_CREATE_QUESTION", MenuItemStyle.green));

        menu.addMenuItem(new MenuMenuItem(createMenu.Name, createMenu));

        foreach(var question in CrimeQuestions.Values.SelectMany(q => q).Distinct()) {
            var questionMenu = new Menu(question.Name, "Was möchtest du tun?");

            foreach(var infoItem in question.getSupportMenuInfo()) {
                questionMenu.addMenuItem(infoItem);
            }

            questionMenu.addMenuItem(new ClickMenuItem("Frage löschen", "Lösche diese Frage", "", "ON_SUPPORT_DELETE_QUESTION", MenuItemStyle.red).withData(new Dictionary<string, dynamic> {
                { "Question", question }
            }));

            menu.addMenuItem(new MenuMenuItem(questionMenu.Name, questionMenu));
        }

        return new MenuMenuItem(menu.Name, menu);
    }

    private bool onSupportCreateQuestion(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

        var questionHullType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name == evt.elements[4].FromJson<ListMenuItem.ListMenuItemEvent>().currentElement);
        var questionHull = Activator.CreateInstance(questionHullType) as HoboCrimeQuestion;

        var settings = questionHull.onSupportCreateMenuItems();


        var dict = new Dictionary<string, dynamic> {
            { "Keywords", evt.elements[0].FromJson<InputMenuItem.InputMenuItemEvent>().input.Split(",").Select(k => k.Trim()).ToList() },
            { "Name", evt.elements[1].FromJson<InputMenuItem.InputMenuItemEvent>().input },
            { "Pillar", CrimeNetworkController.getAllPillars().FirstOrDefault(p => p.Name == evt.elements[2].FromJson<ListMenuItem.ListMenuItemEvent>().currentElement) },
            { "RequiredReputation", int.Parse(evt.elements[3].FromJson<InputMenuItem.InputMenuItemEvent>().input) },
            { "QuestionHull", questionHull },
            { "QuestionType", questionHullType }
        };
        
        var menu = new Menu("Einstellungen", "Stelle die Einstellungen für die Frage ein");
        foreach(var setting in settings) {
            menu.addMenuItem(setting);
        }
        menu.addMenuItem(new MenuStatsMenuItem("Erstelle Frage", "Erstelle die Frage", "ON_SUPPORT_CREATE_QUESTION_FINAL", MenuItemStyle.green)
            .withData(dict));

        player.showMenu(menu);

        return true;
    }

    private bool onSupportCreateQuestionFinal(IPlayer player, string itemEvent, int menuItemId, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        var evt = menuItemCefEvent as MenuStatsMenuItem.MenuStatsMenuItemEvent;

        var settingsData = data["QuestionHull"].onSupportCreateSettings(evt);

        using(var db = new ChoiceVDb()) {
            var newQuestion = new configcrimehoboquestion {
                codeType = (data["QuestionType"] as Type).Name,
                labels = (data["Keywords"] as List<string>).ToJson(),
                name = data["Name"],
                pillarId = (data["Pillar"] as CrimeNetworkPillar)?.Id,
                requiredReputation = (int)data["RequiredReputation"],
                settings = (settingsData as Dictionary<string, string>).ToJson()
            };

            db.configcrimehoboquestions.Add(newQuestion);

            db.SaveChanges();

            player.sendNotification(Constants.NotifactionTypes.Success, "Die Frage wurde erstellt", "Frage erstellt");
        }
        loadQuestions();

        return true;
    }

    #endregion
}