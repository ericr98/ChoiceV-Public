import React from 'react';
import MenuElementShow, {
    StaticMenuItem,
    ClickMenuItem,
    MenuMenuItem,
    InputMenuItem,
    HoverMenuItem,
    CheckBoxMenuItem,
    ListMenuItem,
    MenuStatsItem,
    FileMenuItem
} from './MenuElement'
import './style/Menu.css';

var MAX_SHOWN_ELEMENTS = 7;

export default class MenuShow extends React.Component {
    constructor(props) {
        super(props);
        
        var menu = new Menu(props.data, null);
        this.state = {
            menu: menu,
            currentElementIndex: 0,
            currentElement: menu.elements[0],
            blockInput: false,
        }
        
        this.removed = false;
        this.onKeyDown = this.onKeyDown.bind(this);
        this.onMenuItemEvent = this.onMenuItemEvent.bind(this);

        if(this.state.currentElement != undefined) {
            this.state.currentElement.isCurrent = true;
            this.state.currentElement.onSelect();
            this.state.currentElement.onChanged(this.onMenuItemEvent);
        }

    }

    componentDidMount(){
        document.addEventListener("keydown", this.onKeyDown, false);

        var current = this.state.currentElement;
        //if (current != undefined && current.eventOnSelect != undefined && current.eventOnSelect && current.check == undefined) {
        if (current != undefined) {
            current.onSelect();
            current.onChanged(this.onMenuItemEvent);
        }
    }

    onKeyDown(evt) {
        var index = this.state.currentElementIndex;
        var max = this.state.menu.elements.length;
        //Advance one MenuItem down
        if(evt.key === "ArrowDown") {
            if (this.state.currentElement == null || this.state.blockInput) {
                return;
            }
            
            this.state.currentElement.isCurrent = false;
            this.state.currentElement.onUnselect();

            if (index + 1 >= max) {
                index = 0;
            } else {
                index++;
            }

            var current = this.state.menu.elements[index];
            current.isCurrent = true;
            current.onSelect();

            this.setState({
                currentElementIndex: index,
                currentElement: current,
            })

            //Update Shown Menu Elements
            this.updateMenuShow(current, index);

            if (current.eventOnSelect != undefined && current.eventOnSelect && current.check == undefined)
                current.onChanged(this.onMenuItemEvent);

            current.onSelect();
        //Advance one item up
        } else if(evt.key === "ArrowUp") {
            if (this.state.currentElement == null) {
                return;
            }

            this.state.currentElement.isCurrent = false;
            this.state.currentElement.onUnselect();
            
            if (index - 1 < 0) {
                index = max-1;
            } else {
                index--;
            }

            var current = this.state.menu.elements[index];
            current.isCurrent = true;
            current.onSelect();

            this.setState({
                currentElementIndex: index,
                currentElement: current,
            })

            //Update Shown Menu Elements
            this.updateMenuShow(current, index);

            if (current.eventOnSelect != undefined && current.eventOnSelect && current.check == undefined)
                current.onChanged(this.onMenuItemEvent);
        
            current.onSelect();
        //Trigger Event
        } else if (evt.keyCode === 32) {
            if (this.state.currentElement != undefined && this.state.currentElement.check != undefined && this.state.currentElement.eventOnSelect != undefined && this.state.currentElement.eventOnSelect) {
                this.state.currentElement.onChanged(this.onMenuItemEvent);
            }
        } else if (evt.key === "Enter") {
            if (this.state.currentElement != undefined) {
                this.state.currentElement.onEnter(this.onMenuItemEvent);

                this.forceUpdate();
            }
        } else if (evt.key === "Backspace") {
            if (this.state.menu.parent !== null && (!(this.state.currentElement instanceof InputMenuItem) || evt.ctrlKey)) {
                this.changeMenu(this.state.menu.parent, true, (current) => {
                    this.props.sendEvent({id: current.id, subEvt: "SUBMENU_CLOSE"})
                });
            }
        } else if (evt.key === "ArrowLeft") {
            if (this.state.currentElement != undefined) {
                this.state.currentElement.onArrowLeft();
                this.setState({
                    update: !this.state.update,
                });

                if (this.state.currentElement.eventOnSelect != undefined && this.state.currentElement.eventOnSelect) {
                    this.state.currentElement.onChanged(this.onMenuItemEvent);
                }

            }
        } else if (evt.key === "ArrowRight") {
            if (this.state.currentElement != undefined) {
                this.state.currentElement.onArrowRight();

                this.setState({
                    update: !this.state.update,
                });

                if (this.state.currentElement.eventOnSelect != undefined && this.state.currentElement.eventOnSelect) {
                    this.state.currentElement.onChanged(this.onMenuItemEvent);
                }
            }
        }
    }
    
    addSubMenuData(data) {
        var item = this.state.menu.elements.filter(el => el.id == data.menuMenuItemId)[0];
        
        if(item != undefined) {
             this.setState({
                blockInput: false,
            }, () => {
                item.setMenu(data);
                this.changeMenu(item.menu, true);
                this.forceUpdate();
            });
        }
    }
    
    updateMenuItem(data) {
        var itemIdentifier = data.identifier;
        var rightValue = data.value;
        
        var root = this.state.menu;
        while(root.parent != null) {
            root = root.parent;
        }
        
        var item = this.findItemRecursive(root, itemIdentifier);
        if(item !== undefined) {
            item.setRightElement(rightValue);
            this.forceUpdate();
        }
    }
    
    findItemRecursive(menu, identifier) {
        var item = menu.elements.filter(el => el.updateIdentifier === identifier)[0];
        if(item !== undefined) {
            return item;
        } else {
            menu.elements.forEach((el) => {
                if(el instanceof MenuMenuItem) {
                    var item = this.findItemRecursive(el.menu, identifier);
                    if(item !== undefined) {
                        return item;
                    }
                }
            });
        }
    }

    updateMenuShow(current, index) {
        var list = this.state.menu.elements.filter((el, i) => { 
            if (el.isShown ) {
                el.tempIndex = i;
                return true;
            } else {
                return false;
            }
        });

        if(!list.includes(current)) {
            if (index == 0) {
                var count = 0;
                this.state.menu.elements.forEach((el, idx) => {
                    el.tempIndex = null;
                    count++;
                    el.isShown = count <= MAX_SHOWN_ELEMENTS;
                })
            } else if(index < list[0].tempIndex) {
                this.state.menu.elements.forEach((el, idx) => {
                    el.tempIndex = null;
                    if(idx < index + MAX_SHOWN_ELEMENTS && idx >= index) {
                        el.isShown = true;
                    } else {
                        el.isShown = false;
                    }
                })
            } else if(index > list[list.length - 1].tempIndex) {
                this.state.menu.elements.forEach((el, idx) => {
                    el.tempIndex = null;
                    if(idx > index - MAX_SHOWN_ELEMENTS && idx <= index) {
                        el.isShown = true;
                    } else {
                        el.isShown = false;
                    }
                })
            }

            this.forceUpdate();
        }
    }

    onMenuItemEvent(evtName, data) {
        //If Element can have event but it is disabled
        if(data.evt === null || this.removed) {
            return;
        }

        if(evtName === "click") {
            if(data.evt != null && data.evt != "") {
                this.props.sendEvent(data, data.closeMenu);
            }
        } else if(evtName === "changed") {
            if(data.evt != null && data.evt != "") {
                this.props.sendEvent(data, false);
            }
        } else if(evtName == "menu") {
            this.changeMenu(data, true);
        } else if(evtName == "input") {
            if(data.evt != null && data.evt != "") {
                this.props.sendEvent(data, data.close);
            }
        } else if(evtName == "check") {
            if(data.evt != null && data.evt != "") {
                this.props.sendEvent(data, data.closeMenu);
            }
            this.forceUpdate();
        } else if(evtName == "list") {
            if(data.evt != null && data.evt != "") {
                this.props.sendEvent(data, true);
            }
        } else if(evtName == "stats") {
            this.props.sendEvent({id: data.id, evt: data.evt, elements: this.getMenuData(this.state.menu.elements), action: data.action}, data.closeMenu);
        } else if(evtName == "menu_request") {
            this.setState({
                blockInput: true,
            });
            this.props.sendEvent({id: data.id, subEvt: "MENU_REQUEST_SUB_MENU"}, false);
        } else if (evtName == "file") {
            this.props.sendEvent(data, data.closeMenu);
        }
    }

    getMenuData(elements) {
        var arr = [];
        elements.forEach((el) => {
            var data = el.getData();
            if(Array.isArray(data)) {
                data.forEach((el1) => {
                    arr.push(JSON.stringify(el1));
                });
            } else {
                arr.push(JSON.stringify(data));
            }
        });

        return arr;
    }

    componentWillUnmount() {
        this.removed = true; 
    }

    changeMenu(newMenu, saveLastIndex, callback = null) {
        if(this.state.currentElement == null) {
            return;
        }

        if(saveLastIndex) {
            this.state.menu.lastSelectedIndex = this.state.currentElementIndex;
        } else {
            this.state.menu.lastSelectedIndex = 0;
        }

        this.state.currentElement.isCurrent = false;

        var current = newMenu.elements[newMenu.lastSelectedIndex];
        if(current === undefined) {
            this.state.currentElement.isCurrent = true;
            return;
        }
        current.isCurrent = true;
        this.setState({
            menu: newMenu,
            currentElementIndex: newMenu.lastSelectedIndex,
            currentElement: current,
        }, () => {
            current.onChanged(this.onMenuItemEvent);
            current.onSelect();
            if(callback != null) {
                callback(current);
            }
                           
            if(this.state.menu.parent == null) {             
                this.props.sendEvent({id: -1, subEvt: "SUB_MENU_OPEN"}, false);  
            } else {
                var parentMenu = this.state.menu.parent;
                var menuElement = parentMenu.elements[parentMenu.lastSelectedIndex];
                this.props.sendEvent({id: menuElement.id, subEvt: "SUB_MENU_OPEN"}, false); 
            }  
        })
    }

    render() {
        if(this.removed) {
            return null;
        }
        
        var desc = "";
        if(this.state.currentElement != null) {
            desc = this.state.currentElement.description;
        }

        return(
            <div className="menuWrapper">
                <div className="menu">
                    <div className="menuTitle"><div style={{display: "table-cell", verticalAlign: "middle"}}><div>{this.state.menu.name}</div></div></div>
                    <div className="menuDescription">
                        <div className="menuDescriptionLeft">{this.state.menu.subtitle}</div>
                        <div className="menuDescriptionRight">{(this.state.currentElementIndex + 1) + "/" + this.state.menu.elements.length}</div>
                    </div>
                    <MenuItemContainer elements={this.state.menu.elements} onMenuItemEvent={this.onMenuItemEvent} />
                    {this.state.menu.elements.length > MAX_SHOWN_ELEMENTS ?
                    <div className="menuUpDown">            
                        <div className="menuUpDownArrow">▲</div>
                        <div className="menuUpDownArrow">▼</div>
                    </div>
                    : null}
                    <div className="menuItemDescriptionOuter">
                        <div className="menuItemDescriptionInner menuDescriptionPadding">{desc}</div>
                    </div>
                </div>
            </div>);
    }
}

class MenuItemContainer extends React.Component {
    render() {
        return this.props.elements.map((el) => {
            if(el.isShown) {
                return <MenuElementShow element={el} onMenuItemEvent={this.props.onMenuItemEvent}/>
            }
        });
    }
}

export class Menu {
    constructor(data, parent) {
        this.elements = [];
        var count = 1;
        
        this.lastSelectedIndex = 0;

        data.elements.forEach((el) => {
            var obj = JSON.parse(el);
            var item;

            switch(obj.type) {
                case "static":
                    item = new StaticMenuItem(obj.id, obj.name, obj.right, obj.description, obj.className, obj.eventOnSelect, obj.updateIdentifier);
                    break;
                case "click":
                    item = new ClickMenuItem(obj.id, obj.name, obj.right, obj.description, obj.evt, obj.className, obj.eventOnSelect, obj.closeOnAction, obj.updateIdentifier);
                    break;
                case "menu":
                    item = new MenuMenuItem(obj.id, obj.name, obj.right, obj.description, obj.menuData, this, obj.className, obj.evt, obj.eventOnSelect, obj.alwaysCreateNew);
                    break;
                case "input":
                    item = new InputMenuItem(obj.id, obj.name, obj.description, obj.input, obj.inputType, obj.evt, obj.className, obj.eventOnSelect, obj.eventOnAnyUpdate, obj.startValue, obj.disableEnter, obj.dontCloseOnEnter, obj.stepSize, obj.options, obj.disabled);
                    break;
                case "check":
                    item = new CheckBoxMenuItem(obj.id, obj.name, obj.description, obj.check, obj.evt, obj.className, obj.closeOnAction, obj.eventOnSelect);
                    break;
                case "list":
                    item = new ListMenuItem(obj.id, obj.name, obj.description, obj.disableEnter, obj.elements, obj.evt, obj.className, obj.eventOnSelect, obj.noLoopOver, obj.noLoopOverStart);
                    break;
                case "stats":
                    item = new MenuStatsItem(obj.id, obj.name, obj.description, obj.evt, obj.className, obj.eventOnSelect, obj.rightInfo);
                    break;
                case "hover":
                    item = new HoverMenuItem(obj.id, obj.name, obj.right, obj.description, obj.evt, obj.className, true);
                    break;
                case "file":
                    item = new FileMenuItem(obj.id, obj.name, obj.description, obj.evt, obj.className, obj.rightInfo);
                    break;
            }

            item.isShown = count <= MAX_SHOWN_ELEMENTS;
            this.elements.push(item);
            count++;
        });

        this.parent = parent;
        this.name = data.name;
        this.subtitle = data.subtitle;
    }
}