import React from 'react';
import './style/Company.css';

import EmployeePanel, { Employee } from './Panels/EmployeePanel';
import RankPanel, { Rank, Permission } from './Panels/RankPanel';
import SettingsPanel from './Panels/SettingsPanel';
import TaxPanel from './Panels/TaxPanel';

export default class CompanyPanel extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            companyName: props.data.companyName,
            data: props.data,
            currentSite: "employees", //Back to employees
        }

        this.onSiteChange = this.onSiteChange.bind(this);
    }

    onSiteChange(newSite) {
        this.setState({
            currentSite: newSite,
        });
    }

    render() {
        return(
            <div id="companyPanel">
                <CompanyTopBar companyName={this.state.companyName} siteChange={this.onSiteChange} />
                <div id="companyPanelMain">
                    <CompanyPanelWorkplace data={this.state.data} currentSite={this.state.currentSite} sendToServer={this.props.sendToServer} />
                </div>
            </div>);
    }
}

class CompanyPanelWorkplace extends React.Component {
    constructor(props) {
        super(props);

        var permissions = [];
        props.data.permissions.forEach((el) => {
            var obj = JSON.parse(el);
            permissions.push(new Permission(
                obj.id,
                obj.companyType,
                obj.name,
            ));
        });
        
        permissions.sort((a, b) => {
            if(a.companyType < b.companyType) {
                return -1;
            } else if(a.companyType > b.companyType) {
                return 1;
            } else {
                return 0;
            }
        });

        var ranks = [];
        props.data.ranks.forEach((el) => {
            var obj = JSON.parse(el);
            var perms = permissions.filter((el) => {
                return obj.permissions.includes(el.id);
            });

            ranks.push(new Rank(
                obj.name,
                obj.salary,
                perms,
                obj.isCEO,
                obj.isStandart,
            ));
        });

        ranks = ranks.sort(function(a, b) {
            return b.permissions.length - a.permissions.length;
        });

        var employees = [];
        props.data.employees.forEach((el) => {
            var obj = JSON.parse(el);
            employees.push(new Employee(
                obj.id,
                obj.firstName,
                obj.lastName,
                ranks.filter((el) => { return el.name === obj.rank})[0],
                obj.salary,
                obj.bank,
                obj.todayDuty,
                obj.onDuty,
            ));
        });

        //Sort onDuty Players on top of list
        employees = employees.sort(function(a, b ){
            if(a.onDuty && b.onDuty || !a.onDuty && !b.onDuty ) {
                return a.lastName.localeCompare(b.lastName);
            } else if(a.onDuty && !b.onDuty) {
                return -1;
            } else {
                return 1;
            } 
        });

        this.state = {
            employees: employees,
            ranks: ranks,
            permissions: permissions,
            rankPanelAccess: props.data.rankPanelAccess,
        }

        this.forceUpdate = this.forceUpdate.bind(this);
        this.addNewRank = this.addNewRank.bind(this);
        this.deleteRank = this.deleteRank.bind(this);
    }

    forceUpdate() {
        this.setState(this.state);
    }

    addNewRank() {
        var arr = this.state.ranks;
        var count = 1;
        var standName = "Neuer Rang";
        var name = "Neuer Rang";
        while(this.state.ranks.filter((el) => { return el.name == name }).length > 0) {
            name = standName + " " + count;
            count++;
        }

        arr.push(new Rank(name, 0, []));

        this.setState({
            ranks: arr,
        })

        this.props.sendToServer("ADD_COMPANY_NEW_RANK", {newRankName: name});
    }

    deleteRank(rank) {
        var bool = this.state.employees.filter((el) => {
            return el.rank === rank;
        }).length === 0;

        if(bool) {
            this.setState({
                ranks: this.state.ranks.filter((el) => { return el !== rank }),
            })

            this.props.sendToServer("DELETE_COMPANY_RANK", {rankName: rank.name});
        }
    }

    render() {
        switch(this.props.currentSite) {
            case "employees":
                return (<EmployeePanel employees={this.state.employees} ranks={this.state.ranks} update={this.forceUpdate} sendToServer={this.props.sendToServer} rankPanelAccess={this.state.rankPanelAccess}/>);
            case "ranks":
                return (<RankPanel employees={this.state.employees} ranks={this.state.ranks} permissions={this.state.permissions} update={this.forceUpdate} sendToServer={this.props.sendToServer} deleteRank={this.deleteRank} newRank={this.addNewRank} rankPanelAccess={this.state.rankPanelAccess} />);
            case "taxes":
                return (<TaxPanel taxes={this.props.data.taxes}/>);
            case "settings":
                return (<SettingsPanel />);
            default:
                return null;
        }
    }
}


class CompanyTopBar extends React.Component {
    render() {
        return(
            <div id="companyTopBar">
                <div id="companyNameWrapper">
                    <span className="topBarTextElement">{this.props.companyName}</span>
                </div>

                <div id="companySelectionMenu">
                    <CompanyTopBarSelectionButton name="Angestellte" siteChange="employees" callback={this.props.siteChange} />
                    <CompanyTopBarSelectionButton name="Ränge" siteChange="ranks" callback={this.props.siteChange} />
                    <CompanyTopBarSelectionButton name="Fahrzeuge" siteChange="vehicles" callback={this.props.siteChange} />
                    <CompanyTopBarSelectionButton name="Steuern" siteChange="taxes" callback={this.props.siteChange} />
                    <CompanyTopBarSelectionButton name="Einstellungen" siteChange="settings" callback={this.props.siteChange} />
                </div>
                
            </div>
        );
    }
}

class CompanyTopBarSelectionButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.callback(this.props.siteChange);
    }

    render() {
        return(
            <div className="companyTopBarButtonContainer">
                <div className="companyTopbarButton" onClick={this.onClick}>{this.props.name}</div>
            </div>
        );
    }
}


export class CompanySideBarElement extends React.Component {
    render() {
        return(
            <div className="companySideBarElement">
                <div className="companySideBarElementTitle">{this.props.name}</div>
                <div className="companySideBarElementContent">
                    {this.props.container}
                </div>
            </div>
        );
    }
}