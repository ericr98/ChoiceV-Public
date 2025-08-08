import React from 'react';
import './style/ItemContainer.css';
import InventoryItem from './Item';

import { ReactSortable } from "react-sortablejs";
import { setCallback, onDrag, onDrop } from './InventorySideBar';
import { objectItemSimilar } from './InventoryTypes';

export default class ItemContainer extends React.Component {
    constructor(props) {
        super(props);

        this.onEnd = this.onEnd.bind(this);
        this.onDropEvent = this.onDropEvent.bind(this);

        setCallback(this.onDropEvent);

        this.filterPredicate = this.filterPredicate.bind(this);
    }

    onEnd(evt) {
        onDrop();
        this.props.callback(evt);
    }

    onDropEvent(event, item, amount, closeInv) {
        if(event == "GIVE_ITEM") {
            this.props.changeItemAmount(item, amount);
        }

        this.props.sendCallback(event, item, amount, this.props.inv.id, closeInv);
    }

    render() {
        return (
            <div className="inventoryContainer">
            <ReactSortable 
                list={this.props.items} 
                setList={this.props.changeItems} 
                onChange={this.onChange}
                chosenClass="chosen"
                group={{name: "inventory", pull:"clone"}}
                animation={100}
                forceFallback={true}
                sort={false}
                onStart={onDrag}
                onEnd={this.onEnd}
                onClone={this.onClone}
                //onAdd={this.onAdd}
                handle=".handlePart"
                filter= {this.props.single ? "" : ".equipped"}
                className="fullContainer"
            >
                {this.props.items.map((el) => {
                    if(this.filterPredicate(el)) {
                        return (<InventoryItem item={el} />);
                    } else {
                        return (<div></div>);
                    }
                })}
            </ReactSortable>
        </div>);
    }

    filterPredicate(item) {
        if(this.props.filter == "" || !this.props.hasFilter) {
            return true;
        } else {
            return item.name.toLowerCase().includes(this.props.filter.toLowerCase()) || item.description.toLowerCase().includes(this.props.filter.toLowerCase()) || (item.additionalInfo != null && item.additionalInfo.toLowerCase().includes(this.props.filter.toLowerCase()));
        }
    }
}