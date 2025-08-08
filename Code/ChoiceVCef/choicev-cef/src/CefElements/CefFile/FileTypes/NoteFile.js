import React from 'react';

export default class NoteFile extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            titleValue: this.props.data.title,
            textValue: this.props.data.text,
            readOnly: this.props.data.readOnly,
            id: this.props.data.id,
        }

        this.onTitleChange = this.onTitleChange.bind(this);
        this.onTextChange = this.onTextChange.bind(this);

        this.onSave = this.onSave.bind(this);      
        this.onFinalize = this.onFinalize.bind(this);
    }

    onTitleChange(e) {
        this.setState({
            titleValue: e.target.value,
        });
    }

    onTextChange(e) {
        this.setState({
            textValue: e.target.value,
        });
    }

    onSave(e) {
        if(!this.state.readOnly) {
            this.props.output.sendToServer("SAVE_NOTE", {
                title: this.state.titleValue,
                text: this.state.textValue,
                readOnly: false,
                id: this.state.id,
            }, true, "CEF_FILE");
        }
        
        this.props.close();
    }

    onFinalize(e) {
        this.props.output.sendToServer("SAVE_NOTE", {
            title: this.state.titleValue,
            text: this.state.textValue,
            readOnly: true,
            id: this.state.id,
        }, true, "CEF_FILE");
        
        this.props.close();
    }

    render() {
        return(
            <div id="background">
                <div id="noteFile">
                    <textarea id="noteTitle" class="noteTextarea" spellCheck="false" placeholder="Trage einen Titel ein." readOnly={this.state.readOnly} onChange={this.onTitleChange} value={this.state.titleValue}></textarea>
                    <textarea id="noteBody" class="noteTextarea"  spellCheck="false" placeholder="Trage hier den Text des Dokuments ein." readOnly={this.state.readOnly} onChange={this.onTextChange} value={this.state.textValue}></textarea>
                    <div id="noteButtons">
                        <button id="noteSave" className="noteButton" onClick={this.onSave} style={{backgroundColor: "#8E8E8B"}}>Einpacken</button>
                        <button id="noteFinalize" className="noteButton" disabled={this.state.readOnly} onClick={this.onFinalize} style={{backgroundColor: "#8E8E8B"}}>Finalisieren</button>
                    </div>
                </div> 
            </div> 
        );
    }
}