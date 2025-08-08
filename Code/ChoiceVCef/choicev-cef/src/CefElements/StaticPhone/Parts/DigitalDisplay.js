import React, { Fragment } from 'react';


export class DigitalDisplay extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            CurrentMenu: DigitalDisplayHomeMenu,
        }

        this.onSelectMenu = this.onSelectMenu.bind(this);
    }

    onSelectMenu(newMenu) {
        this.setState({
            CurrentMenu: newMenu,
        });
    }

    onClickNumber(number) {
        if(number != "*" && number != "#" && this.state.CurrentMenu != DigitalDisplaySelectNumberMenu && this.state.CurrentMenu != DigitalDisplayNewContactMenu) {
            this.setState({
                CurrentMenu: DigitalDisplaySelectNumberMenu,
            }, () => {
                this.refs.menu.onClickNumber(number);
            });
        } else {
            if(this.refs.menu.onClickBigButton != undefined) {
                this.refs.menu.onClickNumber(number);
            }
        }
    }

    onClickBigButton(name) {
        if(this.refs.menu.onClickBigButton != undefined) {
            this.refs.menu.onClickBigButton(name);
        }
    }

    render() {
        return(
            <div className="standardWrapper">
                <div id="staticPhoneDisplayWrapper">
                    <div id="staticPhoneDisplay">
                        <this.state.CurrentMenu ref="menu" data={this.props.data} changeMenu={this.onSelectMenu} />
                        
                        <div id="staticPhoneDisplayMenuList">
                            <DigitalDisplayMenuElement name="Home"/>
                            <DigitalDisplayMenuElement name="Kontakte"/>
                            <DigitalDisplayMenuElement name="Anrufliste"/>
                        </div>
                    </div>
                    <div id="staticPhoneDisplayButtons">
                        <DigitalDisplayMenuButton onSelect={this.onSelectMenu} menu={DigitalDisplayHomeMenu} />
                        <DigitalDisplayMenuButton onSelect={this.onSelectMenu} menu={DigitalDisplayContactMenu} />
                        <DigitalDisplayMenuButton onSelect={this.onSelectMenu} />
                    </div>
                </div>
            </div>);
    }
}

class DigitalDisplayMenuElement extends React.Component {
    render() {
        return(<div className="standardWrapper">
                <div className="staticPhoneDisplayMenuElement standardWrapper">{this.props.name}</div>
            </div>);
    }
}

class DigitalDisplayMenuButton extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.onSelect(this.props.menu);
    }

    render() {
        return(<div className="standardWrapper">
                <div className="staticPhoneDisplayButton staticPhoneButton" onClick={this.onClick}></div>
            </div>);
    }
}

class DigitalDisplayDateTime extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            date: new Date(),
        };
    }

    componentDidMount() {
        this.interval = setInterval(() => this.setState({ date: new Date() }), 1000);
    }

    render() {
        var hours = this.state.date.getHours();
        var minutes = this.state.date.getMinutes();

        var day = this.state.date.getDate();
        var month = this.state.date.getMonth() + 1;
        var year = this.state.date.getFullYear();

        return(<div className="staticPhoneDisplayClock staticPhoneDisplayPadding">
                {hours + ":" + minutes}<br />
                {day + "." + month + "." + year}<br />
            </div>
        );
    }
}

class DigitalDisplaySelectNumberMenu extends React.Component {
    constructor(props) {
        super(props);    

        this.state = {
            selectedNumber: "",
        }
    }

    onClickNumber(number) {
        if (this.state.selectedNumber.length <= 12) {
            this.setState({
                selectedNumber: this.state.selectedNumber + number,
            });
        }
    }

    onClickBigButton(name) {
        if(name == "clear") {
            this.setState({
                selectedNumber: "",
            })
        }
    }

    render() {
        return (
            <Fragment>
                <div id="staticPhoneDisplaySelectNumber" className="staticPhoneDisplayPadding">
                        {this.state.selectedNumber}
                    </div>
                <DigitalDisplayDateTime />
            </Fragment>
        );
    }
}

class DigitalDisplayHomeMenu extends React.Component {
    render() {
        return (
            <Fragment>
                <div id="staticPhoneDisplayHome" className="staticPhoneDisplayPadding">
                    Eigene Nummer: {this.props.data.number}
                </div>
                <DigitalDisplayDateTime />
            </Fragment>
        );
    }
}

class DigitalDisplayContactMenu extends React.Component {
    constructor(props) {
        super(props);

        var contacts = [];
        this.props.data.contacts.forEach((el) => {
            contacts.push(JSON.parse(el));
        });

        this.state = {
            contacts: contacts,
            currentIdx: 0,
        };

        this.onKeyDown = this.onKeyDown.bind(this);
    
        this.displayRef = React.createRef();

        this.onClickBigButton = this.onClickBigButton.bind(this);
    }

    onKeyDown(evt) {
        if(evt.keyCode == 87 || evt.keyCode == 38) {
            this.setState({
                currentIdx: (this.state.currentIdx-1).mod(this.state.contacts.length),
            });
        } else if(evt.keyCode == 83 || evt.keyCode == 40) {
            this.setState({
                currentIdx: (this.state.currentIdx+1).mod(this.state.contacts.length),
            });
        }

        if(this.state.currentIdx >= 5) {
            this.displayRef.current.scrollTo(0, (window.innerHeight)*0.02 * this.state.currentIdx);
        } else {
            this.displayRef.current.scrollTo(0, 0);
        }
    }

    onClickNumber(number) {
        if (number == "*") {
            this.props.changeMenu(DigitalDisplayNewContactMenu);
        }
    }

    onClickBigButton(name) {
        switch(name){
            case "clear":
                this.state.contacts.splice(this.state.currentIdx, 1);
                this.setState({
                    contacts: this.state.contacts,
                });
                    //TODO SEND TO SERVER
                break;
            case "call":
                    //TODO SEND TO SERVER
                break;
        }
    }

    componentDidMount(){
        document.addEventListener("keydown", this.onKeyDown);
    }

    componentWillUnmount() {
        document.removeEventListener("keydown", this.onKeyDown);
    }

    render() {
        return (
            <Fragment>
                <div id="staticPhoneDisplayContact" className="staticPhoneDisplayPadding" ref={this.displayRef}>
                    {this.state.contacts.map((el, idx) => {
                        return (<div className={"staticPhoneDisplayContactElement " + (idx == this.state.currentIdx ? "staticPhoneDisplaySelected" : "")}>{idx}.{el.name}:{el.number}</div>);
                    })}
                </div>
                <div className="staticPhoneDisplayBottomRightInfo">C: Loeschen<br/>*: Neu</div>
                <DigitalDisplayDateTime />
            </Fragment>
        );
    }
}

class DigitalDisplayNewContactMenu extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            name: "",
            number: "",

            nameSelected: true,
        }

        this.onKeyDown = this.onKeyDown.bind(this);
    }

    onKeyDown(evt) {
        if(evt.keyCode == 87 || evt.keyCode == 38 || evt.keyCode == 83 || evt.keyCode == 40) {
           this.setState({
               nameSelected: !this.state.nameSelected,
           }) 
        }
    }

    componentDidMount(){
        document.addEventListener("keydown", this.onKeyDown);
    }

    componentWillUnmount() {
        document.removeEventListener("keydown", this.onKeyDown);
    }

    render() {
        return (
            <Fragment>
                <div id="staticPhoneDisplayNewContact" className="staticPhoneDisplayPadding">
                    <div className="staticPhoneDisplayNewContactField">
                        <div className={"staticPhoneDisplayNewContactFieldLabel " + (this.state.nameSelected ? "staticPhoneDisplaySelected" : "")}>Name</div>
                        <div type="text" className="staticPhoneDisplayNewContactFieldValue">____________</div>
                    </div>
                    <div className="staticPhoneDisplayNewContactField">
                        <div className={"staticPhoneDisplayNewContactFieldLabel " + (!this.state.nameSelected ? "staticPhoneDisplaySelected" : "")}>Nummer</div>
                        <div type="text" className="staticPhoneDisplayNewContactFieldValue">____________</div>
                    </div>
                </div>
                <DigitalDisplayDateTime />
            </Fragment>
        );
    }
}