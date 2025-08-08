import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon, ConfirmationType } from '../../SmartphoneController';

import './CallApp.css';

import { url } from './../../../../index';
import ContactApp, { ContactModel } from '../ContactApp/ContactApp';
import PhoneCallApp, { PhoneCall, PhoneCallMember } from '../PhoneCallApp/PhoneCallApp';
import { AllSettings } from '../SettingsApp/SettingsApp';

export function formatPhoneNumber(number) {
    if(number.charAt(0) == '+') {
        if(number.length <= 3) {
            return number;
        } else if(number.length <= 6) {
            return `${number.substring(0, 3)}-${number.substring(3)}`
        } else if(number.length <= 9) {
            return `${number.substring(0, 3)}-${number.substring(3, 6)}-${number.substring(7)}`
        } else {
            return `${number.substring(0, 3)}-${number.substring(3, 6)}-${number.substring(7, 10)}-${number.substr(11)}`
        }
    } else {
        if(number.length <= 3) {
            return number;
        } else if(number.length <= 6) {
            return `(${number.substring(0, 3)}) ${number.substring(3)}`
        } else {
            return `(${number.substring(0, 3)}) ${number.substring(3, 6)}-${number.substring(6)}`
        }
    }
}

export default class CallApp extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            CurrentSubMenu: NumberBlock,
            currentNumber: "",
        }

        this.onCallNumber = this.onCallNumber.bind(this);
        this.changeSubMenu = this.changeSubMenu.bind(this);
        this.onChangeNumber = this.onChangeNumber.bind(this);
        this.onSelectCall = this.onSelectCall.bind(this);
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

    static getIcon(callback) {
        return <Icon key={"phone"} icon="phone" column={1} row={5} missedInfo={CallList.persistentCallList.filter((el) => {
            return el.icon != "list_out" && el.missed && !el.check
        }).length} callback={callback} type={CallApp} />
    }
    
    static dispose() {
        CallList.persistentCallList = [];
        CallList.lastCallListUpdate = null;
    }

    onCallNumber(number) {
        //TODO 
    }

    triggerBackButton() {
        this.setState({
            CurrentSubMenu: NumberBlock,
        })
    }

    changeSubMenu(newSubMenu) {
        this.setState({
            CurrentSubMenu: newSubMenu,
        })
    }

    onChangeNumber(number) {
        this.setState({
            currentNumber: number,
        })
    }

    onSelectCall(number) {
        this.setState({
            currentNumber: number,
            CurrentSubMenu: NumberBlock,
        })
    }

    render() {
        if(PhoneCallApp.PhoneCall != null) {
            this.props.openOtherApp(PhoneCallApp, []);
        }

        return (
            <div className="phoneCallWorkplace standardAppBackground">
                <SelectBar changeSubMenu={this.changeSubMenu} changeSetting={this.props.changeSetting}/>
                <this.state.CurrentSubMenu currentNumber={this.state.currentNumber} changeNumber={this.onChangeNumber} selectCall={this.onSelectCall} changeSubMenu={this.changeSubMenu} onCallNumber={this.onCallNumber} registerCallback={this.props.registerCallback} changeState={this.props.changeState} requestData={this.props.requestData} requestState={this.props.requestState} sendToServer={this.props.sendToServer} openOtherApp={this.props.openOtherApp} changeSetting={this.props.changeSetting} requestConfirmation={this.props.requestConfirmation} />
            </div>);
    }
}

class SelectBar extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return(
            <div className="phoneCallSelectBarWrapper">
                <SelectBarButton icon="phone" text="Anrufliste" subMenu={CallList} changeSubMenu={this.props.changeSubMenu} />
                <SelectBarButton icon="block" text="Ziffernblock" subMenu={NumberBlock} changeSubMenu={this.props.changeSubMenu} changeSetting={this.props.changeSetting} />
            </div>
        );
    }
}

class SelectBarButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.changeSubMenu(this.props.subMenu);
    }

    render() {
        return(
            <Fragment>
                <div className="phoneCallSelectBarElement noSelect" onClick={this.onClick}>
                    <img className="phoneCallSelectBarElementIcon" src={url + "phone/icons/icon_" + this.props.icon + ".png"} draggable={false} alt="The Icon of the Button" />
                    <div className="phoneCallSelectBarElementText">{this.props.text}</div>
                </div>
            </Fragment>
        );
    }
}

//NumberBlock

class NumberBlock extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            showNumber: formatPhoneNumber(props.currentNumber),
        }
        
        this.onClickNumber = this.onClickNumber.bind(this);
        this.onBack = this.onBack.bind(this);

        this.formatNumber = this.formatNumber.bind(this);

        this.onStartCall = this.onStartCall.bind(this);
    }

    onClickNumber(text) {
        if(this.props.currentNumber.length < 13) {
            this.formatNumber(this.props.currentNumber + text);
        }
    }

    onStartCall() {
        var settings = this.props.requestState("settings");
        if(settings[AllSettings.FLY_MODE]) {
            this.props.requestConfirmation(ConfirmationType.YES_NO, "Flugmodus deaktivieren um Anruf zu tätigen?", () => {
                this.props.changeSetting(AllSettings.FLY_MODE, false);
                this.startCall();
            }, null);
        } else {
            this.startCall();
        }
    }

    startCall() {
        var ownNumber = this.props.requestState("number");

        PhoneCallApp.PhoneCall = new PhoneCall(new PhoneCallMember(ownNumber, false, false),[
            new PhoneCallMember(this.props.currentNumber, false, false),
        ], new Date(), 1, true);

        this.props.openOtherApp(PhoneCallApp, []);

        PhoneCallApp.startCallSound(this.props.changeState);

        this.props.sendToServer("PHONE_START_CALL", {
            owner: ownNumber,
            number: this.props.currentNumber,
        });
    }

    onBack() {
        if(this.props.currentNumber.length > 0) {
            this.formatNumber(this.props.currentNumber.slice(0, -1));
        }
    }

    formatNumber(number) {
        var shown = formatPhoneNumber(number);

        this.props.changeNumber(number);
        this.setState({
            showNumber: shown, 
        });
    }

    render() {
        return(
            <div className="phoneCallNumberBlockWrapper">
                <div className="phoneCallNumberBlockNumber">{this.state.showNumber}</div>
                <div className="phoneCallNumberBlockGridWrapper">
                    <div className="phoneCallNumberBlockGrid noSelect">
                        <NumberBlockButton text={1} additional={""} row={1} column={1} callback={this.onClickNumber}/>
                        <NumberBlockButton text={2} additional={"ABC"} row={1} column={2} callback={this.onClickNumber}/>
                        <NumberBlockButton text={3} additional={"DEF"} row={1} column={3} callback={this.onClickNumber}/>
                        
                        <NumberBlockButton text={4} additional={"GHI"} row={2} column={1} callback={this.onClickNumber}/>
                        <NumberBlockButton text={5} additional={"JKL"} row={2} column={2} callback={this.onClickNumber}/>
                        <NumberBlockButton text={6} additional={"MNO"}row={2} column={3} callback={this.onClickNumber}/>

                        <NumberBlockButton text={7} additional={"PQRS"}row={3} column={1} callback={this.onClickNumber}/>
                        <NumberBlockButton text={8} additional={"TUV"}row={3} column={2} callback={this.onClickNumber}/>
                        <NumberBlockButton text={9} additional={"WXYZ"}row={3} column={3} callback={this.onClickNumber}/>

                        <NumberBlockButton text={"+"} row={4} column={1} callback={this.onClickNumber}/>
                        <NumberBlockButton text={0} row={4} column={2} callback={this.onClickNumber}/>
                        <NumberBlockButton text={"<"} row={4} column={3} callback={this.onBack}/>

                        <NumberBlockButton icon={"phone_green"} row={5} column={2} callback={this.onStartCall}/>
                    </div>
                </div>
            </div>
        );
    }
}


class NumberBlockButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.callback(this.props.text);
    }

    getShowElement() {
        if(this.props.icon != undefined) {
            return (<img className="phoneCallNumberBlockButtonImg" src={url + "phone/icons/icon_" + this.props.icon + ".png"} draggable={false} alt="The Icon of the Button" />);
        } else {
            if(this.props.additional != undefined) {
                return (
                <div className="phoneCallNumberBlockButtonAdditionalWrapper">
                    <div className="phoneCallNumberBlockButtonAdditionalText">{this.props.text}</div>
                    <div className="phoneCallNumberBlockButtonAdditionalAdditional">{this.props.additional}</div>
                </div>
                );
            } else {
                return (<div className="phoneCallNumberBlockButton">{this.props.text}</div>);
            }
        }
    }

    render() {
        return(
            <div className="phoneCallNumberBlockButtonWrapper" style={{gridRow: this.props.row, gridColumn: this.props.column}} onClick={this.onClick}>
                {this.getShowElement()}
            </div>
        );
    }
}


//CallList

export class CallList extends React.Component { 
    static lastCallListUpdate = null;

    //Static Stuff
    static persistentCallList = [];
    static onAnswerCallList(sender, data) {
        var arr = [];
        data.callList.forEach((el) => {
            var obj = JSON.parse(el);
        
            arr.push(new CallElementModel(obj.id, obj.icon, obj.number, obj.dateTime, obj.missed, obj.check))
        })
    
        CallList.persistentCallList.push(...arr);
        CallList.lastCallListUpdate = new Date();
        sender.forceUpdate();
    }
    
    static onCallListAddNew(sender, data) {
        var arr = [];
        data.callList.forEach((el) => {
            var obj = JSON.parse(el);
        
            arr.push(new CallElementModel(obj.id, obj.icon, obj.number, obj.dateTime, obj.missed, obj.check))
        })
    
        CallList.persistentCallList = arr;
        sender.forceUpdate();
    }

    constructor(props) {
        super(props);

        this.state = {
            callList: CallList.persistentCallList,
        }
    }

    componentDidMount() {
        this._isMounted = true;

        var noLongerMissed = [];
        CallList.persistentCallList.forEach((el) => {
            if(!el.check && el.missed) {
                el.check = true;
                noLongerMissed.push(el.id);
            }
        });

        this.setState({
            callList: CallList.persistentCallList,
        });

        this.props.requestData("PHONE_CHECK_CALLLIST", { lastUpdate: CallList.lastCallListUpdate, noLongerMissed: noLongerMissed }, this);

        if(CallList.persistentCallList.length == 0) {
            this.props.requestData("PHONE_REQUEST_CALLLIST", { }, this);
        }
    }

    componentWillUnmount() {
        this._isMounted = false;
    }

    render() {
        return (
        <div className="phoneCallList">
            {this.state.callList.map((el) => {
                return <CallElement key={el.id} el={el} openOtherApp={this.props.openOtherApp} selectCall={this.props.selectCall} />
            })}
        </div>);
    }
}

class CallElement extends React.Component {
    constructor(props) {
        super(props);

        var cand = ContactApp.contactList.filter((el) => {
            return el.number == props.el.number;
        })[0];

        var contact = cand != undefined ? cand : null;

        this.state = {
            contact: contact,
        }

        this.onNewContact = this.onNewContact.bind(this);
        this.onOpenContact = this.onOpenContact.bind(this);

        this.getIcon = this.getIcon.bind(this);
        this.getNumberOrName = this.getNumberOrName.bind(this);

        this.onSelectCall = this.onSelectCall.bind(this);
    }

    onSelectCall() {
        this.props.selectCall(this.props.el.number.toString());
    }

    onNewContact(evt) {
        var newCont = new ContactModel(-1, false, this.props.el.number, this.props.el.number, "", "");
        ContactApp.contactList.push(newCont);

        CallList.persistentCallList.forEach((el) => {
            if(el.number == this.props.el.number) {
                el.contact = newCont;
            }
        })

        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: newCont}
        ]);
    }

    onOpenContact(evt) {
        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: this.state.contact}
        ]);
    }

    getIcon() {
        if(this.state.contact == null) {
            return (<img className="phoneCallListElementImg" src={url + "phone/icons/icon_cont_edit.png"} draggable={false} alt="The Icon of the Button" onClick={this.onNewContact} />);
        } else {
            return (<img className="phoneCallListElementImg" src={url + "phone/icons/icon_cont_person.png"} draggable={false} alt="The Icon of the Button" onClick={this.onOpenContact} />);
        }
    }

    getNumberOrName() {
        return (<div className="phoneCallListElementText">{
            this.state.contact == null ?
            formatPhoneNumber(this.props.el.number.toString()) :
            this.state.contact.name
        }</div>)
    }

    render() {
        return(
            <div className="phoneCallListElement">
                <div className="standardWrapper">
                    <img className="phoneCallListElementFirstIcon" src={url + "phone/icons/icon_" + this.props.el.icon + ".png"} draggable={false} alt="The Icon of the Button" />
                </div>
                {this.getNumberOrName()}
                <div className="phoneCallListElementText">{this.props.el.dateTime}</div>
                <div className="standardWrapper">
                    <img className="phoneCallListElementImg" src={url + "phone/icons/icon_cont_call.png"} draggable={false} alt="The Icon of the Button" onClick={this.onSelectCall} />
                </div>
                <div className="standardWrapper">
                    {this.getIcon()}
                </div>
            </div>
        );
    }
}

class CallElementModel {
    constructor(id, icon, number, dateTime, missed, check) {
        this.id = id;
        this.icon = icon;
        this.number = number;
        this.dateTime = dateTime;
        this.missed = missed;
        this.check = check;
    }
}

registerCallback("PHONE_ANSWER_CALLLIST", CallList.onAnswerCallList);
registerCallback("PHONE_CALLLIST_NEW", CallList.onCallListAddNew);

registerApp(CallApp, 0);