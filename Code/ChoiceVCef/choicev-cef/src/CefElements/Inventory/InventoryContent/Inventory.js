import React, { Fragment } from 'react';
import ItemContainer from './ItemContainer';
import HealthScreen from './HealthScreen';
import { url } from './../../../index';

export class Inventory extends React.Component {
    constructor(props) {
        super(props);


        this.id = props.id;
        this.maxWeight = props.maxWeight;
        this.state = {
            mode: "items",
            filter: props.filter,
            ownFilter: "",
        };

        this.onItemsChange = this.onItemsChange.bind(this);
        this.onChangeMode = this.onChangeMode.bind(this);

        this.onAddItem = this.onAddItem.bind(this);

        this.onFilter = this.onFilter.bind(this);
    }

    onItemsChange(newState) {
        this.props.items = newState;
    }

    onChangeMode(newMode) {
        this.setState({
            mode: newMode,
        });
    }

    onAddItem(evt) {
        this.props.callback(evt, this.props.right);
    }

    getDisplayedMode() {
        if(this.props.modeChangeDisabled) {
            return (<ItemContainer items={this.props.items} inv={this} changeItems={this.props.changeItems} callback={this.onAddItem} single={this.props.single} filter={this.props.filter??this.state.ownFilter} hasFilter={true} />);
        }

        switch(this.state.mode) {
            case "items":
                return (<ItemContainer items={this.props.items} inv={this} changeItems={this.props.changeItems} changeItemAmount={this.props.changeItemAmount} callback={this.onAddItem} sendCallback={this.props.sendCallback} single={this.props.single} filter={this.props.filter??this.state.ownFilter} hasFilter={this.props.searchBar || this.props.hasSearchbar}/>);
            case "health":
                return (<HealthScreen painPercent={this.props.painPercent} wastedPain={this.props.wastedPain} parts={this.props.parts} />);
            case "character":
                return null
        }
    }

    onFilter(input) {
        this.setState({
            ownFilter: input,
        })
    }

    render() {
        return (
            <div className={this.props.className}>
              <InventoryTopBar name={this.props.name} changeMode={this.onChangeMode} currentWeight={this.props.currentWeight} maxWeight={this.props.maxWeight}/>
              <InventoryMiddleBar cash={this.props.cash} duty={this.props.duty} info={this.props.info} searchBar={this.props.searchBar} filter={this.props.filter??this.state.ownFilter} onFilter={this.props.onFilter??this.onFilter}/>
              {this.getDisplayedMode(this.state.mode)}
            </div>
        );
    }
}

class InventoryMiddleBar extends React.Component {
    constructor(props) {
        super(props);

        this.onChange = this.onChange.bind(this);
    }

    onChange(evt) {
        this.props.onFilter(evt.target.value);
    }

    render() {
        if(this.props.searchBar) {
            return (
                <div className="middleBar">
                    <div className="middleBarSearch">
                        <input id={"input" + this.id} className="middleBarSearchInput" type="search" placeholder={"Filter nach Namen"} onChange={this.onChange} spellCheck={false} value={this.props.filter} />
                    </div>
                </div>)
        } else {
            return(
                <div className="middleBar">
                    <div className="middleBarContainer">
                        <InventoryMiddleBarElement info={"Bargeld: $" + (this.props.cash === undefined ? "" : this.props.cash)} extraClass="borderRight"/>
                        <InventoryMiddleBarElement info={"Letzte Info: " + (this.props.info === undefined ? "" : this.props.info)} extraClass="borderRight"/>
                        <input id={"input" + this.id} className="middleBarElement middleBarSingleInventoryInput" type="search" placeholder={"Filter nach Details"} onChange={this.onChange} spellCheck={false} value={this.props.filter} />
                        {/* <InventoryMiddleBarElement info={"Dienst: " + (this.props.duty === undefined ? "" : this.props.duty)} extraClass="borderRight"/> */}
                    </div>
                </div>);
        }
    }
}
class InventoryMiddleBarElement extends React.Component {
    render() {
        return <div className={"middleBarElement " + this.props.extraClass}>{this.props.info}</div>
    }
}

class InventoryTopBar extends React.Component {
    render() {
        if(this.props.name == undefined || this.props.name == "") {
            return(
                <div className="topBar">
                    <div className="buttonBar">
                        <InventoryModeButton mode="health" imgName="health.svg" changeMode={this.props.changeMode}/>
                        <InventoryModeButton mode="items" imgName="inventory.svg" changeMode={this.props.changeMode}/>
                        <InventoryModeButton mode="character" imgName="character.svg" changeMode={this.props.changeMode}/>
                    </div>
                    <div className="divider"/>
                    <InventoryWeightDisplay imgName="weight.svg" currentWeight={this.props.currentWeight} maxWeight={this.props.maxWeight} />
                </div>
            );
        } else {
            return (
                <div className="topBar">
                    <div className="topBarName">
                        <div className="topBarNameText">{this.props.name}</div>
                    </div>
                    <div className="divider"/>
                    <InventoryWeightDisplay imgName="weight.svg" currentWeight={this.props.currentWeight} maxWeight={this.props.maxWeight} />
                </div>
            );
        }
    }
}

class InventoryModeButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(e) {
        this.props.changeMode(this.props.mode);
    }

    render() {
        var style ={
            background: "transparent",
            backgroundImage: "url(" + url + "inventory/" + this.props.imgName + ")",
            backgroundRepeat  : 'no-repeat',
            backgroundPosition: 'center',
            backgroundSize: "100% 100%",
        }
        
        return(
            <div className="buttonBarGridElement">
                <button className="topBarButton" style={style} onClick={this.onClick} />
            </div>);
    }
}

class InventoryWeightDisplay extends React.Component {
    render() {
        return (
            <div className="weightBar">
                <img className="weightIcon" src={url  + "inventory/" + this.props.imgName}/>
                <div className="weightDisplay">
                    <div>{this.props.currentWeight.toFixed(2) + " kg"}</div>
                    <div className="maxWeight">{"von " + this.props.maxWeight.toFixed(2) + " kg"}</div>
                </div>
            </div>
        );
    }
}