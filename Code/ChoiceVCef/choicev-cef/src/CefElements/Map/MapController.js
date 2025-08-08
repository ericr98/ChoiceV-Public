import React from 'react';
import { register } from './../../App';
import './MapController.css'

import { url } from './../../index';
import { leftLegList } from '../MedicalAnalyse/MedicalAnalyseMapPoint';

export default class MapController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            open: false,
            pos: null,

            currentColor: "#CC8A25",
        }

        //Bind functions
        this.toggleMap = this.toggleMap.bind(this);
        this.updateMapPosition = this.updateMapPosition.bind(this);
        this.closeMap = this.closeMap.bind(this);
        this.closeCef = this.closeCef.bind(this);

        //Register Events
        this.props.input.registerEvent("TOGGLE_MAP", this.toggleMap);
        this.props.input.registerEvent("MAP_POSITION_UPDATE", this.updateMapPosition);

        this.props.input.registerEvent("CLOSE_CEF", this.closeMap);
        this.props.input.registerEvent("CLOSE_MAP", this.closeMap);
    }

    toggleMap(data) {
        this.setState({
            open: !this.state.open,
            pos: this.mapPosition(data.x, data.y),
        });
    }

    updateMapPosition(data) {
        if(this.state.open) {
            this.setState({
                pos: this.mapPosition(data.x, data.y),
            }); 
             
            // setTimeout(() => {
            //     this.props.output.sendToServer('REQUEST_MAP_POSITION_UPDATE', this.state.data, true);
            // }, 200);
        }     
    }

    closeCef() {
        this.setState({
            open: false,
            position: null,
        });
        this.props.output.sendToServer('CLOSE_MAP_BY_CEF');
    }


    closeMap() {
        this.setState({
            open: false,
            position: null,
        });
    }

    mapPosition(x, y) {
        return {x: x * 0.145 - 75, y: y * -0.148 + 300};
    }

    render() {
        if(this.state.open) {
            return (
                <div className="standardWrapper noSelect">
                    <img className="map" src={url + "map/map.png"} alt="Los Santos Tactical Map" />
                    <div className="mapPlayerPosition" style={{marginLeft: this.state.pos.x, marginTop: this.state.pos.y}}></div>
                </div>);
        } else {
            return null;
        }
    }
}

register(MapController);