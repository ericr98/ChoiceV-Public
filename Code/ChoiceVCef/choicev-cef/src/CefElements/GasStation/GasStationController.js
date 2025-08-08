import React from 'react';
import { url } from './../../index';
import { register } from '../../App';
import './style/GasStation.css'

export default class GasStationController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            type: null,

            fuel: 0.0,
            fuelmax: 0.0,
            fuelname: "Benzin",
            fuelprice: 0.0,

            showcash: false,
            showbank: false,
            showcomp: false,

            togglecash: false,
            togglebank: false,
            togglecomp: false,

            fuelammount: 0,
            fuelaccount: "cash",
        }

        this.interval_id = 0;

        this.onStopRefill = this.onStopRefill.bind(this);
        this.onStartRefill = this.onStartRefill.bind(this);
        this.onCreateGasStation = this.onCreateGasStation.bind(this);
        this.onCloseGasStation = this.onCloseGasStation.bind(this);

        this.props.input.registerEvent("CREATE_GASSTATION", this.onCreateGasStation);
        this.props.input.registerEvent("CLOSE_GASSTATION", this.onCloseGasStation);
        this.props.input.registerEvent("CLOSE_CEF", this.onCloseGasStation);

        this.onClickCash = this.onClickCash.bind(this);
        this.onClickBank = this.onClickBank.bind(this);
        this.onClickComp = this.onClickComp.bind(this);
    }
    
    onStartRefill() {
        var cashtype = document.getElementById("idStationCashType")
        var cash = document.getElementById("idStationCash")
        var fuel = document.getElementById("idStationFuel")
        var level = document.getElementById("idStationFuelLevel");
        
        if (cashtype != null && cash != null && fuel != null && level != null) {
            cashtype.innerHTML = "";

            var data = { Action: "Started", Account: this.state.fuelaccount, FuelPrice: this.state.fuelprice, FuelAmmount: this.state.fuelammount, FuelName: this.state.fuelname }
            this.props.output.sendToServer("GASSTATION_EVENT", data, false);

            if (this.state.fuelname != "Elektrizität")
                this.interval_id = setInterval(doRefill, 75, this);
            else
                this.interval_id = setInterval(doRefill, 150, this);

            function doRefill(parent) {
                if ((parent.state.fuel + parent.state.fuelammount + 0.1) >= parent.state.fuelmax) {
                    if (parent.interval_id > 0)
                        clearInterval(parent.interval_id);

                    parent.state.fuelammount = (parent.state.fuelmax - parent.state.fuel);

                    cash.innerHTML = (parent.state.fuelammount * parent.state.fuelprice).toFixed(2)
                    fuel.innerHTML = parent.state.fuelammount.toFixed(2);
                    level.style.width = "100%";

                    var data = { Action: "Finished", Account: parent.state.fuelaccount, FuelPrice: parent.state.fuelprice, FuelAmmount: parent.state.fuelammount, FuelName: parent.state.fuelname }
                    parent.props.output.sendToServer("GASSTATION_EVENT", data, true, "GAS_REFUEL");

                    parent.setState({
                        type: null,
                        
                        fuel: 0.0,
                        fuelmax: 0.0,
                        fuelname: "Benzin",
                        fuelprice: 0.0,
            
                        showcash: false,
                        showbank: false,
                        showcomp: false,
            
                        togglecash: false,
                        togglebank: false,
                        togglecomp: false,
            
                        fuelammount: 0,
                        fuelaccount: "cash",
                    });
                } else {
                    parent.state.fuelammount += 0.075; //(parent.state.fuelmax / 800);

                    cash.innerHTML = (parent.state.fuelammount * parent.state.fuelprice).toFixed(2)
                    fuel.innerHTML = parent.state.fuelammount.toFixed(2);
                    level.style.width = ((100 / parent.state.fuelmax) * (parent.state.fuel + parent.state.fuelammount)) + "%";
                }
            }
        }
    } 

    onStopRefill() {
        if (this.interval_id > 0)
            clearInterval(this.interval_id);

        var data = { Action: "Stopped", Account: this.state.fuelaccount, FuelPrice: this.state.fuelprice, FuelAmmount: this.state.fuelammount, FuelName: this.state.fuelname }
        this.props.output.sendToServer("GASSTATION_EVENT", data, true, "GAS_REFUEL");

        this.state.fuel += this.state.fuelammount;
        this.state.fuelammount = 0;

        this.setState({
            togglecash: false,
            togglebank: false,
            togglecomp: false,
        });
} 

    onCreateGasStation(data) {
        this.setState({
            type: data.StationType,

            fuel: data.Fuel,
            fuelmax: data.FuelMax,
            fuelname: data.FuelName,
            fuelprice: data.FuelPrice,

            showcash: data.ShowCash,
            showbank: data.ShowBank,
            showcomp: data.ShowComp,

            togglecash: false,
            togglebank: false,
            togglecomp: false,

            fuelammount: 0,
            fuelaccount: "cash",
        });
    }

    onCloseGasStation() {
        if(this.state.type != null) {
            var data = { Action: "Closed", Account: this.state.fuelaccount, FuelPrice: this.state.fuelprice, FuelAmmount: this.state.fuelammount, FuelName: this.state.fuelname }
            this.props.output.sendToServer("GASSTATION_EVENT", data, true, "GAS_REFUEL");
        
            this.setState({
                type: null,
                
                fuel: 0.0,
                fuelmax: 0.0,
                fuelname: "Benzin",
                fuelprice: 0.0,

                showcash: false,
                showbank: false,
                showcomp: false,

                togglecash: false,
                togglebank: false,
                togglecomp: false,

                fuelammount: 0,
                fuelaccount: "cash",
            });
        }
    }

    onClickCash(evt) {
        this.state.fuelaccount = "cash"

        if (this.state.togglecash == true) {
            this.onStopRefill();
            this.setState({togglecash: false});
        } else {
            this.setState({togglecash: true});
            this.onStartRefill();
        }
    }

    onClickBank(evt) {
        this.state.fuelaccount = "bank"

        if (this.state.togglebank == true) {
            this.onStopRefill();
            this.setState({togglebank: false});
        } else {
            this.setState({togglebank: true});
            this.onStartRefill();
        }
    }

    onClickComp(evt) {
        this.state.fuelaccount = "company"

        if (this.state.togglecomp == true) {
            this.onStopRefill();
            this.setState({togglecomp: false});
        } else {
            this.setState({togglecomp: true});
            this.onStartRefill();
        }
    }
    
    render() {
        if (this.state.type == null) {
            return null;
        
        } else {
            return (
                <div className="noSelect gasStationWrapper">
                    <div className="gasStationFrame">
                        <img className="gasStationImage" src={url + "gasstation/company_" + this.state.type + ".png"} />

                        <div id = "idStationCashType" className="gasStationCashType">Volltanken =</div>
                        <div id = "idStationCash" className="gasStationCash">{((this.state.fuelmax - this.state.fuel) * this.state.fuelprice).toFixed(2)}</div>
                        
                        <div className="gasStationFuelName">{this.state.fuelname}</div>
                        <div id="idStationFuel" className="gasStationFuel">{(this.state.fuelmax - this.state.fuel).toFixed(2)}</div>

                        <div className="gasStationFuelLevelFrame">
                            <div id="idStationFuelLevel" className="gasStationFuelLevel" style={{width: ((100 / this.state.fuelmax) * this.state.fuel)  + "%", backgroundImage: "url(" + url + "gasstation/filler.png"}}></div>
                        </div>

                        {(this.state.showcash && !this.state.togglebank && !this.state.togglecomp) ?
                            <div className="gasStationButtonCash gasStationButton" onClick={this.onClickCash}>
                                <img src={url + "gasstation/btn_" + this.state.type + "_bar_" + (this.state.togglecash ? "neg" : "pos") + ".png"} />
                            </div>
                            :
                            <div className="gasStationButtonCash gasStationButton">
                                <img src={url + "gasstation/btn_" + this.state.type + "_bar_pos_20.png"} />
                            </div>
                        }
                        
                        {(this.state.showbank && !this.state.togglecash && !this.state.togglecomp) ?
                            <div className="gasStationButtonBank gasStationButton" onClick={this.onClickBank}>
                                <img src={url + "gasstation/btn_" + this.state.type + "_kon_" + (this.state.togglebank ? "neg" : "pos") + ".png"} />
                            </div>
                            :
                            <div className="gasStationButtonBank gasStationButton">
                                <img src={url + "gasstation/btn_" + this.state.type + "_kon_pos_20.png"} />
                            </div>
                        }
                        
                        {(this.state.showcomp && !this.state.togglecash && !this.state.togglebank) ?
                            <div className="gasStationButtonComp gasStationButton" onClick={this.onClickComp}>
                                <img src={url + "gasstation/btn_" + this.state.type + "_unt_" + (this.state.togglecomp ? "neg" : "pos") + ".png"} />
                            </div>
                            :
                            <div className="gasStationButtonComp gasStationButton">
                                <img src={url + "gasstation/btn_" + this.state.type + "_unt_pos_20.png"} />
                            </div>
                        }
                    </div>
                </div>);
        }
    }
}

register(GasStationController);