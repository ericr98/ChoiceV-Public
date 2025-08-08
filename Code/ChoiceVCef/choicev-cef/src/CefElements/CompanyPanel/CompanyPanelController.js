import React from 'react';
import { register } from '../../App';
import CompanyPanel from './CompanyPanel';

export default class CompanyPanelController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            currentData: null,
        }

        this.onOpenPanel = this.onOpenPanel.bind(this);
        this.onCloseCompany = this.onCloseCompany.bind(this);
        this.sendToServer = this.sendToServer.bind(this);

        this.props.input.registerEvent("OPEN_COMPANY_PANEL", this.onOpenPanel);
        this.props.input.registerEvent("CLOSE_CEF", this.onCloseCompany);
        this.props.input.registerEvent("CLOSE_COMPANY_PANEL", this.onCloseCompany);
    }

    onCloseCompany() {
        this.setState({
            currentData: null,
        });
    }

    onOpenPanel(data) {
        this.setState({
            currentData: data,
        });
    }

    sendToServer(evt, data) {
        data["companyId"] = this.state.currentData.companyId;
        this.props.output.sendToServer(evt, data, false);
    }

    render() {
        if(this.state.currentData !== null) {
            return (
            <div id="companyWrapper">
                <CompanyPanel data={this.state.currentData} sendToServer={this.sendToServer} />
            </div>);
        } else {
            return null;
        }
    }
}

register(CompanyPanelController);