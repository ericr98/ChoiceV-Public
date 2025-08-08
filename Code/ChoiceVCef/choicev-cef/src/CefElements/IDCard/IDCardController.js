import React from 'react';
import { register } from './../../App';
import './IDCardController.css';

import { url } from './../../index';


export default class IDCardController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            CurrentIDCard: null,
            data: props.data,
        }

        this.onLoadDriversLicense = this.onLoadDriversLicense.bind(this);
        this.onLoadSocialSecurityCard = this.onLoadSocialSecurityCard.bind(this);
        this.onLoadVehicleRegistrationCard = this.onLoadVehicleRegistrationCard.bind(this);
        this.onLoadCompanyIDCard = this.onLoadCompanyIDCard.bind(this);
        this.onCloseIdCard = this.onCloseIdCard.bind(this);
        
        this.props.input.registerEvent("OPEN_DRIVERS_LICENSE", this.onLoadDriversLicense);
        this.props.input.registerEvent("OPEN_SOCIAL_SECURITY_CARD", this.onLoadSocialSecurityCard);
        this.props.input.registerEvent("OPEN_VEHICLE_REGISTRATION_CARD", this.onLoadVehicleRegistrationCard);
        this.props.input.registerEvent("OPEN_COMPANY_ID_CARD", this.onLoadCompanyIDCard);
        this.props.input.registerEvent("CLOSE_CEF", this.onCloseIdCard);
    }

    onLoadDriversLicense(data) {
        this.setState({
            CurrentIDCard: DriversLicense,
            data: data,
        })
    }

    onLoadSocialSecurityCard(data) {
        this.setState({
            CurrentIDCard: SocialSecurityCard,
            data: data,
        })
    }

    onLoadVehicleRegistrationCard(data) {
        this.setState({
            CurrentIDCard: VehicleRegistrationCard,
            data: data,
        })
    }

    onLoadCompanyIDCard(data) {
        this.setState({
            CurrentIDCard: CompanyIdCard,
            data: data,
        })
    }

    onCloseIdCard() {
        this.setState({
            CurrentIDCard: null,
            data: null,
        })
    }

    render() {
        if(this.state.CurrentIDCard == null) {
            return null;
        } else {
            return (
                <div className="idCardWrapper">
                    <this.state.CurrentIDCard data={this.state.data} />
                </div>);
        }
    }
}

class SocialSecurityCard extends React.Component {
    constructor(props) {
        super(props);

        this.getData = this.getData.bind(this);
    }
    
    getData(name) {
        return this.props.data.data.filter((el) => el.name === name)[0].data;
    }

    formatNumber(i) {
        return `${i.substring(0, 3)}-${i.substring(3, 5)}-${i.substring(5)}`
    }

    render() {
        return (
            <div className="idCardIdentBackground" style={{backgroundImage: "url(" + url + "idcards/socialSecurity.png"}}>
                <div />
                <div className="idCardIdentNumber standardWrapper">{this.formatNumber(this.getData("number") + "")}</div>
                <div className="idCardLice standardWrapper noSelect">THIS CARD HAS BEEN ESTABLISHED FOR</div>
                <div className="idCardIdentName standardWrapper noSelect">{this.getData("name")}</div>
                <div className="idCardIdentSignature noSelect">
                    <div className="idCardIdentSignatureText">{this.getData("name")}</div>
                    <div className="standardWrapper">
                        <div className="idCardIdentSignatureLine"/>
                    </div>
                    <div className="idCardIdentSignatureSubTitle">SIGNATURE</div>
                </div>
            </div>
        );
    }
}

class DriversLicense extends React.Component {
    constructor(props) {
        super(props);

        this.getData = this.getData.bind(this);
    }
    
    getData(name) {
        return this.props.data.data.filter((el) => el.name === name)[0].data;
    }
    
    render() {
        return (
            <div className="idCardDriverBackground noSelect" style={{backgroundImage: "url(" + url + "idcards/driversLicense.png"}}>
                <div />
                <div className="idCardDriverGrid">
                    <div className="idCardGridElement idCardDriverMargin idCardDriverRed">{this.getData("dlNumber")}</div>
                    <div className="idCardGridSec idCardDriverMargin">
                        <div className="idCardGridElement idCardDriverRed">{this.getData("expDate")}</div>
                        <div className="idCardGridElement">{this.getData("vehicleClass")}</div>
                    </div>
                    <div className="idCardGridElement idCardDriverMargin">{this.getData("lastName")}</div>
                    <div className="idCardGridElement idCardDriverMargin">{this.getData("firstName")}</div>
                    <div className="idCardGridElement idCardDriverMargin idCardDriverRed">{this.getData("dateOfBirth")}</div>
                    <div className="idCardGridSix idCardDriverMargin">
                        <div className="idCardGridElement">{this.getData("gender")}</div>
                        <div className="idCardGridElement">{this.getData("hairColor")}</div>
                        <div className="idCardGridElement">{this.getData("eyeColor")}</div>
                    </div>
                    <div className="idCardGridSeven idCardDriverMargin">
                        <div className="idCardGridElement">{this.getData("issueDate")}</div>
                        <div className="idCardGridElement">{this.getData("issuer")}</div>
                    </div>
                    <div className="idCardGridSignature">{this.getData("signature")}</div>
                </div>
                <div />
            </div>
        );
    }
}

class VehicleRegistrationCard extends React.Component {
    constructor(props) {
        super(props);

        this.getData = this.getData.bind(this);
    }
    
    getData(name) {
        return this.props.data.data.filter((el) => el.name === name)[0].data;
    }

    render() {
        return (
            <div className="idCardVehicleRegistrationBackground noSelect" >
                <div className="idCardVehicleRegistrationLogoTitle">
                    <div className="standardWrapper idCardVehicleRegistrationLogoWrapper">
                        <img className="idCardVehicleRegistrationLogo" src={url + "cefFile/logos/City.png"}></img>
                    </div>
                    <div></div>
                    <div className="standardLeftWrapper" style={{fontSize: "2.25vh"}}>Vehicle Registration Card</div>
                    <div className="standardLeftWrapper" style={{fontSize: "1.5vh"}}>City of Los Santos - Department of Motor Vehicles</div>
                    <div></div>
                </div>
                <div className="idCardVehicleRegistrationLicNumber standardLeftWrapper">Liz. Nummer:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("licNumber")}</span> </div>
                <div className="idCardVehicleRegistrationDate1">Ausgestellt am:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("startDate")}</span> </div>
                <div className="idCardVehicleRegistrationDate2">Gültig bis:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("expDate")}</span> </div>
                <div className="idCardVehicleRegistrationInfo">
                    Diese Karte ist gültig in der oben genannten Zeit Periode. Diese Karte ist nur signiert und mit Namen versehen gültig, gemäß dem San Andreas Vehicle Code.
                </div>
                <div className="idCardVehicleRegistrationInputs">
                    <div className="idCardVehicleRegistrationInputsInput">Ausgestellt für:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("owner")}</span> </div>
                    <div className="idCardVehicleRegistrationInputsInput">Fahrzeugnummer:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("chassisNumber")}</span> </div>
                    <div className="idCardVehicleRegistrationInputsInput">Gültig in:&nbsp;<span class="idCardVehicleRegistrationInput">SAN ANDREAS</span> </div>
                    <div className="idCardVehicleRegistrationInputsInput">Kennzeichen:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("numberPlate")}</span> </div>
                    <div className="idCardVehicleRegistrationInputsInput">Fahrzeugname:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("vehicleName")}</span> </div>
                    <div className="idCardVehicleRegistrationInputsInput">Fahrzeughersteller:&nbsp;<span class="idCardVehicleRegistrationInput">{this.getData("vehicleProducer")}</span> </div>
                </div>
                <div className="idCardVehicleRegistrationSignature">
                    <span>Unterschrift City of Los Santos Representative</span>
                    <span className="standardLeftWrapper idCardVehicleRegistrationSignatureText">{this.getData("dmvSignature")}</span>
                </div>

                <div className="idCardVehicleRegistrationSignature idCardVehicleRegRight">
                    <span>Unterschrift Fahrzeuginhaber</span>
                    <span className="standardLeftWrapper idCardVehicleRegistrationSignatureText">{this.getData("ownerSignature")}</span>
                </div>
            </div>
        );
    }
}

class CompanyIdCard extends React.Component {
    constructor(props) {
        super(props);
        
        this.getData = this.getData.bind(this); 
    }
    
    getData(name) {
        return this.props.data.data.filter((el) => el.name === name)[0].data;
    }
    
    render() {
        if (this.getData("type") === "NORMAL") {
            return (
                <div className="idCardCompanyBackground noSelect" style={{backgroundImage: "url(" + url + "idcards/company/normal/" + this.getData("icon") + ".png"}}>
                    <div id="idCardCompanyInfo">
                        <div className="idCardCompanyInfoText" style={{"fontSize": "2vh"}}>{this.getData("name")}<br /></div>
                        <div className="idCardCompanyInfoText" style={{"fontSize": "1.5vh"}}>{this.getData("rank")}<br /></div>
                        <br />
                        <div className={"idCardCompanyInfoText"} style={{"fontSize": "1.5vh"}}>{this.getData("company")}<br /></div>
                        <div className={"idCardCompanyInfoText"} style={{"fontSize": "1.1vh"}}>{this.getData("address")}<br /></div>
                    </div>
                </div>
            )
        } else if (this.getData("type") === "DEPARTMENT") {
            return (
                <div className="idCardCompanyBackground noSelect"  style={{backgroundImage: "url(" + url + "idcards/company/department/" + this.getData("icon") + ".png"}}>
                    <div id="idCardCompanyDepartmentInfo">
                        <div className="idCardCompanyDepartmentInfoText" style={{"fontSize": "1.75vh"}}>{this.getData("name")}<br/></div>
                        <div className="idCardCompanyDepartmentInfoText" style={{"fontSize": "1.5vh"}}>{this.getData("number")}<br/></div>
                        <div className="idCardCompanyDepartmentInfoText" style={{"fontSize": "1.2vh"}}>{this.getData("rank")}<br/></div>
                        <br/>
                        <br/>
                        <div className={"idCardCompanyDepartmentInfoText"} style={{"fontSize": "1.4vh"}}>{this.getData("address")}<br/></div>
                    </div>
                </div>
            )
        } else if (this.getData("type") === "BADGE") {
            return (
                <div className="idCardCompanyBadgeBackground noSelect" style={{backgroundImage: "url(" + url + "idcards/company/badge/" + this.getData("icon") + ".png"}}>
                    <div id="idCardCompanyBadgeInfo">
                        <div className="idCardCompanyBadgeInfoText" style={{"fontSize": "2vh"}}>{this.getData("name")}<br/></div>
                        <div className="idCardCompanyBadgeInfoText" style={{"fontSize": "2vh"}}>{this.getData("birthday")}<br/></div>
                        <div className="idCardCompanyBadgeInfoText" style={{"fontSize": "2vh"}}>{this.getData("rank")}<br/></div>
                        <div className="idCardCompanyBadgeInfoText" style={{"fontSize": "2vh"}}>{this.getData("number")}<br/></div>
                        <div className="idCardCompanyBadgeInfoText" style={{"fontSize": "2vh"}}>{this.getData("hiringDay")}<br/></div>
                    </div>
                </div>
            )
        }
    }
}


register(IDCardController);