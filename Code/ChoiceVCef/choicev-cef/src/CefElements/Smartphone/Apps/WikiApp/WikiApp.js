import React, { Fragment } from 'react';

import { registerApp, registerCallback, Icon, ConfirmationType } from '../../SmartphoneController';
import { AllSettings } from '../SettingsApp/SettingsApp';

import './WikiApp.css';

import { url } from './../../../../index';


export default class WikiApp extends React.Component {
    constructor(props) {
        super(props);
    }
  
    static hasTime() {
        return false;
    }

    static stopsMovement() {
        return true;
    }

    static hasVerticalMode() {
        return true;
    }

    static getIcon(callback) {
        return <Icon key={"wiki"} icon="wiki" column={2} row={4} missedInfo={0} callback={callback} type={WikiApp} />
    }

    static dispose() {
        //DO NOTHING
    }

    static deselect() {
        
    }

    checkIfProceed() {
        var settings = this.props.requestState("settings");
        if(settings[AllSettings.FLY_MODE]) {
            this.props.requestConfirmation(ConfirmationType.YES_NO, "Flugmodus deaktivieren um Konten zu laden?", this.onConfirmFlyMode.bind(this), null);
            return false;
        } else {
            return true;
        }
    }

    onConfirmFlyMode() {
        this.props.changeSetting(AllSettings.FLY_MODE, false);
    }

    render() {
        if(!this.checkIfProceed()) {
            return (<div className='standardAppBackground fullWrapper'></div>);
        } else {
            return (<iframe loading="lazy" style={{width: "100%", height: "100%"}} frameBorder={0} src='https://choicev.net/wiki/index.php'></iframe>)
        }
    }
}

registerApp(WikiApp, 0);