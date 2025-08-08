import React from 'react';

import { Menu } from './Menu'
import './style/MenuElement.css';
//import Autocomplete from 'react-autocomplete';
//import Select from "react-select";
import SearchableDropdown from './OtherElements/SearchableDropDown';

export default class MenuElementShow extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        var classAdd = "";
        if(this.props.element.isCurrent) {
            classAdd = "IsCurrent";
        }
        return (
        <div className={"menuElement " + this.props.element.className + classAdd} key={this.props.element.id}>
            <div className="menuPadding">
                {this.props.element.name}
            </div>
            {this.props.element.getRightElement(this)}
        </div>);
    }
}

//Should all be Components but im an idiot. Greetings from Erk

class MenuElement {
    constructor(id, name, description, className, eventOnSelect, updateIdentifier) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.isCurrent = false;
        this.className = className;
        this.eventOnSelect = eventOnSelect;
        
        this.updateIdentifier = updateIdentifier;
    }

    onEnter() {
        //Nothing
    }

    onChanged() {
        //Nothing
    }

    onSelect() {
        //Nothing
    }

    onUnselect() {
        //Nothing
    }

    onArrowLeft() {
        //Nothing
    }

    onArrowRight() {
        //Nothing
    }

    getRightElement(parent) { }
    
    setRightElement(value) { }

    getData() {
        return {id: this.id}
    }
}

export class StaticMenuItem extends MenuElement {
    constructor(id, name, rightString, description, className, eventOnSelect, updateIdentifier) {
        super(id, name, description, className, eventOnSelect, updateIdentifier);

        this.rightString = rightString;
    }

    setRightElement(value) {
        this.rightString = value;
    }

    getRightElement() {
        return(
            <div className={"menuElementRight"}>
                {this.rightString}
            </div>
        );
    }
}

export class ClickMenuItem extends MenuElement {
    constructor(id, name, rightString, description, evt, className, eventOnSelect, closeOnAction, updateIdentifier) {
        super(id, name, description, className, eventOnSelect, closeOnAction);
        
        this.rightString = rightString;
        this.evt = evt;
        this.updateIdentifier = updateIdentifier;

        this.closeOnAction = closeOnAction;
    }

    getRightElement() {
        return(
            <div className="menuElementRight">
                {this.rightString}
            </div>
        );
    }
    
    setRightElement(value) {
        this.rightString = value;
    }

    onEnter(callback) {
        callback("click", {id: this.id, evt: this.evt, action: "enter", closeMenu: this.closeOnAction});
    }
    
    onChanged(callback) {
        if(this.eventOnSelect) {
            callback("changed", {id: this.id, evt: this.evt, action: "changed"});
        }
    }
}

export class HoverMenuItem extends MenuElement {
    constructor(id, name, rightString, description, evt, className, eventOnSelect) {
        super(id, name, description, className, eventOnSelect);
        
        this.rightString = rightString;
        this.evt = evt;
    }

    getRightElement() {
        return(
            <div className="menuElementRight">
                {this.rightString}
            </div>
        );
    }

    onEnter(callback) {
       //Do nothing
    }
    
    onChanged(callback) {
        callback("changed", {id: this.id, evt: this.evt, action: "changed"});
    }
}

export class MenuMenuItem extends MenuElement {
    constructor(id, name, rightString, description, menuData, parent, className, evt, eventOnSelect, alwaysCreateNew) {
        super(id, name, description, className, eventOnSelect);
        
        this.rightString = rightString;
        this.evt = evt;
        this.alwaysCreateNew = alwaysCreateNew;

        //Only send MenuData if needed!
        if(menuData != null) {
            this.menu = new Menu(menuData, parent);
        } else {
            this.parent = parent;
            this.menu = null;
        }
    }

    setMenu(data) {
        this.menu = new Menu(data, this.parent);
    }

    getRightElement() {
        if(this.rightString != null) {
            return(
                <div className="menuElementRight">
                    {this.rightString}
                </div>
            );
        } else {
            return(
                <div className="menuElementRight">
                    {">"}
                </div>
            );

        }
    }

    onEnter(callback) {
        if(this.menu != null && !this.alwaysCreateNew) {
            callback("menu", this.menu);
        } else {
            callback("menu_request", {id: this.id, evt: ""});
        }
    }

    onChanged(callback) {
        if(this.eventOnSelect) {
            callback("changed", {id: this.id, evt: this.evt, action: "changed"});
        }
    }

    getData() {
        var arr = [];
        if(this.menu != null) {
            this.menu.elements.forEach((el) => {
                var data = el.getData();
                if(Array.isArray(data)) {
                    data.forEach((el1) => {
                        arr.push(el1);
                    });
                } else {
                    arr.push(data);
                }
            });
        } else {
            arr.push(null);
        }
        return arr;
    }
}

export class ListMenuItem extends MenuElement {
    constructor(id, name, description, disableEnter, elements, evt, className, eventOnSelect, noLoopOver, noLoopOverStart) {
        super(id, name, description, className, eventOnSelect);
        
        this.elements = elements;
        this.disableEnter = disableEnter;
        this.evt = evt;
        
        this.currentElement = elements[0];
        this.currentIndex = 0;

        this.noLoopOver = noLoopOver;
        this.noLoopOverStart = noLoopOverStart;
        this.noLoopOverEnd = elements[(elements.findIndex((el) => el == noLoopOverStart) - 1) % elements.length];
    }

    getRightElement() {
        return(     
            <div className="menuElementRight">
                ◂ {this.currentElement} ▸
            </div>);
    }

    onEnter(callback) {
        if(!this.disableEnter) {
            callback("list", {id: this.id, evt: this.evt, action: "enter", currentElement: this.currentElement, currentIndex: this.currentIndex});
        }
    }

    onArrowLeft() {
        if(this.noLoopOver && this.currentElement == this.noLoopOverStart) {
            return;
        }

        if(this.currentIndex - 1 < 0) {
                this.currentIndex = this.elements.length - 1;
                this.currentElement = this.elements[this.currentIndex];
        } else {
            this.currentIndex--;
            this.currentElement = this.elements[this.currentIndex];
        }
    }

    onArrowRight() {
        if(this.noLoopOver && this.currentElement == this.noLoopOverEnd) {
            return;
        }

        if(this.currentIndex + 1 > this.elements.length - 1) {
            if(!this.noLoopOver) {
                this.currentIndex = 0;
                this.currentElement = this.elements[0];
            }
        } else {
            this.currentIndex++;
            this.currentElement = this.elements[this.currentIndex];
        }
    }

    onChanged(callback) {
        if(this.eventOnSelect) {
            callback("changed", {id: this.id, evt: this.evt, action: "changed", currentElement: this.currentElement, currentIndex: this.currentIndex});
        }
    }

    getData() {
        return {id: this.id, evt: this.evt, currentElement: this.currentElement, currentIndex: this.currentIndex}
    }
}


export class CheckBoxMenuItem extends MenuElement {
    constructor(id, name, description, check, evt, className, closeOnAction, eventOnSelect) {
        super(id, name, description, className, eventOnSelect);
        
        this.evt = evt;
        this.check = check;
        this.closeOnAction = closeOnAction;

        this.onChange = this.onChange.bind(this);
    }

    onChange(evt) {
        this.check = evt.target.checked;
    }

    getRightElement() {
        return(
            <div className="menuElementRight">
               <input id={"check" + this.id} type="checkbox" checked={this.check} onChange={this.onChange} className="menuElementCheck"/>
            </div>
        );
    }

    onEnter(callback) {
        this.check = !this.check;
        callback("check", {id: this.id, name: this.name, check: this.check, evt: this.evt, action: "enter", closeMenu: this.closeOnAction});
    }

    onSelect() {
        var input = document.getElementById("check" + this.id);
        if(input !== null) {
            input.focus();
        }
    }

    onUnselect() {
        var input = document.getElementById("check" + this.id);
        if(input !== null) {
            input.blur();
        }
    }

    onChanged(callback) {
        if(this.eventOnSelect) {
            this.check = !this.check;
            callback("changed", {id: this.id, name: this.name, check: this.check, evt: this.evt, action: "changed"});
        }
    }

    getData() {
        return {id: this.id, name: this.name, check: this.check}
    }
}

export class InputMenuItem extends MenuElement {
    constructor(id, name, description, input, inputType, evt, className, eventOnSelect, eventOnAnyUpdate, startValue, disableEnter, dontCloseOnEnter, stepSize, options, isDisabled) {
        super(id, name, description, className, eventOnSelect);
        
        this.placeholder = input;
        this.inputType = inputType;
        this.evt = evt;

        this.eventOnAnyUpdate = eventOnAnyUpdate;
        if(startValue == null) startValue = "";
        this.input = startValue;
        this.disableEnter = disableEnter;
        this.dontCloseOnEnter = dontCloseOnEnter;

        var counter = 0;

        if(options == null) {
            this.options = null;
        } else {
            this.options = options.map(el => {return {id: counter++, name: el}});
            this.dropdownRef = React.createRef();
        }

        this.stepSize = stepSize;

        this.isDisabled = isDisabled;

        this.onChange = this.onChange.bind(this);
    }

    onChange(value, parent) {
        this.input = value;
        parent.forceUpdate();

        if(this.eventOnAnyUpdate) {
            parent.props.onMenuItemEvent("input", {id: this.id, input: this.input, evt: this.evt, action: "updated", close: false});
        }
    }

    getRightElement(parent) {
        if(this.options == null || this.options == []) {
            return(
                <div className="menuElementRight">
                    <input disabled={this.isDisabled} id={"input" + this.id} type={this.inputType} step={this.stepSize} className="menuElementInput" placeholder={this.placeholder} onChange={(evt) => { this.onChange(evt.target.value, parent)}} value={this.input} spellCheck={false} onWheel={(evt) => {evt.preventDefault()}} onKeyDown={(evt) => {if(evt.keyCode == "38" || evt.keyCode == "40") evt.preventDefault()}} ></input>        
                </div>
            );
        } else {
            // var menuStyle = {
            //     borderRadius: '3px',
            //     boxShadow: '0 2px 12px rgba(0, 0, 0, 0.1)',
            //     background: 'rgba(255, 255, 255, 0.9)',
            //     padding: '2px 0',
            //     fontSize: '90%',
            //     position: 'fixed',
            //     overflow: 'auto',
            //     maxHeight: '26vh',
            //     maxWidth: '20vh' // TODO: don't cheat, let it flow to the bottom
            // }

            return (
                <div className="menuElementRightAutocomplete">

                <SearchableDropdown
                    ref={this.dropdownRef}
                    inputId={"input" + this.id}
                    options={this.options}
                    label="name"
                    selectedVal={this.input}
                    handleChange={(value) => this.onChange(value, parent)}
                />

                    {/* <Autocomplete
                        getItemValue={(item) => item.toString()}
                        items={this.filterOptions()}
                        autoSelect={true}
                        renderItem={(item, isHighlighted) => (
                            <div
                                key={item.id}
                                style={{ 
                                    background: isHighlighted ? "rgba(75, 75, 75, 0.9" : "rgb(0, 0, 0, 0.85)",
                                    textAlign: "left",
                                    paddingLeft: "5px"
                                }}
                            >
                            {item}
                        </div>
                        )}

                        menuStyle={menuStyle}    
                        inputProps={{
                            id: "input" + this.id,
                            type: this.inputType,
                            className: "menuElementInput",
                        }}           
                        // renderInput={(props) => (
                        //     <input id={"input" + this.id} type={this.inputType} className="menuElementInput" placeholder={this.placeholder} spellCheck={false} onWheel={(evt) => {evt.preventDefault()}} onKeyDown={(evt) => {if(evt.keyCode == "38" || evt.keyCode == "40") evt.preventDefault()}} ></input>        
                        // )}
                        onChange={(event, value) => this.onChange(value, parent)}
                        onSelect={(value, item) => this.onChange(value, parent)}
                        value={this.input}
                    /> */}
                </div>
            );
        }
    }

    onEnter(callback) {
        if(!this.disableEnter) {
            callback("input", {id: this.id, input: this.input, evt: this.evt, action: "enter", close: !this.dontCloseOnEnter});
        }
    }

    onSelect() {
        var input = document.getElementById("input" + this.id);
        if(input != null) {
            input.focus();
        }
    }

    onUnselect() {
        var input = document.getElementById("input" + this.id);
        if(input != null) {
            input.blur()

            if (this.dropdownRef && this.dropdownRef.current) {
                this.dropdownRef.current.toggle();
            }
        }
    }

    onChanged(callback) {
        if(this.eventOnSelect) {
            callback("changed", {id: this.id, input: this.input, evt: this.evt, action: "changed", close: false});
        }
    }

    getData() {
        return {id: this.id, input: this.input}
    }
}

export class MenuStatsItem extends MenuElement {
    constructor(id, name, description, evt, className, eventOnSelect, rightInfo) {
        super(id, name, description, className, eventOnSelect);
        
        this.evt = evt;
        this.rightInfo = rightInfo;
    }

    getRightElement() {
        return(
            <div className="menuElementRight">
                {this.rightInfo}
            </div>
        );
    }

    onEnter(callback) {
        callback("stats", {id: this.id, evt: this.evt, action: "enter", closeMenu: true});
    }

    onChanged(callback) {
        if(this.eventOnSelect) {
            callback("stats", {id: this.id, evt: this.evt, action: "changed", closeMenu: false});
        }
    }

    getData() {
        return {id: this.id, evt: this.evt}
    }
}

export class FileMenuItem extends MenuElement {
    constructor(id, name, description, evt, className, rightInfo) {
        super(id, name, description, className, false);

        this.evt = evt;
        this.rightInfo = rightInfo;
        this.data = null;
    }

    onChange(evt, parent) {
        var file = evt.target.files[0];
        var reader = new FileReader();
        reader.onload = (e) => {
            var data = e.target.result;
            this.data = data;
            console.log(data);
        };
        try {

            reader.readAsDataURL(file);
        } catch (error) {
            console.log(error);
        }
    }

    onEnter(callback) {
        callback("file", {id: this.id, evt: this.evt, action: "enter", fileData: this.data, closeMenu: true});
    }
    
    getData() {
        return {id: this.id, evt: this.evt, fileData: this.data}        
    }

    getRightElement(parent) {
        return (
            <div className="menuElementRight">
                <input type="file" id={"file" + this.id} className="menuElementFile" accept=".jpg, .jpeg, .png, .pdf" onChange={(evt) => {this.onChange(evt, parent)}}></input>
            </div>
        );
    }
}