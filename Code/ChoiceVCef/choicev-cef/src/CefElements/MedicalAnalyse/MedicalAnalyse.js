import React, { Fragment } from 'react';

import './MedicalAnalyse.css';

import { url } from './../../index';
import { register } from '../../App';
import * as PointData from './MedicalAnalyseMapPoint';


export default class MedicalAnalyse extends React.Component {
    constructor(props) {
        super(props);

        this.charId = -1;

        this.state = {
            //Fill wit PointData.List for debug!
            points: null,
            injuries: null,
            render: false,
        }

        this.onCreate = this.onCreate.bind(this);
        this.onClose = this.onClose.bind(this);
        this.onEvent = this.onEvent.bind(this);

        this.props.input.registerEvent("MEDICAL_ANALYSE", this.onCreate);
        this.props.input.registerEvent("CLOSE_CEF", this.onClose);
    }

    onClose() {
        this.setState({
            render: false,
        })
        this.charId = -1;
    }

    onEvent(injuryId) {
        this.props.output.sendToServer("INJURY_CLICK", {injuryId: injuryId, charId: this.charId}, false);
    }

    onCreate(data) {
        var inj = [];

        data.injuries.forEach((el) => {
            var obj = JSON.parse(el);
            var pos;
            switch(obj.bodyPart) {
                case PointData.BodyPart.RIGHT_ARM:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.rightArmList);
                    break;
                case PointData.BodyPart.LEFT_ARM:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.leftArmList);
                    break;
                case PointData.BodyPart.HEAD:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.headList);
                    break;
                case PointData.BodyPart.TORSO:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.torsoList);
                    break;
                case PointData.BodyPart.LEFT_LEG:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.leftLegList);
                    break;
                case PointData.BodyPart.RIGHT_LEG:
                    pos = this.createPositionForBodyPart(obj.seed, PointData.rightLegList);
                    break;
            }
            inj.push(new Injury(obj.injuryId, obj.severness, pos.x, pos.y));
        });
        this.charId = data.charId;

        this.setState({
            injuries: inj,
            render: true,
        })
    }

    createPositionForBodyPart(seed, points) {
        var minX = 100;
        var maxX = 0;
        var minY = 100;
        var maxY = 0;

        var tries = 40;
        points.forEach((el) => {
            if(el[0] > maxX) {
                maxX = el[0];
            }

            if(el[0] < minX) {
                minX = el[0]
            }

            if(el[1] > maxY) {
                maxY = el[1];
            }

            if(el[1] < minY) {
                minY = el[1]
            }
        });
        var changeSeed = seed;
        var randX = getPosFromSeed(changeSeed, minX, maxX);
        var randY = getPosFromSeed(changeSeed+1, minY, maxY);
        while(!this.checkPointInside(randX, randY, points)) {
            changeSeed += 2;
            tries--;
            randX = getPosFromSeed(changeSeed, minX, maxX);
            randY = getPosFromSeed(changeSeed+1, minY, maxY);
        }

        //If number cannot be generated fast enough!
        if(tries <= 0) {
            return {x: points[0][0], y: points[0][1]};
        }

        return {x: randX, y: randY};
    }

    checkPointInside(x, y, vs) {      
        var inside = false;
        for (var i = 0, j = vs.length - 1; i < vs.length; j = i++) {
            var xi = vs[i][0], yi = vs[i][1];
            var xj = vs[j][0], yj = vs[j][1];
        
            var intersect = ((yi > y) != (yj > y))
                && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
            if (intersect) inside = !inside;
        }
        
        return inside;
    }
    
    render() {
        if(this.state.render) {
        return (
            <div className="medicalWrapper">
                <div style={{backgroundImage: "url( " + url + "medicAnalyse/medicLayout.png)"}} className="medicalImage">
                    {this.state.points !== null ? <BodyPartDrawer points={this.state.points}/> : null}
                    {this.state.injuries !== null ? <InjuryDrawer callback={this.onEvent} injuries={this.state.injuries} /> : null}
                </div>
            </div>);
        } else {
            return null;
        }
    }
}

class InjuryDrawer extends React.Component {

    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    getColorForSeverness(severness, outer) {
        var color;
        if(severness == -1) {
            color = "rgb(35, 50, 255, ";
        } else if(severness == 0) {
            color = "rgb(51, 156, 21, ";
        } else if(severness <= 3) {
            color = "rgb(204, 213, 16, ";
        } else {
            color = "rgb(203, 11, 11, ";
        }
        if(outer) {
            return color + "0.5)";
        } else {
            return color + "0.8)";
        }
    }

    onClick(element) {
        this.props.callback(element.id);
    }

    render() {
        return this.props.injuries.map((el) => {

            var injWidth = 2;
            var injHeight = 2;

            var displaySeverness = (el.severness == -1 ? 1 : el.severness + 1);

            var injStyle = {
                position: "absolute",
                top: (el.y - injWidth) + "%", 
                left: (el.x - injHeight) + "%", 
                width: injWidth + "vh", 
                height: injHeight + "vh",
                display: "flex",
                justifyContent: "center",
                alignItems: "center", 
            }

            var outerStyle = {
                position: "absolute",
                width: ((40 + 10 * displaySeverness) + "%"), 
                height: ((40 + 10 * displaySeverness) + "%"), 
                backgroundColor: this.getColorForSeverness(el.severness, true),
                borderRadius: (10 * displaySeverness) + "px",
            }

            var innerStyle = {
                position: "absolute",
                width: ((25 + 4 * displaySeverness) + "%"), 
                height: ((25 + 4 * displaySeverness) + "%"), 
                backgroundColor: this.getColorForSeverness(el.severness, false),
                borderRadius: (10 * displaySeverness) + "px",   
            }

            return (
            <div style={injStyle}>
                <div style={outerStyle} onClick={(evt) => {this.onClick(el)}}/>
                <div style={innerStyle} onClick={(evt) => {this.onClick(el)}}/>
            </div>
            )
        });
    }
}

class BodyPartDrawer extends React.Component {
    render() {
        return this.props.points.map((el) => {
            return <div style={{position: "absolute", top: el[1] + "%", left: el[0] + "%", width: "4px", height: "4px", backgroundColor: "black"}}></div>
        });
    }
}

class Injury {
    constructor(id, severness, x, y) {
        this.id = id;
        this.severness = severness;
        this.x = x;
        this.y = y;
    }
}

function getPosFromSeed(seed, min, max) {
    var x = Math.sin(seed++) * 10000;
    var rand = x - Math.floor(x);

    return Math.floor(rand * (max - min + 1)) + min;
}   

register(MedicalAnalyse);