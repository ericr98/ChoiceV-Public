import React from 'react';
import { FileInfoElement, FileSignatureElement } from './../CefFile';
import { url } from './../../../index';

class Data {
    constructor(identifier, x, y, text, fontSize, width, height, placeholder) {
        this.identifier = identifier;
        this.x = x;
        this.y = y;
        this.text = text;
        this.fontSize = fontSize;
        this.width = width;
        this.height = height;
        this.placeholder = placeholder;
    }
}

class SaveButton {
    constructor(identifier, x, y, fontSize, width) {
        this.identifier = identifier;
        this.x = x;
        this.y = y;
        this.fontSize = fontSize;
        this.width = width;
    }
}

class SignatureElement {
    constructor(identifier, x, y, fontSize, width, signatureInfo, signatureText) {
        this.identifier = identifier;
        this.x = x;
        this.y = y;
        this.fontSize = fontSize;
        this.width = width;
        this.signatureInfo = signatureInfo;
        this.signatureText = signatureText;
    }
}

export default class VariableFile extends React.Component {
    constructor(props) {
        super(props);

        var variableDatas = [];
        var staticDatas = [];
        var signatureDatas = [];

        var saveButton;

        var allText = [];
        props.data.data.forEach((obj) => {
            if(obj.type === "STATIC") {
                staticDatas.push(new Data(obj.identifier, obj.x, obj.y, obj.text, obj.fontSize, obj.width, obj.height, obj.placeholder));
            } else if(obj.type === "VARIABLE") {       
                variableDatas.push(new Data(obj.identifier, obj.x, obj.y, obj.text, obj.fontSize, obj.width, obj.height, obj.placeholder));

                allText.push({identifier: obj.identifier, text: obj.text});
            } else if(obj.type === "SAVE_BUTTON") {
                saveButton = new SaveButton(obj.identifier, obj.x, obj.y, obj.fontSize, obj.width);
            } else if(obj.type === "SIGNATURE") {
                console.log(obj);
                signatureDatas.push(new SignatureElement(obj.identifier, obj.x, obj.y, obj.fontSize, obj.width, obj.signatureInfo, obj.signatureText));
            }
        });

        this.state = {
            variableDatas: variableDatas,
            staticDatas: staticDatas,
            signatureDatas: signatureDatas,

            backgroundImage: props.data.backgroundImage,
            width: props.data.width,
            height: props.data.height,

            allText: allText,

            debugMode: props.data.debugMode,

            saveButton: saveButton,
            readOnly: props.data.readOnly,
            isCopy: props.data.isCopy,

            id: props.data.id,
        }

        this.onDataChange = this.onDataChange.bind(this);     
        this.onSave = this.onSave.bind(this);           
        this.onSign = this.onSign.bind(this);      
    }

    onDataChange(identifier, text) {
        var current = this.state.allText;
        var find = current.find((el) => el.identifier === identifier);
        find.text = text;

        this.setState({
            variableData: current,
        });
    }

    onSave(e) {
        if(!this.state.readOnly) {
            this.props.output.sendToServer("SAVE_VARIABLE_FILE", {
                data: this.state.allText,
                id: this.state.id,
            }, true, "CEF_FILE");
        }
        this.props.close();
    }

    onSign(identifier) {
        this.props.output.sendToServer("SIGN_VARIABLE_FILE", {
            identifier: identifier,
            data: this.state.allText,
            id: this.state.id,
        }, true, "CEF_FILE");
        
        this.props.close();
    }

    render() {
        var backgroundStyle = {
            width: this.state.width,
            height: this.state.height,
            backgroundImage: "url(" + this.state.backgroundImage + ")",
            backgroundSize: "100% 100%",
            backgroundPosition: "center",
            backgroundRepeat: "no-repeat",
        }

        var styleButton = {
            top: this.state.saveButton.y,
            left: this.state.saveButton.x,

            width: this.state.saveButton.width,
        }

        var copyStyle = {   
            width: this.state.width * 0.7,
            height: this.state.width * 0.7,
            opacity: "0.3",
        }

        return(
            <div id="background" className="noSelect">
                <div class="standardWrapper">
                    {this.state.isCopy ? <div className="copyWrapper">
                        <img className="copyStamp noSelect" style={copyStyle} src={url + "cefFile/copyStamp.png"} />
                    </div> : null}

                    <div id="variableFile" style={backgroundStyle}>
                        {/* {this.state.staticDatas.map((el) => {
                            return <StaticData el={el}/>;
                        })} */}

                        {this.state.variableDatas.map((el) => {
                            return <VariableData key={el.identifier} el={el} debugMode={this.state.debugMode} readOnly={this.state.readOnly} text={this.state.allText.find((text) => text.identifier === el.identifier).text} onDataChange={this.onDataChange}/>;
                        })}

                        <div className="searchVariableFileSaveButton" style={styleButton}>
                            <button className="searchVariableFileSaveButtonButton" style={{fontSize: this.state.saveButton.fontSize}} onClick={this.onSave}>Einpacken (Speichern)</button>
                        </div>
                        
                        {this.state.signatureDatas.map((el) => {
                            var signatureStyle = {
                                position: "absolute",
                                width: el.width,
                                top: el.y,
                                left: el.x,
                            }
                            
                            if(this.props.debugMode) {
                                signatureStyle["backgroundColor"] = "rgba(0, 255, 0, 0.3)";
                            }

                            return (
                                <div style={signatureStyle}>
                                    <FileSignatureElement showButton={el.signatureText == "" || el.signatureText == null || this.props.debugMode} fontSize={el.fontSize} onSign={() => this.onSign(el.identifier)} dontShowTop={true} signatureName={el.signatureText} buttonName={el.signatureInfo} readOnly={this.state.readOnly}/>           
                                </div>)
                        })}
                    </div> 
                </div>
            </div> 
        );
    }
}

class VariableData extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            el: props.el,
        }

        this.onChange = this.onChange.bind(this);
    }

    onChange(e) {
        this.props.onDataChange(this.state.el.identifier, e.target.value);
    }

    render() {
        var style = {
            left: this.state.el.x,
            top: this.state.el.y, 
            position: "absolute",
            fontSize: this.state.el.fontSize,

            width: this.state.el.width,
            height: this.state.el.height,
            overflow: "hidden",

            fontFamily: "Lato",
        }

        if(this.props.debugMode) {
            style["backgroundColor"] = "rgba(255, 0, 0, 0.3)";
        }

        return(
            <textarea style={style} placeholder={this.state.el.placeholder} onChange={this.onChange} value={this.props.text} readOnly={this.props.readOnly}></textarea>
        );
    }
}

// class StaticData extends React.Component {
//     constructor(props) {
//         super(props);

//         this.state = {
//             el: props.el
//         }
//     }

//     render() {
//         var style={
//             left: this.state.el.x,
//             top: this.state.el.y, 
//             position: "absolute",
//             fontSize: this.state.el.fontSize,
//         }

//         return(
//             <div style={style}>
//                 {this.state.el.text}
//             </div>
//         );
//     }
// }