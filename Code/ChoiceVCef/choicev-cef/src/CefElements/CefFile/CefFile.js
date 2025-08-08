import React from 'react';
import { register } from './../../App';
import './CefFile.css';

import NoteFile from './FileTypes/NoteFile';
import InvoiceFile from './FileTypes/InvoiceFile';
import { USCustomsFile, PericoCustomsFile } from './FileTypes/CustomsFile';
import PrescriptionFile from './FileTypes/PrescriptionFile';
import CertificateFile from './FileTypes/Certificate';
import VariableFile from './FileTypes/VariableFile';
import { Fragment } from 'react';

export default class CefFile extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            CurrentNote: null,
            fileData: {},
        }

        //Bind functions
        this.openNote = this.openNote.bind(this);
        this.openInvoiceFile = this.openInvoiceFile.bind(this);
        this.openUSCustomsFile = this.openUSCustomsFile.bind(this);
        this.openPericoCustomsFile = this.openPericoCustomsFile.bind(this);
        this.openPrescription = this.openPrescription.bind(this);
        this.openCertificate = this.openCertificate.bind(this);

        this.openVariableFile = this.openVariableFile.bind(this);

        this.closeFile = this.closeFile.bind(this);

        //Register Events
        this.props.input.registerEvent("OPEN_NOTE", this.openNote);
        this.props.input.registerEvent("OPEN_INVOICE_FILE", this.openInvoiceFile);
        
        this.props.input.registerEvent("OPEN_US_CUSTOMS_FILE", this.openUSCustomsFile);
        this.props.input.registerEvent("OPEN_PERICO_CUSTOMS_FILE", this.openPericoCustomsFile);

        this.props.input.registerEvent("OPEN_PRESCRIPTION", this.openPrescription);
        this.props.input.registerEvent("OPEN_CERTIFICATE", this.openCertificate);

        this.props.input.registerEvent("OPEN_VARIABLE_FILE", this.openVariableFile);


        this.props.input.registerEvent("CLOSE_CEF", this.closeFile);
    }

    openNote(data) {
        this.setState({
            CurrentNote: NoteFile,
            fileData: data,
        });
    }

    openInvoiceFile(data) {
        this.setState({
            CurrentNote: InvoiceFile,
            fileData: data,
        });
    }

    openUSCustomsFile(data) {
        this.setState({
            CurrentNote: USCustomsFile,
            fileData: data,
        });
    }

    openPericoCustomsFile(data) {
        this.setState({
            CurrentNote: PericoCustomsFile,
            fileData: data,
        });
    }

    openPrescription(data) {
        this.setState({
            CurrentNote: PrescriptionFile,
            fileData: data,
        });
    }

    openCertificate(data) {
        this.setState({
            CurrentNote: CertificateFile,
            fileData: data,
        });
    }

    openVariableFile(data) {
        this.setState({
            CurrentNote: null,
            fileData: {},
        }, () => {
            this.setState({
                CurrentNote: VariableFile,
                fileData: data,
            });
        });
    }



    closeFile() {
        this.setState({
            CurrentNote: null,
            fileData: {},
        });
    }


    render() {
        if(this.state.CurrentNote != null) {
            return <this.state.CurrentNote close={this.closeFile} data={this.state.fileData} output={this.props.output}/>;
        } else {
            return null;
        }
    }
}

register(CefFile);


export class FileInfoElement extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            value: this.props.value,
        }

        this.onChange = this.onChange.bind(this);
    }

    onChange(evt) {
        this.setState({
            value: evt.target.value,
        });
        this.props.onChange(evt);
    }

    render() {
       return (
           <div className="cefFileInfoElement" style={{fontSize: this.props.fontSize}}>
               <div className="cefFileInfoElementKey" style={{fontSize: this.props.fontSize}}>{this.props.info + ":"}</div>
               <textarea className="cefFileInfoElementValue" spellCheck="false" data={this.props.data} rows="1" style={{fontSize: this.props.fontSize}} placeholder={this.props.placeholder} readOnly={this.props.readOnly} onChange={this.onChange} value={this.state.value}></textarea>
           </div>
       ); 
    }
}

export class FileSignatureElement extends React.Component {
    constructor(props) {
        super(props);


        this.onClick = this.onClick.bind(this);
        this.getFirstElement = this.getFirstElement.bind(this);
        this.getSecondElement = this.getSecondElement.bind(this);
    }

    onClick(evt) {
        this.props.onSign(this.props.dataName);
    }

    getFirstElement() {
        if(this.props.dontShowTop) {
            return null;
        } else {
            return (<div className="cefFileSignatureElementKey">{this.props.info + ":"}</div>);
        }
    }

    getSecondElement() {
        if(this.props.showButton) {
            var style = {
                backgroundColor: "#8E8E8B",
            }

            return(
                <button className="cefFileSignatureElementButton" onClick={this.onClick} style={style}>{this.props.buttonName}</button>
            );
        } else {
            var style = {
                fontSize: this.props.fontSize ? this.props.fontSize : "3vh" 
            }
            return(
                <div className="cefFileSignatureElementSignature" style={style}>{this.props.signatureName}</div>
            );
        }
    }

    render() {
       return (
           <div className="cefFileSignatureElement">
                {this.getFirstElement()}
                {this.getSecondElement()}
           </div>
       ); 
    }
}