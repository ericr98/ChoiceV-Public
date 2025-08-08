import React, { Fragment } from 'react';
import ItemContainer from './ItemContainer';

import { url } from './../../../index';

import './style/HealthScreen.css';


export default class HealthScreen extends React.Component {
    constructor(props) {
        super(props);

        this.getPainString = this.getPainString.bind(this);
    }

    getPainString(part) {
        var obj = JSON.parse(part);
        switch(obj.painLevel) {
            case 1:
                return (obj.multiple ? "Mehrere sehr leichte " : "Sehr leichte ") + obj.type + " im " + obj.name;
            case 2:
                return  (obj.multiple ? "Mehrere leichte " : "Leichte ") + obj.type + " im " + obj.name;
            case 3:
                return  (obj.multiple ? "Mehrere mittelstarke " : "Mittelstarke ") + obj.type + " im " + obj.name;
            case 4:
                return  (obj.multiple ? "Mehrere starke " : "Starke ") + obj.type + " im " + obj.name;
            case 5:
                return  (obj.multiple ? "Mehrere sehr starke " : "Sehr starke ") + obj.type + " im " + obj.name;
            case 6:
                return (obj.multiple ? "Mehrere unerträgliche " : "Unerträgliche ") + obj.type + " im " + obj.name;
            default:
                return "Melde dich bitte im Support. Code: PainBear";
        }
    }
d
    scalePainLevel(value) {
        var scale = (0.9 - 0.09) / (1 - 0);
        var capped = Math.min(1, Math.max(0, value)) - 0;
	    return (capped * scale + 0.09) * 100;
    }

    render() {
        return(
            <div className="healthScreen">
                <div className="healthComponent">
                    <div className="healthTitle">Verletzungen</div>
                    <div id="pain">
                        <div class="healthSubTitle">Schmerzlevel:</div>
                        <div class="standardWrapper">
                            <div id="painScale"></div>
                            <div id="painMeter" style={{left: this.scalePainLevel(this.props.painPercent) + "%"}}></div>
                        </div>
                        <div className="healthInfoComponent">{this.props.wastedPain}</div>
                    </div>
                    <div className="healthComponentDivider"></div>
                    <div class="healthSubTitle">Details:</div>
                    <div className="injuryDetails">
                        {this.props.parts.map((el) => {
                            return <HealthScreenInfoComponent info={this.getPainString(el)} />;
                        })}
                    </div>
                </div>
                <HealthScreenComponent title={"Schmerzen und Schmerzmittel"}/>
            </div>
        );
    }
}

class HealthScreenComponent extends React.Component {
    render() {
        return(
            <div className="healthComponent">
                <div className="healthTitle">{this.props.title}</div>
            </div>
        );
    }
}

class HealthScreenInfoComponent extends React.Component {
    render() {
        return(
            <div className="healthInfoComponent">{this.props.info}</div>
        );
    }
}