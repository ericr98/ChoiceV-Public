import React, { Fragment } from 'react';

export default class TaxPanel extends React.Component { 
    constructor(props) {
        super(props);
        console.log(props.taxes);
        var arr = [];
        props.taxes.forEach((el) => {
            var obj = JSON.parse(el);
            arr.push(new TaxModel(obj.id, obj.tax, obj.amount, obj.message, obj.date, obj.automatic));
        });

        arr = arr.sort(function(a, b) {
            return b.date - a.date;
        });


        this.state = {
            currentElement: null,
            taxes: arr,
        }

    }

    render() {
        return (
            <div id="companyPanelChangeable">
                <TaxSidebar element={this.state.currentElement} />
                <TaxMain taxes={this.props.taxes} />
            </div>
        );
    }
}

class TaxSidebar extends React.Component {
    render() {
        return (
            <div className="companySidebar">
                
            </div>
        );
    }
}

class TaxMain extends React.Component {
    render() {
        return(
            <div className="companyMainWorkplace">
                <div className="companyTaxList">
                    <TaxMainList taxes={this.props.taxes}/>
                </div>
            </div>
        );
    }
}

class TaxMainList extends React.Component {
    render() {
        return this.props.taxes.map((el) => {
            return <TaxElementShow el={el}/>;
        });
    }
}

class TaxElementShow extends React.Component {
    render() {
       return (
        <div className="companyTaxElement">
            
        </div>);
    }
}

class TaxModel {
    constructor(id, tax, amount, message, date, automatic) {
        this.id = id;
        this.tax = tax;
        this.amount = amount;
        this.message = message;
        this.date = date;
        this.automatic = automatic;
    }
}