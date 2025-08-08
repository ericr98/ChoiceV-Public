import React from 'react';
import { register } from './../../App';
import './styles/TaximeterController.css';

import { url } from './../../index';

export default class TaximeterController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            open: false,
            startPrice: 0,
            costPer100Meter: 0,
            distance: null,
        }
        
        this.onStartTaxometer = this.onStartTaxometer.bind(this);
        this.props.input.registerEvent("START_TAXOMETER", this.onStartTaxometer);

        this.onUpdateTaxometer = this.onUpdateTaxometer.bind(this);
        this.props.input.registerEvent("UPDATE_TAXOMETER", this.onUpdateTaxometer);

        this.onStopTaxometer = this.onStopTaxometer.bind(this);
        this.props.input.registerEvent("STOP_TAXOMETER", this.onStopTaxometer);
    }

    onStartTaxometer(data) {
        this.setState({
            open: true,
            costPer100Meter: data.price,
            startPrice: data.rate,
            distance: data.distance,
        })
    }

    onStopTaxometer(data) {
        this.setState({
            open: false,
            costPer100Meter: 0,
            startPrice: 0,
            distance: 0,
        })
    }

    onUpdateTaxometer(data) {
        this.setState({
            distance: data.distance,
        })
    }


    render() {
        if(!this.state.open) {
            return null;
        } else {
            return (
                <div className="taximeter">
                   <div className="taximeterTop standardWrapper">
                        <div className="taximeterFareWrapper">
                            <div className="taximeterFareDisplayWrapper">
                                <div className="taximeterFareDisplay">
                                    <div className="taximeterFareDisplayText">
                                        {((this.state.distance * this.state.costPer100Meter) + this.state.startPrice).toFixed(2)}
                                    </div>
                                </div>
                            </div>
                            <div className="taximeterFareText">FAHRPREIS</div>
                        </div>
                   </div>
                   <div className="standardWrapper">
                        <div className="taximeterSmallWrapper">
                            <div className="taximeterSmallDisplayWrapper">
                                <div className="taximeterSmallDisplay">
                                    <div className="taximeterSmallDisplayText">
                                        {(this.state.distance / 10).toFixed(2)}
                                    </div>
                                </div>
                            </div>
                            <div className="taximeterSmallText">DISTANZ (KM)</div>
                        </div>
                    </div>
                    <div className="standardWrapper">
                        <div className="taximeterLogo">
                            <div className="standardWrapper">
                                <img src={url + "/taxi/taximeter.png"} className="taximeterLogoIcon"/>
                            </div>
                            <div className="standardWrapper">
                                <div className="">TaxoMAN</div>
                            </div>
                            <div className="standardWrapper">
                                <div className="">4534A</div>
                            </div>
                        </div>
                    </div>
                    <div className="standardWrapper">
                        <div className="taximeterSmallWrapper">
                            <div className="taximeterSmallDisplayWrapper">
                                <div className="taximeterSmallDisplay">
                                    <div className="taximeterSmallDisplayText">
                                        {this.state.costPer100Meter.toFixed(2)}
                                    </div>
                                </div>
                            </div>
                            <div className="taximeterSmallText">PREIS (100m)</div>
                        </div>
                    </div>
                </div>);
        }
    }
}

register(TaximeterController);