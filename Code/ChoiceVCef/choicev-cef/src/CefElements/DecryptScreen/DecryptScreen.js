import React from 'react';
import { register } from './../../App';
import './DecryptScreen.css';

export default class DecryptScreen extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            activated: false,
        };

        this.timeOut = 1000;
        this.passwordLength = 20;
        
        this.count = 0;
        this.characters = [];
        this.password = 

        this.onDecryptStart = this.onDecryptStart.bind(this);

        this.props.input.registerEvent("DECRYPT_START", this.onDecryptStart);
    }

    onDecryptStart(data) {
        this.setState({
            activated: true,
        });
    }

    onDecryptEnd() {
        this.setState({
            activated: false,
        });
        
        this.props.outPut("DECRYPT_COMPLETE");
    }

    makeid(length) {
        var result           = '';
        var characters       = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        var charactersLength = characters.length;
        for ( var i = 0; i < length; i++ ) {
           result += characters.charAt(Math.floor(Math.random() * charactersLength));
        }
        return result;
     }

    render() {
        if(this.state.activated) {
            return (
                <div className="terminal">
                    <div className="decryptWindow">
                        <div className={"password " + this.state.activated ? "" : "hidden"}></div>
                        <div className="blink granted hidden">ACCESS GRANTED!</div>
                    </div>
                </div>
            );
        } else {
            return null;
        }
    }
}

register(DecryptScreen);