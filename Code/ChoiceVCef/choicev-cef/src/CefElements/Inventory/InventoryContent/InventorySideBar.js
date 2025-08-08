import React, { Fragment } from 'react';
import { ReactSortable } from "react-sortablejs";
import { url } from "./../../../index";
import './style/InventorySideBar.css';

var elementSelected = false;
var dropCallback;

var currentBox = null;
var currentDrag = null;
var currentDragAmount = null;

export function setCallback(callback) {
    dropCallback = callback
}

export function onDrag(e) {
    elementSelected = true;
    currentBox = null;
    currentDrag = JSON.parse(e.item.getAttribute('item'));
    currentDragAmount = Number(e.item.children[4].value);
}

export function onDrop() {
    elementSelected = false;
    if(currentBox != null && currentDrag != null) {
        if(currentBox.props.type == "equip") {
            currentBox.setState({
                hover: false,
                equipped: true,
                equippedList: [{}],
            });
            dropCallback("EQUIP_ITEM", currentDrag, 1, true);
        } else if(currentBox.props.type == "use") {
            dropCallback("USE_ITEM", currentDrag, 1, true);
        } else if(currentBox.props.type == "delete") {
            dropCallback("DELETE_ITEM", currentDrag, currentDragAmount, true);
        } else if(currentBox.props.type == "give") {
            dropCallback("GIVE_ITEM", currentDrag, currentDragAmount, false);
        }
    }

    currentDrag = null;
}

function onUnequip(spot) {
    dropCallback("UNEQUIP_ITEM", spot.props.equipSlot, 1, true);
}

export class SingleInventorySideBar extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="singleSideBar sideBar">
                <WeaponSideBarDropSpotBox items={this.props.items} title="Waffen"/>
                <UseSideBarDropSpotBox items={this.props.items} title="Allgemein" giveVisible={this.props.giveVisible} />
                <ClothesSideBarDropSpotBox items={this.props.items} title="Kleidung"/>
                <AccessoireSideBarDropSpotBox items={this.props.items} title="Accessoires" />
            </div>)
    }
}

class WeaponSideBarDropSpotBox extends React.Component { 
    render() {
        return (
            <div className="sideBarElement">
                <div className="dropSpotBoxTitle">{this.props.title}</div>
                <div className="divider" style={{height: "0.3vh"}}/>
                <SideBarDropSpot width="86%" stlye={{height: "80%"}} equipSlot="longWeapon" type="equip" imgSrc="weapons/longWeapon" hoverClass="hover" items={this.props.items}/>
                <div className="dropSpotBox">
                    <SideBarDropSpot width="7vh" equipSlot="pistol" type="equip" imgSrc="weapons/pistol" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="melee" type="equip" imgSrc="weapons/melee" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="armor" type="equip" imgSrc="weapons/armor" hoverClass="hover" items={this.props.items}/>
                </div>
            </div>);
    }
}

class UseSideBarDropSpotBox extends React.Component {
    render() {
        return (
            <div className="sideBarElement">
                <div className="dropSpotBoxTitle">{this.props.title}</div>
                <div className="divider" style={{height: "0.3vh"}}/>
                <div className="dropSpotBox">
                    <SideBarDropSpot width="7vh" equipSlot="use" type="use" imgSrc="use/use" hoverClass="useHover" items={this.props.items}/>
                    {this.props.giveVisible ? 
                        <SideBarDropSpot width="7vh" equipSlot="alwaysAvailabe" type="give" imgSrc="use/give" hoverClass="useHover" items={this.props.items}/>
                        :
                        <div></div>
                    }
                    <SideBarDropSpot width="7vh" equipSlot="alwaysAvailabe" type="delete" imgSrc="use/bin" hoverClass="deleteHover" items={this.props.items}/>
                </div>
            </div>);
    }
}

class ClothesSideBarDropSpotBox extends React.Component { 
    constructor(props) {
        super(props);

        this.state = {
            displayClothesExtended: false,
        }

        this.hoverOverClothes = this.hoverOverClothes.bind(this);
        this.hoverOutOfClothes = this.hoverOutOfClothes.bind(this);
    }

    hoverOverClothes() {  
        if(this.timer != null) {
            clearTimeout(this.timer);
        }

        this.setState({
            displayClothesExtended: true,
        });
    }

    hoverOutOfClothes() {
        if(this.timer != null) {
            clearTimeout(this.timer);
        }

        this.timer = setTimeout(() => {
            this.setState({
                displayClothesExtended: false,
            });
        }, 500);
    }

    onComponentDidUnmount() {
        if(this.timer != null) {
            clearTimeout(this.timer);
        }
    }

    render() {
        var style = {
            width: this.props.width,
            backgroundImage: "url('" + url + "inventory/clothes/clothesUnequip.png" + "')",
        }

        return (
            <div className="sideBarElement">
                <div className="dropSpotBoxTitle">{this.props.title}</div>
                <div className="divider" style={{height: "0.3vh"}}/>
                <div className="dropSpotBox">       
                    {/* <SideBarDropSpot width="7vh" equipSlot="clothes" type="equip" imgSrc="clothes/clothes" hoverClass="hover" items={this.props.items}/> */}
                    <SideBarDropSpot width="7vh" equipSlot="hat" type="equip" imgSrc="clothes/hat" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="gloves" type="equip" imgSrc="clothes/gloves" hoverClass="hover" items={this.props.items}/>

                    <div id="clothesHover" style={style} onMouseEnter={this.hoverOverClothes} onMouseLeave={this.hoverOutOfClothes}></div>
                    <div className='clothesHoverDisplay' style={{display: this.state.displayClothesExtended ? "block" : "none"}} onMouseEnter={this.hoverOverClothes} onMouseLeave={this.hoverOutOfClothes}>
                        <div className='clothesSelection'>
                            <SideBarDropSpot width="7vh" equipSlot="top" type="equip" imgSrc="clothes/top" hoverClass="hover" items={this.props.items}/>
                            <SideBarDropSpot width="7vh" equipSlot="accessoire" type="equip" imgSrc="clothes/accessoire" hoverClass="hover" items={this.props.items}/>
                            <SideBarDropSpot width="7vh" equipSlot="legs" type="equip" imgSrc="clothes/legs" hoverClass="hover" items={this.props.items}/>
                            <SideBarDropSpot width="7vh" equipSlot="shoes" type="equip" imgSrc="clothes/shoes" hoverClass="hover" items={this.props.items}/>
                        </div>
                    </div>
                </div>
            </div>);
    }
}

class AccessoireSideBarDropSpotBox extends React.Component { 
    render() {
        return (
            <div className="sideBarElement">
                <div className="dropSpotBoxTitle">{this.props.title}</div>
                <div className="divider" style={{height: "0.3vh"}}/>
                <div className="dropSpotBox">
                    <SideBarDropSpot width="7vh" equipSlot="bracelet" type="equip" imgSrc="accessoire/bracelet" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="ears" type="equip" imgSrc="accessoire/ears" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="glasses" type="equip" imgSrc="accessoire/glasses" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="watch" type="equip" imgSrc="accessoire/watch" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="special" type="equip" imgSrc="accessoire/special" hoverClass="hover" items={this.props.items}/>
                    <SideBarDropSpot width="7vh" equipSlot="mask" type="equip" imgSrc="accessoire/mask" hoverClass="hover" items={this.props.items}/>
                </div>
            </div>);
    }
}


class SideBarDropSpot extends React.Component {
    constructor(props) {
        super(props);

        var equipped = false;
        this.props.items.forEach((el) => {
            if(el.equipSlot == this.props.equipSlot && el.isEquipped) {
                equipped = true;
                return;
            }
        });

        this.state = {
            hover: false,
            equipped: equipped,
            equippedList: [{}]
        }


        this.onMouseMove = this.onMouseMove.bind(this);
        this.onMouseLeave = this.onMouseLeave.bind(this);

        this.onUnequip = this.onUnequip.bind(this);
    }

    onMouseMove() {
        if(elementSelected && (currentDrag.equipSlot == this.props.equipSlot || (this.props.equipSlot == "alwaysAvailabe" && currentDrag != null) || (currentDrag != null && this.props.equipSlot == "use" && currentDrag.useable))) {
            this.setState({
                hover: true,
                equipped: false,
                equippedList: []
            })

            currentBox = this;
        }
    }

    onMouseLeave() {
        this.setState({
            hover: false
        });

        currentBox = null;
    }

    onUnequip() {
        this.setState({
            equipped: false
        })

        onUnequip(this);
    }

    render() {
        var style = {
            height: "7vh",
            width: this.props.width,
            backgroundImage: "url('" + url + "inventory/" + this.props.imgSrc + "Unequip" + ".png" + "')",
            backgroundPosition: 'center',
            backgroundSize: "95% 95%",
            backgroundRepeat: 'no-repeat',
        }

        if(!this.state.equipped) {
            return (<div className={this.state.hover ? "dropSpot " + this.props.hoverClass : "dropSpot"}  style={style} onMouseOver={this.onMouseMove} onMouseLeave={this.onMouseLeave} />);
        } else {
            var imgStyle = {
                height: "7vh",
                width: this.props.width,
                backgroundImage: "url('" + url + "inventory/" +  this.props.imgSrc + "Equip" + ".png" + "')",
                backgroundPosition: 'center',
                backgroundSize: "95% 95%",
                backgroundRepeat: 'no-repeat',
            }

            style.backgroundImage = "";
            return (
                <div className="dropSpot" style={style}>
                    <ReactSortable
                        list={this.state.equippedList}
                        setList={() => {}}
                        animation={100}
                        forceFallback={true}
                        onEnd={this.onUnequip}
                    >
                        <div style={imgStyle} />
                    </ReactSortable>
                </div>
            );
        }

    }
}