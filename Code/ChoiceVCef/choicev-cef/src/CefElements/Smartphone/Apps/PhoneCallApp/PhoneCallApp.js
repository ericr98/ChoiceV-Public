import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon } from '../../SmartphoneController';

import { formatPhoneNumber } from './../CallApp/CallApp';

import './PhoneCallApp.css';

import { url } from './../../../../index';
import ContactApp, { ContactModel } from '../ContactApp/ContactApp';
import { CallList } from '../CallApp/CallApp';
import { MainApp } from './../../SmartphoneController';
import { AllSettings } from '../SettingsApp/SettingsApp';
import PhoneBookApp from '../PhoneBookApp/PhoneBookApp';

function toHHMMSS(sec_num) {
    var hours   = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = Math.floor(sec_num - (hours * 3600) - (minutes * 60));

    if (minutes < 10) {minutes = "0"+minutes;}
    if (seconds < 10) {seconds = "0"+seconds;}
    return minutes+':'+seconds;
}

export default class PhoneCallApp extends React.Component {
    static PhoneCall = null;
    static RingtonePlayer = null;
    static CallSoundPlayer = null;


    static tempCallId = null;
    static tempPlayerId = null;

    static onIncomingCall(sender, data) {
        PhoneCallApp.stopCallSound();

        PhoneCallApp.tempCallId = data.callId;
        PhoneCallApp.tempPlayerId = data.targetId;

        var ownerObj = JSON.parse(data.owner);
        var owner = new PhoneCallMember(ownerObj.number, ownerObj.hidden, ownerObj.special);

        var memberArr = [];
        data.members.forEach((el) => {
            var memberObj = JSON.parse(el);
            var member = new PhoneCallMember(memberObj.number, memberObj.hidden, memberObj.special);

            memberArr.push(member);
        });

        var costPerMinute = data.cost;

        var phoneCall = new PhoneCall(owner, memberArr, new Date(), costPerMinute, false);
        PhoneCallApp.PhoneCall = phoneCall

        sender.onOpenOtherApp(PhoneCallApp, []);
        //sender.onStateChange("CurrentApp", PhoneCallApp);
        sender.onStateChange("appData", data);
        sender.onStateChange("animationData", {animation: "shake 1s", animationIterationCount: "infinite"});

        if(PhoneCallApp.RingtonePlayer != null) {
            PhoneCallApp.RingtonePlayer.pause();
            PhoneCallApp.RingtonePlayer = null;
        }
        
        var settings = sender.onRequestState("settings");
        var volume = settings[AllSettings.VOLUME] * 0.1;
        var tone = url + "/phone/ringtones/Ringtone_" + sender.onRequestState("ringtone") + ".mp3";
        if(settings[AllSettings.SILENT_MODE]) {
            tone =  url + "/phone/ringtones/Ringtone_Silent.mp3";
            volume = 0.2;
        }
        var player = new Audio(tone);
        player.load();
        player.volume = volume;
        player.loop = true;
        player.play();
        PhoneCallApp.RingtonePlayer = player;

        setTimeout(() => {
            if(PhoneCallApp.PhoneCall == phoneCall && !PhoneCallApp.PhoneCall.accepted) {
                sender.onStateChange("animationData", {animation: "", animationIterationCount: ""});
                PhoneCallApp.stopRingtone();

                sender.sendToServer("PHONE_END_CALL", {
                    number: sender.onRequestState("number"),
                });
                PhoneCallApp.PhoneCall = null;
                sender.forceUpdate();
            }
        }, 10000);
    }

    static onRemoveMemberFromCall(sender, data) {
        if(PhoneCallApp.PhoneCall != null) {
            PhoneCallApp.PhoneCall.members = PhoneCallApp.PhoneCall.members.filter((el) => {
                return el.number != data.number;
            });
            
            sender.forceUpdate();
        }
    }

    static onEndCall(sender, data) { 
        PhoneCallApp.PhoneCall = null;
        PhoneCallApp.stopRingtone();
        sender.onStateChange("animationData", {animation: "", animationIterationCount: ""});
        sender.onOpenOtherApp(MainApp, []);
        //sender.onStateChange("CurrentApp", MainApp);
        sender.forceUpdate();
    }

    static startCallSound(changeState) {
        PhoneCallApp.CallSoundPlayer = new Audio(url + "/phone/callsound.mp3");
        PhoneCallApp.CallSoundPlayer.load();
        PhoneCallApp.CallSoundPlayer.volume = 0.4;
        PhoneCallApp.CallSoundPlayer.loop = false;
        PhoneCallApp.CallSoundPlayer.play();
        setTimeout(() => {
            if(PhoneCallApp.CallSoundPlayer != null) {
                PhoneCallApp.stopCallSound();
                PhoneCallApp.PhoneCall = null;
                changeState("animationData", {animation: "", animationIterationCount: ""});
                changeState("CurrentApp", MainApp);
            }
        }, 22000);
    }

    static stopCallSound() {
        if(PhoneCallApp.CallSoundPlayer != null) {
            PhoneCallApp.CallSoundPlayer.pause();
            PhoneCallApp.CallSoundPlayer = null;
        }
    }

    static stopRingtone() { 
        if(PhoneCallApp.RingtonePlayer != null) {
            PhoneCallApp.RingtonePlayer.pause();
        }
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

    static deselect() { }

    static dispose() {
        PhoneCallApp.RingtonePlayer = null;
        PhoneCallApp.PhoneCall = null;
    }

    constructor(props) {
        super(props); 

        this.state = {
            update: false,
        }

        this.onRemoveMemberFromCall = this.onRemoveMemberFromCall.bind(this);
    
        this.interval = null;
    }

    componentDidMount() {
        this.interval = setInterval(() => { 
            this.setState({
                update: !this.state.update,
            })
        }, 1000);
    }

    componentWillUnmount() {
        clearInterval(this.interval);
        this.interval = null;
    }

    onRemoveMemberFromCall(member) {
        if(PhoneCallApp.PhoneCall.accepted && PhoneCallApp.PhoneCall.owner.number == this.props.requestState("number")) {
            //TODO SERVER EVENT
        }
    }

    render() {
        if(PhoneCallApp.PhoneCall == null) {
            return null;
        }

        var number = this.props.requestState("number");
        var displayNumber = 0;
        if(number == PhoneCallApp.PhoneCall.owner.number) {
            displayNumber = PhoneCallApp.PhoneCall.members[0].number;
        } else {
            displayNumber = PhoneCallApp.PhoneCall.owner.number;
        }

        var cand = undefined;
        if(!PhoneCallApp.PhoneCall.owner.hiddenNumber) {
            cand = ContactApp.contactList.filter((el) => {
                return (el.number == displayNumber);
            })[0];
        }

        var cont = null;
        if(cand != undefined) {
            cont = cand;
        }

        var time = ((new Date() - PhoneCallApp.PhoneCall.startTime) / 1000);
        var cost = (time / 60 * PhoneCallApp.PhoneCall.costPerMinute * PhoneCallApp.PhoneCall.members.length + 1).toFixed(2);
        time = toHHMMSS(time);

        var name = "";
        if(cont != null) {
            name = cont.name;
        } else {
            var numberName = PhoneBookApp.getNameOfNumber(displayNumber);

            if(numberName != null) {
                name = numberName;
            } else {
                name = "Unbekannte Nummer";
            }
        }

        return (
            <div className="phonePhoneCallWorkplace standardAppBackground noSelect">
                <div className="phonePhoneCallTop">
                    <div className="phonePhoneCallMembers">
                        <PhoneCallMemberList removeMemberFromCall={this.onRemoveMemberFromCall} />
                    </div>
                    <div className="standardWrapper">
                        <img className="phonePhoneCallImg" src={url + "phone/icons/icon_person_big.png"} draggable={false} alt="The Icon of the Button" />
                    </div>
                    <div />
                </div>
                <div className="phonePhoneCallInfoWrapper">
                    <div className="phonePhoneCallInfoTime">{time}</div>
                    <div />
                    {/* <div className="phonePhoneCallInfoCost">{"$"+cost}</div> */}
                </div>
                <div className="phonePhoneCallContactInfo">
                    <div className="phonePhoneCallContactName">{name}</div>
                    <div className="phonePhoneCallContactInfoInfo">{PhoneCallApp.PhoneCall.owner.hiddenNumber ? "Unterdrückte Nummer" : formatPhoneNumber("" + displayNumber)}</div>
                    <div className="phonePhoneCallContactInfoInfo">{cont != null ? cont.note : "Keine Information"}</div>
                </div>

                <PhoneCallButtonBar changeState={this.props.changeState} requestState={this.props.requestState} contact={cont} openOtherApp={this.props.openOtherApp} sendToServer={this.props.sendToServer} />
            </div>
        );
    }
}

class PhoneCallMemberList extends React.Component { 
    render() {
        return PhoneCallApp.PhoneCall.members.map((el) => {
            var cand = undefined;
            if(!PhoneCallApp.PhoneCall.owner.hiddenNumber) {
                cand = ContactApp.contactList.filter((co) => {
                    return (co.number == el.number);
                })[0];
            }

            var cont = null;
            if(cand != undefined) {
                cont = cand;
            }
    
            return <PhoneCallMemberShow key={el.number} el={el} candidate={cont} callback={this.props.removeMemberFromCall}/>
        });
    }
}

class PhoneCallMemberShow extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.callback(this.props.el);
    }

    render() {
        var name = "";

        if(this.props.candidate != null) {
            name = this.props.candidate.name;
        } else {
            name = PhoneBookApp.getNameOfNumber(this.props.el.number);
        }

        return (
            <div className="phonePhoneCallMember">
                <div className="standardWrapper">
                    <img className="phonePhoneCallMemberIcon" src={url + "/phone/icons/icon_call_del.png"} onClick={this.onClick}></img>
                </div>
                <div className="phonePhoneCallMemberName">{name}</div>
            </div>);
    }
}

class PhoneCallButtonBar extends React.Component {
    constructor(props) {
        super(props);

        this.onAcceptCall = this.onAcceptCall.bind(this);
        this.onEndCall = this.onEndCall.bind(this);

        this.onGPS = this.onGPS.bind(this);
        this.onAdd = this.onAdd.bind(this);
    }

    onAcceptCall() {
        PhoneCallApp.PhoneCall.accepted = true;
        this.props.changeState("animationData", {animation: "", animationIterationCount: ""});
        PhoneCallApp.stopRingtone();
        PhoneCallApp.stopCallSound();
        this.forceUpdate();

        this.props.sendToServer("PHONE_ACCEPT_CALL", {
            callId: PhoneCallApp.tempCallId,
            playerId: PhoneCallApp.tempPlayerId,
            number: this.props.requestState("number"),
        });
    }

    onEndCall() {
        this.props.changeState("animationData", {animation: "", animationIterationCount: ""});
        this.props.changeState("CurrentApp", MainApp);
        PhoneCallApp.PhoneCall = null;
        PhoneCallApp.stopRingtone();
        PhoneCallApp.stopCallSound();
        this.forceUpdate();
        
        this.props.sendToServer("PHONE_END_CALL", {
            number: this.props.requestState("number"),
        });
    }

    onGPS() {
        this.props.sendToServer("PHONE_SEND_GPS", {
            number: this.props.requestState("number"),
        });
    }

    onAdd() {
        var newCont = new ContactModel(-1, false, PhoneCallApp.PhoneCall.owner.number, "", "", "");
        ContactApp.contactList.push(newCont);

        CallList.persistentCallList.forEach((el) => {
            if(el.number == PhoneCallApp.PhoneCall.owner.number) {
                el.contact = newCont;
            }
        })

        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: newCont}
        ]);
    }

    render () {
        if(PhoneCallApp.PhoneCall.accepted != null && PhoneCallApp.PhoneCall.accepted) {
            return(<Fragment>
                    <div className={"phonePhoneCallContactTopButtons" + (this.props.contact != null ? "onlyOneButton" : "")}>
                        <div />
                        <PhoneCallButton size={4} icon="gps" callback={this.onGPS} />
                       {this.props.contact == null ? <PhoneCallButton size={4} icon="add" callback={this.onAdd} /> : null}
                        <div />
                    </div>
    
                    <div className="standardWrapper">
                        <PhoneCallButton size={5} icon="neg" callback={this.onEndCall} />
                    </div>
                </Fragment>
            )
        } else {
            return (<Fragment>
                    <div />

                    <div className="phonePhoneCallContactBottomButtons">
                        <div />
                        <PhoneCallButton size={5} icon="pos" callback={this.onAcceptCall} />
                        <PhoneCallButton size={5} icon="neg" callback={this.onEndCall} />
                        <div />
                    </div>
                </Fragment>);
        }
    }
}

class PhoneCallButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.callback();
    }

    render() {
        return(
            <div className="phonePhoneCallButtonWrapper">
                <img className="phonePhoneCallButton" src={url + "phone/icons/icon_call_" + this.props.icon + ".png"} draggable={false} alt="The Icon of the Button" style={{height: this.props.size + "vh", width: this.props.size + "vh"}} onClick={this.onClick}/>
            </div>
        );
    }
}

export class PhoneCall {
    constructor(owner, members, startTime, costPerMinute, accepted) {
        this.owner = owner;
        this.members = members;
        this.startTime = startTime;
        this.costPerMinute = costPerMinute;
        this.accepted = accepted;
    }
}

export class PhoneCallMember {
    constructor(number, hiddenNumber, specialName) {
        this.number = number;
        this.hiddenNumber = hiddenNumber;
        this.specialName = specialName;
    }
}

registerCallback("INCOMING_CALL", PhoneCallApp.onIncomingCall);
registerCallback("STOP_CALL_SOUND", PhoneCallApp.stopCallSound);
registerCallback("REMOVE_MEMBER_FROM_CALL", PhoneCallApp.onRemoveMemberFromCall);
registerCallback("END_CALL", PhoneCallApp.onEndCall);

registerApp(PhoneCallApp, 0);