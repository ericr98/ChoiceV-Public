import React from 'react';

export class Numpad extends React.Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div id="staticPhoneNumpad">
                <NumpadNumber onClick={this.props.onClick} name="1" subName=""/>
                <NumpadNumber onClick={this.props.onClick} name="2" subName="ABC"/>
                <NumpadNumber onClick={this.props.onClick} name="3" subName="DEF"/>
                <NumpadNumber onClick={this.props.onClick} name="4" subName="GHI"/>
                <NumpadNumber onClick={this.props.onClick} name="5" subName="JKL"/>
                <NumpadNumber onClick={this.props.onClick} name="6" subName="MNO"/>
                <NumpadNumber onClick={this.props.onClick} name="7" subName="PQRS"/>
                <NumpadNumber onClick={this.props.onClick} name="8" subName="TUV"/>
                <NumpadNumber onClick={this.props.onClick} name="9" subName="WXYZ"/>
                <NumpadNumber onClick={this.props.onClick} name="*" subName=""/>
                <NumpadNumber onClick={this.props.onClick} name="0" subName=""/>
                <NumpadNumber onClick={this.props.onClick} name="#" subName=""/>
            </div>
        );
    }
}

class NumpadNumber extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.onClick(this.props.name);
    }

    render() {
        return (
            <div className="standardWrapper">
                <div className="staticPhoneNumpadNumber staticPhoneButton" onClick={this.onClick}>
                    <div className="standardWrapper staticPhoneNumpadNumberBig">{this.props.name}</div>
                    <div className="standardWrapper">{this.props.subName}</div>
                </div>
            </div>
        );
    }
}
