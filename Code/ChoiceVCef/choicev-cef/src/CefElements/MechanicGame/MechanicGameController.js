import React, { Fragment } from 'react';
import { url } from './../../index';
import { register } from '../../App';
import './MechanicGameStyle.css'

export default class MechanicGameController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            data: null,
        }

        this.startMechanicGame = this.startMechanicGame.bind(this);
        this.updateMechanicGamePart = this.updateMechanicGamePart.bind(this);
        this.closeGame = this.closeGame.bind(this);

        this.onLvlDown = this.onLvlDown.bind(this);
        this.onClick = this.onClick.bind(this);

        this.props.input.registerEvent("SHOW_MECHANIC_GAME", this.startMechanicGame);
        this.props.input.registerEvent("UPDATE_MECHANIC_GAME_PART", this.updateMechanicGamePart);
    }

    startMechanicGame(data) {
        var parts = data.parts.map((el) => JSON.parse(el)); 
        var placedParts = parts.filter((el) => !el.stash);
        var removedParts = parts.filter((el) =>  el.stash);

        var depths = data.depths.map((el) => {
            var el = JSON.parse(el);
            return { y: el.position.x, x: el.position.y, lvl: el.depth };
        });

        this.setState({
            data: data,
            maxDepth: Math.max(...placedParts.concat(removedParts).map((el) => el.depth)),
            placedParts: placedParts,
            removedParts: removedParts,
            depths: depths,
        });
    }

    updateMechanicGamePart(data) {
        var part = this.state.placedParts.concat(this.state.removedParts).find((el) => el.id == data.partId);
        switch(data.action) {
            case "IDENTIFY":
                part.iden = true;
                break;
            case "REMOVE":
                this.setState({
                    placedParts: this.state.placedParts.filter((el) => el.id != data.partId),
                    removedParts: this.state.removedParts.filter((el) => el.id != data.partId),
                }, () => {
                    this.forceUpdate();
                });
                break;
            case "MOVE_TO_STASH":               
                part.stash = true;
                this.setState({
                    placedParts: this.state.placedParts.filter((el) => el.id != data.partId),
                    removedParts: this.state.removedParts.concat(part),
                }, () => {
                    this.forceUpdate();
                });
                break;
            case "MOVE_BACK_IN":             
                part.stash = false;

                this.state.depths.forEach((depth) => {
                    part.pos.forEach(pos => {                  
                    if(pos.x == depth.x && pos.y == depth.y) {
                        if(part.depth != depth.lvl) {
                            depth.lvl = part.depth;
                            this.props.output.sendToServer("MECHANICAL_GAME_LVL_UP", {position: {x: pos.y, y: pos.x}, depth: part.depth}, false);
                        }
                    }
                    });
                })

                this.setState({
                    removedParts: this.state.removedParts.filter((el) => el.id != data.partId),
                    placedParts: this.state.placedParts.concat(part),
                }, () => {
                    this.forceUpdate();
                });
                break;
        }

        //this.forceUpdate();
    }

    closeGame() {
        this.setState({
            data: null,
        })

        this.props.output.sendToServer("MECHANICAL_GAME_CLOSED", {}, true, "MECHANICAL_GAME");
    }

    onLvlDown(col, row) {
        var already = this.state.depths.find((d) => d.x == col && d.y == row);
        if(already != null) {
            if(already.lvl < this.state.maxDepth) {
                already.lvl += 1;
            }

            this.forceUpdate(() => {
                this.forceUpdate();
            });

            this.props.output.sendToServer("MECHANICAL_GAME_LVL_DOWN", {position: {x: row, y: col}, depth: already.lvl}, false);
        } else {
            this.state.depths.push({x: col, y: row, lvl: 1});
            this.forceUpdate(() => {
                this.forceUpdate();
            });

            this.props.output.sendToServer("MECHANICAL_GAME_LVL_DOWN", {position: {x: row, y: col}, depth: 1}, false);
            return 1;
        }
    }
    
    onClick(id) {
        var blockedByOtherPart = false;
        var part = this.state.placedParts.find((el) => el.id == id);
        if(part != null) {
            blockedByOtherPart = this.state.placedParts.some((el) => {
                return el.depth < part.depth && el.pos.some((pos) => { return part.pos.some((pos2) => { return pos.x == pos2.x && pos.y == pos2.y; }); });
            });
        }

        this.props.output.sendToServer("MECHANICAL_GAME_ON_CLICK", {id: id, blockedByOtherPart: blockedByOtherPart});
    }

    render() {
       if(this.state.data != null) {
            var fields = [];
            for (var j = 0; j < this.state.data.row; j++) {
                for (var i = 0; i < this.state.data.col; i++) {
                    var depth = this.state.depths.find((d) => {return d.x == j && d.y == i});
                    var lvl = 0;
                    if(depth != null) {
                        lvl = depth.lvl;
                    }
                    fields.push(<MechanicGameGrid id={(i * 100) + (j)} key={(i * 100) + (j)} col={j} row={i} maxDepth={this.state.maxDepth} parts={this.state.placedParts} depth={lvl} onLvlDown={this.onLvlDown} onClick={this.onClick}/>);
                }
            }

            return(
                <div className="standardWrapper">
                    <div className="mechGameWrapper">
                        <div className="mechBigWrapper">
                            <div id="mechGameRemovedParts">
                                <button id="mechGameCloserButton" onClick={this.closeGame}>×</button>
                                {this.state.removedParts.map((el, id) => {
                                    return <RemovedMechComponent id={id} key={id} el={el} onClick={this.onClick} />
                                })}
                            </div>
                        </div>
                        <div className="mechBigWrapper">
                            <div id="mechGameWorkplace" style={{gridTemplateColumns: "repeat(" + this.state.data.col + ", 15vh)", gridTemplateRows: "repeat(" + this.state.data.row + ", 15vh)"}}>
                                {fields}
                            </div>
                        </div>
                    </div>
                </div>
            );
       } else {
            return null;
       }
    }
}

class MechanicGameGrid extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
           part: this.props.parts.filter((p) => { return p.depth == this.props.depth && p.pos.some((pos) => {return pos.x == this.props.col && pos.y == this.props.row})})
        }

        this.onButtonDown = this.onButtonDown.bind(this);
    }

    componentDidUpdate() {
        this.state.part = this.props.parts.filter((p) => { return p.depth == this.props.depth && p.pos.some((pos) => {return pos.x == this.props.col && pos.y == this.props.row})})
    }

    onButtonDown() {
        var depth = this.props.depth;
        if(this.state.part.length == 0) {
            depth = this.props.onLvlDown(this.props.col, this.props.row);
        }

        this.setState({
            part: this.props.parts.filter((p) => { return p.depth == depth && p.pos.some((pos) => {return pos.x == this.props.col && pos.y == this.props.row})})
        })
    }

    render() {
        var buttonJsx = null;
        if(this.state.part.length == 0 && this.props.depth < this.props.maxDepth) {
            buttonJsx = (<button className="mechGameGridButton" onClick={this.onButtonDown}>▼</button>);
        }
        return (
        <div className="standardWrapper mechGameGrid">
            <div className="mechTopbar">
            <div className="mechGameDepthCounter">
                {this.props.depth == 0 ? this.props.depth : "-" + this.props.depth}
            </div>
                {buttonJsx}
            </div>
            {this.state.part.map((el, id) => {
                return <MechanicComponent id={id} key={id} col={this.props.col} row={this.props.row} el={el} onClick={this.props.onClick}/>
            })}
        </div>);
    }
}

class MechanicComponent extends React.Component {
    constructor(props) {
        super(props);

        this.calcMargin = this.calcMargin.bind(this);
        this.onClick = this.onClick.bind(this);
    }

    onClick(evt) {
        this.props.onClick(this.props.el.id);
    }

    calcMargin() {
        var pos = this.props.el.pos;

        var rightExtra = false;
        var leftExtra = false;
        var topExtra = false;
        var bottomExtra = false;    

        pos.forEach((el) => {
            if(!(el.x == this.props.col && el.y == this.props.row)) {
                if(el.x == this.props.col) {
                    if(el.y > this.props.row) {
                        rightExtra = true;
                    } else {
                        leftExtra = true;
                    }
                } else if(el.y == this.props.row) {
                    if(el.x > this.props.col) {
                        bottomExtra = true;
                    } else {              
                        topExtra = true;
                    }
                }
            }
        });

        return [leftExtra, rightExtra, bottomExtra, topExtra];
    }

    render() {
        if(!this.props.el.iden) {
            var extras = this.calcMargin();

            var connectors = [];
            if(extras[0]) connectors.push({style: {marginRight: "5vh"}});
            if(extras[1]) connectors.push({style: {marginLeft: "5vh"}});
            if(extras[2]) connectors.push({style: {marginTop: "5vh"}});
            if(extras[3]) connectors.push({style: {marginBottom: "5vh"}});

            return (
                <Fragment>
                    <div className="standardWrapper mechGameNotIdenPart" onClick={this.onClick}>
                        <span>?</span>
                    </div>
                    {connectors.map((el) => {
                        return <div className="mechGameNotIdenPartExtra" style={el.style}></div>
                    })}
                </Fragment>
            );
        } else {
            if(this.props.el.pos.length == 4) {
                var pos = this.props.el.pos;
                var x = this.props.col;
                var y = this.props.row;

                var topLeft = pos.reduce((prev, current) => (prev.x + prev.y < current.x + current.y) ? prev : current);
                var bottomLeft = pos.find((el) => el.y == topLeft.y && el.x > topLeft.x);

                var bottomRight = pos.reduce((prev, current) => (prev.x + prev.y > current.x + current.y) ? prev : current);
                var topRight = pos.find((el) => el.y == bottomRight.y && el.x < bottomRight.x);

                if(topLeft.x == x && topLeft.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",1.png"} style={{marginLeft: "3vh", marginTop: "3vh"}} onClick={this.onClick}></img>)
                } else if(topRight.x == x && topRight.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",2.png"} style={{marginRight: "3vh", marginTop: "3vh"}} onClick={this.onClick}></img>)
                } else if(bottomLeft.x == x && bottomLeft.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",3.png"} style={{marginLeft: "3vh", marginBottom: "3vh"}} onClick={this.onClick}></img>)
                } else {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",4.png"} style={{marginRight: "3vh", marginBottom: "3vh"}} onClick={this.onClick}></img>)
                }

            //Maybe fix, but not right now
            } else if(this.props.el.pos.length == 3) {
                var pos = this.props.el.pos;
                var x = this.props.col;
                var y = this.props.row;

                var left = pos.reduce((prev, current) => (prev.y < current.y) ? prev : current);
                var right = pos.reduce((prev, current) => (prev.y > current.y) ? prev : current);
                var middle = pos.find((el) => el != left && el != right);

                if(left.x == x && left.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",1.png"} style={{marginLeft: "3vh"}} onClick={this.onClick}></img>)
                } else if(middle.x == x && middle.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",2.png"} onClick={this.onClick}></img>)
                } else if(right.x == x && right.y == y) {
                    return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",3.png"} style={{marginRight: "3vh"}} onClick={this.onClick}></img>)
                }
            } else if(this.props.el.pos.length == 2) {
                var pos = this.props.el.pos;
                var x = this.props.col;
                var y = this.props.row;
                var other = pos.find((el) => !(el.x == x && el.y == y));

                if(x == other.x) {
                    if(y < other.y) {
                        return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",1.png"} style={{marginLeft: "3vh"}} onClick={this.onClick}></img>)
                    } else {               
                        return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",2.png"}  style={{marginRight: "3vh"}} onClick={this.onClick}></img>)
                    }
                } else {
                    if(x < other.x) {
                        return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",1.png"} style={{marginTop: "3vh", transform: "rotate(90deg)"}} onClick={this.onClick}></img>)
                    } else {               
                        return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ",2.png"} style={{marginBottom: "3vh",  transform: "rotate(90deg)"}} onClick={this.onClick}></img>)
                    }
                }
            } else {
                return (<img className="mechGameIdenPart" src={url + "mechGame/" + this.props.el.img + ".png"} onClick={this.onClick}></img>)
            }
        }
    }
}

class RemovedMechComponent extends React.Component {
    constructor(props) {
        super(props);

        this.onClick = this.onClick.bind(this);
    }

    onClick() {
        this.props.onClick(this.props.el.id);
    }

    render() {
        return(
            <img className="mechGameRemovedPart" src={url + "mechGame/" + this.props.el.img + ".png"} onClick={this.onClick}></img>
        );
    }
}

register(MechanicGameController);