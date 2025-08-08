import React from 'react';
import { url } from './../../../index';
import { FileInfoElement, FileSignatureElement } from './../CefFile';

export class USCustomsFile extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            data: props.data,
            readOnly: props.data.data.filter((el) => el.name == "signature")[0].data !== "",
        }

        this.getData = this.getData.bind(this);
        this.setData = this.setData.bind(this);

        this.onSign = this.onSign.bind(this);   
        this.onSave = this.onSave.bind(this);      
        
        this.onChangeInfo = this.onChangeInfo.bind(this);
        this.onChangeChecked = this.onChangeChecked.bind(this);
    }

    onSave(e) {
       this.props.output.sendToServer('SAVE_FILE_ITEM', this.state.data, true, "CEF_FILE");
       this.props.close();
    }

    onSign(dataName) {
        var data = this.state.data;
        this.setData(dataName, "TO_FILL");
        this.props.output.sendToServer("SAVE_FILE_ITEM", this.state.data, true, "CEF_FILE");
        this.props.close();
    }

    onChangeInfo(e) {
        var val = e.target.value;
        var attr = e.target.getAttribute('data');
        
        this.setData(attr, val);
    }

    onChangeChecked(e, yes) {
        var val = e.target.checked && yes;
        var attr = e.target.getAttribute('data');
        this.setData(attr, val);
    }

    getData(name) {
        return this.state.data.data.filter((el) => el.name == name)[0].data;
    }

    setData(name, data) {
        var dat = this.state.data;
        var el = dat.data.filter((el) => el.name == name)[0];
        el.data = data;

        this.setState({
            data: dat,
        })
    }

    render() {
        return(
            <div id="background">
                {this.state.data.isCopy ? <div className="copyWrapper">
                    <img className="copyStamp noSelect" src={url + "cefFile/copyStamp.png"} />
                </div> : null}
                <div id="customsFile">
                    <div id="customsDivider" />
                    <div id="customsHeader">
                        <div>Department of Homeland Security</div>
                        <div>U.S. Customs and Border Protection BLO No. 87-156</div>
                    </div>
                    <div id="customsTitle" className="standardWrapper">Welcome to Los Santos</div>
                    <div id="customsLeftText">
                        Dieses Formular muss von jedem
                        Reisenden auf seiner Reise nach
                        Los Santos ausgefüllt werden.
                        Die Angaben müssen die Wahrheit
                        und nur diese enthalten.
                        Bitte deutliche Schrift verwenden.
                        Bei Fragen wenden sie sich an das
                        Personal!
                    </div>
                    <div id="customsLeftFields">
                        <CustomsTextField info="Admissionnummer:" value={this.getData("admission")} readOnly={this.state.readOnly} data={"admission"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Flugnummer/Bootsnummer:" value={this.getData("flightNumber")} readOnly={this.state.readOnly} data={"flightNumber"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Einreisedatum:" value={this.getData("enterDate")} readOnly={this.state.readOnly} data={"enterDate"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Voller Name:" value={this.getData("fullName")} readOnly={this.state.readOnly} data={"fullName"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Geburtsdatum:" value={this.getData("birthday")} readOnly={this.state.readOnly} data={"birthday"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Staatsbürgerschaft:" value={this.getData("citizenship")} readOnly={this.state.readOnly} data={"citizenship"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Kontakttelefonnummer:" value={this.getData("number")} readOnly={this.state.readOnly} data={"number"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="NICHT SELBST AUSFÜLLEN!" value={this.getData("goverment")} readOnly={false} data={"goverment"} onChange={this.onChangeInfo} rows={3} />
                    </div>
                    <div id="customsRightInfo">
                        Kreuzen sie an, ob eine der
                        folgenden Fragen aus sie 
                        zutreffen. Beantworten sie jede
                        Frage gewissenhaft.
                    </div>

                    <div id="customsRightQuestions">
                        <CustomsQuestion info="Haben sie aktuell eine übertragbare Krankheit; physische oder mentale Probleme; oder sind Sie drogenabhängig?" value={this.getData("drugs")} readOnly={this.state.readOnly} data={"drugs"} onChange={this.onChangeChecked}/>
                        <CustomsQuestion info="Wurden sie schonmal für eine Straftat jeglicher Art festgenommen?" value={this.getData("crime")} readOnly={this.state.readOnly} data={"crime"} onChange={this.onChangeChecked}/>
                        <CustomsQuestion info="Waren sie schonmal Teil von Spionage; terroristischen oder sabotierenden Aktivitäten?" value={this.getData("terror")} readOnly={this.state.readOnly} data={"terror"} onChange={this.onChangeChecked}/>
                        <CustomsQuestion info="Suchen sie Arbeit in Los Santos; oder wurden sie schonmal der USA verwiesen; oder haben sie schonmal versucht illegal oder mit Täuschung in die USA einzureisen?" value={this.getData("work")} readOnly={this.state.readOnly} data={"work"} onChange={this.onChangeChecked}/>                 
                        <CustomsQuestion info="Haben sie jemals einem US Bürger das Sorgerecht auf ein Kind verweigert, verwehrt oder untersagt?" value={this.getData("child")} readOnly={this.state.readOnly} data={"child"} onChange={this.onChangeChecked}/>                 
                        <CustomsQuestion info="Wurde schonmal einer Ihrer Visaanträge für die USA abgelehnt?" value={this.getData("visa")} readOnly={this.state.readOnly} data={"visa"} onChange={this.onChangeChecked}/>                 
                        <div id="customsRightQuestionsBottom">
                            WICHTIG! Haben sie Ja zu einer 
                            der obigen Fragen geantwortet 
                            kontaktieren sie bitte das 
                            zuständige Personal!

                            Ich bestätige alle Angaben mit 
                            der Wahrheit und nur dieser 
                            beantwortet zu haben:
                        </div>
                    </div>
                    <div id="customsRightSignature">
                        <FileSignatureElement showButton={!this.state.readOnly} onSign={this.onSign} signatureName={this.getData("signature")} dataName={"signature"} info="Unterschrift" buttonName="Unterschreiben" />
                        <div className="standardWrapper">
                            <button id="customsRightSave" onClick={this.onSave} style={{backgroundColor: "#8E8E8B"}}>Einpacken (Speichern)</button>
                        </div>
                    </div>
                </div>
            </div> 
        );
    }
}

export class PericoCustomsFile extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            data: props.data,
            readOnly: props.data.data.filter((el) => el.name == "signature")[0].data !== "",
        }

        this.getData = this.getData.bind(this);
        this.setData = this.setData.bind(this);

        this.onSign = this.onSign.bind(this);   
        this.onSave = this.onSave.bind(this);      
        
        this.onChangeInfo = this.onChangeInfo.bind(this);
        this.onChangeChecked = this.onChangeChecked.bind(this);
    }

    onSave(e) {
       this.props.output.sendToServer('SAVE_FILE_ITEM', this.state.data, true);
       this.props.close();
    }

    onSign(dataName) {
        var data = this.state.data;
        this.setData(dataName, "TO_FILL");
        this.props.output.sendToServer("SAVE_FILE_ITEM", this.state.data, true);
        this.props.close();
    }

    onChangeInfo(e) {
        var val = e.target.value;
        var attr = e.target.getAttribute('data');
        
        this.setData(attr, val);
    }

    onChangeChecked(e, yes) {
        var val = e.target.checked && yes;
        var attr = e.target.getAttribute('data');
        this.setData(attr, val);
    }

    getData(name) {
        return this.state.data.data.filter((el) => el.name == name)[0].data;
    }

    setData(name, data) {
        var dat = this.state.data;
        var el = dat.data.filter((el) => el.name == name)[0];
        el.data = data;

        this.setState({
            data: dat,
        })
    }

    render() {
        return(
            <div id="background">
                {this.state.data.isCopy ? <div className="copyWrapper">
                    <img className="copyStamp noSelect" src={url + "cefFile/copyStamp.png"} />
                </div> : null}
                <div id="pericoCustomsFile">
                    <div id="customsDivider" />
                    <div id="customsHeader">
                        <div>Cayo Perico Customs Agency</div>
                        <div>Cayo Perico Visa Form 167-AH1</div>
                    </div>
                    <div id="customsTitle" className="standardWrapper">Welcome to Cayo Perico</div>
                    <div id="customsLeftText">
                        Dieses Visa Formular muss von jedem
                        Reisenden auf seiner Reise nach
                        Cayo Perico ausgefüllt werden.
                        Die Angaben müssen die Wahrheit
                        und nur diese enthalten.
                        Bitte deutliche Schrift verwenden.
                        Bei Fragen wenden sie sich an das
                        Personal!
                    </div>
                    <div id="customsLeftFields">
                        <CustomsTextField info="Visa-Nummer:" value={this.getData("visaNumber")} readOnly={this.state.readOnly} data={"visaNumber"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Flugnummer/Bootsnummer:" value={this.getData("flightNumber")} readOnly={this.state.readOnly} data={"flightNumber"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Einreisedatum:" value={this.getData("enterDate")} readOnly={this.state.readOnly} data={"enterDate"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Voller Name:" value={this.getData("fullName")} readOnly={this.state.readOnly} data={"fullName"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Geburtsdatum:" value={this.getData("birthday")} readOnly={this.state.readOnly} data={"birthday"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Staatsbürgerschaft:" value={this.getData("citizenship")} readOnly={this.state.readOnly} data={"citizenship"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Kontakttelefonnummer:" value={this.getData("number")} readOnly={this.state.readOnly} data={"number"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Visa Betrag:" value={this.getData("visaAmount")} readOnly={this.state.readOnly} data={"visaAmount"} onChange={this.onChangeInfo} rows={1} />
                    </div>
                    <div id="customsRightInfo">
                        Kreuzen sie an, ob eine der
                        folgenden Fragen aus sie 
                        zutreffen. Beantworten sie jede
                        Frage gewissenhaft.
                    </div>

                    <div id="customsRightQuestions">
                        <CustomsTextField info="Wie lange denken sie wird ihr Aufenthalt in Cayo Perico dauern?" value={this.getData("time")} diffFontSize="1.4vh" readOnly={this.state.readOnly} data={"time"} onChange={this.onChangeInfo} rows={1} />
                        <CustomsTextField info="Was ist der Grund ihres Besuches in Cayo Perico?" value={this.getData("visit")} diffFontSize="1.4vh" readOnly={this.state.readOnly} data={"visit"} onChange={this.onChangeInfo} rows={1} />
                        <div className="pericoCustomsQuestionDivider" />
                        <CustomsQuestion info="Wollen sie eine Arbeit/Tätigkeit gewerbliche Art in Cayo Perico ausführen?" diffClass="pericoCustomsQuestion" value={this.getData("work")} readOnly={this.state.readOnly} data={"work"} onChange={this.onChangeChecked}/>
                        <CustomsTextField info="Falls ja, welche:" value={this.getData("workMaybe")} diffFontSize="1.4vh" readOnly={this.state.readOnly} data={"workMaybe"} onChange={this.onChangeInfo} rows={2} />
                        <div className="pericoCustomsQuestionDivider" />
                        <CustomsQuestion info="Führen sie Nahrungsmittel jeglicher Art bei sich?" diffClass="pericoCustomsQuestion" value={this.getData("food")} readOnly={this.state.readOnly} data={"food"} onChange={this.onChangeChecked}/>
                        <CustomsTextField info="Falls ja, welche:" value={this.getData("foodMaybe")} diffFontSize="1.4vh" readOnly={this.state.readOnly} data={"foodMaybe"} onChange={this.onChangeInfo} rows={2} />
                        <div className="pericoCustomsQuestionDivider" />
                        <CustomsQuestion info="Führen sie mehr als $10000 Bargeld mit sich?" diffClass="pericoCustomsQuestion" value={this.getData("money")} readOnly={this.state.readOnly} data={"money"} onChange={this.onChangeChecked}/>
                        <CustomsTextField info="Falls ja, wieviel:" value={this.getData("moneyMaybe")} diffFontSize="1.4vh" readOnly={this.state.readOnly} data={"moneyMaybe"} onChange={this.onChangeInfo} rows={2} />
                        
                        <div id="customsRightQuestionsBottom">
                            WICHTIG! Haben sie Ja zu einer 
                            der obigen Fragen geantwortet 
                            kontaktieren sie bitte das 
                            zuständige Personal!

                            Ich bestätige alle Angaben mit 
                            der Wahrheit beantwortet zu haben:
                        </div>
                    </div>
                    <div id="customsRightSignature">
                        <FileSignatureElement showButton={!this.state.readOnly} onSign={this.onSign} signatureName={this.getData("signature")} dataName={"signature"} info="Unterschrift" buttonName="Unterschreiben" />
                        <div className="standardWrapper">
                            <button id="customsRightSave" onClick={this.onSave} style={{backgroundColor: "#8E8E8B"}}>Einpacken (Speichern)</button>
                        </div>
                    </div>
                </div>
            </div> 
        );
    }
}

class CustomsTextField extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        var fontSize = "1.65vh";
        if(this.props.diffFontSize != undefined) {
            fontSize = this.props.diffFontSize;
        }

        return(
            <div className="customsTextField">
                <div className="customsTextFieldInfo" style={{fontSize: fontSize}} >{this.props.info}</div>
                <textarea className="customsTextFieldText" rows={this.props.rows} spellCheck="false" placeholder={this.props.info} data={this.props.data} readOnly={this.props.readOnly} onChange={this.props.onChange} value={this.props.value} style={{fontSize: fontSize}} />
            </div>);
    }
}

class CustomsQuestion extends React.Component {
    constructor(props) {
        super(props);

        this.onChangeYes = this.onChangeYes.bind(this);
        this.onChangeNo = this.onChangeNo.bind(this);
    }

    onChangeYes(e) {
        if(!this.props.readOnly) {
            this.props.onChange(e, true);
        }
    }

    onChangeNo(e) {
        if(!this.props.readOnly) {
            this.props.onChange(e, false);
        }
    }

    render() {
        var cl = "customsQuestion";
        console.log(this.props.diffClass)
        if(this.props.diffClass != undefined) {
            cl = this.props.diffClass;
        }

        return(
            <div className={cl}>
                <div className="customsQuestionText">{this.props.info}</div>
                <div className="customsQuestionCheck standardWrapper">
                    <div className="customsQuestionElement">
                        <input type="checkbox" name={this.props.data} className="customsQuestionElementRadio" readOnly={this.props.readOnly} checked={this.props.value == null ? false : this.props.value} onChange={this.onChangeYes} data={this.props.data} />
                        <br />
                        Ja
                    </div>
                    <div className="customsQuestionElement">
                        <input type="checkbox" name={this.props.data} className="customsQuestionElementRadio" readOnly={this.props.readOnly} checked={this.props.value == null ? false : !this.props.value} onChange={this.onChangeNo} data={this.props.data} />
                        <br />
                        Nein
                    </div>
                </div>
            </div>);
    }
}