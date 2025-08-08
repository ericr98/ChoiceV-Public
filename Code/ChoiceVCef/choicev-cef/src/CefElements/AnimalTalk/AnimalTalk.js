import React from "react";
import Select from 'react-select';
import {register} from "../../App";

import "./AnimalTalk.css";

export default class AnimalTalk extends React.Component {
    
    constructor(props) {
        super(props)

        this.selectRef = React.createRef(); 
        this.state = {
            open: false,
            options: [],
        }
        
        this.onChange = this.onChange.bind(this);
        this.onKeyDown = this.onKeyDown.bind(this);
        
        this.onOpenAnimalTalkWindow = this.onOpenAnimalTalkWindow.bind(this);
        this.props.input.registerEvent("OPEN_ANIMAL_TALK", this.onOpenAnimalTalkWindow);
        
        this.onFocusToggle = this.onFocusToggle.bind(this);
        this.props.input.registerEvent("ANIMAL_TALK_FOCUS_TOGGLE", this.onFocusToggle);
    }

    onOpenAnimalTalkWindow(data) {
        this.setState({
            open: true,
            originalOptions: data.categories
        });
    }

    onFocusToggle(data) {
        if(!this.state.open) return;
        
        if(data.focus) {
            this.selectRef.current.focus();

            this.setState({
                options: this.state.originalOptions.map(option => {
                    return {
                        label: option.name,
                        options: option.words.map(value => {
                            return {
                                value: value + "_" + Date.now(),
                                label: value
                            }
                        })
                    }
                }),
            })
        } else {
            this.selectRef.current.blur();
            
            this.selectRef.current.setValue([]);
        }
    }
    
    onChange(currentValues, option) {
        if (option.action === "select-option") {
            this.setState({
                options: [
                    ...this.state.options,
                    {
                        value: option.option.value + "_" + Date.now(),
                        label: option.option.label
                    }
                ].sort((a, b) => a.label.localeCompare(b.label))
            });
        } else if (option.action === "remove-value" ||option.action ===  "pop-value") {
            this.setState({
                options: [
                    ... this.state.options.filter(opt => opt.label !== option.removedValue.label),
                    {
                        value: option.removedValue.value + "_" + Date.now(),
                        label: option.removedValue.label
                    }
                ].sort((a, b) => a.label.localeCompare(b.label))
            });
        }
    }
    
    onKeyDown(e) {
        if (e.key === "Tab") {
            e.preventDefault();
            this.props.output.sendToServer("ANIMAL_TALK_SELECT", {
                    selection: this.selectRef.current.getValue().map(value => value.label)
            }, true, "ANIMAL_TALK_FOCUS");
            
            
            this.selectRef.current.blur();
            
            setTimeout(() => {
                this.selectRef.current.setValue([]);
            }, 5000);
        }
    }

    render() {
        if (!this.state.open) {
            return null;
        }
        
        return (
            <div id="animalTalkWrapper">
                <Select
                    ref={this.selectRef}
                    options={this.state.options}
                    isMulti
                    onChange={this.onChange}
                    styles={{
                        control: (baseStyles, state) => ({
                            ...baseStyles,
                            borderRadius: 0,
                            opacity: 0.7,
                            border: "none",
                            outline: !state.isFocused ? "none" : "1px solid blue",
                            backgroundColor: !state.isFocused ? "transparent" : "white",
                        }),
                    }}
                    openMenuOnFocus={true}
                    closeMenuOnSelect={false}
                    onKeyDown={this.onKeyDown}
                    placeholder={""}
                    components={{ DropdownIndicator:() => null, IndicatorSeparator:() => null, ClearIndicator:() => null }}
                />
            </div>
        );
    }
}

register(AnimalTalk);
    