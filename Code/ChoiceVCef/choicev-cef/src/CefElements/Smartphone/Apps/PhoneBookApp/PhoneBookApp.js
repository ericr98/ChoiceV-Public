import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon } from '../../SmartphoneController';

import { formatPhoneNumber } from '../CallApp/CallApp';
import PhoneCallApp, { PhoneCall, PhoneCallMember } from '../PhoneCallApp/PhoneCallApp';

import './PhoneBookApp.css';

import { url } from './../../../../index';

var cats = [
    "Restaurant",
    "Hotel",
    "Tes"
]

export default class PhoneBookApp extends React.Component {
    static entryList = [];

    static getNameOfNumber(number) {
        var obj = PhoneBookApp.entryList.find((el) => {
            return el.number == number;
        });

        if (obj != null) {
            return obj.name;
        } else {
            return null;
        }
    }

    static onPhoneBookAnswer(sender, data) {
        data.entries.forEach((el) => {
            var obj = JSON.parse(el);
            PhoneBookApp.entryList.push(new PhoneBookEntry(obj.name, obj.number));
        });
        sender.forceUpdate();
    }

    static queryPhoneBookEntries(sender) {
        if (PhoneBookApp.entryList.length == 0) {
            sender.sendToServer("PHONE_PHONE_BOOK_REQUEST", {});
        }
    }

    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
        }

        this.onSearchValueChange = this.onSearchValueChange.bind(this);
    }

    static hasTime() {
        return true;
    }

    static stopsMovement() {
        return true;
    }

    static dispose() {
        PhoneBookApp.entryList = [];
    }

    static deselect() { }

    static hasVerticalMode() {
        return false;
    }

    static getIcon(callback) {
        return <Icon key={"phonebook"} icon="phonebook" column={4} row={5} missedInfo={0} callback={callback} type={PhoneBookApp} />
    }

    getElementsBasedOnSearch(search) {
        if (search.length == 0) {
            return PhoneBookApp.entryList;
        } else {
            return PhoneBookApp.entryList.filter((el) => {
                return el.name.toLowerCase().includes(search.toLowerCase());
            });
        }
    }

    onSearchValueChange(search) {
        this.setState({
            searchValue: search,
        });
    }

    render() {
        return (
            <div className="fullWrapper standardAppBackground">
                <div className="phoneAppTitle">Telefonbuch</div>
                <div className="fullWrapper">
                    <PhoneBookSearchBar onSearch={this.onSearchValueChange}/> 
                    <div className="phoneContactList">
                        {this.getElementsBasedOnSearch(this.state.searchValue).map((el) => {
                            return (<PhoneBookEntryShow el={el} requestState={this.props.requestState} openOtherApp={this.props.openOtherApp} sendToServer={this.props.sendToServer}/>);
                        })}
                    </div>
                </div>
            </div>
        )
    }
}

class PhoneBookEntryShow extends React.Component {
    constructor(props) {
        super(props);


        this.onClick = this.onClick.bind(this);
    }

    onClick(contact) {
        if (PhoneCallApp.PhoneCall == null) {
            var ownNumber = this.props.requestState("number");
            PhoneCallApp.PhoneCall = new PhoneCall(new PhoneCallMember(ownNumber, false, false), [
                new PhoneCallMember(this.props.el.number, false, false),
            ], new Date(), 1, true);

            this.props.openOtherApp(PhoneCallApp, []);

            PhoneCallApp.startCallSound(this.props.changeState);

            this.props.sendToServer("PHONE_START_CALL", {
                owner: ownNumber,
                number: this.props.el.number,
            });
        }
    }

    cutNameToLength(name) {
        if (name.length > 30) {
            return name.slice(0, 27) + "...";
        } else {
            return name;
        }
    }

    render() {
        return(
            <div className="phonePhoneBookListElement">
                <div className='phonePhoneBookListElementName'>{this.cutNameToLength(this.props.el.name)}</div>
                <div className='phonePhoneBookListElementNumber'>{formatPhoneNumber(this.props.el.number.toString())}</div>
                <div className="standardWrapper phonePhoneBookListElementButtonWrapper">
                    <img className="phonePhoneBookListElementButtonImg" src={url + "phone/icons/icon_cont_call.png"} draggable={false} alt="The Icon of the Button" onClick={this.onClick}/>
                </div>
            </div>
        );
    }
}

class PhoneBookEntry {
    constructor(name, number) {
        this.name = name;
        this.number = number;
    }
}

class PhoneBookSearchBar extends React.Component { 
    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
        }

        this.onValueChange = this.onValueChange.bind(this);
    }

    onValueChange(evt) {
        this.setState({
            searchValue: evt.target.value,
        });

        this.props.onSearch(evt.target.value);
    }

    render() {
        return(
            <div className="phonePhoneBookBar">
                <div className="phonePhoneBookBarSearchWrapper">
                    <input type="search" className="phonePhoneBookBarSearch" placeholder="Eintrag suchen" spellCheck={false} onChange={this.onValueChange} />
                </div>
            </div>);
    }
}


registerCallback("PHONE_PHONE_BOOK_ANSWER", PhoneBookApp.onPhoneBookAnswer);

registerApp(PhoneBookApp, 0);