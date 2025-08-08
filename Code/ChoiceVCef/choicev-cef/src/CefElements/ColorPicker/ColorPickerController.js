import React from 'react';
import { ChromePicker } from 'react-color'
import { PhotoshopPicker } from 'react-color'
import { CompactPicker } from 'react-color'
import { CirclePicker } from 'react-color'
import { SwatchesPicker } from 'react-color'
import { SketchPicker } from 'react-color'
import { GithubPicker } from 'react-color'
import { TwitterPicker } from 'react-color'
import { register } from '../../App';
import './style/ColorPicker.css'

export default class ColorPickerController extends React.Component {
    constructor(props) {
        super(props)
        
        this.state = {
            open: false,
            colortyp: null,
            color: null,
            colorarr: null,
            lastPicker: {},
        }

        this.onCreateColorPicker = this.onCreateColorPicker.bind(this);
        this.onCloseColorPicker = this.onCloseColorPicker.bind(this);
        this.onColorPickerEvent = this.onColorPickerEvent.bind(this);

        this.props.input.registerEvent("CREATE_COLOR", this.onCreateColorPicker);

        this.props.input.registerEvent("CLOSE_CEF", this.onCloseColorPicker);
        this.props.input.registerEvent("CLOSE_COLOR", this.onCloseColorPicker);
    }

    onCreateColorPicker(data) {
        this.setState({
            open: true,
            colortyp: data.ColorTyp,
            color: data.Color,
            colorarr: data.ColorArr
        });
    }

    onCloseColorPicker() {
        if(!this.state.open) {
            return;
        }

        this.setState({
            open: false,
            colortyp: null,
            color: null,
            colorarr: null
        });

        var picker = this.state.lastPicker;
        picker.close = true;
        this.props.output.sendToServer("COLOR_EVENT", picker, true, "COLOR_PICKER");
    }

    onColorPickerEvent(data) {
        var picker = {
            Hex: data.hex,
            Rgb: {
                r: data.rgb.r,
                g: data.rgb.g,
                b: data.rgb.b,
                a: data.rgb.a,
            },
            close: false,
        }

        this.props.output.sendToServer("COLOR_EVENT", picker, false, "COLOR_PICKER");
        
        this.setState({
            color: picker.Rgb,
            lastPicker: picker,
        });
    }

    render() {
        if (this.state.colortyp == null) {
            return null;
        } else {
            if (this.state.colortyp == 1) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <ChromePicker color={this.state.color} disableAlpha={true} onChange={this.onColorPickerEvent} />
                    </div>);
                    
            } else if (this.state.colortyp == 2) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <PhotoshopPicker color={this.state.color} disableAlpha={true} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 3) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <CirclePicker color={this.state.color} colors={this.state.colorarr} circleSize={40} circleSpacing={10} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 4) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <CompactPicker color={this.state.color} colors={this.state.colorarr} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 5) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <SwatchesPicker color={this.state.color} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 6) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <SketchPicker color={this.state.color} disableAlpha={true} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 7) {
                return (
                    <div style={{position: "absolute", display: "grid", bottom: "10%", right: "5%", gridTemplateRows: "88% 12%", gridGap: "0.5vh"}}>
                        <GithubPicker color={this.state.color} colors={this.state.colorarr} triangle={"hide"} onChange={this.onColorPickerEvent} />
                        <div className="standardWrapper">
                            <button className="colorEndButton" onClick={this.onCloseColorPicker}>Abschließen</button>
                        </div>
                    </div>);
            
            } else if (this.state.colortyp == 8) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <TwitterPicker color={this.state.color} colors={this.state.colorarr} triangle={"hide"} onChange={this.onColorPickerEvent} />
                    </div>);
            
            } else if (this.state.colortyp == 9) {
                return (
                    <div style={{position: "absolute", display: "flex", bottom: "10%", right: "5%"}}>
                        <CirclePicker color={this.state.color} colors={this.state.colorarr} circleSize={25} circleSpacing={10} onChange={this.onColorPickerEvent} />
                    </div>);
            } else {
                return null;
            }
        }
    }
}

register(ColorPickerController);