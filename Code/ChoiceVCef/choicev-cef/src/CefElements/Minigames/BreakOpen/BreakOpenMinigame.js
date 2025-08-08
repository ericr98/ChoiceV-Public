import React from 'react';
import { register } from '../../../App';

import { url } from '../../../index';

import './BreakOpenMinigame.css';

const CROWBAR_STATES = {
    "MOVING": 0,
    "CHARGING": 1,
    "IN_OBJECT": 2,
    "BREAKING": 3,
}

class BreakOpenMinigame extends React.Component {
    constructor(props) {
        super(props);
        
        this.state = {
            open: null,
            crackedOpen: false,

            crowbarStatus: CROWBAR_STATES.MOVING,
            crowbarStatusProgress: 0,
            
            locks: []
        }
        
        this.openBreakOpen = this.openBreakOpen.bind(this);
        
        this.props.input.registerEvent("OPEN_BREAKOPEN", this.openBreakOpen);

        this.onKeyDown = this.onKeyDown.bind(this);
        this.onKeyUp = this.onKeyUp.bind(this);
        
        this.keyInterval = null;
    }

    onKeyDown(e) {
        if (this.state.keyPressed === e.key) {
            return;
        }
        
        this.setState({
            "keyPressed": e.key,
        }, () => {
            if (this.keyInterval != null) {
                clearInterval(this.keyInterval);
            }

            if (this.state.crowbarStatus === CROWBAR_STATES.MOVING && (this.state.data.crowbarDirection === "LtR" || this.state.data.crowbarDirection === "RtL") && (e.key === "ArrowDown" || e.key === "ArrowUp")) {
                this.keyInterval = setInterval(() => {
                    if ((e.key === "ArrowDown" && this.state.crowbarY >= 50 + this.state.data.end) || (e.key === "ArrowUp" && this.state.crowbarY <= 50 - this.state.data.end)) { 
                       return; 
                    }
                    
                    console.log(this.state.crowbarY);
                    this.setState({
                        crowbarY: this.state.crowbarY + (e.key === "ArrowDown" ? 0.25 : -0.25),
                    });
                }, 10);
            } else if (this.state.crowbarStatus === CROWBAR_STATES.MOVING && (this.state.data.crowbarDirection === "TtB" || this.state.data.crowbarDirection === "BtT") && (e.key === "ArrowLeft" || e.key === "ArrowRight")) {
                this.keyInterval = setInterval(() => {
                    this.setState({
                        crowbarX: this.state.crowbarX + (e.key === "ArrowRight" ? 0.25 : -0.25),
                    });
                });
            } else if (e.key === "e") {
                if (this.state.crowbarStatus === CROWBAR_STATES.MOVING) {
                    this.setState({
                        crowbarStatus: CROWBAR_STATES.CHARGING,
                    });

                    this.keyInterval = setInterval(() => {
                        this.setState((prevState) => ({
                            crowbarStatusProgress: Math.min(100, prevState.crowbarStatusProgress + 2),
                        }));
                    }, 100);
                } else if (this.state.crowbarStatus === CROWBAR_STATES.IN_OBJECT) {
                    this.setState({
                        crowbarStatus: CROWBAR_STATES.BREAKING,
                    });
                    
                    this.keyInterval = setInterval(() => {
                        this.setState((prevState) => ({
                            crowbarStatusProgress: Math.min(100, prevState.crowbarStatusProgress + 2),
                        }));
                    }, 100);
                }
            }
        });
    }

    onKeyUp(e) {
        this.setState({
            "keyPressed": null,
        }, () => {
            if (this.keyInterval != null) {
                clearInterval(this.keyInterval);
            }

            if (e.key === "e") {
                if (this.state.crowbarStatus === CROWBAR_STATES.CHARGING) {
                    if (this.state.crowbarStatusProgress >= 90) {
                        this.setState({
                            crowbarStatus: CROWBAR_STATES.IN_OBJECT,
                            crowbarStatusProgress: 0,
                        });
                    } else {
                        this.setState({
                            crowbarStatus: CROWBAR_STATES.MOVING,
                            crowbarStatusProgress: 0,
                        });
                    }
                } else if (this.state.crowbarStatus === CROWBAR_STATES.BREAKING) {
                    if (this.state.crowbarStatusProgress >= 90) {
                        this.setState({
                            crowbarStatus: CROWBAR_STATES.MOVING,
                            crowbarStatusProgress: 0,
                        });
                        
                        console.log([this.state.crowbarX, this.state.crowbarY]);
                        this.setState({
                            holes: this.state.holes.concat([[this.state.crowbarX, this.state.crowbarY]]),
                        });
                    } else {
                        this.setState({
                            crowbarStatus: CROWBAR_STATES.MOVING,
                            crowbarStatusProgress: 0,
                        });
                    }
                }
            }
        });
    }
    
    componentWillUnmount() {
        document.removeEventListener("keydown", this.onKeyUp, false);
        document.removeEventListener("keyup", this.onKeyUp, false);
    } 
    
    openBreakOpen(data) {
        this.setState({
            open: true,
            data: data,
            
            crowbarX: data.crowbarX,
            crowbarY: data.crowbarY,
            
            locks: data.locks,
            holes: [[0, 20]],
        });
        
        document.addEventListener("keydown", this.onKeyDown, false);
        document.addEventListener("keyup", this.onKeyUp, false);
    }
    
    render () {
        if(!this.state.open) {
            return null;
        }
        
        const areaStyle = {
            width: "100vh",
            height: "100vh",
        }
        
        return (
            <div className="standardWrapper">
                <div style={areaStyle}>
                    <Crowbar
                        xBarrier={this.state.data.xCrowbarBarrier}
                        yBarrier={this.state.data.yCrowbarBarrier}

                        x={this.state.crowbarX}
                        y={this.state.crowbarY}
                        direction={this.state.data.crowbarDirection}
                        height={this.state.data.crowbarHeight}
                        
                        state={this.state.crowbarStatus}
                        progress={this.state.crowbarStatusProgress}
                        chargeInDistance={this.state.data.crowbarChargeInDistance}
                    />
                    <div className="standardWrapper">
                        <CrackOpenBackground
                            img={!this.state.crackedOpen ? this.state.data.backgroundOpenImg : this.state.data.backgroundClosedImg}
                            width={this.state.data.width}
                            height={this.state.data.height}
                            direction={this.state.data.crowbarDirection}
                            holes={this.state.holes}
                            locksSize={this.state.data.locksSize}
                            lockLineDist={this.state.data.lockLineDist}
                        />
                    </div>
                </div>
            </div>
        );
    }
}

class CrackOpenBackground extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        const style = {
            position: "absolute",
            backgroundImage: "url(" + url + "minigames/breakopen/" + this.props.img + ".png)",
            backgroundSize: "cover",
            width: this.props.width,
            height: this.props.height,
        }

        return (
            <div style={style}>
                
            </div>
        )
    }
}

class Crowbar extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        let height = this.props.height;
        let width = this.props.height * 0.23;

        let x = this.props.x;
        let y = this.props.y;
        
        if (this.props.state === CROWBAR_STATES.IN_OBJECT || this.props.state === CROWBAR_STATES.BREAKING) {
            if (this.props.direction === "LtR") {
                x += this.props.chargeInDistance;
            } else if (this.props.direction === "RtL") {
                x -= this.props.chargeInDistance;
            } else if (this.props.direction === "TtB") {
                y += this.props.chargeInDistance;
            } else if (this.props.direction === "BtT") {
                y -= this.props.chargeInDistance;
            }
        }
        
        if (this.props.direction === "LtR" || this.props.direction === "RtL") {
            width = this.props.height;
            height = height * 0.23;
        }

        if (this.props.xBarrier) {
           //Cut off the crowbar if it goes past the barrier
           if (x + width > this.props.xBarrier) {
                width = this.props.xBarrier - x;
           }
        }
        
        let animation = null;
        if (this.props.state === CROWBAR_STATES.CHARGING) {
            animation = "shake 0.5s";
        } else if (this.props.state === CROWBAR_STATES.BREAKING) {
            if (this.props.direction === "LtR" || this.props.direction === "RtL") {
                animation = "shakeTopDown 0.5s";
            } else {
                animation = "shakeLeftRight 0.5s";
            }
        }
        
        const style = {
            position: "relative",
            height: height + "%",
            width: width + "%",
            objectFit: "cover",
            objectPosition: "0% 0%",
            
            left: x + "%",
            top: y + "%",
            
            zIndex: 1,

            animation: animation,
            animationIterationCount: (this.props.state === CROWBAR_STATES.CHARGING || this.props.state === CROWBAR_STATES.BREAKING) ? "infinite" : null,

            filter: "blur(" + this.props.progress / 100 + "px) grayscale(" + this.props.progress / 100 + ")",
        }


        return (
            <img style={style} src={url + "minigames/breakopen/crowbar" + this.props.direction + ".png"} />
        )
    }
}

register(BreakOpenMinigame);