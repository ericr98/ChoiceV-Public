using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using ChoiceVServer.Base;
using ChoiceVServer.EventSystem;
using ChoiceVServer.Model.Color;
using Newtonsoft.Json;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace ChoiceVServer.Controller {
    public enum ColorPickerActions {
        Select,
        Close,
    }

    public delegate bool ColorPickerEventDelegate(IPlayer player, Dictionary<string, dynamic> data, Rgba colorRgb, string ColorHex, ColorPickerActions action);

    public class ColorPickerController : ChoiceVScript {
        public static Dictionary<int, ColorPicker> CurrentlyDisplayedColorPicker = new Dictionary<int, ColorPicker>();

        public ColorPickerController() {
            EventController.addCefEvent("COLOR_EVENT", onColorPickerEvent);

            EventController.PlayerDisconnectedDelegate += onPlayerDisconnect;
        }

        private void onPlayerDisconnect(IPlayer player, string reason) {
            var charId = player.getCharacterId();
            if(CurrentlyDisplayedColorPicker.ContainsKey(charId)) {
                CurrentlyDisplayedColorPicker.Remove(charId);
            }
        }

        public static void showColorPicker(IPlayer player, ColorPicker color, bool blockMovement = true) {
            if(CurrentlyDisplayedColorPicker.ContainsKey(player.getCharacterId())) {
                CurrentlyDisplayedColorPicker[player.getCharacterId()] = color;
            } else {
                CurrentlyDisplayedColorPicker.Add(player.getCharacterId(), color);
            }

            if(blockMovement) {
                player.emitCefEventWithBlock(color.toCef(), "COLOR_PICKER");
            } else {
                player.emitCefEventNoBlock(color.toCef());
            }
        }

        public static void closeColorPicker(IPlayer player) {
            if(CurrentlyDisplayedColorPicker.ContainsKey(player.getCharacterId())) {
                CurrentlyDisplayedColorPicker.Remove(player.getCharacterId());
            }

            player.emitCefEventNoBlock(new OnlyEventCefEvent("CLOSE_COLOR"));
        }

        private class DefaultColorPickerWebSocketEvent {
            public string Hex = "";
            public Rgba Rgb = Rgba.Zero;
            public bool close;
        }

        private void onColorPickerEvent(IPlayer player, PlayerWebSocketConnectionDataElement evt) {
            if(CurrentlyDisplayedColorPicker.ContainsKey(player.getCharacterId())) {
                var picker = CurrentlyDisplayedColorPicker[player.getCharacterId()];
                if(picker != null) {
                    var cefData = new DefaultColorPickerWebSocketEvent();
                    JsonConvert.PopulateObject(evt.Data, cefData);

                    if(cefData.Rgb != Rgba.Zero) {
                        cefData.Rgb.A = 255;
                    }

                    picker.Callback?.Invoke(player, picker.Data, cefData.Rgb, cefData.Hex, cefData.close ? ColorPickerActions.Close : ColorPickerActions.Select);
                } else {
                    Logger.logError($"onColorPickerEvent: Player triggered event of colorpicker that was not opened!",
                          $"Fehler bei Farbenwähler: Spieler hat versucht Farbe zu wählen ohne Farbenwähler geöffnet zu haben", player);
                }
            } else {
                Logger.logError($"onColorPickerEvent: Player triggered event though he hasnt opened a colorpicker yet.",
                          $"Fehler bei Farbenwähler: Spieler hat versucht Farbe zu wählen ohne Farbenwähler geöffnet zu haben", player);
            }
        }
    }
}
