import React, { Fragment } from 'react';

import { CompanySideBarElement } from './../CompanyPanel';

export default class EmployeePanel extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            currentEmployee: null,
        }

        this.onSelectElement = this.onSelectElement.bind(this);
    }

    onSelectElement(element) {
        this.setState({
            currentEmployee: element,
        });
    }

    render() {
        return(
            <div id="companyPanelChangeable">
                <EmployeeSidebar element={this.state.currentEmployee} ranks={this.props.ranks} update={this.props.update} sendToServer={this.props.sendToServer} rankPanelAccess={this.props.rankPanelAccess} />
                <EmployeeMain employees={this.props.employees} current={this.state.currentEmployee} callback={this.onSelectElement} sendToServer={this.props.sendToServer} />
            </div>
        );
    }
}

class EmployeeMain extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="companyMainWorkplace">
                <div className="employeeList">
                    {this.props.employees.map((el) => {
                        return <EmployeeShow element={el} callback={this.props.callback} isCurrent={this.props.current !== null ? el.id === this.props.current.id : false}/>
                    })}
                </div>
            </div>
        );
    }
}

class EmployeeSidebar extends React.Component {
    constructor(props) {
        super(props);


        this.handleRankChange = this.handleRankChange.bind(this);
        this.onSalaryChange = this.onSalaryChange.bind(this);
        this.onBankChange = this.onBankChange.bind(this);
        this.onFire = this.onFire.bind(this);
    }

    handleRankChange(evt) {
        var val = evt.target.value;
        var rank = this.props.ranks.filter((el) => {
            return el.name === val;
        })[0];
        this.props.element.rank = rank;
        this.props.update();

        this.props.sendToServer("CHANGE_RANK", {charId: this.props.element.id, newRank: val})
    }

    onSalaryChange(evt) {
        var val = evt.target.value;
        if(val == "") {
            val = 0;    
        }

        this.props.element.salary = val;
        this.props.update();
        
        this.props.sendToServer("CHANGE_EMPLYOEE_SALARY", {charId: this.props.element.id, newSalary: val});
    }

    onBankChange(evt) {
        var val = evt.target.value;
        if(val == "") {
            val = 0;    
        }

        this.props.element.bank = val;
        this.props.update();

        this.props.sendToServer("CHANGE_EMPLYOEE_BANKACCOUNT", {charId: this.props.element.id, newBankaccount: val});
    }

    onFire() {
        this.props.sendToServer("FIRE_EMPLOYEE", {charId: this.props.element.id});
    }


    createRankList() {
        return (<select onChange={this.handleRankChange} style={{width: "90%", height: "2vh", fontSize: "1.5vh"}}>
        {this.props.ranks.map((el) => {
          if(!el.isCEO) {
            return (
                <option selected={el === this.props.element.rank} value={el.id}>{el.name}</option>
            )
          }
        })}
       </select>);
    }

    render() {
        if(this.props.element != null) {
            var rankList = this.createRankList();
            var salaryInput = <input type="number" placeholder={"$" + this.props.element.salary} style={{width: "70%", height: "1.8vh", fontSize: "1.5vh"}} onChange={this.onSalaryChange} />
            var bankAccontInput = <input type="number" placeholder={this.props.element.bank} style={{width: "70%", height: "1.8vh", fontSize: "1.5vh"}} onChange={this.onBankChange} />
            var fireButton = <button className="employeeFireButton" onClick={this.onFire}>Entlassen!</button>
            return (
                <div className="companySidebar">
                    <div className="companyEmployeeSidebar">
                        {this.props.rankPanelAccess && !this.props.element.rank.isCEO ? <CompanySideBarElement name="Rang ändern" container={rankList} /> : null}
                        <CompanySideBarElement name="Gehalt ändern" container={salaryInput} />
                        <CompanySideBarElement name="Bankaccount ändern" container={bankAccontInput} />
                        {!this.props.element.rank.isCEO ? <CompanySideBarElement name="Spieler entlassen" container={fireButton} /> : null}
                    </div>
                </div>
            );
        } else {
            return <div className="companySidebar" />;
        }
    }
}

class EmployeeShow extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.callback(this.props.element);
    }

    render() {
        return (
            <div className={"companyEmployee " + (this.props.isCurrent ? "isElementCurrent" : "")} onClick={this.onClick}>
                <div className="employeeDutyWrapper"> 
                    <div className={"employeeOnDuty " + (this.props.element.onDuty ? "dutyGreen" : "dutyRed")} />
                </div>
                <div className="employeeElement">{this.props.element.getNameCombo()}</div>
                <div className="employeeElement">{"Rang: " + this.props.element.rank.name}</div>
                <div className="employeeElement">{"Gehalt: $" + this.props.element.salary}</div>
                <div className="employeeElement">{"Heutiger Dienst: " + this.props.element.todayDuty}</div>
            </div>
        );
    }
}

export class Employee {
    constructor(id, firstName, lastName, rank, salary, bank, todayDuty, onDuty) {
        this.id = id;
        this.firstName = firstName;
        this.lastName = lastName;
        this.rank = rank;
        this.salary = salary;
        this.bank = bank;
        this.todayDuty = todayDuty;
        this.onDuty = onDuty;
    }

    getNameCombo() {
        if(this.firstName.length + this.lastName.length < 25) {
            return this.firstName + " " + this.lastName;
        } else if(this.firstName.length + 5 < 25) {
            return this.firstName + " " + this.lastName.substring(0,5) + ".";
        } else {
            return this.firstName.substring(0,5) + ". " + this.lastName.substring(0,5) + ".";
        }
    }
}