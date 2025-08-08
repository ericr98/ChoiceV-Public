import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon } from '../../SmartphoneController';
import { formatPhoneNumber } from '../CallApp/CallApp';

import './SettingsApp.css';

import { url } from './../../../../index';

export var AllSettings = {
    VOLUME: "volume",
    SILENT_MODE: "silent",
    FLY_MODE: "flyMode",
    HIDDEN_NUMBER: "hiddenNumber",
    SOCIAL_MEDIA_HORIZONTAL_START: "socialMediaHorizontalStart",
}

export default class SettingsApp extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            
        }

        this.onChangeSetting = this.onChangeSetting.bind(this);
    }
  
    static hasTime() {
        return true;
    }

    static stopsMovement() {
        return false;
    }

    static hasVerticalMode() {
        return false;
    }

    static getIcon(callback) {
        return <Icon key={"setup"} icon="setup" column={4} row={1} missedInfo={0} callback={callback} type={SettingsApp} />
    }

    static dispose() {
        //DO NOTHING
    }

    static deselect() { }

    onChangeSetting(type, name, value) {
        this.props.changeSetting(name, value);
    }

    render() {
        var settings = this.props.requestState("settings");
        var number = this.props.requestState("number");

        return (
            <div className="phoneSettingsWorkplace standardAppBackground">
                <div className='phoneSettingsHeading'>Allgemein</div>
                <SettingsTab title="Meine Handynummer" settingsElement={<SettingsInfoElement info={formatPhoneNumber(number.toString())} />} />
                <SettingsTab title="Netzanbieter" settingsElement={<SettingsInfoElement info="CV Telecommunications" />} />
                <SettingsTab title="Guthaben" settingsElement={<SettingsInfoElement info="Flatrate" />} />
                <SettingsTab title="Laufstärke" settingsElement={<SettingsSliderElement value={settings[AllSettings.VOLUME]} name={AllSettings.VOLUME} changeSettings={this.onChangeSetting} />} />
                <SettingsTab title="Vibration" settingsElement={<SettingsCheckboxElement checked={settings[AllSettings.SILENT_MODE]} name={AllSettings.SILENT_MODE} changeSettings={this.onChangeSetting} />} />
                <SettingsTab title="Flugmodus" settingsElement={<SettingsCheckboxElement checked={settings[AllSettings.FLY_MODE]} name={AllSettings.FLY_MODE} changeSettings={this.onChangeSetting} />} />
                <SettingsTab title="Nummer unterdrücken" settingsElement={<SettingsCheckboxElement checked={settings[AllSettings.HIDDEN_NUMBER]} name={AllSettings.HIDDEN_NUMBER} changeSettings={this.onChangeSetting} />} />
                
                <div className='phoneSettingsHeading'>Social Media</div>
                <SettingsTab title="Direkt im Querformat starten" settingsElement={<SettingsCheckboxElement checked={settings[AllSettings.SOCIAL_MEDIA_HORIZONTAL_START]} name={AllSettings.SOCIAL_MEDIA_HORIZONTAL_START} changeSettings={this.onChangeSetting} />} />
            
            </div>);
    }
}

class SettingsTab extends React.Component {
    render() {
        return(
            <div className="phoneSettingsTab">
                <div className="phoneSettingsTitle">{this.props.title}</div>
                {this.props.settingsElement}
            </div>
        );
    }
}

class SettingsInfoElement extends React.Component {
    render() {
        return (
            <div className="phoneSettingsInfoElement">{this.props.info}</div>
        );
    }
}

class SettingsSliderElement extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            value: props.value,
        }

        this.onChange = this.onChange.bind(this);
    }

    onChange(evt) {
        this.setState({
            value: evt.target.value,
        })

        this.props.changeSettings("slider", this.props.name, evt.target.value);
    }

    render() {
        return (
            <input className="phoneSettingsSliderElement" type="range" min="1" max="10" value={this.state.value} onChange={this.onChange}/>
        );
    }
}

class SettingsCheckboxElement extends React.Component {
    constructor(props) {
        super(props);

        this.state= {
            checked: props.checked,
        }

        this.onValueChange = this.onValueChange.bind(this);
    }

    onValueChange(evt) {
        this.setState({
            value: evt.target.checked,
        });

        
        this.props.changeSettings("checkbox", this.props.name, evt.target.checked);
    }

    render() {
        return (
            <input className="phoneSettingsCheckBoxElement" type="checkbox" defaultChecked={this.state.checked} value={this.state.checked} onChange={this.onValueChange} />
        );
    }
}

registerApp(SettingsApp, 0);