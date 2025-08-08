import React from 'react';
import { url } from '../../../index';
import { FileInfoElement, FileSignatureElement } from '../CefFile';

var currentId = 0;

export default class InvoiceFile extends React.Component {
    constructor(props) {
        super(props);

        var prds = [];

        props.data.products.forEach((el) => {
            var obj = JSON.parse(el);
            prds.push(new InvoiceProduct(currentId, obj.count, obj.price, obj.name));
            currentId++;
        });

        this.state = {
            data: props.data,
            products: prds,

            readOnly: props.data.sellerSignature != "" || props.data.buyerSignature != "",
        }

        this.onSign = this.onSign.bind(this);   
        this.onSave = this.onSave.bind(this);      
        this.onChangeInfo = this.onChangeInfo.bind(this);

        this.onRemoveProduct = this.onRemoveProduct.bind(this);
        this.onAddProduct = this.onAddProduct.bind(this);
    }

    onSave(e) {
        var data = this.state.data;
        data["products"] = this.state.products.map(p => JSON.stringify(p));
        this.props.output.sendToServer('SAVE_INVOICE_FILE', data, true, "CEF_FILE");
        this.props.close();
    }

    onSign(dataName) {
        var data = this.state.data;
        data["products"] = this.state.products.map(p => JSON.stringify(p));
        data[dataName] = "TO_FILL";
        this.props.output.sendToServer("SAVE_INVOICE_FILE", data, true, "CEF_FILE");
        this.props.close();
    }

    onChangeInfo(attr, val) {
        var data = this.state.data;
        data[attr] = val;
        this.setState({
            data: data,
        });
    }

    onRemoveProduct(product) {
        this.setState({
            products: this.state.products.filter((el) => { return el.id !== product.id })
        })
    }

    onAddProduct() {
        if(this.state.products.length < 10) {
            var arr = this.state.products;
            arr.push(new InvoiceProduct(currentId, 0, 0, ""));
            currentId++;
            this.setState({
                product: arr,
            })
        }
    }

    render() {
        return(
            <div id="background">
                {this.state.data.isCopy ? <div className="copyWrapper">
                    <img className="copyStamp noSelect" src={url + "cefFile/copyStamp.png"} />
                </div> : null}
                <div id="invoiceFile">
                    <div className="fullWrapper">
                        <div className="invoiceCompany invoiceTitlePadding">
                            {this.state.data.companyName}
                        </div>
                    </div>

                    <div className="fullWrapper">
                        <div className="invoiceTitle invoiceTitlePadding fullWrapper">
                            <div className="invoiceTitleBig">{"Rechnung: " + this.state.data.invoiceId}</div>
                            <div>
                                <div className="invoiceTitleSmall">{this.state.data.charName}</div>
                                <div className="invoiceTitleSmall">{this.state.data.street}</div>
                                <div className="invoiceTitleSmall">{this.state.data.city}</div>
                            </div>
                        </div>
                    </div>
                    
                    <InvoiceDate readOnly={this.state.readOnly} data={this.state.data} changeInfo={this.onChangeInfo} />
                    <InvoiceProductList readOnly={this.state.readOnly} products={this.state.products} tax={this.props.data.tax} removeProduct={this.onRemoveProduct} addProduct={this.onAddProduct} />
                    <InvoiceAdditionalInfo additionalInfo={this.state.data.additionalInfo} paymentInfo={this.state.data.paymentInfo} readOnly={this.state.readOnly} changeInfo={this.onChangeInfo} />
                    <InvoiceSignature data={this.state.data} sign={this.onSign}/>
                    <div className="standardWrapper">
                        <button className="invoiceSaveButton" onClick={this.onSave} style={{backgroundColor: "#8E8E8B"}}>Einpacken (Speichern)</button>
                    </div>
                </div>
            </div>);
    }
}

class InvoiceSignature extends React.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        return (
            <div>
                <div className="invoiceSignatureWrapper">
                    <FileSignatureElement showButton={this.props.data.sellerSignature == ""} onSign={this.props.sign} signatureName={this.props.data.sellerSignature} dataName={"sellerSignature"} info="Unterschrift Verkäufer" buttonName="Als Verkäufer unterschreiben" readOnly={true}/>
                    <div />
                    <FileSignatureElement showButton={this.props.data.buyerSignature == ""} onSign={this.props.sign} signatureName={this.props.data.buyerSignature} dataName={"buyerSignature"} info="Unterschrift Käufer" buttonName="Als Käufer unterschreiben" readOnly={true}/>
                </div>
            </div>
        );
    }
}

class InvoiceAdditionalInfo extends React.Component {
    constructor(props) {
        super(props);
    
        this.state = {
            paymentInfo: props.paymentInfo,
            additionalInfo: props.additionalInfo,
        }

        this.onPaymentChange = this.onPaymentChange.bind(this);
        this.onChangeInfo = this.onChangeInfo.bind(this);
    }

    onPaymentChange(evt) {
        this.setState({
            paymentInfo: evt.target.value,
        })

        this.props.changeInfo("paymentInfo", evt.target.value);
    }

    onChangeInfo(evt) {
        this.setState({
            additionalInfo: evt.target.value,
        })

        this.props.changeInfo("additionalInfo", evt.target.value);
    }

    render() {
        return(
            <div className="invoiceAdditionalInfo fullWrapper">
                <FileInfoElement info="Die Zahlung erfolgte" value={this.state.paymentInfo} readOnly={true} onChange={this.onPaymentChange}/>
                <div className="invoiceAdditionalInfoInfo">
                    <div className="invoiceAdditionalInfoInfoTitle">Zusätzliche Informationen:</div>
                    <textarea className="invoiceAdditionalInfoInfoValue" rows="5" spellCheck="false" placeholder="Hier zusätzliche Informationen für Käufer o. Verkäufer eintragen" readOnly={this.props.readOnly} onChange={this.onChangeInfo} value={this.state.additionalInfo} />
                </div>
            </div>
        );
    }
}

class InvoiceProductList extends React.Component { 
    constructor(props) {
        super(props);

        this.update = this.update.bind(this);
    }

    getNetto() {
        var count = 0;
        this.props.products.forEach((el) => {
            if(!isNaN(el.price) && !isNaN(el.count)) {
                count += el.price * el.count;
            }
        });

        return count;
    }

    update() {
        this.forceUpdate();
    }

    render() {
        var netto = this.getNetto();
        var taxAmount = Math.round(netto * this.props.tax * 100) / 100;

        return (
            <div className="invoiceProductListWrapper">
                {this.props.readOnly ? null : <img className="invoiceProductAdd" src={url + "cefFile/invoice/addIcon.svg"} onClick={this.props.addProduct} />}
                <div className="invoiceProductList">
                    <div className="invoiceProduct invoiceBottomBorder">
                        <div className="invoiceProductTitle">Anzahl</div>
                        <div className="invoiceProductTitle">Einzelpreis</div>
                        <div className="invoiceProductTitle">Beschreibung</div>
                        <div className="invoiceProductTitle">Gesamtpreis</div>
                    </div>
                    {this.props.products.map((el) => {
                        return <InvoiceProductShow key={el.id} el={el} readOnly={this.props.readOnly} removeProduct={this.props.removeProduct} update={this.update} />
                    })}
                    <div className="invoiceProductCombined">
                        <div className="invoiceProductCombinedInfo">
                            <div className="invoiceProductCombinedInfoText fullWrapper">Netto</div>
                            <div className="invoiceProductCombinedInfoText fullWrapper">{"$" + netto.toFixed(2)}</div>
                        </div>
                        <div className="invoiceProductCombinedInfo">
                            <div className="invoiceProductCombinedInfoText fullWrapper">{"Umsatzsteuer (" + (this.props.tax * 100) + "%)"}</div>
                            <div className="invoiceProductCombinedInfoText fullWrapper">{"$" + taxAmount}</div>
                        </div>
                        <div className="invoiceProductCombinedInfo">
                            <div className="invoiceProductCombinedInfoText fullWrapper">Rechnungsbetrag</div>
                            <div className="invoiceProductCombinedInfoText fullWrapper">{"$" + (netto + taxAmount).toFixed(2)}</div>
                        </div>
                    </div>
                </div>
            </div>);
    }
}

class InvoiceProductShow extends React.Component {
    constructor(props) {
        super(props);

        this.onRemove = this.onRemove.bind(this);

        this.onChange = this.onChange.bind(this);
    }

    onRemove(evt) {
        this.props.removeProduct(this.props.el);
    }

    onChange(evt) {
        var attr = evt.target.getAttribute('data');
        var val = evt.target.value;
        this.props.el[attr] = val;
        this.props.update();
    }

    render() {
        var price = this.props.el.count * this.props.el.price;

        return(
            <div className="invoiceProductWrapper">
                <div className="invoiceProduct">
                    <input className="invoiceProductInfo fullWrapper" readOnly={this.props.readOnly} spellCheck={false} data={"count"} onChange={this.onChange} value={this.props.el.count}></input>
                    <div className="invoiceProductInfoWrapper">
                        <div className="invoiceProductInfoPrefix">$</div>
                        <input className="invoiceProductInfo fullWrapper" readOnly={this.props.readOnly} spellCheck={false} data={"price"} onChange={this.onChange} value={this.props.el.price}></input>
                    </div>

                    <input className="invoiceProductInfo fullWrapper" readOnly={this.props.readOnly} spellCheck={false} data={"name"} onChange={this.onChange} value={this.props.el.name}></input>
                    <div className="invoiceProductInfo fullWrapper">{"$" + (isNaN(price) ? "0.00" : price.toFixed(2))}</div>
                </div>
                {this.props.readOnly ? null : <img className="invoiceProductRemove" src={url + "cefFile/invoice/removeIcon.svg"} onClick={this.onRemove} />}
            </div>);
    }
}

class InvoiceDate extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            dateValue: props.data.date,
        }

        this.onChangeDate = this.onChangeDate.bind(this);
    }
    
    onChangeDate(evt) {
        this.setState({
            dateValue: evt.target.value, 
        });

        this.props.changeInfo("date", evt.target.value);
    }

    // getDateString(date) {
    //     return `${date.getDay().toString().padStart(2, '0')}.${(date.getMonth()+1).toString().padStart(2, '0')}.${date.getFullYear()}`;
    // }

    render() {
        return(
            <div className="invoiceDate standardWrapper">
                <div className="invoiceDatePart">
                    <div className="invoiceDateInfo fullWrapper">Liefer/Leistungsdatum:</div>
                    <div className="standardWrapper">
                        <textarea className="invoiceDateInput" spellCheck="false" rows="1" placeholder={"Datum"} readOnly={this.props.readOnly} onChange={this.onChangeDate} value={this.state.dateValue} />
                    </div>
                </div>
                <div className="invoiceDatePart">
                    <div className="invoiceDateInfo fullWrapper">Rechnungsdatum:</div>
                    <div className="invoiceDateCreateDate">
                        <div>{this.props.data.signDate}</div>   
                    </div>
                </div>
            </div>
        );
    }
}

class InvoiceProduct {
    constructor(id, count, price, name) {
        this.id = id;
        this.count = count;
        this.price = price;
        this.name = name;
    }
}