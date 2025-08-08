import React from 'react';

export default class PrescriptionFile extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            show: false
        }
    }

    onOpenPrescription(e) {
        this.setState({
            show: true,
        })
    }

    render() {
        return (
            <div id="background" className='standardWrapper'>
                <div style={{backgroundColor: "black", height: "80%", width: "30%"}}></div>
                Test
            </div>
        );
    }
}