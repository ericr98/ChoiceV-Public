import React, { Fragment } from 'react';
import { url } from '../../../index';

export class ControlStick extends React.Component {
    render() {
        return(
            <div className="standardWrapper">
                <div id="staticPhoneControlStick">
                    <div className="staticPhoneControlStickElement standardWrapper" style={{gridRow: "2", gridColumn: "1"}}>◀</div>
                    <div className="staticPhoneControlStickElement standardWrapper" style={{gridRow: "1", gridColumn: "2"}}>▲</div>
                    <div className="staticPhoneControlStickElement standardWrapper" style={{gridRow: "2", gridColumn: "3"}}>▶</div>
                    <div className="staticPhoneControlStickElement standardWrapper" style={{gridRow: "3", gridColumn: "2"}}>▼</div>
                </div>
            </div>
        );
    }
}

export class BigButtons extends React.Component {
    constructor(props) {
        super(props);

        this.onClickBigButton = this.onClickBigButton.bind(this);
    }

    onClickBigButton(evt) {
        this.props.onClickBigButton(evt.target.getAttribute('name'));
    }

    render() {
        return(
            <div id="staticPhoneBigButtonsList">
                <div className="standardWrapper">
                    <div className="staticPhoneBigButton staticPhoneButton" name="clear" onClick={this.onClickBigButton}>C</div>
                </div>
                <div className="standardWrapper">
                    <div className="staticPhoneBigButton staticPhoneButton" name="forward" onClick={this.onClickBigButton}>
                        <img className="staticPhoneBigButtonImage" src={url + "/staticPhone/arrow.svg"}></img>
                    </div>
                </div>
                <div className="standardWrapper">
                    <div className="staticPhoneBigButton staticPhoneButton" name="call" onClick={this.onClickBigButton}>
                        <img className="staticPhoneBigButtonImage" src={url + "/staticPhone/call.png"}></img>
                    </div>
                </div>
                <div className="standardWrapper">
                    <div className="staticPhoneBigButton staticPhoneButton" name="hangup" onClick={this.onClickBigButton}>
                        <img className="staticPhoneBigButtonImage" src={url + "/staticPhone/hangUp.png"}></img>
                    </div>
                </div>
            </div>
        );
    }
}