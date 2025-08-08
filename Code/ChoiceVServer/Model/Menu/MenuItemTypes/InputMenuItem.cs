using ChoiceVServer.Base;
using Newtonsoft.Json;

namespace ChoiceVServer.Model.Menu {
    public enum InputMenuItemTypes {
        text,
        number,
        password,
    }

    public class InputMenuItem : UseableMenuItem {
        public string Placeholder;
        public InputMenuItemTypes InputType;

        private bool EventOnAnyUpdate;
        private string StartValue;
        private bool DisableEnterAction;
        private bool DontCloseOnEnter;

        private float StepSize = 1;

        private string[] Options = null;

        private bool DisabledInput = false;

        /// <summary>
        /// Item that can be filled with text by the player
        /// </summary>
        public InputMenuItem(string name, string description, string placeholder, InputMenuItemTypes inputType, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false) : base(name, description, evt, style, eventonselect) {
            Placeholder = placeholder;
            InputType = inputType;

        }

        /// <summary>
        /// Old constructor, does not allow to set input type
        /// </summary>
        public InputMenuItem(string name, string description, string placeholder, string evt, MenuItemStyle style = MenuItemStyle.normal, bool eventonselect = false) : base(name, description, evt, style, eventonselect) {
            Placeholder = placeholder;
            InputType = InputMenuItemTypes.text;
        }

        public InputMenuItem withStartValue(string value) {
            StartValue = value;

            return this;
        }

        public InputMenuItem withEventOnAnyUpdate() {
            EventOnAnyUpdate = true;

            return this;
        }

        public InputMenuItem withEnterDisabled() {
            DisableEnterAction = true;

            return this;
        }

        public InputMenuItem withNoCloseOnEnter() {
            DontCloseOnEnter = true;

            return this;
        }

        public InputMenuItem withNumberStep(float step) {
            StepSize = step;

            return this;
        }

        public InputMenuItem withOptions(string[] options) {
            Options = options;

            return this;
        }

        public InputMenuItem withDisabledInputField(bool disabled) {
            DisabledInput = disabled;

            return this;
        }

        public override bool doesMenuItemAutomaticallyBlockMovement() {
            return true;
        }

        private class InputMenuItemRepresentative : MenuItemCefRepresentative {
            public string input;
            public string inputType;
            public string evt;

            public bool eventOnAnyUpdate;
            public string startValue;
            public bool disableEnter;
            public bool dontCloseOnEnter;

            public float stepSize;

            public string[] options;

            public bool disabled;

            public InputMenuItemRepresentative(int id, string name, string description, string input, InputMenuItemTypes inputType, string evt, MenuItemStyle style, bool eventonselect, bool eventOnAnyUpdate, string startValue, bool disableEnter, bool dontCloseOnEnter, float stepSize, string[] options, bool disabled) : base(id, "input", name, description, style, eventonselect) {
                this.input = input;
                this.inputType = inputType.ToString();
                this.evt = evt;
                this.eventOnAnyUpdate = eventOnAnyUpdate;
                this.startValue = startValue;
                this.disableEnter = disableEnter;
                this.dontCloseOnEnter = dontCloseOnEnter;
                this.stepSize = stepSize;

                this.options = options;
                this.disabled = disabled;
            }
        }

        public override string toCef() {
            return (new InputMenuItemRepresentative(Id, Name, Description, Placeholder, InputType, Event, Style, EventOnSelect, EventOnAnyUpdate, StartValue, DisableEnterAction, DontCloseOnEnter, StepSize, Options, DisabledInput)).ToJson();
        }

        public class InputMenuItemEvent : MenuItemCefEvent {
            public string input;
        }

        public override MenuItemCefEvent getCefEvent(string data) {
            var obj = new InputMenuItemEvent();
            JsonConvert.PopulateObject(data, obj);
            return obj;
        }
    }
}
