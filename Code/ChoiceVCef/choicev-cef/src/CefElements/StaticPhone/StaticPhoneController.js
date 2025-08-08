import React from 'react';
import { register } from '../../App';
import './StaticPhone.css';
import { url } from '../../index';
import { Numpad } from './Parts/Numpad';
import { DigitalDisplay } from './Parts/DigitalDisplay';
import { ControlStick, BigButtons } from './Parts/LeftInput';

export default class StaticPhoneController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            phoneData: null
        }
    
        this.openPhone = this.openPhone.bind(this);
        this.closePhone = this.closePhone.bind(this);

        this.props.input.registerEvent("OPEN_STATIC_PHONE", this.openPhone);

        this.onClickNumber = this.onClickNumber.bind(this);
        this.onClickBigButton = this.onClickBigButton.bind(this);
    }

    openPhone(data) {
        this.setState({
           phoneData: data
        });
    }

    closePhone() {
        this.setState({
            phoneData: null,
        });
    }

    onClickNumber(value) {
        this.refs.display.onClickNumber(value);
    }

    onClickBigButton(value) {
        this.refs.display.onClickBigButton(value);
    }

    render() {
        if(this.state.phoneData != null) {
            return (
                <div className="standardWrapper">
                    <div id="staticPhone">
                        <div id="staticPhoneLeft">
                            <ControlStick />
                            <BigButtons onClickBigButton={this.onClickBigButton} />
                        </div>

                        <div id="staticPhoneMiddle">
                            <DigitalDisplay ref="display" data={this.state.phoneData} />
                            <Numpad onClick={this.onClickNumber} />
                        </div>
                    </div>
                </div>
            );
        } else {
            return null;
        }
    }
}

register(StaticPhoneController);