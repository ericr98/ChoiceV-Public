import React from 'react'
import CombinationLock from './CombinationLock'
import { register } from '../../App';

export default class CombinationLockController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            combination: null,
            id: -1,
        }

        this.onCreate = this.onCreate.bind(this);
        this.onMatch = this.onMatch.bind(this);
        this.onCheck = this.onCheck.bind(this);
        this.onClose = this.onClose.bind(this);
        this.onUnlocked = this.onUnlocked.bind(this);

        this.props.input.registerEvent("COMBINATION_LOCK_OPEN", this.onCreate);
        this.props.input.registerEvent("CLOSE_CEF", this.onClose);

        this.props.input.registerEvent("COMBINATION_LOCK_UNLOCKED", this.onUnlocked);
    }

    onCreate(data) {
        this.setState({
            combination: Array.from(Array(data.length).keys()),
            id: data.id,
        })
    }

    onClose () {
        this.setState({
            combination: null,
            id: null,
        })
    }

    onMatch() {
        setTimeout(() => {
            this.props.output.sendToServer("COMBINATION_LOCK_ACCESSED", {id: this.state.id}, true, "COMBINATION_LOCK");

            this.setState({
                combination: null,
                id: null,
            })
        }, 2000)
    }

    onCheck(checker) {
        this.props.output.sendToServer("COMBINATION_LOCK_CHECK_COMBINATION", {id: this.state.id, combination: checker.join("")}, false);
    }

    onUnlocked() {
        this.refs.combinationLock.open();

        this.onMatch();
    }

    render() {
        if(this.state.combination !== null) {
            return (
            <div style={{position: "absolute", width: "100%", height: "100%", display: "flex", alignItems: "center", justifyContent: "center"}}>
                <CombinationLock ref="combinationLock" combination={this.state.combination} height={80} onCheck={this.onCheck} onMatch={this.onMatch} openText={"Geöffnet!"}/>
            </div>);
        } else {
            return null;
        }
    }
}

register(CombinationLockController)