import React from 'react';
import { register } from './../../App';
import './FishingGame.css';

export default class FishingController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            activated: false,
        };

        this.props.input.registerEvent("FISHING_START", this.onFishingStart);
    }

    onFishingStart(data) {
        this.setState({
            activated: true,
            level: data.level
        });
    }

    render() {
        return (
            <div id="fishingGame">
                <div id="fishingRod">

                </div>
            </div>
        );
    }
}

register(FishingController);