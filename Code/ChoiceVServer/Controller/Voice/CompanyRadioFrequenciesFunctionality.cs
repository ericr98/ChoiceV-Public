using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.Controller.Companies;
using ChoiceVServer.Model.Menu;
using static ChoiceVServer.Model.Menu.InputMenuItem;

namespace ChoiceVServer.Controller.Voice;

public class CompanyRadioFrequencyFunctionality : CompanyFunctionality {
    private List<float> Frequencies;
    
    public CompanyRadioFrequencyFunctionality() { }

    public CompanyRadioFrequencyFunctionality(Company company) : base(company) { }

    public override string getIdentifier() {
        return "COMPANY_RADIO_FREQUENCY";
    }

    public override SelectionInfo getSelectionInfo() {
        return new SelectionInfo(getIdentifier(), "Funkfrequenzen", "Ermöglicht es Firmen bestimmten verschlüsselte Frequenzen beizutreten");
    }

    public override void onLoad() {
        var frequencies = Company.getSettings("FREQUENCIES");
        if(frequencies != null) {
            Frequencies = frequencies.Select(f => f.settingsValue.FromJson<float>()).ToList();
        } else {
            Frequencies = [];
        }

        Company.registerCompanyAdminElement(
            "RADIO_FREQUENCY",
            frequencyMenuGenerator,
            frequencyCallback
        );
    }

    public bool hasFrequency(float frequency) {
        return Frequencies.Contains(frequency);
    }

    private MenuElement frequencyMenuGenerator(IPlayer player) {
        var menu = new Menu("Funkfrequenzen", "Füge Funkfrequenzen hinzu oder entferne sie");

        menu.addMenuItem(new InputMenuItem("Frequenz hinzufügen", "Füge eine Funkfrequenz hinzu", "Frequenz", "ADD_FREQUENCY"));

        foreach(var frequency in Frequencies) {
            menu.addMenuItem(new ClickMenuItem($"Frequenz {frequency}Mhz", $"Frequenz {frequency} entfernen", "", "REMOVE_FREQUENCY", MenuItemStyle.red)
                .withData(new Dictionary<string, dynamic> {
                    { "frequency", frequency }
                }));
        }

        return menu;
    }

    private void frequencyCallback(Company company, IPlayer player, string subEvent, Dictionary<string, dynamic> data, MenuItemCefEvent menuItemCefEvent) {
        if(subEvent == "ADD_FREQUENCY") {
            var evt = menuItemCefEvent as InputMenuItemEvent;
            var frequency = evt.input.FromJson<float>();
            Frequencies.Add(frequency);
            Company.setSetting("Frequency-" + frequency.ToString(), frequency.ToJson(), "FREQUENCIES");

            player.sendNotification(Constants.NotifactionTypes.Success, $"Frequenz {frequency} hinzugefügt", $"Frequenz {frequency} hinzugefügt", Constants.NotifactionImages.Radio);
            Logger.logDebug(LogCategory.Support, LogActionType.Created, player, $"Frequency {frequency} added to company {company.Name}");
        } else if(subEvent == "REMOVE_FREQUENCY") {
            var frequency = data["frequency"];
            Frequencies.Remove(frequency);
            Company.deleteSetting("Frequency-" + frequency.ToString(), "FREQUENCIES");

            player.sendNotification(Constants.NotifactionTypes.Success, $"Frequenz {frequency} entfernt", $"Frequenz {frequency} entfernt", Constants.NotifactionImages.Radio);
            Logger.logDebug(LogCategory.Support, LogActionType.Removed, player, $"Frequency {frequency} removed from company {company.Name}");
        }
    }

    public override void onRemove() {
        Company.unregisterCompanyElement("RADIO_FREQUENCY");
        Company.deleteSettings("FREQUENCIES");
    }
}