using AltV.Net.Data;
using ChoiceVServer.Base;
using ChoiceVServer.Controller;
using ChoiceVServer.EventSystem;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChoiceVServer.Model.Color {


    public enum ColorPickerType : int {
        None = 0,
        ChromePicker = 1,
        PhotoshopPicker = 2,
        CirclePicker = 3,
        CompactPicker = 4,
        SwatchesPicker = 5,
        SketchPicker = 6,
        GithubPicker = 7,
        TwitterPicker = 8,
        SmallCirclePicker = 9
    }

    public class ColorPickerCefEvent {
        public int id;
        public string evt;
    }

    public class ColorCefRepresentative : IPlayerCefEvent {
        public string Event { get; }
        public ColorPickerType ColorTyp = ColorPickerType.None;
        public string Color;
        public string[] ColorArr;

        public ColorCefRepresentative(ColorPickerType type, string color, string[] colorarr) {
            Event = "CREATE_COLOR";
            ColorTyp = type;
            Color = color;
            ColorArr = colorarr;
        }
    }

    public class ColorPicker {
        public ColorPickerType ColorTyp = ColorPickerType.None;
        public string Color;
        public string[] ColorArr;

        [JsonIgnore]
        public ColorPickerEventDelegate Callback;
        [JsonIgnore]
        public Dictionary<string, dynamic> Data = new Dictionary<string, dynamic>();

        public ColorPicker(ColorPickerType type, ColorPickerEventDelegate callback, string color, string[] colorarr = null) {
            ColorTyp = type;
            Callback = callback;
            Color = color;
            ColorArr = colorarr;
        }

        public ColorPicker(ColorPickerType type, ColorPickerEventDelegate callback, Rgba color, string[] colorarr = null) {
            ColorTyp = type;
            Callback = callback;
            Color = color.ToJson();
            ColorArr = colorarr;
        }

        public ColorPicker withData(Dictionary<string, dynamic> data) {
            if(data == null) {
                return this;
            }

            if(Data == null) {
                Data = data;

            } else {
                foreach(var pair in data) {
                    Data.Add(pair.Key, pair.Value);
                }
            }

            return this;
        }

        public ColorCefRepresentative toCef() {
            return new ColorCefRepresentative(ColorTyp, Color, ColorArr);
        }
    }
}
