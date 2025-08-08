import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon, ConfirmationType } from '../../SmartphoneController';

import ContactApp, { ContactModel } from '../ContactApp/ContactApp';

import { formatPhoneNumber } from '../CallApp/CallApp';

import './MessengerApp.css';

import { url } from './../../../../index';
import { AllSettings } from '../SettingsApp/SettingsApp';
//import { isString } from 'react-pdf/dist/umd/shared/utils';

export default class MessengerApp extends React.Component {
    static ChatList = [];
    static CurrentChat = null;

    static onAnswerMessengerChats(sender, data) {
        data.chats.forEach((el) => {
            var obj = JSON.parse(el);
            var already = MessengerApp.ChatList.filter((el) => {
                return el.id == obj.id;
            })[0];

            if(already == undefined) {
                MessengerApp.ChatList.push(new ChatModel(obj.id, obj.number, obj.missed, obj.lastD, obj.lastM));
            } else {
                already = new ChatModel(obj.id, obj.number, obj.missed, obj.lastD, obj.lastM);
            }
        });

        sender.forceUpdate();
        MessengerApp.sortChats();
    }

    static onAnswerMessengerMessenges(sender, data) {
        var id = data.id;
        var chat = MessengerApp.ChatList.filter((el) => {
            return el.id == id;
        })[0];

        if(chat != undefined) {
            data.messages.forEach((el) => {
                var obj = JSON.parse(el);
                chat.messages.push(new ChatMessageModel(obj.id, obj.text, obj.date, obj.from, obj.read));

                if(!obj.read) {
                    chat.missedMessageCount++;
                }
            });

            MessengerApp.sortMessenges(chat);
            sender.forceUpdate();
        } else {
            console.log("error: onAnswerMessengerMessenges: chat not found");
        }
    }

    static onReceiveMessengerMessage(sender, data) {
        var chatId = data.chatId;
        var chat = MessengerApp.ChatList.filter((el) => {
            return el.id == chatId;
        })[0];

        if(chat == undefined) {
            chat = MessengerApp.ChatList.filter((el) => {
                return el.id == -1;
            })[0];

            if(chat != undefined) {
                chat.id = data.chatId;
            }
        }

        if(chat != undefined) {
            if(chat.messages.length == 0) {
                var settings = sender.onRequestState("settings");
                if(!shown || currentApp != MessengerApp) {
                    var player = new Audio(url + "/phone/shortVibration.mp3");
                    player.load();
                    player.volume = (0.1 * settings[AllSettings.VOLUME]).toFixed(2);
                    player.loop = false;
                    player.play();
                }

                sender.requestData("PHONE_MESSENGER_REQUEST_MESSAGES", {id: chat.id, currentlySelected: false});
                return;
            }

            var shown = sender.onRequestState("shown");
            var currentApp = sender.onRequestState("CurrentApp");
            var settings = sender.onRequestState("settings");

            var obj = JSON.parse(data.message);
            chat.messages.push(new ChatMessageModel(obj.id, obj.text, new Date(obj.date), obj.from, obj.read || MessengerApp.CurrentChat == chat));
            if(!obj.read && MessengerApp.CurrentChat != chat) {
                chat.missedMessageCount++;
            }

            if(MessengerApp.CurrentChat == chat) {
                sender.sendToServer("PHONE_MESSENGER_READ_SINGLE_MESSAGE", {id: obj.id})
            }

            chat.lastD = new Date(obj.date);
            chat.lastM = obj.text.substring(0, 35);

            MessengerApp.sortMessenges(chat);

            if(!shown || currentApp != MessengerApp) {
                var player = new Audio(url + "/phone/shortVibration.mp3");
                player.load();
                player.volume = 0.1 * settings[AllSettings.VOLUME].toFixed(2);
                player.loop = false;
                player.play();
            }

            sender.forceUpdate();
        } else {
            var obj = JSON.parse(data.message);
            MessengerApp.ChatList.push(new ChatModel(data.chatId, obj.from, 1, new Date(), obj.text));
        }
    }

    static sortChats() {
        MessengerApp.ChatList.sort((a, b) => {
            if(a.missedUpdate == b.missedUpdate) {
                if(a.lastMessageDate > b.lastMessageDate) {
                    return -1;
                } else if(a.lastMessageDate < b.lastMessageDate) {
                    return 1;
                } else {
                    return 0;
                }
            } else if(a.missedUpdate) {
                return -1;
            } else if(b.missedUpdate){
                return 1;
            }
        })
    }

    static sortMessenges(chat) {
        chat.messages.sort((a, b) => {
            if(a.sendDate == b.sendDate) {
                return 0;
            } else if(a.sendDate < b.sendDate) {
                return -1;
            } else if(a.sendDate > b.sendDate){
                return 1;
            }
        })
    }

    constructor(props) {
        super(props);

        this.state = {
           currentChat: null,
        }

        this.onSelectChat = this.onSelectChat.bind(this);
        this.startNewConversation = this.startNewConversation.bind(this);
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
        const reducer = (accumulator, currentValue) => accumulator + currentValue.missedMessageCount;
        return <Icon key={"sms"} icon="sms" column={3} row={5} missedInfo={MessengerApp.ChatList.filter((el) => {return el.missedMessageCount > 0}).reduce(reducer, 0)} callback={callback} type={MessengerApp} />
    }

    static dispose() {
        MessengerApp.ChatList = [];
    }

    static deselect() { 
        MessengerApp.CurrentChat = null;
    }

    componentDidMount() {     
        MessengerApp.sortChats();
    }

    startNewConversation(number) {
        if(number == 0) {
            return;
        }
        
        var chat = MessengerApp.ChatList.filter((el) => {
            return el.number == number;
        })[0];

        if(chat != undefined) {
            this.setState({
                currentChat: chat,
            });
            MessengerApp.CurrentChat = chat;

            return;
        }

        var newChat = new ChatModel(-1, number, 0, new Date(), "");
        MessengerApp.ChatList.push(newChat);
        this.setState({
            currentChat: newChat,
        })
        MessengerApp.CurrentChat = newChat;
    }

    triggerBackButton() {
        MessengerApp.CurrentChat = null;
        this.setState({
            currentChat: null,
        })
    }

    onSelectChat(chat) {
        MessengerApp.CurrentChat = chat;

        if(chat.missedMessageCount > 0) {
            this.props.sendToServer("PHONE_MESSENGER_READ_MESSAGES", {
                id: chat.id,
            })

            chat.messages.forEach((el) => {
                el.read = true;
            });
        
            chat.missedMessageCount = 0;
        }

        this.setState({
            currentChat: chat,
        });
    }

    selectShowElement() {
        if(this.state.currentChat != null) {
            return <ChatInfo chat={this.state.currentChat} requestData={this.props.requestData} sendToServer={this.props.sendToServer} requestState={this.props.requestState} changeState={this.props.changeState} openOtherApp={this.props.openOtherApp} requestConfirmation={this.props.requestConfirmation} changeSetting={this.props.changeSetting} />;
        } else {
            return <ChatList callback={this.onSelectChat} openOtherApp={this.props.openOtherApp} startNewConversation={this.startNewConversation}/>;
        }
    }

    render() {
        return(
            <div className="phoneMessengerWorkplace standardAppBackground">
                {this.selectShowElement()}
            </div>
        )
    }
}

//ChatInfo

class ChatInfo extends React.Component{
    constructor(props) {
        super(props);

        var cand = ContactApp.contactList.filter((el) => {
            return el.number == props.chat.number;
        })[0];

        var contact = cand != undefined ? cand : null;

        this.state = {
            messageValue: "",
            contact: contact
        }
    }

    componentDidMount() {
        if(this.props.chat.messages.length == 0) {
            this.props.requestData("PHONE_MESSENGER_REQUEST_MESSAGES", {id: this.props.chat.id, currentlySelected: true})
        }

        this.scrollToBottom();
    }

    scrollToBottom = () => {
        this.messagesEnd.scrollIntoView({ behavior: "auto" });
    }

    componentDidUpdate() {
        this.messagesEnd.scrollIntoView({ behavior: "auto" });
    }

    render() {
        var number = this.props.requestState("number");
        return (
            <div className="phoneMessengerChatMessengeWholeWrapper">
                <ChatInfoTopBar contact={this.state.contact} chat={this.props.chat} openOtherApp={this.props.openOtherApp} />
                <div className="phoneMessengerChatMessengeListWrapper" ref={(ref) => this.list = ref}>
                    <div className="phoneMessengerChatMessengeList">
                        {this.props.chat.messages.map((el) => {
                            return <ChatMessages el={el} number={number} />
                        })}
                        <div style={{height: "1vw", width: "1vw", clear: "both" }}ref={(el) => { this.messagesEnd = el; }} />
                    </div>
                </div>
                <ChatInfoBottomBar requestConfirmation={this.props.requestConfirmation} requestState={this.props.requestState} changeState={this.props.changeState} sendToServer={this.props.sendToServer} chat={this.props.chat} number={number} changeSetting={this.props.changeSetting} />
            </div>);
    }
}

class ChatInfoTopBar extends React.Component {
    constructor(props) {
        super(props);

        this.onSelectContact = this.onSelectContact.bind(this);
        this.onNewContact = this.onNewContact.bind(this);
    }

    onSelectContact() {
        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: this.props.contact}
        ]);
    }

    onNewContact() {
        var newCont = new ContactModel(-1, false, this.props.chat.number, "", "", "");
        ContactApp.contactList.push(newCont);

        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: newCont}
        ]);
    }

    getTopInfo() {
        if(this.props.contact != null) {
            if(this.props.contact.name.length > 12) {
                return this.props.contact.name.substring(0, 12) + "..";
            } else {
                return this.props.contact.name;
            }
        } else {
            return this.props.chat.number.toString().substring(0, 18);
        }
    }

    getTopButton() {
        if(this.props.contact != null) {
            return <img className="phoneMessengerChatMessengeTopButton" src={url + "phone/icons/icon_cont_person.png"} onClick={this.onSelectContact} />
        } else {
            return <img className="phoneMessengerChatMessengeTopButton" src={url + "phone/icons/icon_cont_edit.png"} onClick={this.onNewContact} />
        }
    }

    render() {
        return(
            <div className="phoneMessengerChatMessengeTopInfo">
                <div className="phoneMessengerChatMessengeTopNameNumber">
                    <div>{this.getTopInfo()}</div>
                </div>
                <div className="standardWrapper">
                    {this.getTopButton()}
                </div>
                <div />
            </div>
        );
    }
}

class ChatInfoBottomBar extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            value: "",
        }

        this.onSendMessenge = this.onSendMessenge.bind(this);
        this.onMessengeValueChange = this.onMessengeValueChange.bind(this);
        this.onMessengeKeyUp = this.onMessengeKeyUp.bind(this);

        this.onConfirmSend = this.onConfirmSend.bind(this);
    }

    onMessengeValueChange(evt) {
        this.setState({
            value: evt.target.value,
        })
    }

    onMessengeKeyUp(evt) {
        if(evt.keyCode === 13) { //ENTER
            evt.preventDefault();
            this.onSendMessenge();
        }
    }

    onSendMessenge() {
        if(this.state.value === "") {
            return;
        }

        var settings = this.props.requestState("settings");
        if(settings[AllSettings.FLY_MODE]) {
            this.props.requestConfirmation(ConfirmationType.YES_NO, "Flugmodus deaktivieren um abzuschicken?", this.onConfirmSend, null);
        } else {
            this.props.sendToServer("PHONE_MESSENGER_SEND_MESSAGE", {
                chatId: this.props.chat.id,
                from: this.props.number,
                to: this.props.chat.number,
                text: this.state.value,
            });

            this.setState({
                value: "",
            });
        }
    }

    onConfirmSend() {
        this.props.changeSetting(AllSettings.FLY_MODE, false);
        this.props.sendToServer("PHONE_MESSENGER_SEND_MESSAGE", {
            chatId: this.props.chat.id,
            from: this.props.number,
            to: this.props.chat.number,
            text: this.state.value,
        });

        this.setState({
            value: "",
        });
    }

    render() {
        return(
            <div className="phoneMessengerChatMessengeInput">
                <div className="standardWrapper">
                    <input type="text" className="phoneMessengerChatMessengeInputField" spellCheck={false} value={this.state.value} onChange={this.onMessengeValueChange} onKeyUp={this.onMessengeKeyUp}/>
                </div>
                <div className="standardWrapper">
                    <img className="phoneMessengerChatMessengeInputButton" src={url + "phone/icons/icon_message_send.png"} onClick={this.onSendMessenge} />
                </div>
            </div>);
    }
}

class ChatMessages extends React.Component {
    constructor(props) {
        super(props);
    }

    getDateString(date) {
        return date.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }) + " " + `${date.getDate().toString().padStart(2, '0')}.${(date.getMonth()+1).toString().padStart(2, '0')}`;
    }

    render() {
        return (
            <div className="phoneMessengerChatMessengeWrapper">
                <div className={"phoneMessengerChatMessenge " + (this.props.el.fromNumber == this.props.number ? "rightMessenges" : "leftMessenges")}>
                    <div className="standardWrapper">
                        <div className="phoneMessengerChatAllTextWrapper">
                            <div className="phoneMessengerChat">{this.props.el.text}</div>
                            <div className={"phoneMessengerDate " + (this.props.el.fromNumber == this.props.number ? "rightDate" : "leftDate") }>{this.getDateString(this.props.el.sendDate)}</div>
                        </div>
                    </div>
                </div>
            </div>);
    }
}

//ChatList

class ChatList extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
        }

        this.onSearchValueChange = this.onSearchValueChange.bind(this);
    }

    onSearchValueChange(value) {
        this.setState({
            searchValue: value,
        })
    }

    render() {
        return(
            <Fragment>
                <div className="phoneAppTitle noSelect">Nachrichten</div>
                <ChatSearchBar searchValueChange={this.onSearchValueChange} startNewConversation={this.props.startNewConversation} />
                <div className="phoneMessengerChatList noSelect">
                    {MessengerApp.ChatList.map((el) => {
                        console.log(el);
                        return <ChatElement key={el.id} el={el} searchValue={this.state.searchValue} callback={this.props.callback} openOtherApp={this.props.openOtherApp}/>
                    })}
                </div>
            </Fragment>
        )
    }
}

class ChatSearchBar extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            searchValue: "",
            convValue: "",
        }

        this.onSearchChange = this.onSearchChange.bind(this);
        this.onConversationChange = this.onConversationChange.bind(this);
        this.onStartNewConversation = this.onStartNewConversation.bind(this);
        this.onKeyUp = this.onKeyUp.bind(this);
    }

    onSearchChange(evt) {
        this.setState({
            searchValue: evt.target.value,
        })

        this.props.searchValueChange(evt.target.value)
    }

    onKeyUp(evt) {
        if(evt.keyCode === 13) { //ENTER
            evt.preventDefault();
            this.onStartNewConversation();
        }
    }

    onStartNewConversation() {
        if(!isNaN(this.state.convValue)) {
            this.props.startNewConversation(this.state.convValue);
        }
    }

    onConversationChange(evt) {
        this.setState({
            convValue: evt.target.value,
        })
    }

    render() {
        return (
        <div className="phoneChatBar">
            <div className="standardWrapper">
                <input type="text" className="phoneContactBarField" placeholder="Konv. suchen" spellCheck={false} value={this.state.searchValue} onChange={this.onSearchChange} />
            </div>
            <div className="standardWrapper">
                <input type="text" className="phoneContactBarField" placeholder="Konv. starten" spellCheck={false} value={this.state.convValue} onChange={this.onConversationChange} onKeyUp={this.onKeyUp} />
            </div>
            <div className="standardWrapper">
            <img className="phoneMessengerChatElementButton" src={url + "phone/icons/icon_cont_sms.png"} onClick={this.onStartNewConversation} />
            </div>
        </div>);
    }
}

class ChatElement extends React.Component {
    constructor(props) {
        super(props);

        var cand = ContactApp.contactList.filter((el) => {
            return el.number == props.el.number;
        })[0];

        var contact = cand != undefined ? cand : null;

        this.state = {
            contact: contact,
        }

        this.onClick = this.onClick.bind(this);
        this.onSelectContact = this.onSelectContact.bind(this);
        this.onNewContact = this.onNewContact.bind(this);
        
    }

    onSelectContact() {
        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: this.state.contact}
        ]);
    }

    onNewContact() {
        var newCont = new ContactModel(-1, false, this.props.el.number, this.props.el.number, "", "");
        ContactApp.contactList.push(newCont);

        this.props.openOtherApp(ContactApp, [
            {name: "currentContact", value: newCont}
        ]);
    }

    onClick() {
        this.props.callback(this.props.el);
    }

    getDateString(date) {
        return `${date.getDate().toString().padStart(2, '0')}.${(date.getMonth()+1).toString().padStart(2, '0')}`
    }

    getButton() {
        if(this.state.contact != null) {
            return (<img className="phoneMessengerChatElementButton" src={url + "phone/icons/icon_cont_person.png"} onClick={this.onSelectContact} />);
        } else {
            return (<img className="phoneMessengerChatElementButton" src={url + "phone/icons/icon_cont_edit.png"} onClick={this.onNewContact} />);;
        }
    }

    filter() {
        var el = this.props.el;
        var search = this.props.searchValue;
        if(search != "") {
            return el.number.toString().toLowerCase().includes(search.toLowerCase()) || (this.state.contact != null && this.state.contact.name.toLowerCase().includes(search.toLowerCase()));
        } else {
            return true;
        }
    }

    getPreview() {
        if(this.props.el.messages.length == 0) {
            return this.props.el.lastMessagePreview.toString().substring(0, 33);
        } else {
            return this.props.el.messages[this.props.el.messages.length - 1].text.substring(0, 33);
        }
    }

    getMissedMessageBubble() {
        if(this.props.el.messages.length == 0) {
            if(this.props.el.missedMessageCount > 0) {
                return (
                    <div className="phoneMessengerChatElementMissedWrapper">
                        <div className="phoneMessengerChatElementMissed">{this.props.el.missedMessageCount > 99 ? 99 : this.props.el.missedMessageCount}</div>
                    </div>
                );
            } else {
                return null;
            }
        } else {
            var notRead = this.props.el.messages.filter((el) => { return !el.read});
            if(notRead.length > 0) {
                return (
                    <div className="phoneMessengerChatElementMissedWrapper">
                        <div className="phoneMessengerChatElementMissed">{notRead.length > 99 ? 99 : notRead.length}</div>
                    </div>
                );
            } else {
                return null;
            }
        }
    }

    render() {
        if(this.filter()) {
            var name = this.state.contact != null ? this.state.contact.name : formatPhoneNumber(this.props.el.number.toString());
            if(typeof name === 'string') {
                name = name.substring(0, 15);
            } else {
                name = "Fehler";
            }

            return (
                <div className="phoneMessengerChatElement">
                    <div className="phoneMessengerChatElementInfo phoneChatTextWrapper" onClick={this.onClick}>{name}</div>
                    <div className="phoneMessengerChatElementPreview phoneChatTextWrapper" onClick={this.onClick}>{this.getPreview()}</div>
                    <div className="phoneMessengerChatElementDateMissed">
                        <div className="phoneMessengerChatElementDate">{this.getDateString(this.props.el.lastMessageDate)}</div>
                        {this.getMissedMessageBubble()}
                    </div>
                    <div className="standardWrapper">
                        {this.getButton()}
                    </div>
                </div>
            );
        } else {
            return null;
        }
    }
}

export class ChatModel {
    constructor(id, number, missedMessageCount, lastMessageDate, lastMessagePreview) {
        this.id = id;
        this.number = number;
        this.missedMessageCount = missedMessageCount;
        this.lastMessageDate = new Date(lastMessageDate);
        this.lastMessagePreview = lastMessagePreview;
        this.messages = [];
    }
}

class ChatMessageModel {
    constructor(id, text, sendDate, fromNumber, read) {
        this.id = id;
        this.text = text;
        this.sendDate = new Date(sendDate);
        this.fromNumber = fromNumber;
        this.read = read;
    }
}

registerCallback("PHONE_ANSWER_MESSENGER_CHATS", MessengerApp.onAnswerMessengerChats);
registerCallback("PHONE_MESSENGER_ANSWER_MESSAGES", MessengerApp.onAnswerMessengerMessenges);

registerCallback("PHONE_MESSENGER_MESSAGE_RECEIVE", MessengerApp.onReceiveMessengerMessage);

registerApp(MessengerApp, 0);