import React, { Fragment } from 'react';
import { registerApp, Icon, registerCallback } from '../../SmartphoneController';

import './ContactApp.css';

import { url } from './../../../../index';
import MessengerApp, { ChatModel } from '../MessengerApp/MessengerApp';
import PhoneCallApp, { PhoneCall, PhoneCallMember } from '../PhoneCallApp/PhoneCallApp';

export class ContactModel {
    constructor(id, favorit, number, name, note, email) {
        this.id = id;
        this.favorit = favorit;
        this.number = number;
        this.name = name;
        this.note = note;
        this.email = email;
    }
}

export default class ContactApp extends React.Component {

    static contactList = [];

    static createContactList(data) {
        var contArr = [];
        data.contacts.forEach((el) => {
            var obj = JSON.parse(el);
            contArr.push(new ContactModel(obj.id, obj.favorit, obj.number, obj.name, obj.note, obj.email));
        });

        ContactApp.contactList = contArr;
        ContactApp.sortContacts(contArr);
    }

    static setContactId(data) {
        var contact = ContactApp.contactList.filter((el) => {
            return el.id === -1;
        })[0];

        if(contact != undefined) {
            contact.id = data.newId;
        }
    }

    static sortContacts() {
        ContactApp.contactList.sort((a, b) => {
            if(a.favorit == b.favorit) {
                var aName = a.name.toUpperCase();
                var bName = b.name.toUpperCase();
                if(aName < bName) {
                    return -1;
                } else if(aName > bName) {
                    return 1;
                } else {
                    return 0;
                }
            } else if(a.favorit) {
                return -1;
            } else if(b.favorit){
                return 1;
            }
        })
    }

    constructor(props) {
        super(props);

        this.state = {
           currentContact: null,
        }

        this.onSelect = this.onSelect.bind(this);

        this.onSaveContact = this.onSaveContact.bind(this);
        this.onContactRemove = this.onContactRemove.bind(this);

        this.onNewContact = this.onNewContact.bind(this);
    }

    static hasTime() {
        return true;
    }

    static stopsMovement() {
        return true;
    }

    static hasVerticalMode() {
        return false;
    }

    static getIcon(callback) {
        return <Icon key={"contact"} icon="contact" column={2} row={5} missedInfo={0} callback={callback} type={ContactApp} />
    }

    static dispose() {
        ContactApp.contactList = [];
    }

    static deselect() { }

    triggerBackButton() {
        this.setState({
            currentContact: null,
        })
    }

    onSelect(contact) {
        this.setState({
            currentContact: contact,
        })
    }

    onSaveContact() {
        this.props.sendToServer("SMARTPHONE_CHANGE_CONTACT", this.state.currentContact);

        this.setState({
            currentContact: null,
        });

        ContactApp.sortContacts();
    }

    onContactRemove(contact) {
        this.setState({
            currentContact: null,
        });

        ContactApp.contactList = ContactApp.contactList.filter((el) => {
            return el !== contact;
        })

        this.props.sendToServer("SMARTPHONE_DELETE_CONTACT", {
            id: contact.id,
        });
    }

    onNewContact() {
        var newCont = new ContactModel(-1, false, "", "", "", "");
        ContactApp.contactList.push(newCont);

        this.setState({
            currentContact: newCont,
        })
    }

    selectShowElement() {
        if(this.state.currentContact != null) {
            return <ContactInfo contact={this.state.currentContact} onSave={this.onSaveContact} onRemove={this.onContactRemove} />;
        } else {
            return <ContactList onSelect={this.onSelect} onNewContact={this.onNewContact} openOtherApp={this.props.openOtherApp} changeState={this.props.changeState} requestState={this.props.requestState} sendToServer={this.props.sendToServer}/>
        }
    }

    render() {
        return (
            <div className="phoneContactWorkplace standardAppBackground">
                {this.selectShowElement()}
            </div>
        );
    }
}

class ContactInfo extends React.Component {
    constructor(props) {
        super(props);

        if(this.props.contact == null) {
            props.contact = new ContactModel(-1, false, "FEHLER", null, null);
        }
        
        this.state = {
            contact: props.contact,

            favorit: props.contact.favorit,
            name: props.contact.name,
            note: props.contact.note,
            number: props.contact.number,
            email: props.contact.email,
        }

        this.onChangeInfo = this.onChangeInfo.bind(this);

        this.onSave = this.onSave.bind(this);

        this.onRemove = this.onRemove.bind(this);
    }

    onChangeInfo(name, value) {
        this.state[name] = value;
    }

    onSave(evt) {
        var cont = this.state.contact;

        cont.favorit = this.state.favorit;
        cont.name = this.state.name;
        cont.note = this.state.note;
        cont.number = this.state.number;
        cont.email = this.state.email;

        this.props.onSave();
    }

    onRemove() {
        this.props.onRemove(this.state.contact);
    }
    
    render() {
        return (
            <div className="phoneContactInfo noSelect">
                <div className="phoneContactInfoDelete">
                    <div className="phoneContactInfoDeleteText">Kontakt löschen</div>
                    <img className="phoneContactInfoDeleteIcon" src={url + "phone/icons/icon_cont_del.png"} onClick={this.onRemove} />
                </div>
                <div className="phoneContactInfoImgWrapper">
                    <img className="phoneContactInfoImg" src={url + "phone/icons/icon_person_big.png"} draggable={false} alt="The Icon of the Button" />
                </div>
                <ContactInfoValueField title="Name" placeholder="Name" value={this.state.name} withBorder={true} valueName="name" onChangeInfo={this.onChangeInfo} maxLength={18} />
                <ContactInfoValueField title="Telefonnummer" placeholder="Telefonnummer" value={this.state.number} withBorder={true} valueName="number" onChangeInfo={this.onChangeInfo} maxLength={15} inputType="number" />
                <ContactInfoValueField title="E-Mail" placeholder="E-Mail" value={this.state.email} withBorder={true} valueName="email" onChangeInfo={this.onChangeInfo} maxLength={25} />
                <ContactInfoValueField title="Notiz" placeholder="Notiz" value={this.state.note} withBorder={true} valueName="note" onChangeInfo={this.onChangeInfo} maxLength={22} />   
                <ContactInfoCheckField title="Favorit" checked={this.state.favorit} withBorder={false} valueName="favorit" onChangeInfo={this.onChangeInfo} />
                <div />
                <div className="phoneContactInfoSave">
                    <div className="phoneContactInfoSaveIconWrapper">
                        <img className="phoneContactInfoSaveIcon" src={url + "phone/icons/icon_choicev.png"} draggable={false} alt="The Icon of the Button" />
                    </div>
                    <div className="phoneContactInfoSaveText" onClick={this.onSave}>Speichern</div>
                </div>
            </div>);
    }
}

class ContactInfoValueField extends React.Component {
    constructor(props) {
        super(props);

        this.state= {
            value: props.value,
        }

        this.onValueChange = this.onValueChange.bind(this);
    }

    onValueChange(evt) {
        if(this.props.inputType === "number") {
            if(isNaN(+evt.target.value)) {
                return;
            }
        }

        this.setState({
            value: evt.target.value,
        });

        this.props.onChangeInfo(this.props.valueName, evt.target.value);
    }

    render() {
        return (
            <div className={"phoneContactInfoValueField" + (this.props.withBorder ? " bottomBorder" : "")}>
                <div className="phoneContactInfoValueFieldTitle">{this.props.title}</div>
                <input type="text" className="phoneContactInfoValueFieldText" spellCheck={false} placeholder={this.props.placeholder} value={this.state.value} onChange={this.onValueChange} maxLength={this.props.maxLength} />
            </div>);
    }
}

class ContactInfoCheckField extends React.Component {
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

        this.props.onChangeInfo(this.props.valueName, evt.target.checked);
    }

    render() {
        return (
            <div className={"phoneContactInfoValueField" + (this.props.withBorder ? " bottomBorder" : "")}>
                <div className="phoneContactInfoValueFieldTitle">{this.props.title}</div>
                <div className="phoneContactInfoValueFieldCheckWrapper">
                    <input type="checkbox" className="phoneContactInfoValueFieldCheck" defaultChecked={this.state.checked} value={this.state.checked} onChange={this.onValueChange} />
                </div>
            </div>);
    }
}

class ContactList extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
        }

        this.onSearch = this.onSearch.bind(this);

        this.filterSearch = this.filterSearch.bind(this);
    }

    onSearch(value) {
        this.setState({
            searchValue: value,
        })
    }

    filterSearch(el) {
        if(this.state.searchValue != "") {
            return (el.name.toLowerCase().includes(this.state.searchValue.toLowerCase()) || el.note.toLowerCase().includes(this.state.searchValue.toLowerCase()));
        } else {
            return true;
        }
    }

    render() {
        return (
            <Fragment>
                <div className="phoneAppTitle">Kontaktliste</div>
                <div className="phoneContactsWrapper">
                    <ContactBar onSearch={this.onSearch} onNewContact={this.props.onNewContact} />
                    <div className="phoneContactList noSelect">
                        {ContactApp.contactList.filter(this.filterSearch).map((el) => {
                            return <ContactElement key={el.id} el={el} onSelect={this.props.onSelect} requestState={this.props.requestState} changeState={this.props.changeState} openOtherApp={this.props.openOtherApp} sendToServer={this.props.sendToServer}/>
                        })}
                    </div>
                </div>
            </Fragment>
        );
    }
}

class ContactBar extends React.Component { 
    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
        }

        this.onClick = this.onClick.bind(this);
        this.onValueChange = this.onValueChange.bind(this);
    }

    onClick(evt) {
        this.props.onNewContact();
    }

    onValueChange(evt) {
        this.setState({
            searchValue: evt.target.value,
        });

        this.props.onSearch(evt.target.value);
    }

    render() {
        return(
            <div className="phoneContactBar">
                <div className="phoneContactBarSearchWrapper">
                    <input type="search" className="phoneContactBarSearch" placeholder="Kontakt suchen" spellCheck={false} onChange={this.onValueChange} />
                </div>
                <div className="phoneContactBarNewWrapper">
                    <div className="phoneContactBarNewText standardWrapper">Neuer Kontakt</div>
                    <div className="standardWrapper">
                        <img className="phoneContactBarNewImg " src={url + "phone/icons/icon_cont_add.png"} draggable={false} alt="The Icon of the Button" onClick={this.onClick}/>
                    </div>
                </div>
            </div>);
    }
}

class ContactElement extends React.Component {
    constructor(props) {
        super(props);

        this.selectIcon = this.selectIcon.bind(this);

        this.onCallContact = this.onCallContact.bind(this);
        this.onMessageContact = this.onMessageContact.bind(this);
        this.onEditContact = this.onEditContact.bind(this);
    }

    selectIcon() {
        if(this.props.el.favorit) {
            return "cont_fav";
        } else {
            return "person";
        }
    }

    onCallContact(contact) {
        if(PhoneCallApp.PhoneCall == null) {
            var ownNumber = this.props.requestState("number");
            PhoneCallApp.PhoneCall = new PhoneCall(new PhoneCallMember(ownNumber, false, false),[
                new PhoneCallMember(contact.number, false, false),
            ], new Date(), 1, true);
    
            this.props.openOtherApp(PhoneCallApp, []);
    
            PhoneCallApp.startCallSound(this.props.changeState);
    
            this.props.sendToServer("PHONE_START_CALL", {
                owner: ownNumber,
                number: contact.number,
            });
        }
    }

    onMessageContact(contact) {
        var chat = MessengerApp.ChatList.filter((el) => {
            return el.number == contact.number;
        })[0];
        
        if(chat != undefined) {
            this.props.openOtherApp(MessengerApp, [
                {name: "currentChat", value: chat}
            ]);
        } else {
            var newChat = new ChatModel(-1, contact.number, 0, new Date(), "");
            MessengerApp.ChatList.push(newChat);
            this.props.openOtherApp(MessengerApp, [
                {name: "currentChat", value: newChat}
            ]);
        }
    }

    onEditContact(contact) {
        this.props.onSelect(contact);
    }
    
    render() {
        return(
            <div className="phoneContactListElement">
                <div className="standardWrapper">
                    <img className="phoneContactListElementImg" src={url + "phone/icons/icon_" + this.selectIcon() + ".png"} draggable={false} alt="The Icon of the Button" />
                </div>
                <div className="phoneContactListElementText">
                    <div className="phoneContactListElementTextUpper">{this.props.el.name}</div>
                    <div className="phoneContactListElementTextLower">{this.props.el.note}</div>
                </div>
                <div className="phoneContactListElementButtons">
                    <ContactElementButton icon="cont_call" callback={this.onCallContact} el={this.props.el} />
                    <ContactElementButton icon="cont_sms" callback={this.onMessageContact} el={this.props.el} />
                    <ContactElementButton icon="cont_edit" callback={this.onEditContact} el={this.props.el} />
                </div>
            </div>
        );
    }
}

class ContactElementButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.callback(this.props.el);
    }

    render() {
        return (
            <div className="phoneContactElementButtonWrapper">
                <img className="phoneContactElementButtonImg" src={url + "phone/icons/icon_" + this.props.icon + ".png"} draggable={false} alt="The Icon of the Button" onClick={this.onClick}/>
            </div>
        );
    }
}


registerCallback("PHONE_CONTACT_SET_ID", ContactApp.setContactId);
registerApp(ContactApp, 0);