import React from 'react';
import './style/Item.css';
import { url } from './../../../index';

export default class InventoryItem extends React.Component {
    constructor(props) {
        super(props);
    }

    getQualityString(quality) {
        if(quality === 0 || quality === -1) {
            return "";
        } else {
            var st = "Qualität: (";
            for(var i = 0; i < quality; i++) {
                st += "*";
            }
            st +=") | "
            return st;
        }
    }

    render() {
        return (
            <div className={"itemContainer " + (this.props.item.isEquipped ? "equipped" : "")} item={JSON.stringify(this.props.item)}>
                <img className="itemIcon handlePart" src={url + "inventory/categories/" +  this.props.item.category + ".png"}></img>
                <div className="nameWeightContainer handlePart">
                    <div className="name">{this.props.item.name}</div>
                    <div className="weight">{this.props.item.weight.toFixed(2) + "kg"}</div>
                    <div className="additionalInfo">{this.props.item.additionalInfo}</div>
                </div>
                <div className="descriptionWrapper handlePart">
                    <div className="description">{this.getQualityString(this.props.item.quality) + this.props.item.description}</div>
                </div>
                <div className="amount handlePart">{this.props.item.amount}</div>
                <input type="number" className="amountSelect" min="1"></input>
            </div>);
    }
}

export class Item {
    constructor(configId, name, quality, category, description, amount, weight, useable, equipSlot, isEquipped, currentInvId, additionalInfo) {
        this.configId = configId;
        this.name = name;
        this.quality = quality;
        this.category = category;
        this.description = description; 
        this.amount = amount;
        this.weight = weight;
        this.useable = useable;
        this.equipSlot = equipSlot;
        this.isEquipped = isEquipped;

        this.additionalInfo = additionalInfo;

        this.currentInvId = currentInvId;
    }
    
    getWeight() {
        return this.weight * this.amount;
    }
}