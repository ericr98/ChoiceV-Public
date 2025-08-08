import React from 'react';
import { register } from './../../App';

export default class DebugInformationController extends React.Component {
    constructor(props) {
        super(props);

        this.state = { };

        this.onAddDebugInfo = this.onAddDebugInfo.bind(this);
        this.onRemoveDebugInfo = this.onRemoveDebugInfo.bind(this);

        this.props.input.registerEvent("SET_DEBUG_INFO", this.onAddDebugInfo);
        this.props.input.registerEvent("REMOVE_DEBUG_INFO", this.onRemoveDebugInfo);
    }

    onAddDebugInfo(data) {
        this.setState({
            [data.key]: data.value,
        });
    }

    onRemoveDebugInfo(data) {
        this.setState({
            [data.key]: undefined,
        });
    }

    render() {
        return (
            <table style={{position: 'absolute', top:"70px", left: "10px"}}>
                <tbody>
                    <tr>
                        {Object.keys(this.state).map((key) => {
                            return (<td key={key} style={{fontSize: "20px", color: "white"}} dangerouslySetInnerHTML={{__html: this.state[key]}}></td>);
                        })}
                    </tr>
                </tbody>
            </table>
        );
    }
}

register(DebugInformationController);