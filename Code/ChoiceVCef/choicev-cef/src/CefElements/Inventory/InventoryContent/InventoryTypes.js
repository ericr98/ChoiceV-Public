import React, { Fragment } from 'react';
import  { Inventory } from './Inventory.js';
import { SingleInventorySideBar } from './InventorySideBar';
import './style/Inventory.css';
import  { Item } from './Item';

import { url } from './../../../index';

export function objectItemSimilar(obj, item) {
    return obj.name == item.name && obj.description == item.description && obj.configId == item.configId && obj.quality == item.quality && obj.weight == item.weight && obj.isEquipped == item.isEquipped && obj.additionalInfo == item.additionalInfo
}

//Items still json!
const weightFolder = (accumulator, currentValue) => {
    if(accumulator.weight) { 
        return accumulator.weight * (accumulator.amount === 0 ? 1 : accumulator.amount) + currentValue.weight * (currentValue.amount === 0 ? 1 : currentValue.amount);
    } else {
        return accumulator + currentValue.weight * (currentValue.amount === 0 ? 1 : currentValue.amount);
    }
};

function calcInvWeight(inv) {
    if(inv.length == 0) {
        return 0;
    } else if(inv.length == 1) {
        return inv[0].amount * inv[0].weight;
    } else {
        var val = 0;
        if(inv == NaN) {
            return val;
        }
        inv.forEach((el) => {
            if(el != NaN) {
                val += el.weight * (el.amount === 0 ? 1 : el.amount);
            }
        });

        return Math.round(val * 100) / 100;
    }
}

export class SingleInventory extends React.Component {
    constructor(props) {
        super(props);

        var items = this.props.data.items.map((el) => {
            var json = JSON.parse(el);
            return new Item(json.configId, json.name, json.quality, json.category, json.description, json.amount, json.weight, json.useable, json.equipSlot, json.isEquipped, -1, json.additionalInfo);
        });

        this.state = {
            items: items,
        }

        this.onChangeItems = this.onChangeItems.bind(this);
        this.changeItemAmount = this.changeItemAmount.bind(this);
    }

    onChangeItems(newList) {
        this.setState({
            items: newList,
        })
    }

    changeItemAmount(item, amount) {
        if(amount <= 0) {
            amount = 1;
        }

        var arr;
        if(amount >= item.amount) {
            arr = this.state.items.filter((el) => { 
                return !objectItemSimilar(item, el);
            });

            this.setState({
                items: arr,
            });
        } else {
            this.state.items.forEach((el) => {
                if(objectItemSimilar(el, item)) {
                    el.amount -= amount;
                    return;
                }
            });

            this.forceUpdate();
        }
    }

    onAddItem(evt) {
        //Ignore
    }
    
    decideIfSideBar() {
        if(!this.props.data.onlyStatic) { 
            return <SingleInventorySideBar items={this.state.items} giveVisible={this.props.data.giveItemTarget != -1} />
        } else {
            return null;
        }
    }

    render() {
        return (
          <div style={{ height: "100%" }}>
            <Inventory
              items={this.state.items}
              id={this.props.data.id}
              maxWeight={this.props.data.maxWeight}
              currentWeight={calcInvWeight(this.state.items)}
              modeChangeDisabled={false}
              changeItems={this.onChangeItems}
              className="inventory"
              callback={this.onAddItem}
              sendCallback={this.props.sendCallback}

              changeItemAmount={this.changeItemAmount}

              //MiddleBarInfo
              cash={this.props.data.cash}
              duty={this.props.data.duty}
              info={this.props.data.info}

              single={true}

              //HealthData
              painPercent={this.props.data.painPercent}
              parts={this.props.data.parts}
              wastedPain={this.props.data.wastedPain}
              hasSearchbar={true}

              //Filter
              filter={this.props.filter}
              onFilter={this.props.onFilter}
            />
            {this.decideIfSideBar()}
          </div>
        );
    }
}

export class DoubleInventory extends React.Component {
    constructor(props) {
        super(props);

        var itemsLeft = this.props.data.itemsLeft.map((el) => {
            var json = JSON.parse(el);
            return new Item(json.configId, json.name, json.quality, json.category, json.description, json.amount, json.weight, json.useable, json.equipSlot, json.isEquipped, props.data.idLeft, json.additionalInfo);
        });

        var itemsRight = this.props.data.itemsRight.map((el) => {
            var json = JSON.parse(el);
            return new Item(json.configId, json.name, json.quality, json.category, json.description, json.amount, json.weight, json.useable, json.equipSlot, json.isEquipped, props.data.idRight, json.additionalInfo);
        });

        this.state = {
            itemsLeft: itemsLeft,
            itemsRight: itemsRight,
        }
        
        this.onChangeLeft = this.onChangeLeft.bind(this);
        this.onChangeRight = this.onChangeRight.bind(this);

        this.onAddItem = this.onAddItem.bind(this);
    }

    onChangeLeft(newItems) {
        //Do like this because the clone is a reference clone!
        var newList = [];
        newItems.forEach((el) => {
            newList.push(Object.assign({}, el));
        });

        this.state.itemsLeft = newList;
    }

    
    onChangeRight(newItems) {
        //Do like this because the clone is a reference clone!
        var newList = [];
        newItems.forEach((el) => {
            newList.push(Object.assign({}, el));
        });

        this.state.itemsRight = newList;
    }

    onAddItem(evt, toInvIsLeft) {
        //Item dropped over list, or nothing
        if(evt.to == evt.from) {
            return;
        }

        var fromInvId;
        var toInvId;
        var oldItem;
        var newItem = null;
        //The the new and old item in the list
        if(toInvIsLeft) {
            fromInvId = this.props.data.idRight;
            toInvId = this.props.data.idLeft;
            var json = JSON.parse(evt.item.getAttribute('item'));
            oldItem = this.state.itemsRight.filter((el) => {
                return objectItemSimilar(json, el);
            })[0];

            newItem = this.state.itemsLeft.filter((el) => {
                return el.currentInvId === fromInvId;
            })[0];

            //console.log(newItem)
        } else {
            fromInvId = this.props.data.idLeft;
            toInvId = this.props.data.idRight;

            var json = JSON.parse(evt.item.getAttribute('item'));
            oldItem = this.state.itemsLeft.filter((el) => {
                return objectItemSimilar(json, el);
            })[0];
            newItem = this.state.itemsRight.filter((el) => {
                return el.currentInvId === fromInvId;
            })[0];
        }

        var amount = Number(evt.item.children[4].value);
        if(amount <= 0) {
            amount = 1;
        } else if(amount > oldItem.amount) {
            amount = oldItem.amount;
        }

        if(toInvIsLeft) {
            //calculate current weight, with element already inside list!
            if(calcInvWeight(this.state.itemsLeft) - newItem.weight * (newItem.amount - amount) > this.props.data.maxWeightLeft) {
                this.setState({
                    itemsLeft: this.state.itemsLeft.filter((el) => { return el !== newItem }),
                });
                return;
            }
        } else {
            // console.log("calcInvWeight1 " + calcInvWeight(this.state.itemsRight))
            // console.log("calcInvWeight2 " + newItem.weight * (newItem.amount - amount))
            // console.log("maxWeightRight " + this.props.data.maxWeightRight)
            //calculate current weight, with element already inside list!
            if(calcInvWeight(this.state.itemsRight) - newItem.weight * (newItem.amount - amount) > this.props.data.maxWeightRight) {
                this.setState({
                    itemsRight: this.state.itemsRight.filter((el) => {return el !== newItem }),
                });
                return;
            }
        }

        //Item All Items are moved
        if(amount == oldItem.amount) {
            newItem.amount = amount;
            if(toInvIsLeft) { 
                this.setState({
                    itemsRight: this.state.itemsRight.filter((el) => {return !objectItemSimilar(el, oldItem)}),
                });
            } else {
                this.setState({
                    itemsLeft: this.state.itemsLeft.filter((el) => {return !objectItemSimilar(el, oldItem)}),
                });
            }
            
        //Only some items are moved
        } else {
            oldItem.amount = oldItem.amount - amount;
            newItem.amount = amount;
        }

        newItem.currentInvId = toInvId;

        const folder = (accumulator, currentValue) => { if(accumulator.amount) { return accumulator.amount + currentValue.amount } else { return accumulator + currentValue.amount }}
        if(toInvIsLeft) {
            var dupes = this.state.itemsLeft.filter((el) => {
                return objectItemSimilar(el, newItem);
            });

            if(dupes.length > 1) {
                var newAmount = dupes.reduce(folder);
                //filter duplicates and set amount of new item
                this.setState({
                    itemsLeft: this.state.itemsLeft.filter((el) => {
                        if(el == newItem) {
                            el.amount = newAmount;
                        }
                        return !(dupes.includes(el) && el != newItem);
                    })
                })

            }
        } else {
            var dupes = this.state.itemsRight.filter((el) => {
                return objectItemSimilar(el, newItem);
            });

            if(dupes.length > 1) {
                var newAmount = dupes.reduce(folder);
                //filter duplicates and set amount of new item
                this.setState({
                    itemsRight: this.state.itemsRight.filter((el) => {
                        if(el == newItem) {
                            el.amount = newAmount;
                        }
                        return !(dupes.includes(el) && el != newItem);
                    })
                })
            }
        }

        this.props.moveCallback(newItem, amount, fromInvId, toInvId);
        this.forceUpdate(); 
    }

    render() {
        return (
          <div style={{ height: "100%" }}>
            <Inventory
              items={this.state.itemsLeft}
              id={this.props.data.idLeft}
              maxWeight={this.props.data.maxWeightLeft}
              currentWeight={calcInvWeight(this.state.itemsLeft)}
              modeChangeDisabled={true}
              className="inventory left"
              changeItems={this.onChangeLeft}
              callback={this.onAddItem}
              right={false}
              
              //Middle Bar Info
              cash={this.props.data.cash}
              duty={this.props.data.duty}
              info={this.props.data.info}

              name={this.props.data.leftName}
              
              //Filter
              filter={this.props.filter}
              onFilter={this.props.onFilter}
            />
            <Inventory
              items={this.state.itemsRight}
              id={this.props.data.idRight}
              maxWeight={this.props.data.maxWeightRight}
              currentWeight={calcInvWeight(this.state.itemsRight)}
              odeChangeDisabled={true}
              className="inventory right"
              changeItems={this.onChangeRight}
              callback={this.onAddItem}
              right={true}

              //Middle Bar Info
              cash={this.props.data.cash}
              duty={this.props.data.duty}
              info={this.props.data.info}

              name={this.props.data.rightName}
              searchBar={this.props.data.showRightSearchbar}
            />
          </div>
        );
    }
}