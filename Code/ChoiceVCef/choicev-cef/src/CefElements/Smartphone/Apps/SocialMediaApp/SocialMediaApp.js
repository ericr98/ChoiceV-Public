import React, { Fragment } from 'react';

import { registerApp, registerCallback, Icon, ConfirmationType } from '../../SmartphoneController';
import { AllSettings } from '../SettingsApp/SettingsApp';

import './SocialMediaApp.css';

import { url } from './../../../../index';


export default class SocialMediaApp extends React.Component {
    static onReceiveAccounts(sender, data) {
        SocialMediaApp.Accounts = data.accounts;
            
        sender.forceUpdate();
    }

    static onReceiveAccountCredentials(sender, data) {
        SocialMediaApp.FrameData = data;

        if(SocialMediaApp.FrameData != null && SocialMediaApp.DiscordToken != null) {
            SocialMediaApp.CurrentScreen = "FRAME";

            var settings = sender.onRequestState("settings");
            if(settings[AllSettings.SOCIAL_MEDIA_HORIZONTAL_START]) {
                sender.onStateChange("horizontal", true);
            }

            sender.forceUpdate();
        }
    }

    static onAnswerAccountCreation(sender, data) {
        if(data.success) {
            sender.onRequestConfirmation(ConfirmationType.NOTIFICATION, data.message, () => {
                SocialMediaApp.CurrentScreen = "ACCOUNT_SELECT";
                SocialMediaApp.AccountRefresh = true;
                sender.forceUpdate();
            });
        } else {
            sender.onRequestConfirmation(ConfirmationType.NOTIFICATION, data.message);
        }
    }

    static Accounts = [];
    static AccountRefresh = false;
    static CurrentScreen = null;
    static FrameData = null;
    static DiscordToken = null;

    constructor(props) {
        super(props);

        if(SocialMediaApp.CurrentScreen == null) {
            SocialMediaApp.CurrentScreen = this.props.horizontal ? "FRAME" : "ACCOUNT_SELECT";
        }

        this.onDiscordTokenCallback = this.onDiscordTokenCallback.bind(this);
        this.onGoToRegistrationScreen = this.onGoToRegistrationScreen.bind(this);
    }

    componentDidMount() {
        // var settings = this.props.requestState("settings");
        // if(!settings[AllSettings.SOCIAL_MEDIA_DIRECT_START]) {
        //     if(SocialMediaApp.CurrentScreen == "ACCOUNT_SELECT") {
        //         this.props.sendToServer("REQUEST_SOCIAL_MEDIA_ACCOUNTS", {});
        //     }
        // }

        if(SocialMediaApp.CurrentScreen == "ACCOUNT_SELECT") {
            this.props.sendToServer("REQUEST_SOCIAL_MEDIA_ACCOUNTS", {});
        }
    }

    componentDidUpdate() {
        if(SocialMediaApp.AccountRefresh) {
            this.props.sendToServer("REQUEST_SOCIAL_MEDIA_ACCOUNTS", {});
            SocialMediaApp.AccountRefresh = false;
        }
    }

  
    static hasTime() {
        return false;
    }

    static stopsMovement() {
        return true;
    }

    static hasVerticalMode() {
        return SocialMediaApp.CurrentScreen == "FRAME";
    }

    static getIcon(callback) {
        return <Icon key={"connect"} icon="connect" column={3} row={4} missedInfo={0} callback={callback} type={SocialMediaApp} />
    }

    static dispose() {
        //DO NOTHING
    }

    static deselect() {
        SocialMediaApp.CurrentScreen = null;
    }

    componentWillUnmount() {
        var settings = this.props.requestState("settings");
        if(this.props.horizontal && settings[AllSettings.SOCIAL_MEDIA_HORIZONTAL_START]) {
            this.props.changeState("horizontal", false);
        }
    }

    onDiscordTokenCallback(token) {
        SocialMediaApp.DiscordToken = token;

        if(SocialMediaApp.FrameData != null && SocialMediaApp.DiscordToken != null) {
            SocialMediaApp.CurrentScreen = "FRAME";

            var settings = this.props.requestState("settings");
            if(!this.props.horizontal && settings[AllSettings.SOCIAL_MEDIA_HORIZONTAL_START]) {
                this.props.changeState("horizontal", true);
            }

            this.forceUpdate();
        }
    }

    onGoToRegistrationScreen() {
        SocialMediaApp.CurrentScreen = "REGISTRATION";
        this.forceUpdate();
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
        if (!this.checkIfProceed()) {
            return (<div className='phoneSocialMediaAccountSelectGrid'>
                        <div className='standardWrapper'>
                            <img src={url + "phone/socialmedia/vconnect_logo.png"} className='phoneSocialMediaAccountSelectLogo'/>
                        </div>
                    </div>)
        } else {
            if (SocialMediaApp.CurrentScreen == "FRAME") {
                var urlStart = SocialMediaApp.FrameData.urlStart;
                var guid = SocialMediaApp.FrameData.guid;
                var token = SocialMediaApp.FrameData.token;
                var userId = SocialMediaApp.FrameData.userId;
                var schema = SocialMediaApp.FrameData.schema;
                var discordToken = SocialMediaApp.DiscordToken;

                return (<SocialMediaIFrame url={`${urlStart}?userId=${userId}&schema=${schema}&discordToken=${discordToken}&token=${token}&guid=${guid}`} />);
            } else if (SocialMediaApp.CurrentScreen == "ACCOUNT_SELECT") {
                return (<SocialMediaAccountSelect accounts={SocialMediaApp.Accounts} sendToServer={this.props.sendToServer} auth={this.props.auth} onDiscordTokenCallback={this.onDiscordTokenCallback} onGoToRegistrationScreen={this.onGoToRegistrationScreen} />);
            } else {
                return (<SocialMediaAccountCreation sendToServer={this.props.sendToServer} requestConfirmation={this.props.requestConfirmation} />)
            }
        }
    }
}

registerApp(SocialMediaApp, 0);

class SocialMediaAccountSelect extends React.Component {
    render() {
        return (
            <div className='phoneSocialMediaAccountSelectGrid'>
                <div className='standardWrapper'>
                    <img src={url + "phone/socialmedia/vconnect_logo.png"} className='phoneSocialMediaAccountSelectLogo'/>
                </div>
                <div className="phoneSocialMediaAccountSelectText">
                    <div />
                    <div className="standardWrapper phoneSocialMediaAccountSelectTextLogin">LOG IN</div>
                    <div className="standardWrapper phoneSocialMediaAccountSelectTextText">Wähle deinen Account!</div>
                </div>
                <div className='standardWrapper'>
                    <div className="phoneSocialMediaAccountSelectAccounts">
                        {this.props.accounts.map((el) => {
                            return <SocialMediaAccount el={el} sendToServer={this.props.sendToServer} auth={this.props.auth} onDiscordTokenCallback={this.props.onDiscordTokenCallback}/>
                        })}
                    </div>
                </div>
                <div className="standardWrapper">
                    {this.props.accounts.filter((el) => el.type == 0).length < 3 ? 
                    <button className="phoneSocialMediaAccountLoginButton" onClick={this.props.onGoToRegistrationScreen}>Jetzt registrieren</button>
                    : <div />}
                </div>
                <div className="phoneSocialMediaSmallPrint standardWrapper">
                    *Indem Sie sich anmelden, stimmen Sie zu, dass Ihre Daten für gezielte Werbung verwendet werden. Connect übernimmt keine Verantwortung für verlorene Produktivität oder unerwartete Diskussionen über unwahre politische Themen.
                </div>
            </div>
        );
    }
}

class SocialMediaAccount extends React.Component {
    constructor(props) {
        super(props);
        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.sendToServer("REQUEST_SOCIAL_MEDIA_CREDENTIALS", {
            userName: this.props.el.userName
        });

        this.props.auth.requestDisordToken(this.props.onDiscordTokenCallback);
    }

    render() {
        var iconUrl = url + "phone/socialmedia/icon_user.png";
        if(this.props.el.type === 1) {
            iconUrl = url + "phone/socialmedia/icon_company.png";
        }

        var name = this.props.el.firstName + " " + this.props.el.lastName;

        if(name.length > 20) {
            name = name.substring(0, 17) + "..";
        }

        return (
            <div className="phoneSocialMediaAccountWrapper">
                <div className="phoneSocialMediaAccount" onClick={this.onClick}>
                    <img src={iconUrl} className="phoneSocialMediaAccountIcon"></img>
                    <div className="phoneSocialMediaAccountInfo">
                        <div />
                        <div className="phoneSocialMediaAccountInfoName standardWrapper">{name}</div>
                        <div className="phoneSocialMediaAccountInfoUsername standardWrapper">@{this.props.el.userName}</div>
                        <div />
                    </div>
                </div>
            </div>
        );
    }
}

class SocialMediaAccountCreation extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            userName: "",
            firstName: "",
            lastName: "",
            title: "",
            phoneNumber: ""
        }

        this.onChangeInput = this.onChangeInput.bind(this);
        this.onClickRegistration = this.onClickRegistration.bind(this);
    }

    onChangeInput(key, value) {
        this.setState({
            [key]: value
        });
    }

    onClickRegistration() {
        if(this.state.userName.length < 6 || this.state.userName.length > 20) {
            this.props.requestConfirmation(ConfirmationType.NOTIFICATION, "Benutzername muss zwischen 6 und 20 Zeichen lang sein!");
        } else if(this.state.userName === "" || this.state.firstName === "" || this.state.lastName === "") {
            this.props.requestConfirmation(ConfirmationType.NOTIFICATION, "Bitte füllen Sie alle Pflichtfelder aus!");
        } else if(!/^[A-Za-z0-9]*$/.test(this.state.userName)) {
            this.props.requestConfirmation(ConfirmationType.NOTIFICATION, "Benutzername darf nur Zahlen und Buchstaben enthalten!");
        } else {
            this.props.requestConfirmation(ConfirmationType.YES_NO, "Wollen sie sich registrieren?", () => {
                this.props.sendToServer("REQUEST_SOCIAL_MEDIA_ACCOUNT_CREATION", {
                    userName: this.state.userName,
                    firstName: this.state.firstName,
                    lastName: this.state.lastName,
                    title: this.state.title,
                    phoneNumber: this.state.phoneNumber
                });
            }, () => {});
        }
    }

    render() {
        return (
            <div className='phoneSocialMediaAccountRegisterGrid'>
                 <div className='standardWrapper'>
                    <img src={url + "phone/socialmedia/vconnect_logo.png"} className='phoneSocialMediaAccountSelectLogo'/>
                </div>
                <div className="phoneSocialMediaAccountSelectText">
                    <div />
                    <div className="standardWrapper phoneSocialMediaAccountSelectTextLogin">REGISTRIEREN</div>
                    <div className="standardWrapper phoneSocialMediaAccountSelectTextText">Wähle deine Identität!</div>
                </div>

                <SocialMediaAccountCreationInput placeholder={"Benutzername *"} value={this.state.userName} valueKey={"userName"} onChangeInput={this.onChangeInput} />
                <SocialMediaAccountCreationInput placeholder={"Vorname *"} value={this.state.firstName} valueKey={"firstName"} onChangeInput={this.onChangeInput} />
                <SocialMediaAccountCreationInput placeholder={"Nachname *"} value={this.state.lastName} valueKey={"lastName"} onChangeInput={this.onChangeInput }/>
                <SocialMediaAccountCreationInput placeholder={"Titel"} value={this.state.title} valueKey={"title"} onChangeInput={this.onChangeInput} />
                <SocialMediaAccountCreationInput placeholder={"Telefonnummer"} value={this.state.phoneNumber} valueKey={"phoneNumber"} onChangeInput={this.onChangeInput} />


                <div className="standardWrapper">
                    <button className="phoneSocialMediaAccountRegisterButton" onClick={this.onClickRegistration}>Beginne deine Connection!</button>
                </div>
                <div className="phoneSocialMediaSmallPrint standardWrapper" style={{fontSize: "1.1vh"}}>
                    *Pflichtfelder 
                    <br /> Benutzername muss eimalig sein!
                </div>
            </div>
        );
    }
}

class SocialMediaAccountCreationInput extends React.Component {
    constructor(props) {
        super(props);

        this.onChange = this.onChange.bind(this);
    }

    onChange(event) {
        this.props.onChangeInput(this.props.valueKey, event.target.value);
    }

    render() {
        return (
            <div className="standardWrapper">
                <input className="phoneSocialMediaAccountRegisterInput" placeholder={this.props.placeholder} value={this.props.value} onChange={this.onChange}/>
            </div>
        );
    }
}

export class SocialMediaIFrame extends React.PureComponent {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <iframe className="socialMediaIFrame" src={this.props.url}
                frameBorder={0} 
                style={{
                    width: "100%",
                    height: "100%",
                }}
                loading="lazy"
                />
        );
    }
}


registerCallback("ANSWER_SOCIAL_MEDIA_ACCOUNTS_REQUEST", SocialMediaApp.onReceiveAccounts);
registerCallback("ANSWER_SOCIAL_MEDIA_CREDENTIALS_REQUEST", SocialMediaApp.onReceiveAccountCredentials);
registerCallback("ANSWER_SOCIAL_MEDIA_ACCOUNT_CREATION", SocialMediaApp.onAnswerAccountCreation);