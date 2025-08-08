import React, { Fragment } from 'react';

import { CompanySideBarElement } from './../CompanyPanel';

export default class RankPanel extends React.Component { 
    constructor(props) {
        super(props);

        this.state = {
            currentRank: null,
        }

        this.onSelectRank = this.onSelectRank.bind(this);
        this.needsUpdate = this.needsUpdate.bind(this);

        this.onDeleteRank = this.onDeleteRank.bind(this);
    }

    onSelectRank(rank) {
        this.setState({
            currentRank: rank,
        });
    }

    needsUpdate() {
        this.forceUpdate();
    }

    onDeleteRank() {
        this.props.deleteRank(this.props.currentRank);
        this.setState({
            currentRank: null,
        });
    }

    render() {
            return(
                <div id="companyPanelChangeable">
                    {this.props.rankPanelAccess ? <RankSidebar ranks={this.props.ranks} current={this.state.currentRank} update={this.props.update} permissions={this.props.permissions} needsUpdate={this.needsUpdate} newRank={this.props.newRank} deleteRank={this.props.deleteRank} sendToServer={this.props.sendToServer} /> : null }
                    {this.props.rankPanelAccess ? <RankMain ranks={this.props.ranks} employees={this.props.employees} current={this.state.currentRank} callback={this.onSelectRank} /> : null }
                </div>
            );
    }
}

class RankSidebar extends React.Component {
    constructor(props) {
        super(props);

        this.onNameChange = this.onNameChange.bind(this);
        this.onSalaryChange = this.onSalaryChange.bind(this);
        this.onDeleteRank = this.onDeleteRank.bind(this);
        this.onNewRank = this.onNewRank.bind(this);
    }

    onNameChange(evt) {
        var val = evt.target.value;
        if(this.props.ranks.filter((el) => {return el.name === val}).length === 0) {
            this.props.sendToServer("CHANGE_COMPANY_RANK_NAME", {oldName: this.props.current.name, newName: val});

            this.props.current.name = val;
        }
        this.props.update();
    }

    onSalaryChange(evt) {
        var val = evt.target.value;
        this.props.current.salary = val;

        this.props.sendToServer("CHANGE_COMPANY_RANK_SALARY", {rankName: this.props.current.name, newSalary: val});
        this.props.update();
    }

    onDeleteRank(evt) {
        this.props.deleteRank(this.props.current);
    }

    onNewRank(evt) {
        this.props.newRank();
    }

    render() {
        var newRankButton = <button className="rankButton companyRankCreate" onClick={this.onNewRank}>Neuer Rang</button>

        if(this.props.current != null) {
            var nameInput = <input type="text" spellCheck={false} placeholder={this.props.current.name} style={{width: "90%", height: "1.8vh", fontSize: "1.5vh"}} onChange={this.onNameChange} />
            var salaryInput = <input type="number" placeholder={"$" + this.props.current.salary} style={{width: "90%", height: "1.8vh", fontSize: "1.5vh"}} onChange={this.onSalaryChange} />
            var perm = <RankPermissionSideBarElement current={this.props.current} permissions={this.props.permissions} needsUpdate={this.props.needsUpdate} sendToServer={this.props.sendToServer} />
            var removeRankButton = <button className="rankButton companyRankRemove" onClick={this.onDeleteRank}>Rang entfernen!</button>

            return (
                <div className="companySidebar">
                    <div className="companyRankSidebar">
                        <CompanySideBarElement name="Name ändern" container={nameInput} />
                        <CompanySideBarElement name="Gehalt ändern" container={salaryInput} />
                        {!this.props.current.isCEO ? <CompanySideBarElement name="Berechtigungen" container={perm} /> : null}
                        {!this.props.current.isCEO && !this.props.current.isStandart ? <CompanySideBarElement name="Rang löschen" container={removeRankButton} /> : null}
                        <CompanySideBarElement name="Neuen Rang erstellen" container={newRankButton} />
                    </div>
                </div>
            );
        } else {
            return (
            <div className="companySidebar"> 
                <div className="companyRankSidebar">
                    <CompanySideBarElement name="Neuen Rang erstellen" container={newRankButton} />
                </div>
            </div>);
        }
    }
}

class RankPermissionSideBarElement extends React.Component {
    constructor(props) {
        super(props);

        this.onCheck = this.onCheck.bind(this);
    }

    onCheck(evt, permission) {
        if(evt.target.checked) {
            var perm = this.props.permissions.filter((el) => {return el === permission})[0];
            if(!this.props.current.permissions.includes(perm)) {
                this.props.current.permissions.push(perm);
                this.props.sendToServer("CHANGE_COMPANY_RANK_PERMISSION", {rankName: this.props.current.name, permissionId: permission.id, add: true});
            }
        } else {
            this.props.current.permissions = this.props.current.permissions.filter((el) => { return el.id !== permission.id});
            this.props.sendToServer("CHANGE_COMPANY_RANK_PERMISSION", {rankName: this.props.current.name, permissionId: permission.id, add: false});
        }

        this.props.needsUpdate();
    }

    render() {
        return (
        <div className="companyPermissionsList">
            {this.props.permissions.map((el) => {
                return(
                    <div className="companyPermissionsElement">
                        <div className="companyPermissionsElementName">{el.name}</div>
                        <div className="companyPermissionsElementCheck">
                            <input checked={this.props.current.permissions.includes(el)} type="checkbox" onChange={(evt) => {this.onCheck(evt, el)}} style={{height: "50%", width: "50%" }} />
                        </div>
                    </div>
                );
            })}
        </div>);
    }
}

class RankMain extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="companyMainWorkplace">
                <div className="rankList">
                    {this.props.ranks.map((el) => {
                        return <RankShow rank={el} employees={this.props.employees} callback={this.props.callback} isCurrent={this.props.current !== null ? el === this.props.current : false}/>
                    })}
                </div>
            </div>
        );
    }
}

class RankShow extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.callback(this.props.rank);
    }

    render() {
        return (
            <div className={"companyRank " + (this.props.isCurrent ? "isElementCurrent" : "")} onClick={this.onClick}>
                <div className="PADDING"></div>
                <div className="employeeElement">{this.props.rank.name + (this.props.rank.isStandart ? " (Startrang)" : "") + (this.props.rank.isCEO ? " (Inhaber)" : "")}</div>
                <div className="employeeElement">{"Gehalt: $" + this.props.rank.salary}</div>
                <div className="employeeElement">{"Berechtigungen: " + this.props.rank.permissions.length}</div>
                <div className="employeeElement">{"Mitarbeiter mit Rang: " + this.props.employees.filter((el) => {return el.rank === this.props.rank}).length}</div>
            </div>
        );
    }
}

export class Rank {
    constructor(name, salary, permissions, isCEO, isStandart) {
        this.name = name;
        this.salary = salary;
        this.permissions = permissions;
        this.isCEO = isCEO;
        this.isStandart = isStandart;
    }
}

export class Permission {
    constructor(id, companyType, name) {
        this.id = id;
        this.companyType = companyType;
        this.name = name;
    }
}