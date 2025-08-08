import React from 'react';
import { url } from './../../index';
import { register } from '../../App';
import './style/VoiceRange.css'

export default class VoiceRangeController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            range: null,
        }

        this.onCreateVoiceRange = this.onCreateVoiceRange.bind(this);
        this.onCloseVoiceRange = this.onCloseVoiceRange.bind(this);
        this.onVoiceSolutionMute = this.onVoiceSolutionMute.bind(this);

        this.props.input.registerEvent("CREATE_VOICERANGE", this.onCreateVoiceRange);
        this.props.input.registerEvent("VOICE_SOLUTION_MUTE", this.onVoiceSolutionMute);

        this.onToggleHud = this.onToggleHud.bind(this);
        this.props.input.registerEvent("TOGGLE_HUD", this.onToggleHud);
    }

    onToggleHud() {
        this.setState({
            hidden: !this.state.hidden,
        })
    }
    
    onCreateVoiceRange(data) {
        this.setState({
            range: data.Range,
        });
    }

    onCloseVoiceRange() {
        this.setState({
            range: null,
        });
    }
    
    onVoiceSolutionMute(data) {
        this.setState({
            externalState: data.State,
            externalIcon: data.Icon
        });
    }

    render() {
        if ((this.state.range == null && !this.state.externalState) || this.state.hidden) {
            return null;
        } else {
            return (
                <div className="noSelect" style={{position: "absolute", display: "flex", top: "2.5%", right: "2.5%"}}>
                    {this.state.externalState ? <img src={url + "voice/icon_external_mute_" + this.state.externalIcon + ".png"} style={{width: "2vw", height: "4vh", marginRight: "1.5vh"}} /> : null}
                    <img className="voiceRange" src={url + "voice/icon_sound_" + this.state.range + ".png"} style={{width: "2.5vw", height: "5vh"}} />
                </div>);

        }
    }
}

register(VoiceRangeController);