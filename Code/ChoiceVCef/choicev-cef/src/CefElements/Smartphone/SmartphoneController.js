import React, { Fragment } from 'react';
import { register } from '../../App';

import './phone.css';

import { url } from './../../index';

//Needed!
import CallApp from './Apps/CallApp/CallApp';
import ContactApp, { ContactModel } from './Apps/ContactApp/ContactApp';
import SettingsApp from './Apps/SettingsApp/SettingsApp';
import PhoneCallApp from './Apps/PhoneCallApp/PhoneCallApp';
import DesignSettingsApp from './Apps/DesignSettingsApp/DesignSettingsApp';
import MessengerApp from './Apps/MessengerApp/MessengerApp';
import BankingApp from './Apps/BankingApp/BankingApp';
import PhoneBookApp from './Apps/PhoneBookApp/PhoneBookApp';
import SocialMediaApp, {SocialMediaIFrame} from './Apps/SocialMediaApp/SocialMediaApp';
import WikiApp from './Apps/WikiApp/WikiApp';

const zeroPad = (num, places) => String(num).padStart(places, '0')

var currentAppId = 0;
var registeredApps = [];

var callbacks = [];

export var ConfirmationType = {
    YES_NO: "yesNo",
    INPUT: "input",
    NOTIFICATION: "notification",
}

export function registerApp(el, version) {
    if(registeredApps == undefined) {
        setTimeout(() => {
            registerApp(el, version);
        }, 100);
    } else {
        registeredApps.push({id: currentAppId, El: el, version: version});
        currentAppId++;
    }
}

export function registerCallback(evt, callback) {
    if(callbacks == undefined) {
        setTimeout(() => {
            registerCallback(evt, callback);
        }, 100);
    } else {
        callbacks = callbacks.filter((el) => {
            return el.evt != evt;
        });
    
        callbacks.push({
            evt: evt,
            callback: callback,
        });
    }
}

export default class SmartphoneController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            shown: false,
            open: false,
            horizontal: false,
            phoneData: null,

            appData: {},

            CurrentApp: MainApp, //TODO CHANGE TO MainApp
            
            itemId: -1,
            version: -1,

            number: -1,

            background: -1,
            ringtone: -1,

            settings: {},

            animationData: {}
        }

        this.child = React.createRef();
        this.confirmation = React.createRef();

        this.onStateChange = this.onStateChange.bind(this);

        this.onClosePhone = this.onClosePhone.bind(this);
        this.onOpenPhone = this.onOpenPhone.bind(this);
        this.onEquipPhone = this.onEquipPhone.bind(this);
        this.onUnequipPhone = this.onUnequipPhone.bind(this);
        this.onPhoneAppEvent = this.onPhoneAppEvent.bind(this);
        this.onPhoneNotification = this.onPhoneNotification.bind(this);

        //Needs implementing
        //this.props.input.registerEvent("CLOSE_CEF", this.onClosePhone);
        this.props.input.registerEvent("CLOSE_PHONE", this.onClosePhone);

        this.props.input.registerEvent("EQUIP_PHONE", this.onEquipPhone);
        this.props.input.registerEvent("UNEQUIP_PHONE", this.onUnequipPhone);
        this.props.input.registerEvent("OPEN_PHONE", this.onOpenPhone);
        this.props.input.registerEvent("PHONE_APP_EVENT", this.onPhoneAppEvent);

        this.props.input.registerEvent("PHONE_NOTIFICATION", this.onPhoneNotification);

        this.requestData = this.requestData.bind(this);
        this.sendToServer = this.sendToServer.bind(this);
        this.onChangeSetting = this.onChangeSetting.bind(this);

        this.onOpenOtherApp = this.onOpenOtherApp.bind(this);

        this.onRequestState = this.onRequestState.bind(this);

        this.onRequestConfirmation = this.onRequestConfirmation.bind(this);
    }

    componentDidMount() {
        setTimeout(() => {
        registeredApps.forEach((el) => {
            if(el.El.registerStaticEvents != undefined) {
                el.El.registerStaticEvents();
            }
        })}, 1000);

        //TODO REMOVEs

        // setTimeout(() => {
        //     this.onPhoneAppEvent({subEvt: "PHONE_PHONE_BOOK_ANSWER", 
        //         entries : [
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //             JSON.stringify({name: "Los Santos Institute of Science of Los Santos", number: 55555512354}),
        //             JSON.stringify({name: "LSPD", number: 911}),
        //         ]
        //     });
        // }, 100)

        // setTimeout(() => {
        //     this.onPhoneAppEvent({subEvt: "INCOMING_CALL", 
        //         owner: JSON.stringify({number: 675496745896, hidden: false, special: null}),
        //         members: [
        //             JSON.stringify({number: 5555555555, hidden: false}),
        //             JSON.stringify({number: 456, hidden: false}),
        //             JSON.stringify({number: 789, hidden: false}),
        //         ],
        //         cost: 1.2,
        //     });

        //     setTimeout(() => {
        //         this.onPhoneAppEvent({subEvt: "REMOVE_MEMBER_FROM_CALL", number: 456});
        //     }, 3000)
        // }, 1000)

        // setTimeout(() => {
        //     this.onPhoneAppEvent({subEvt: "PHONE_ANSWER_CALLLIST", callList: [
        //         JSON.stringify({id: 0, icon: "list_in", number: "123", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 1, icon: "list_out", number: "5555555555", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 2, icon: "list_miss", number: "5555555555", dateTime: "06.03 11:40 PM", missed: true}),
        //         JSON.stringify({id: 0, icon: "list_in", number: "123", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 1, icon: "list_out", number: "5555555555", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 2, icon: "list_miss", number: "5555555555", dateTime: "06.03 11:40 PM", missed: true}),
        //         JSON.stringify({id: 0, icon: "list_in", number: "123", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 1, icon: "list_out", number: "5555555555", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 2, icon: "list_miss", number: "5555555555", dateTime: "06.03 11:40 PM", missed: true}),
        //         JSON.stringify({id: 0, icon: "list_in", number: "123", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 1, icon: "list_out", number: "5555555555", dateTime: "06.03 11:40 PM", missed: false}),
        //         JSON.stringify({id: 2, icon: "list_miss", number: "5555555555", dateTime: "06.03 11:40 PM", missed: true}),
        //     ]})
        // }, 300)

        // setTimeout(() => {
        //     this.onPhoneAppEvent({subEvt: "PHONE_ANSWER_MESSENGER_CHATS", chats: [
        //         JSON.stringify({id: 0, number: 123, missed: 3, lastD: new Date(2020, 5, 15, 20, 15, 1), lastM: "Hey, wie geht's, ich hab da etwas.."}),
        //         JSON.stringify({id: 1, number: 5555555555, missed: 90, lastD: new Date(2020, 5, 13, 20, 14, 2), lastM: "Die Bestellung ist angekommen .."}),
        //         JSON.stringify({id: 2, number: 789, missed: 20, lastD: new Date(2020, 5, 17, 20, 16, 3), lastM: "Die Registerierung im Hotel ist.."}),
        //     ]})
        // }, 300)
    }

    onStateChange(name, value) {
        var state = this.state;
        state[name] = value;
        this.setState(state);
    }
 
    onChangeSetting(name, value) {
        var settings = this.state.settings;
        settings[name] = value;
        this.setState({
            settings: settings,
        });

        this.sendToServer("PHONE_CHANGE_SETTING", settings);
    }

    onEquipPhone(data) {
        ContactApp.createContactList(data);
        PhoneBookApp.queryPhoneBookEntries(this);

        this.setState({
            shown: true,
            phoneData: data,

            itemId: data.itemId,
            version: data.version,
            number: data.number,
            background: data.background,
            ringtone: data.ringtone,

            settings: JSON.parse(data.settings),
        });
    }

    onUnequipPhone(data) {
        this.setState({
            shown: false,
            phoneData: null,


            number: null,
            background: null,
            ringtone: null,
        });

        registeredApps.forEach((el) => {
            el.El.dispose();
        })
    }

    onClosePhone() {
        this.setState({
            open: false,
            horizontal: false,
        });

        if(this.state.CurrentApp.stopsMovement()) {
            this.sendToServer("PHONE_CHANGE_APP", { stopsMovement: false });
        }
    }

    onOpenPhone() {
        this.setState({
            open: true,
        });

        this.sendToServer("PHONE_CHANGE_APP", { stopsMovement: this.state.CurrentApp.stopsMovement() });
    }

    onPhoneAppEvent(data) {
        var obj = callbacks.filter((el) => {
            return data.subEvt == el.evt;
        })[0];

        if(obj != undefined) {
            obj.callback(this, data);
        } else {
            console.log("Error: Smartphone Sub-Event not found: " + data.subEvt);
        }
    }

    sendToServer(evt, data) {
        data["itemId"] = this.state.itemId;
        this.props.output.sendToServer(evt, data);
    }

    requestData(evt, data) {
        data["number"] = this.state.number;
        this.props.output.sendToServer(evt, data, false);

        if(evt == "PHONE_MESSENGER_REQUEST_MESSAGES") {
            // this.onPhoneAppEvent({
            //     subEvt: "PHONE_MESSENGER_ANSWER_MESSAGES",
            //     id: 0,
            //     messages: [
            //         JSON.stringify({id: 0, text: "Hallo!gdfghsgfhjrtzhr5eh5hhfghfghghdgbvfgjhjkghkdsfsdfsdfsdfsdgwevfdsghffsfsdfsdfsdfhjrzgsdghfgjtsdfh", date: new Date(2020, 4, 15, 20, 1), from: 123, read: false}),
            //         JSON.stringify({id: 1, text: "Hallo zurück!", date: new Date(2020, 4, 15, 20, 2), from: 111, read: false}),
            //         JSON.stringify({id: 2, text: "Ich hätte gerne Geld von dir! Gib her die Mühle!", date: new Date(2020, 4, 15, 20, 3), from: 111, read: false}),
            //         JSON.stringify({id: 3, text: "Dies ist ein langer Text. Er muss auch gehen. Völlig egal was man macht und die Texte sind einfach lang", date: new Date(2020, 4, 15, 20, 15), from: 111, read: false}),
            //         JSON.stringify({id: 4, text: "Hallo!", date: new Date(2020, 4, 15, 20, 4), from: 123, read: false}),
            //         JSON.stringify({id: 5, text: "Hallo zurück!", date: new Date(2020, 4, 15, 20, 5), from: 111, read: false}),
            //         JSON.stringify({id: 6, text: "Ich hätte gerne Geld von dir! Gib her die Mühle!", date: new Date(2020, 4, 15, 20, 6), from: 111, read: false}),
            //         JSON.stringify({id: 7, text: "Dies ist ein langer Text. Er muss auch gehen. Völlig egal was man macht und die Texte sind einfach lang", date: new Date(2020, 4, 15, 20, 15), from: 111, read: false}),
            //         JSON.stringify({id: 8, text: "Hallo!", date: new Date(2020, 4, 15, 20, 7), from: 123, read: false}),
            //         JSON.stringify({id: 9, text: "Hallo zurück!", date: new Date(2020, 5, 15, 20, 8), from: 111, read: false}),
            //     ]
            // });
        } else if(evt == "PHONE_BANKING_REQUEST_BANKACCOUNTS") {
            // this.onPhoneAppEvent({
            //     subEvt: "PHONE_BANKING_ANSWER_BANKACCOUNTS",
            //     accounts: [
            //         JSON.stringify({number: 123, name: "Erk Racone", balance: 1234.45, company: "fleeca"}),
            //         JSON.stringify({number: 456, name: "Erk Racone der 2.", balance: 1234.45, company: "maze"}),
            //     ]
            // });
        } else if(evt == "PHONE_BANKING_REQUEST_TRANSACTIONS") {
            // this.onPhoneAppEvent({
            //     subEvt: "PHONE_BANKING_ANSWER_TRANSACTIONS",
            //     number: 123,
            //     transactions: [
            //         JSON.stringify({from: 123, to: 999, balance: 100.4, note: "Strafzettel Döner", date: new Date(2020, 5, 15, 20, 15)}),
            //         JSON.stringify({from: 999, to: 123, balance: 50.4, note: "Deine Mutter", date: new Date(2020, 5, 15, 20, 14)}),
            //     ]
            // });
        }
    }

    onRequestState(name) {
        return this.state[name];
    }

    onOpenOtherApp(OtherApp, stateList) {
        this.sendToServer("PHONE_CHANGE_APP", { stopsMovement: OtherApp.stopsMovement() });
        this.state.CurrentApp.deselect();
        this.setState({
            CurrentApp: OtherApp,
        }, () => { 
            var stateObj = {};
            stateList.forEach((el) => {
                stateObj[el.name] = el.value;
            });

            this.child.current.setState(stateObj);
        });
    }

    onPhoneNotification(data) {
        this.onRequestConfirmation(ConfirmationType.NOTIFICATION, data.title);
    }

    onRequestConfirmation(type, title, yesCallback, noCallback, settings) {
        this.confirmation.current.setState({
          type: type,
          title: title,
          yesCallback: yesCallback,
          noCallback: noCallback,
          settings: settings,
          shown: true,
        });
    }

    render() {
        return (
            <Fragment>
            <SmartphoneRenderer 
                state={this.state} 
                auth={this.props.auth}
                confirmation={this.confirmation} 
                onStateChange={this.onStateChange} 
                child={this.child} 
                onOpenOtherApp={this.onOpenOtherApp} 
                registerCallback={this.registerCallback}  
                requestData={this.requestData} 
                sendToServer={this.sendToServer} 
                onRequestState={this.onRequestState} 
                onRequestConfirmation={this.onRequestConfirmation} 
                onChangeSetting={this.onChangeSetting}
                />
            </Fragment>
        );
      }
}

class SmartphoneRenderer extends React.Component {
    getVaribaleWorkplace() {
        const { state, child, onStateChange, requestData, registerCallback, sendToServer, onOpenOtherApp, onRequestState, onRequestConfirmation, onChangeSetting } = this.props;

        if (!state.horizontal || !state.CurrentApp.hasVerticalMode()) {
            var rotateButton = null;
            if (state.CurrentApp.hasVerticalMode()) {
                rotateButton = (<input className="phoneRotationChangeButton" type="image" src={url + "phone/socialmedia/rotate-smartphone.png"} onClick={() => onStateChange("horizontal", true)}></input>);
            }

            if (state.CurrentApp.hasTime()) {
                return (
                    <Fragment>
                        {rotateButton}
                        <PhoneTimeDate />
                        <div className={"phoneApp"}>
                            <state.CurrentApp ref={child} version={state.version} auth={this.props.auth} data={state.phoneData} appData={state.appData} horizontal={state.horizontal} registerCallback={registerCallback} requestData={requestData} sendToServer={sendToServer} changeState={onStateChange} openOtherApp={onOpenOtherApp} requestState={onRequestState} requestConfirmation={onRequestConfirmation} changeSetting={onChangeSetting} />
                        </div>
                    </Fragment>);
            } else {
                return (
                    <Fragment>
                        {rotateButton}
                        <div className="phoneAppFull" >
                            <state.CurrentApp ref={child} version={state.version} auth={this.props.auth} data={state.phoneData} appData={state.appData} horizontal={state.horizontal} registerCallback={registerCallback} changeState={onStateChange} requestData={requestData} sendToServer={sendToServer} openOtherApp={onOpenOtherApp} requestState={onRequestState} requestConfirmation={onRequestConfirmation} changeSetting={onChangeSetting}/>
                        </div>
                    </Fragment>);
            }
        } else {
            return (
                <Fragment>
                    <input className="phoneRotationChangeButtonHorizontal" type="image" src={url + "phone/socialmedia/rotate-smartphone-vertical.png"} onClick={() => onStateChange("horizontal", false)}></input>
                    <div className="phoneAppFullHorizontal" >
                        <state.CurrentApp ref={child} version={state.version} auth={this.props.auth} data={state.phoneData} appData={state.appData} horizontal={state.horizontal} registerCallback={registerCallback} changeState={onStateChange} requestData={requestData} sendToServer={sendToServer} openOtherApp={onOpenOtherApp} requestState={onRequestState} requestConfirmation={onRequestConfirmation} changeSetting={onChangeSetting} />
                    </div>
                </Fragment>);
        }
    }

    render() {
        const { state, confirmation, frame, onStateChange, child, onOpenOtherApp } = this.props;

        if (!state.shown) {
            return null;
        }

        if (!state.horizontal || !state.CurrentApp.hasVerticalMode()) {
            var style = {
                marginTop: "38vh",
                backgroundImage: "url(" + url + "phone/background.png)",
                animation: state.animationData.animation,
                animationIterationCount: state.animationData.animationIterationCount,
            };

            if (!state.open) {
                style.marginTop = "96vh";
            }

            return (
                <div className="phoneBackground noSelect" style={style}>
                    <PhoneConfirmationController ref={confirmation} />
                    <div className="phoneWorkplace" style={{ backgroundImage: "url(" + url + "phone/backgrounds/Background_" + state.background + ".png)" }}>
                        {this.getVaribaleWorkplace(state.animHeight)}
                    </div>
                    <PhoneButtonsBar changeState={onStateChange} currentApp={child} openOtherApp={onOpenOtherApp} horizontal={this.props.state.horizontal && state.CurrentApp.hasVerticalMode()} />
                </div>
            );
        } else {
            var style = {
                backgroundImage: "url(" + url + "phone/backgroundHorizontal.png)",
                animation: state.animationData.animation,
                animationIterationCount: state.animationData.animationIterationCount,
            };

            return (
                <div className='standardWrapper'>
                    <div className="phoneBackgroundHorizontal noSelect" style={style}>
                        <div className="phoneWorkplaceHorizontal">
                            {this.getVaribaleWorkplace()}
                        </div>
                        <PhoneButtonsBar changeState={onStateChange} currentApp={child} openOtherApp={onOpenOtherApp} horizontal={this.props.state.horizontal && state.CurrentApp.hasVerticalMode()}/>
                    </div>
                </div>
            );
        }
    }
}

class PhoneButtonsBar extends React.Component {
    constructor(props) {
        super(props);

        this.onHomeClick = this.onHomeClick.bind(this);
        this.onBackClick = this.onBackClick.bind(this);
    }

    onHomeClick(evt) {
        this.props.changeState("animHeight", "0%");
        this.props.changeState("horizontal", false);
        this.props.openOtherApp(MainApp, []);
    }

    onBackClick(evt) {
        if(this.props.currentApp.current.triggerBackButton != undefined) {
            this.props.currentApp.current.triggerBackButton();
        }
    }

    render() {
        if(!this.props.horizontal) {
            return(
                <div className="phoneButtonsBar noSelect">
                    <div className="phoneButtonsBarGrid">
                        <PhoneButton icon="left" size="2.9vh" onClick={this.onBackClick} />
                        <PhoneButton icon="home" size="2.9vh" onClick={this.onHomeClick} openOtherApp={this.props.onOpenOtherApp} />
                        <PhoneButton icon="right" size="2.9vh" />
                    </div>
                </div>
            );
        } else {
            return(
                <div className="horizontalPhoneButtonsBar noSelect">
                    <div className="horizontalPhoneButtonsBarGrid">
                        <PhoneButton icon="right_horizontal" size="5vh" onClick={this.onBackClick} />
                        <PhoneButton icon="home_horizontal" size="5vh" onClick={this.onHomeClick} openOtherApp={this.props.onOpenOtherApp} />
                        <PhoneButton icon="left_horizontal" size="5vh" />
                    </div>
                </div>
            );
        }
    }
}

class PhoneButton extends React.Component {
    render() {
        return(
            <div className="phoneButtonWrapper">
                <input className="phoneButtonImage" type="image" style={{width: this.props.size, height: this.props.size}} src={url + "phone/mainButtons/" + this.props.icon + ".png"} draggable={false} alt="The Icon of the Button" onClick={this.props.onClick} />
            </div>
        );
    }
}

class PhoneTimeDate extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            date: new Date(),
        };
    }

    componentDidMount() {
        this.interval = setInterval(() => this.setState({ date: new Date() }), 1000);
    }

    componentWillUnmount() {
        clearInterval(this.interval);
    }

    getAMPM(now) {
        return now.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true });
    }

    getDateString(now) {
        var day = now.getDate();
        var month = now.getMonth() + 1;
        var year = now.getYear() + 1900;
        var dayIndx = now.getDay();
        var days = ['Sonntag', 'Montag', 'Dienstag', 'Mittwoch', 'Donnerstag', 'Freitag', 'Samstag'];

        return days[dayIndx] + ', ' + zeroPad(day, 2)  + '.' + zeroPad(month, 2) + '.' + year;
    }

    render() {
        return(
            <div className="phoneClock noSelect">
                <div className="phoneClockTime">{this.getAMPM(this.state.date)}</div>
                <div className="phoneClockDate">{this.getDateString(this.state.date)}</div>
            </div>);
    }
}

class PhoneConfirmationController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            shown: false,
            title: "",
            yesCallback: null,
            noCallback: null,
        }

        this.onYes = this.onYes.bind(this);
        this.onNo = this.onNo.bind(this);
        this.onEnter = this.onEnter.bind(this);
    }

    onYes() {
        if(this.state.yesCallback != null) {
            this.state.yesCallback();
        }

        this.setState({
            shown: false,
        })
    }

    onNo() {
        if(this.state.noCallback != null) {
            this.state.noCallback();
        }
        
        this.setState({
            shown: false,
        })
    }

    onEnter(input) {
        if(this.state.yesCallback != null) {
            this.state.yesCallback(input);
        }
        
        this.setState({
            shown: false,
        })
    }

    render() {
        if(this.state.shown) {
            switch(this.state.type) {
                case ConfirmationType.YES_NO:
                    return <PhoneTrueFalseConfirmation onYes={this.onYes} onNo={this.onNo} title={this.state.title} />;
                case ConfirmationType.INPUT:
                    return <PhoneInputConfirmation onEnter={this.onEnter} onNo={this.onNo} title={this.state.title} settings={this.state.settings} />
                case ConfirmationType.NOTIFICATION:
                    return <PhoneNotificationConfirmation onYes={this.onYes} title={this.state.title} settings={this.state.settings}/>
            }
        } else {
            return null;
        }
    }
}

class PhoneTrueFalseConfirmation extends React.Component {
    render() {
        return (
            <div className="standardWrapper phoneConfirmationWrapper">
                <div className="phoneConfirmation">
                    <div className="standardWrapper phoneConfirmationWrapper">
                        <div className="phoneConfirmationTitle">{this.props.title}</div>
                    </div>
                    <div className="standardWrapper">
                    <button className="phoneConfirmationButton yesButton" onClick={this.props.onYes}>Ja</button>
                    </div>
                    <div className="standardWrapper">
                        <button className="phoneConfirmationButton noButton" onClick={this.props.onNo}>Abbrechen</button>
                    </div>
                </div>
            </div>
        );
    }
}


//Settings:
// type: e.g "text", "password"
// textAlign: e.g "left", "center"
class PhoneInputConfirmation extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            value: "",
        }

        this.onChange = this.onChange.bind(this);
        this.onKeyPress = this.onKeyPress.bind(this);
        this.onClick = this.onClick.bind(this);
    }

    onChange(evt) {
        this.setState({
            value: evt.target.value,
        })
    }

    onKeyPress(evt) {
        if(evt.keyCode == 13) {
            this.onClick();
        }
    }

    onClick() {
        this.props.onEnter(this.state.value);
    }

    render() {
        return (
            <div className="standardWrapper phoneConfirmationWrapperFull">
                <div className="phoneConfirmation">
                    <div className="standardWrapper phoneConfirmationWrapper">
                        <div className="phoneConfirmationTitle">{this.props.title}</div>
                        <img className="phoneConfirmationIcon" src={url + "phone/icons/closeIcon.svg"} onClick={this.props.onNo} draggable={false} alt="The Icon of the Button" />
                    </div>
                    <div className="standardWrapper phoneConfirmationInputWrapper">
                        <div className="standardWrapper">
                            <input className="phoneConfirmationInput" style={{textAlign: this.props.settings.textAlign}} type={this.props.settings.type} spellCheck={false} onKeyPress={this.onKeyPress} value={this.state.value} onChange={this.onChange} />
                        </div>
                        <div className="standardWrapper">
                            <button className="phoneConfirmationButton yesButton" onClick={this.onClick}>O.K.</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

class PhoneNotificationConfirmation extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.onYes();
    }

    render() {
        return (
            <div className="standardWrapper phoneConfirmationWrapperFull">
                <div className="phoneConfirmation">
                    <div className="standardWrapper phoneConfirmationWrapper">
                        <div className="phoneConfirmationTitle">{this.props.title}</div>
                    </div>
                    <div className="standardWrapper phoneConfirmationNotificationButtonWrapper">
                        <button className="phoneConfirmationNotificationButton yesButton" onClick={this.onClick}>O.K.</button>
                    </div>
                </div>
            </div>
        );
    }
}

export class MainApp extends React.Component {
    constructor(props) {
        super(props);

        this.onSelectApp = this.onSelectApp.bind(this);
    }

    onSelectApp(app) {
        this.props.openOtherApp(app, []);
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

    render() {
        return (
            <div className="mainAppWrapper">
                <div className="mainAppGrid noSelect">
                    {registeredApps.map((el) => {
                        if(el.El.getIcon != undefined && el.version <= this.props.version) {
                            return el.El.getIcon(this.onSelectApp);
                        }
                    })}
                </div>
            </div>
        );
    }
}

export class Icon extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.callback(this.props.type);
    }

    render() {
        return (
            <div className="phoneIconWrapper" style={{gridColumn: this.props.column, gridRow: this.props.row}}>
                <input className="phoneIconImage" type="image" src={url + "phone/appIcons/app_" + this.props.icon + ".png"} draggable={false} alt="The Icon of the App" onClick={this.onClick} />
                {this.props.missedInfo > 0 ? <div className="phoneIconMissedInfo noSelect">{this.props.missedInfo <= 99 ? this.props.missedInfo : 99}</div> : null}
            </div>
        );
    }
}

register(SmartphoneController);
