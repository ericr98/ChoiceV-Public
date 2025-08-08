import React, { Fragment } from 'react';
import { registerApp, registerCallback, Icon, ConfirmationType } from '../../SmartphoneController';

import './BankingApp.css';

import { url } from './../../../../index';
import { AllSettings } from '../SettingsApp/SettingsApp';

export default class BankingApp extends React.Component {
    static accountList = [];

    static onAnswerAccount(sender, data) {
        BankingApp.accountList = [];
        data.accounts.forEach((el) => {
            var obj = JSON.parse(el);
            BankingApp.accountList.push(new AccountModel(obj.type, obj.number, obj.name, obj.balance, [], obj.company));
        });

        sender.forceUpdate();
    }

    static onAnswerTransationsList(sender, data) {
        var account = BankingApp.accountList.filter((el) => {
            return el.accountNumber == data.number;
        })[0];

        if(account != undefined) {
            account.transactionList = [];
            data.transactions.forEach((el) => {
                var obj = JSON.parse(el);
                account.transactionList.push(new Transaction(obj.from, obj.to, obj.balance, obj.note, new Date(obj.date)));
            });

            account.transactionList.sort(function(a,b){
                return new Date(b.date) - new Date(a.date);
              });

            sender.forceUpdate();
        } else {
            console.log("Error: onAnswerTransationsList: account not found!");
        }
    }

    static getCompanyColor(name) {
        switch(name) {
            case "maze": return "#e8252f";
            case "fleeca": return "#268f3a";
            case "liberty": return "#002f83";
        }
    }

    constructor(props) {
        super(props);

        this.state = {
            currentAccount: null,
            CurrentAccountSubMenu: null,
        }

        this.onSelectAccount = this.onSelectAccount.bind(this);
        this.onNewTransaction = this.onNewTransaction.bind(this);

        this.onConfirmFlyMode = this.onConfirmFlyMode.bind(this);
    }

    static hasTime() {
        return false;
    }

    static stopsMovement() {
        return true;
    }
    
    static hasVerticalMode() {
        return false;
    }

    static getIcon(callback) {
        return <Icon key={"vara"} icon="vara" column={1} row={4} missedInfo={0} callback={callback} type={BankingApp} />
    }

    static deselect() { }

    triggerBackButton() {
        if(this.state.currentAccount != null) {
            if(this.state.CurrentAccountSubMenu != BankingAccountShow) {
                this.setState({
                    CurrentAccountSubMenu: BankingAccountShow, 
                })
            } else {
                this.setState({
                    currentAccount: null,
                })
            }
        }
    }


    static dispose() {
        BankingApp.accountList = [];
    }

    componentDidMount() {
        //Always request because something could have happend
        //if(BankingApp.accountList.length == 0) {
            this.props.requestData("PHONE_BANKING_REQUEST_BANKACCOUNTS", {});
        //}
    }

    onSelectAccount(account) {
        this.setState({
            currentAccount: account, 
            CurrentAccountSubMenu: BankingAccountShow,
        })
    }

    onNewTransaction(account) {
        this.setState({
            currentAccount: account, 
            CurrentAccountSubMenu: BankingNewTransaction,
        })
    }

    getShowElement() {
        if(this.state.currentAccount != null) {
            return <this.state.CurrentAccountSubMenu account={this.state.currentAccount} onSelectAccount={this.onSelectAccount} requestData={this.props.requestData} newTransaction={this.onNewTransaction} requestConfirmation={this.props.requestConfirmation} sendToServer={this.props.sendToServer} />;
        } else {
            return <BankingAccountList onSelectAccount={this.onSelectAccount} />
        }
    }

    checkIfProceed() {
        var settings = this.props.requestState("settings");
        if(settings[AllSettings.FLY_MODE]) {
            this.props.requestConfirmation(ConfirmationType.YES_NO, "Flugmodus deaktivieren um Konten zu laden?", this.onConfirmFlyMode, null);
            return false;
        } else {
            return true;
        }
    }

    onConfirmFlyMode() {
        this.props.changeSetting(AllSettings.FLY_MODE, false);
    }

    render() {
        if(this.checkIfProceed()) {
            return (
                <div className="phoneBankingBackground">
                    {this.getShowElement()}
                </div>);
        } else {
            return (
                <div className="phoneBankingBackground">
                    <div className="phoneBankingListWrapper" style={{backgroundImage: "url(" + url + "phone/banking/background.png)"}} />
                </div>);
        }
    }
}

//New Transaction

class BankingNewTransaction extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            data: {},
        }

        this.onChangeInputAttr = this.onChangeInputAttr.bind(this);
        this.onDoTransaction = this.onDoTransaction.bind(this);
        this.onConfirmTransaction = this.onConfirmTransaction.bind(this);
    }

    onChangeInputAttr(attr, val) {
        var data = this.state.data;
        data[attr] = val;
        this.setState({
            data: data,
        });

    }

    onDoTransaction() {
        if(this.props.account.accountType === "Firmenkonto") {
            this.onConfirmTransaction("0");
        } else {
            this.props.requestConfirmation(ConfirmationType.INPUT, "Bitte PIN eingeben:", this.onConfirmTransaction, null, {
                type: "password",
                textAlign: "center"
            });
        }
    }

    onConfirmTransaction(input) {
        this.props.sendToServer("PHONE_BANKING_NEW_TRANSACTION", {
            from: this.props.account.accountNumber,
            pin: input,
            owner: this.state.data.owner,
            to: this.state.data.number,
            use: this.state.data.use,
            amount: this.state.data.amount,
        }, false);

        this.props.onSelectAccount(this.props.account);
    }

    render() {
        return (
            <div className="phoneBankingAccount" style={{backgroundImage: "url(" + url + "phone/banking/backgrounds/bg_" + this.props.account.company +".png)"}}>
                    <div />
                    <div className="standardWrapper">
                        <BankingAccountShowTopInfo account={this.props.account} />
                    </div>
                    <BankingNewTransactionInput onChange={this.onChangeInputAttr} data={this.state.data} />
                    <div className="phoneBankingAccountButton" style={{backgroundColor: BankingApp.getCompanyColor(this.props.account.company)}} onClick={this.onDoTransaction}> 
                        <div className="standardWrapper">
                            <div className="phoneBankingAccountButtonInner">
                                <img className="phoneBankingAccountButtonIcon" src={url + "phone/icons/icon_choicev_white.png"} draggable={false} alt="The Icon of the Button" />
                                Überweisung bestätigen
                            </div>
                        </div>
                    </div>
            </div>);
    }
}

class BankingNewTransactionInput extends React.Component {

    render() {
        return(
            <div className="standardWrapper">
                <div className="phoneBankingNewTransactionInput"> 
                    <div className="phoneBankingNewTransactionInputTitle">Neue Überweisung</div>
                    <BankingNewTransactionInputField name="Kontoinhaber" inputType="text" onChange={this.props.onChange} attr="owner" value={this.props.data["owner"]}/>
                    <BankingNewTransactionInputField name="Kontonummer" inputType="number" onChange={this.props.onChange} attr="number"  value={this.props.data["number"]}/>
                    <BankingNewTransactionInputField name="Verwendungszweck" inputType="text" onChange={this.props.onChange} attr="use"  value={this.props.data["use"]}/>
                    <BankingNewTransactionInputField name="Betrag" inputType="number" onChange={this.props.onChange} attr="amount" value={this.props.data["amount"]}/>
                </div>
            </div>
        );
    }
}

class BankingNewTransactionInputField extends React.Component {
    constructor(props) {
        super(props);

        this.onChange = this.onChange.bind(this);
    }

    onChange(evt) {
        this.props.onChange(this.props.attr, evt.target.value);
    }

    render() {
        return(
            <div className="phoneBankingNewTransactionInputField">
                <div className="phoneBankingNewTransactionInputFieldTitle">{this.props.name + ":"}</div>
                    <input className="phoneBankingNewTransactionInputFieldInput" type={this.props.inputType} spellCheck={false} value={this.props.value} onChange={this.onChange} />
            </div>
        );
    }
}

class BankingAccountShow extends React.Component {
    constructor(props) {
        super(props);

        this.onNewTransaction = this.onNewTransaction.bind(this);
    }

    onNewTransaction() {
        this.props.newTransaction(this.props.account);
    }

    componentDidMount() {
        //Always update
        //if(this.props.account.transactionList.length == 0) {
            this.props.requestData("PHONE_BANKING_REQUEST_TRANSACTIONS", {accountId: this.props.account.accountNumber});
        //}
    }

    render() {
        return (
                <div className="phoneBankingAccount" style={{backgroundImage: "url(" + url + "phone/banking/backgrounds/bg_" + this.props.account.company +".png)"}}>
                    <div />
                    <div className="standardWrapper">
                        <BankingAccountShowTopInfo account={this.props.account} />
                    </div>
                    <BankingAccountTransactionList account={this.props.account} />
                    <div className="phoneBankingAccountButton" style={{backgroundColor: BankingApp.getCompanyColor(this.props.account.company)}} onClick={this.onNewTransaction}> 
                        <div className="standardWrapper">
                            <div className="phoneBankingAccountButtonInner">
                                <img className="phoneBankingAccountButtonIcon" src={url + "phone/icons/icon_choicev_white.png"} draggable={false} alt="The Icon of the Button" />
                                Neue Überweisung
                            </div>
                        </div>
                    </div>
            </div>);
    }
}

class BankingAccountShowTopInfo extends React.Component {
    render() {
        return (
            <div className="phoneBankingAccountTop">
                <div className="phoneBankingAccountTopText">{"Kontotyp: "}<div className="phoneBankPadding fullWrapper select">{" " + this.props.account.accountType}</div></div>
                <div className="phoneBankingAccountTopText">{"Kontoinhaber:"}<div className="phoneBankPadding fullWrapper select">{" " + this.props.account.ownerName}</div></div>
                <div className="phoneBankingAccountTopText">{"Kontonummer:"}<div className="phoneBankPadding fullWrapper select">{" " + this.props.account.accountNumber}</div></div>
                <div className="phoneBankingAccountTopText">{"Kontostand: "}<div className="phoneBankingGreen fullWrapper select">{" $" + this.props.account.balance}</div></div>
            </div>
        );
    }
}

class BankingAccountTransactionList extends React.Component {
    render() {
        return (
            <div className="standardWrapper">
                <div className="phoneBankingAccountTransactions">
                    <div className="phoneBankingAccountTransactionsInfo">Letzte Aktivität:</div>
                    <div className="phoneBankingAccountTransactionsList">
                        {this.props.account.transactionList.map((el) => {
                            return <BankingAccountTransactionsShow account={this.props.account} el={el} />
                        })}
                    </div>
                </div>
            </div>);
    }
}

class BankingAccountTransactionsShow extends React.Component {
    getDateString(date) {
        return date.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }) + " " + `${date.getDate().toString().padStart(2, '0')}.${(date.getMonth()+1).toString().padStart(2, '0')}`;
    }

    getAccountNumber() {
        if(this.props.account.accountNumber == this.props.el.from) {
            if(this.props.el.to == -1) {
                return "n.a"
            } else {
                return this.props.el.to;
            }
        } else {
            if(this.props.el.from == -1) {
                return "n.a"
            } else {
                return this.props.el.from;
            } 
        }
    }

    getBalance() {
        if(this.props.account.accountNumber == this.props.el.from) {
            return <div className="phoneBankingAccountTransactionBalance"><div className="phoneBankingAccountTextRed">{"-$" + (this.props.el.balance)}</div></div>;
        } else {
            return <div className="phoneBankingAccountTransactionBalance"><div className="phoneBankingAccountTextGreen">{"+$" + (this.props.el.balance)}</div></div>;
        }
    }

    getNote() {
        if(this.props.el.note.length > 30) {
            return this.props.el.note.substring(0, 30) + "...";
        } else {
            return this.props.el.note;
        }
    }

    render() {
        return(
            <div className="phoneBankingAccountTransactionWrapper">
                <div className="phoneBankingAccountTransaction">
                    <div className="phoneBankingAccountTransactionInfo">{this.getDateString(this.props.el.date) + " - " + this.getAccountNumber()}</div>
                    <div className="phoneBankingAccountTransactionNote">{this.getNote()}</div>
                    <div className="phoneBankingAccountTransactionBalanceWrapper">
                        {this.getBalance()}
                    </div>
                </div>
            </div>);
    }
}

//AccountList

class BankingAccountList extends React.Component {
    render() {
        return (
        <div className="phoneBankingListWrapper" style={{backgroundImage: "url(" + url + "phone/banking/background.png)"}}>
            <div className="phoneBankingList">
                {BankingApp.accountList.map((el) => {
                    return <BankingListAccountShow el={el} selectAccount={this.props.onSelectAccount}/>
                })}
            </div>
        </div>);
    }
}

class BankingListAccountShow extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.selectAccount(this.props.el);
    }

    render() {
        return (
            <div className="phoneBankingListAccountWrapper" style={{backgroundImage: "url(" + url + "phone/banking/boxes/box_" + this.props.el.company + ".png"}} onClick={this.onClick}>
                <div className="phoneBankingListAccount">
                    <div className="phoneBankingListAccountElementWrapper">
                        <div className="phoneBankingListAccountElement">{"Kontoinhaber: " + this.props.el.ownerName}</div>
                    </div>
                    <div className="phoneBankingListAccountElementWrapper">
                        <div className="phoneBankingListAccountElement">{"Kontonummer: " + this.props.el.accountNumber}</div>
                    </div>
                    <div className="phoneBankingListAccountElementWrapper">
                        <div className="phoneBankingListAccountElement">{"Kontostand: "} <div className="phoneBankingGreen">{" $" + this.props.el.balance}</div></div>
                    </div>
                </div>
            </div>);
    }
}

class AccountModel {
    constructor(accountType, accountNumber, ownerName, balance, transactionList, company) {
        this.accountType = accountType;
        this.accountNumber = accountNumber;
        this.ownerName = ownerName;
        this.balance = balance;
        this.transactionList = transactionList;
        this.company = company;
    }
}


class Transaction {
    constructor(from, to, balance, note, date) {
        this.from = from;
        this.to = to;
        this.balance = balance;
        this.note = note;
        this.date = date;
    }
}

registerCallback("PHONE_BANKING_ANSWER_BANKACCOUNTS", BankingApp.onAnswerAccount);
registerCallback("PHONE_BANKING_ANSWER_TRANSACTIONS", BankingApp.onAnswerTransationsList);

registerApp(BankingApp, 1);