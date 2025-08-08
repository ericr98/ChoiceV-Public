import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon } from '../../SmartphoneController';

import './DesignSettingsApp.css';

import { url } from './../../../../index';

export default class DesignSettingsApp extends React.Component {

    static BackgroundList = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22];
    static RingtoneList = [1, 2, 3, 4, 5];

    constructor(props) {
        super(props);

        this.state = {
            playedSound: null,
            ringtoneAudio: null,
        }

        this.onPlaySound = this.onPlaySound.bind(this);
        this.onStopSound = this.onStopSound.bind(this);

        this.onChangeBackground = this.onChangeBackground.bind(this);
        this.onChangeRingtone = this.onChangeRingtone.bind(this);
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
        return <Icon key={"design"} icon="design" column={3} row={1} missedInfo={0} callback={callback} type={DesignSettingsApp} />
    }

    static dispose() {
        // DO NOTHING
    }

    static deselect() { }

    onPlaySound(number) {
        this.onStopSound();
        var player = new Audio(url + "/phone/ringtones/Ringtone_" + number + ".mp3");
        player.load();
        player.volume = 0.25;
        player.loop = false
        player.play();
        player.addEventListener('ended', () => {
            this.onStopSound();
        });

        this.setState({
            ringtoneAudio: player,
            playedSound: number,
        })
    }

    onStopSound() {
        if(this.state.ringtoneAudio != null) {
            this.state.ringtoneAudio.pause();
        }

        this.setState({
            ringtoneAudio: null,
            playedSound: null,
        });
    }

    onChangeBackground(number) {
        this.props.changeState("background", number);

        this.sendDesignUpdate();
    }

    onChangeRingtone(number) {
        this.props.changeState("ringtone", number);

        this.sendDesignUpdate();
    }

    sendDesignUpdate() {
        this.props.sendToServer("SMARTPHONE_DESIGN_CHANGE", {
            backgroundId: this.props.requestState("background"),
            ringtoneId: this.props.requestState("ringtone"),
        }, false);
    }

    render() {
        return (
            <div className="phoneDesignSettingsWorkplace standardAppBackground noSelect">
                <div className="phoneDesignSettingsBackgroundsSelector">
                    <div className="phoneDesignSettingsTitle">Hintergründe</div>
                    <div className="phoneDesignSettingsListWrapper">
                        <div className="phoneDesignSettingsList">
                            <DesignBackgroundSelectorList changeBackground={this.onChangeBackground} />
                        </div>
                    </div>
                </div>
                <div className="phoneDesignSettingsRingtoneSelector">
                    <div className="phoneDesignSettingsTitle">Klingeltöne</div>
                    <div className="phoneDesignSettingsListWrapper">
                        <div className="phoneDesignSettingsList">
                            <DesignRingtoneList playedSound={this.state.playedSound} playSound={this.onPlaySound} stopSound={this.onStopSound} changeRingtone={this.onChangeRingtone} />
                        </div>
                    </div>
                </div>
            </div>);
    }
}

var backgroundId = 0;
class DesignBackgroundSelectorList extends React.Component {
    render() {
        return DesignSettingsApp.BackgroundList.map((el) => {
            backgroundId++;
            return <DesignBackgroundSelectorElement key={backgroundId} el={el} changeBackground={this.props.changeBackground} />
        });
    }
}

class DesignBackgroundSelectorElement extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.changeBackground(this.props.el);    
    }

    render() {
        return (
            <div className="phoneDesignSettingsBackgroundsSelectorWrapper standardWrapper">
                <div className="phoneDesignSettingsBackgroundsCropper standardWrapper">
                    <img className="phoneDesignSettingsBackgroundElement" src={url + "phone/backgrounds/Background_" + this.props.el + ".png"} onClick={this.onClick} />
                </div>
            </div>
        );
    }
}

var ringtoneId = 0;

class DesignRingtoneList extends React.Component {
    render() {
        return DesignSettingsApp.RingtoneList.map((el) => {
            ringtoneId++;
            return <DesignRingtoneSelectorElement key={ringtoneId} el={el} playedSound={this.props.playedSound} playSound={this.props.playSound} stopSound={this.props.stopSound} changeRingtone={this.props.changeRingtone} />
        });
    }
}

class DesignRingtoneSelectorElement extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        if(this.props.playedSound == this.props.el) {
            this.props.stopSound();
        } else {
            this.props.playSound(this.props.el);
            this.props.changeRingtone(this.props.el); 
        }
    }

    render() {
        return (
            <div className="phoneDesignSettingsBackgroundsSelectorWrapper standardWrapper">
                <div className="phoneDesignSettingsBackgroundsCropper standardWrapper">
                    <div className="phoneDesignSettingsRingtoneElement">{this.props.el}</div>
                    <div className="phoneDesignSettingsRingtoneGrid">
                        <div className="phoneDesignSettingsRingtoneNote standardWrapper">♫</div>
                        <div className="phoneDesignSettingsRingtoneNumber standardWrapper">{"#" + this.props.el}</div>
                        <div className="phoneDesignSettingsRingtonePlayPause standardWrapper" onClick={this.onClick}>{this.props.playedSound == this.props.el ? "❚❚" : "▶"}</div>
                    </div>
                </div>
            </div>
        );
    }
}

registerApp(DesignSettingsApp, 0);