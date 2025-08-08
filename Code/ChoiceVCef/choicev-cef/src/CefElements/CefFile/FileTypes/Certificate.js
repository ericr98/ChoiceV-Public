import React from 'react';
import { url } from './../../../index';
import { FileInfoElement, FileSignatureElement } from './../CefFile';

export default class CertificateFile extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            data: props.data,
        }

        this.getData = this.getData.bind(this);
    }

    getData(name) {
        return this.state.data.data.filter((el) => el.name == name)[0].data;
    }

    render() {
        var pic = "url(" + url + "cefFile/certificate/Background.png" + ")";
        return(
            <div id="background" class="standardWrapper">
                {this.state.data.isCopy ? <div className="copyWrapper">
                    <img className="copyStamp noSelect" src={url + "cefFile/copyStamp.png"} />
                </div> : null}
                <div id="certificateFile" style={{backgroundImage: pic}}>
                    <div></div>
                    <div id="certificateTitle">{this.getData("title")}</div>
                    <div id="certificateName">{this.getData("name")}</div>
                    <div id="certificateText">{this.getData("text")}</div>
                    <div id="certificateBottom" className='standardWrapper'>
                        <div></div>
                        <div>{this.getData("signDate")}</div>
                        <div></div>
                        <div>{this.getData("signName")}</div>
                        <div></div>
                    </div>
                </div>
            </div> 
        );
    }
}