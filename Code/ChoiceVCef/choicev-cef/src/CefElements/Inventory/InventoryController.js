import React from 'react';
import { register } from '../../App';
import './InventoryController.css';
import { SingleInventory, DoubleInventory } from './InventoryContent/InventoryTypes';

export default class InventoryController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            CurrentInventory: null,
            data: null,
            filter: "",
        }

        this.loadSingleInventory = this.loadSingleInventory.bind(this);
        this.loadDoubleInventory = this.loadDoubleInventory.bind(this);
        this.closeInventory = this.closeInventory.bind(this);

        this.onSendEvent = this.onSendEvent.bind(this);
        this.onSendMoveEvent = this.onSendMoveEvent.bind(this);

        this.props.input.registerEvent("LOAD_SINGLE_INVENTORY", this.loadSingleInventory);
        this.props.input.registerEvent("LOAD_DOUBLE_INVENTORY", this.loadDoubleInventory);

        this.props.input.registerEvent("CLOSE_INVENTORY", this.closeInventory);
        this.props.input.registerEvent("CLOSE_CEF", this.closeInventory);


        this.onFilter = this.onFilter.bind(this);
    }

    loadSingleInventory(data) {
        this.setState({
            CurrentInventory: SingleInventory,
            data: data,
        });
    }

    loadDoubleInventory(data) {
        this.setState({
            CurrentInventory: DoubleInventory,
            data: data,
        });
    }

    closeInventory() {
        this.setState({
            CurrentInventory: null,
        });

        this.props.output.sendToServer("INVENTORY_CLOSED", this.state.data, false, "INVENTORY");
    }

    closeCef() {
        this.setState({
            CurrentInventory: null,
        });
    }

    onSendEvent(eventName, item, amount, invId, closeInv) {
        if(amount <= 0) {
            amount = 1;
        } else if(amount > item.amount) {
            amount = item.amount;
        }

        this.props.output.sendToServer(eventName, {item: JSON.stringify(item), amount: amount, invId: invId, giveItemTarget: this.state.data.giveItemTarget}, closeInv, "INVENTORY");
        if(closeInv) {
            this.setState({
                CurrentInventory: null,
            });
        }
    }

    onSendMoveEvent(item, amount, fromInvId, toInvId, giveItemTarget) {
        this.props.output.sendToServer("MOVE_ITEM", {item: JSON.stringify(item), amount: amount, fromInvId: fromInvId, toInvId: toInvId, giveItemTarget: giveItemTarget}, false);
    }

    onFilter(input) {
        this.setState({
            filter: input,
        })
    }

    render() {
        if(this.state.CurrentInventory !== null) {
            return (
                <div className="inventoryWrapper">
                    <this.state.CurrentInventory 
                        data={this.state.data} 
                        sendCallback={this.onSendEvent} 
                        moveCallback={this.onSendMoveEvent} 

                        //Filter
                        filter={this.state.filter}
                        onFilter={this.onFilter}/>
                </div>);
        } else {
            return null;
        }
    }
}

register(InventoryController);