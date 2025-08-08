import React, { Fragment } from 'react';
import { url } from './../../index';
import { register } from '../../App';
import './style/HUDController.css'

Number.prototype.pad = function(size) {
    var s = String(this);
    while (s.length < (size || 2)) {s = "0" + s;}
    return s;
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function mapNumberToImage(number) {
    if(number >= 40) {
        return 1;
    } else if(number >= 20) {
        return 2;
    } else if(number >= 7.5) {
        return 3;
    } else {
        return 4;
    }
}

export default class HUDController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            hidden: false,

            hudInfo: null,
            speed: 0,
            carInfo: null,
            carIcons: {light: 0, mech: 0, tempo: 0, belt: 0},

            terminalInfo: null,
            
            receivingChannels: [],
            sendingChannels: [],
        }

        this.onUpdateFoodHud = this.onUpdateFoodHud.bind(this);
        this.onUpdateCarHud = this.onUpdateCarHud.bind(this);
        this.removeCarHud = this.removeCarHud.bind(this);

        this.onUpdateCarSpeed = this.onUpdateCarSpeed.bind(this);
        this.onUpdateCarInfo = this.onUpdateCarInfo.bind(this);

        this.onUpdateTerminalHud = this.onUpdateTerminalHud.bind(this);
        this.removeTerminalHud = this.removeTerminalHud.bind(this);

        this.onToggleHud = this.onToggleHud.bind(this);
        
        this.onUpdateChannelHud = this.onUpdateChannelHud.bind(this);
        
        this.props.input.registerEvent("TOGGLE_HUD", this.onToggleHud);

        this.props.input.registerEvent("UPDATE_FOOD_HUD", this.onUpdateFoodHud);
        this.props.input.registerEvent("UPDATE_CAR_HUD", this.onUpdateCarHud);
        this.props.input.registerEvent("REMOVE_CAR_HUD", this.removeCarHud);
        this.props.input.registerEvent("UPDATE_CAR_SPEED_CLIENT", this.onUpdateCarSpeed);
        this.props.input.registerEvent("UPDATE_CAR_ICON", this.onUpdateCarInfo);

        this.props.input.registerEvent("UPDATE_TERMINAL_HUD", this.onUpdateTerminalHud);
        this.props.input.registerEvent("REMOVE_TERMINAL_HUD", this.removeTerminalHud);
        
        this.props.input.registerEvent("UPDATE_CHANNEL_HUD", this.onUpdateChannelHud);
    }

    onToggleHud() {
        this.setState({
            hidden: !this.state.hidden,
        })
    }

    onUpdateFoodHud(data) {
        this.setState({
            hudInfo: data,
        })
    }

    onUpdateCarHud(data) {
        this.setState({
            carInfo: data,
        })
    }

    removeCarHud() {
        this.setState({
            carInfo: null,
        })
    }

    onUpdateCarSpeed(data) {
        this.setState({
            speed: data.speed,
        })
    }

    onUpdateCarInfo(data) {
        var info = this.state.carIcons;
        info[data.name] = data.value;
        this.setState({
            carIcons: info,
        })
    }

    onUpdateTerminalHud(data) {
        this.setState({
            terminalInfo: data,
        })
    }

    removeTerminalHud() {
        this.setState({
            terminalInfo: null,
        })
    }

    onUpdateChannelHud(data) {
        var channel = data.channel;
        var isReceiving = data.isReceiving;
        var isRemoving = data.isRemoving;

        if (isRemoving) {
            if (isReceiving) {
                this.setState({
                    receivingChannels: this.state.receivingChannels.filter((el) => el != channel),
                });
            } else {
                this.setState({
                    sendingChannels: this.state.sendingChannels.filter((el) => el != channel),
                });
            }
        } else {
            if (isReceiving) {
                this.setState({
                    receivingChannels: [...this.state.receivingChannels, channel],
                });
            } else {
                this.setState({
                    sendingChannels: [...this.state.sendingChannels, channel],
                });
            }
        }
    }

    render() {
        if (this.state.hidden) {
            return null;
        } else {
            return (
                <Fragment>
                    {this.state.hudInfo != null ? <div className="foodHudBackground noSelect">
                        <FoodHudComponent value={this.state.hudInfo.hunger} category={"hunger"}/>
                        <FoodHudComponent value={this.state.hudInfo.thirst} category={"thirst"}/>
                        <FoodHudComponent value={this.state.hudInfo.energy} category={"energy"}/>
                    </div> : null}
                    

                    {this.state.carInfo != null ? <div className="carHudBackground noSelect">
                        <div className="hudSpeedInfos">
                            <CarHudInfoIcon category="light" value={this.state.carIcons.light} />
                            <CarHudInfoIcon category="mech" value={this.state.carIcons.mech}/>
                            <CarHudInfoIcon category="tempo" value={this.state.carIcons.tempo}/>
                            <CarHudInfoIcon category="belt" value={this.state.carIcons.belt}/>
                        </div>
                        <div className="hudSpeedIconWrapper">
                            <img className="hudSpeedIcon" src={url + "hud/ico_tacho.png"} />
                        </div>
                        <div className="hudSpeedTextWrapper">
                            <div className="hudSpeedText">{this.state.speed.toFixed(0) + " km/h"}</div>
                        </div>
                        <div className="hudKmIconWrapper">
                            <img className="hudSpeedIcon" src={url + "hud/ico_km.png"} />
                        </div>
                        <div className="hudKmTextWrapper">
                            <div className="hudKmText">{this.state.carInfo.milage.toFixed(2) + " km"}</div>
                        </div>
                        <div className="hudFuelIconWrapper">
                            <img className="hudFuelIcon" src={url + (this.state.carInfo.fuelType == 3 ? "hud/ico_power.png" : "hud/ico_fuel.png")} />
                        </div>
                        {this.state.carInfo.fuelMax != -1 ? 
                        <div className="hudFuelBarWrapper">
                            <div className="hudFuelFrame">
                                <div className="hudFuelBar below" style={{width: ((100 / this.state.carInfo.fuelMax) * this.state.carInfo.fuel)  + "%", backgroundImage: "url(" + url + "hud/filler.png"}}></div>
                                <div className="hudFuelBar top" style={{backgroundImage: "url(" + url + "hud/gas_bar.png"}}></div>
                            </div>
                        </div> : <div/>}
                    </div> : null}

                    {this.state.terminalInfo != null ? <div className="terminalHudBackground noSelect">
                        <div className='terminalHudToken standardWrapper'><span style={{fontWeight: 'bold'}}>{this.state.terminalInfo.tokens}</span>&nbsp;Marken</div>
                    </div> : null}

                    {(this.state.receivingChannels?.length > 0 || this.state.sendingChannels?.length > 0) ? <div className="channelHudBackground noSelect">
                        <DisplayChannelsHud sendingChannels={this.state.sendingChannels} receivingChannels={this.state.receivingChannels} />
                    </div> : null}
                </Fragment>);
        }
    }
}

class CarHudInfoIcon extends React.Component {
    render() {
        return(
            <div className="standardWrapper">
                <img className="hudCarInfoIcon" src={url + "hud/ico_" + this.props.category + "_" + this.props.value + ".png"}/>
            </div>
        );
    }
}

class FoodHudComponent extends React.Component {
    render() {
        var opacity = Math.max(0, 1 - 0.35 - (this.props.value / 100));

        return(
            <div className="standardWrapper">
                <img className="hungerThirstIcon" src={url + "hud/ico_" + this.props.category + "_" + mapNumberToImage(this.props.value) + ".png"} style={{opacity: opacity}}/>
            </div>
        );
    }
}

class DisplayChannelsHud extends React.Component {
    render() {
        return(
            <div id="hudChannelsWrapper">
                {this.props.sendingChannels.map((channel, index) => {
                    return <div key={index} className="channelWrapper">
                        <div className="hudChannelName"><span style={{color: 'orange'}}>←</span>&nbsp;{channel}</div>
                    </div>
                })}
                {this.props.sendingChannels.length > 0 && this.props.receivingChannels.length > 0 ? <hr align="left" width="30%" size="1" color="#666666"/> : null}
                {this.props.receivingChannels.map((channel, index) => {
                    return <div key={index} className="channelWrapper">
                        <div className="hudChannelName"><span style={{color: 'green'}}>→</span>&nbsp;{channel}</div>
                    </div>
                })}
            </div>
        );
    }
}

register(HUDController);