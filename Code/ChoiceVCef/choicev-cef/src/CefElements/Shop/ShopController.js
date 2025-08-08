import React from 'react';
import { register } from '../../App';
import './style/Shop.css';
import { url } from '../../index';

const MenuType = {
    BUY: 0,
    SHOPPING_CART: 1
};

const Filters = {
    ALL: "ALL",
    FOOD: "Nahrungsmittel",
    TOOLS: "Werkzeug",
    MEDIC: "Medizinisch",
    CAR: "Fahrzeuge",
    CAR_REPAIR: "Fahrzeugreperatur",
    CAR_TUNING: "Fahrzeugtuning",
    WEAPONS: "Waffen",
    MISC: "Sonstiges",
};

function calculateQuantityDiscount(amount, price) {
    return "Nicht verfügbar";
    return Math.min((amount * price).map(0, 500000, 0, 0.2), 0.2).toFixed(4);
}

// function calculateQuantityDiscount(amount, price, constant) {
//     return Math.min((amount * price / constant).map(0, 500000, 0, 0.2), 0.2).toFixed(4);
// }

export default class ShopController extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            data: null,
            menuType: MenuType.BUY,
            currentFilter: Filters.ALL,
            companyId: -1,
            quantDiscConst: -1,
            items: [],
            optionSets: [],
            shoppingCart: [],
            shownOptionSet: null,
        }
    
        this.openShop = this.openShop.bind(this);
        this.props.input.registerEvent("OPEN_SHOP", this.openShop);

        this.onClick = this.onClick.bind(this);
        this.onFilterChange = this.onFilterChange.bind(this);

        this.addItemToShoppingCart = this.addItemToShoppingCart.bind(this);
        this.removeItemFromShoppingCart = this.removeItemFromShoppingCart.bind(this);
        this.onSelectOption = this.onSelectOption.bind(this);
        this.onClosePopUp = this.onClosePopUp.bind(this);
        this.buyCart = this.buyCart.bind(this);
        
        this.onClose = this.onClose.bind(this);
        this.props.input.registerEvent("CLOSE_CEF", this.onClose);
    }

    onClose() {
        this.setState({
            data: null,
        });
    }

    openShop(data) {
        var list = [];
        data.items.forEach((el) => {
           list.push(JSON.parse(el)); 
        });

        list = list.sort((a, b) => {
            if(a.category > b.category) { return 1;}
            if(a.category < b.category) { return -1;}
            if(a.name < b.name) { return -1; }
            if(a.name > b.name) { return 1; }
        })

        var optionSets = [];
        data.optionSets.forEach((el) => {
            optionSets.push(JSON.parse(el)); 
        });

        this.setState({
            data: data,
            items: list,
            companyId: data.companyId,
            quantDiscConst: data.quantDiscConst,
            optionSets: optionSets
        }, () => {
            this.refs.title.innerHTML = "<div>" + data.title + "</div>";
        });
    }

    onClick() {
        var nextMenu = MenuType.SHOPPING_CART;
        if(this.state.menuType == MenuType.SHOPPING_CART) {
            nextMenu = MenuType.BUY;
        }

        this.setState({
            menuType: nextMenu
        });
    }

    onFilterChange(newFilter) {
        this.setState({
            currentFilter: newFilter,
        });
    }

    addItemToShoppingCart(item, amount, firstCall = true, option = null) {
        if(amount == NaN || amount == null) {
            amount = 1;
        }

        if(firstCall && item.optionsSet != -1) {
            this.setState({
               shownOptionSet: this.state.optionSets.filter((el) => el.id == item.optionsSet)[0],
               optionItem: item,
               optionAmount: amount,
            });
        } else {
            this.setState({
                shownOptionSet: null,
            });

            amount = new Number(amount);

            var shoppingCart = this.state.shoppingCart;

            var already = shoppingCart.filter((el) => {
                return el.item.configId == item.configId && el.option == option;
            })[0];

            if(already == undefined) {
                shoppingCart.push({item: item, amount: amount, option: option, optionsSet: item.optionsSet});
            } else {
                already.amount += amount;
            }
        }
    }

    onSelectOption(evt) {
        this.addItemToShoppingCart(this.state.optionItem, this.state.optionAmount, false, evt.target.name)
    }

    onClosePopUp(evt) {
        this.setState({
            shownOptionSet: null
        })
    }

    removeItemFromShoppingCart(item, amount) {
        amount = new Number(amount);

        var shoppingCart = this.state.shoppingCart;

        var already = shoppingCart.filter((el) => {
            return el.item.configId == item.configId && el.item.name == item.name;
        })[0];

        if(already != undefined) {
            if(amount > already.amount) {
                already.amount = 0;
            } else {
                already.amount -= amount;
            }

            if(already.amount == 0) {
                shoppingCart = shoppingCart.filter((el) => {
                    return el.item.configId != item.configId && el.item.name != item.name;
                });

                this.setState({
                    shoppingCart: shoppingCart,
                });
            } else {
                this.forceUpdate();
            }
        }
    }

    buyCart() {
        if(this.state.shoppingCart.length > 0) {
            this.onClose();

            var price = 0;
            var list = [];
            this.state.shoppingCart.forEach((el) => {
                console.log("BuyCart: " + el.item.optionsSet);
                price += el.item.price * el.amount;
                list.push(JSON.stringify({
                    type: el.item.type,
                    category: el.item.category,
                    name: el.item.name,
                    configId: el.item.configId,
                    additionalInfo: el.item.additionalInfo,
                    optionSet: el.item.optionsSet,
                    amount: el.amount,
                    option: el.option,
                }));
            });
            
            this.props.output.sendToServer("ORDER_SHOP_BUY", {companyId: this.state.companyId, items: list, price: price}, true, "ORDER_SHOP");

            this.setState({
                data: null,
                menuType: MenuType.BUY,
                currentFilter: Filters.ALL,
                quantDiscConst: -1,
                items: [],
                optionSets: [],
                shoppingCart: [],
                shownOptionSet: null,
            });
        }
    }

    getItemListMenuType() {
        if(this.state.menuType == MenuType.BUY) {
            return (<BuyItemsList items={this.state.items} preFilter={this.state.currentFilter} onClickButton={this.addItemToShoppingCart} />);
        } else {
            return (<ShoppingCartItemList quantDiscConst={this.state.quantDiscConst} items={this.state.shoppingCart} preFilter={this.state.currentFilter} onClickButton={this.removeItemFromShoppingCart} onBuyCart={this.buyCart} />);
        }
    }

    getTitleButtonIconSrc() {
        if(this.state.menuType == MenuType.BUY) {
            return url + "/shop/shopping-cart.svg"; 
        } else {
            return  url + "/shop/back-arrow.svg"; 
        }
    }

    getPopUpWindow() {
        if(this.state.shownOptionSet != null) {
            return (
                <div className="shopPopupWrapper">
                    <div className="shopPopUpHeader standardLeftWrapper">
                        Wähle die {this.state.shownOptionSet.description} aus:
                        <button className="shopPupUpCloseButton" onClick={this.onClosePopUp}><div>✕</div></button>
                    </div>
                    <div className="standardWrapper">
                        <div className="shopPopUpMain">
                            {this.state.shownOptionSet.options.map((el) => {
                                return (
                                    <div className="shopPopUpOption">
                                        <div className="standardLeftWrapper">
                                            <div className="shopPopUpOptionText">{el.name}</div>
                                        </div>
                                        <div className="standardLeftWrapper">
                                            <div className="shopPopUpOptionText">Zuschlag: +${el.extraPrice}</div>
                                        </div>
                                        <div className="standardWrapper">
                                            <button className="shopItemListTableElementButton" onClick={this.onSelectOption} name={el.name}>🗸</button>  
                                        </div>
                                    </div>);
                            })}
                        </div>
                    </div>
                </div>
            );
        } else {
            return null;
        }
    }

    render() {
        if(this.state.data != null) {
            return (
                <div className="standardWrapper">  
                {this.getPopUpWindow()}
                    <div id="shopBackground">
                        <div id="shopTitle" className="noSelect" style={{backgroundColor: this.state.data.bannerColor}}>
                            <div id="shopTitleText" ref="title"></div>
                            <div className="shopTitleButtonWrapper">
                                <div id="shopTitleButton" onClick={this.onClick}>
                                    <div className="standardWrapper">
                                        <img className="shopTitleButtonIcon" src={this.getTitleButtonIconSrc()}></img>
                                    </div>
                                    <div className="standardWrapper">
                                        <div className="shopTitleButtonText">{this.state.menuType == MenuType.BUY ? "Warenkorb" : "Einkaufen"}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <SideBar items={this.state.items} selected={this.state.currentFilter} changeFilter={this.onFilterChange} />
                        {this.getItemListMenuType()}
                    </div>
                </div>
            );
        } else {
            return null;
        }
    }
}

class SideBar extends React.Component {
    constructor(props) {
        super(props);
        

        var categories = ["ALL", ];
        this.props.items.forEach((el) => {
            if(!categories.includes(el.category)) {
                categories.push(el.category);
            }
        })

        this.state = {
            categories: categories,
        };
    }

    onClick(filter) {
        this.props.changeFilter(filter);
    }

    render() {
        return (
            <div id="shopSideBar">
                {this.state.categories.map((el) => {
                    return (
                    <div className={"shopSideBarElementWrapper noSelect " + (this.props.selected == el ? " shopSideBarElementWrapperSelected" : "")} onClick={() => this.onClick(el)} >
                        <img className={"shopSideBarElement"} src={url + "/shop/categories/" + el + ".png"}></img>
                    </div>);
                })}
            </div>);
    }
}

class BuyItemsList extends React.Component {
    constructor(props) {
        super(props);
        

        this.state = {
            filterValue: "",
        }

        this.onFilterChange = this.onFilterChange.bind(this);
    }

    onFilterChange(evt) {
        this.setState({
            filterValue: evt.target.value,
        });
    }

    runItemsThroughFilter(items) {
        var searchValue = this.state.filterValue.toLowerCase();
        return items.filter((el) => {
            if (this.props.preFilter != Filters.ALL && this.props.preFilter != el.category) {
                return false;
            }
            
            return el.name.toLowerCase().includes(searchValue) || el.weight.toLowerCase().includes(searchValue) || el.price.toString().toLowerCase().includes(searchValue) || el.category.toLowerCase().includes(searchValue);
        });
    }

    render() {
        var items = this.runItemsThroughFilter(this.props.items);

        return (
            <div className="standardWrapper">
                <div className="shopItemListWrapper">
                    <div className="shopItemListTopBar">
                        <div className="shopItemListTopBarText standardWrapper">Suchen: </div>
                        <input type="text" className="shopItemListTopBarInput" spellCheck="false" value={this.state.filterValue} onChange={this.onFilterChange} />
                    </div>
                    <div className="shopItemListTable">
                        <tr className="shopItemListTableElement shopItemListTableHeadWrapper">
                            <th className="shopItemListTableHead">Name</th>
                            <th className="shopItemListTableHead">Gewicht</th>
                            <th className="shopItemListTableHead">Preis/Stück</th>
                            <th className="shopItemListTableHead">Kategorie</th>
                            <th className="shopItemListTableHead">Max. Anzahl</th>
                            <th className="shopItemListTableHead">Menge</th>
                            <th className="shopItemListTableHead"></th>
                        </tr>
                        {items.length > 0 ? items.map((el) => {                   
                            return <ItemRowElement el={el} onClickButton={this.props.onClickButton} shoppingCart={false} />
                        }) : <ItemRowElement el={null} />}
                    </div>
                </div>
            </div>);
    }
}

class ShoppingCartItemList extends React.Component {
    constructor(props) {
        super(props);
        

        this.state = {
            filterValue: "",
        }

        this.onFilterChange = this.onFilterChange.bind(this);
    }

    onFilterChange(evt) {
        this.setState({
            filterValue: evt.target.value,
        });
    }

    runItemsThroughFilter(items) {
        var searchValue = this.state.filterValue.toLowerCase();

        return items.filter((el) => {
            if(this.props.preFilter != Filters.ALL && this.props.preFilter != el.item.category) {
                return false;
            }

            return el.item.name.toLowerCase().includes(searchValue) || el.item.weight.toLowerCase().includes(searchValue) || el.item.price.toString().toLowerCase().includes(searchValue) || el.item.category.toLowerCase().includes(searchValue) || el.item.delivery.toLowerCase().includes(searchValue);
        });
    }

    render() {
        var items = this.runItemsThroughFilter(this.props.items);
        var cost = 0;
        this.props.items.forEach((el) => {
            cost += el.item.price * el.amount;
        });
        cost = cost.toFixed(2);
        return (
            <div className="standardWrapper">
                <div className="shopItemListWrapper">
                    <div className="shopItemListTopBar">
                        <div className="shopItemListTopBarText standardWrapper">Suchen: </div>
                        <input type="text" className="shopItemListTopBarInput" spellCheck="false" value={this.state.filterValue} onChange={this.onFilterChange} />
                        <div className="shopItemListTopBarPrice">{"Gesamtpreis: $" + cost}</div>
                        <div id="shopTitleButton" className="shopItemEndBuy noSelect" onClick={this.props.onBuyCart}>
                            <div className="standardWrapper">
                                <img className="shopTitleButtonIcon shopItemEndBuyIcon" src={url + "/shop/checkOut.svg"}></img>
                            </div>
                            <div className="standardWrapper">
                                <div className="shopTitleButtonText">Abschließen</div>
                            </div>
                        </div>
                    </div>
                    <div className="shopItemListTable">
                        <tr className="shopItemListTableElement shopItemListTableHeadWrapper">
                            <th className="shopItemListTableHead">Name</th>
                            <th className="shopItemListTableHead">Gewicht</th>
                            <th className="shopItemListTableHead">Preis/Stück</th>
                            <th className="shopItemListTableHead">Kategorie</th>
                            <th className="shopItemListTableHead">Anzahl</th>
                            <th className="shopItemListTableHead">Mengenrabatt</th>
                            <th className="shopItemListTableHead"></th>
                        </tr>
                        {items.length > 0 ? items.map((el) => {
                            return <ItemRowElement el={el.item} amount={el.amount} option={el.option} quantDiscConst={this.props.quantDiscConst} onClickButton={this.props.onClickButton} shoppingCart={true} />
                        }) : <ItemRowElement el={null} />}
                    </div>
                </div>
            </div>);
    }
}

class ItemRowElement extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            amount: this.props.amount,
            showOptions: [],
        }

        this.onAmountChange = this.onAmountChange.bind(this);
        this.onClick = this.onClick.bind(this);
    }

    onAmountChange(evt) {
        this.setState({
            amount: evt.target.value,
        });
    }

    onClick() {
        this.props.onClickButton(this.props.el, this.state.amount);
    }

    render() {
        if(this.props.el != null) {
            var name = this.props.el.name;
            if(this.props.option != null) {
                name += " (" + this.props.option + ")";
            }
            return (
                <tr className="shopItemListTableElement">
                    <td className="shopItemListTableElementRow" width="26%">{name}</td>
                    <td className="shopItemListTableElementRow" width="8%">{this.props.el.weight + (isNaN(this.props.el.weight) ? "" : "kg")}</td>
                    <td className="shopItemListTableElementRow" width="8%">{"$" + this.props.el.price}</td>
                    <td className="shopItemListTableElementRow" width="14%">{Filters[this.props.el.category]}</td>
                    {this.props.shoppingCart ?            
                        <td className="shopItemListTableElementRow" width="14%">{this.state.amount.toString()}</td>
                        :
                        <td className="shopItemListTableElementRow" width="14%">{this.props.el.maxAmount}</td>
                    }
                    {this.props.shoppingCart ?
                        <td className="shopItemListTableElementRow" width="12%">
                            <input type="text" className="shopItemListTableElementAmount" readOnly value={"Noch nicht verfügbar"}/>
                        </td>
                        :
                        <td className="shopItemListTableElementRow" width="12%">
                            <input type="number" className="shopItemListTableElementAmount" value={this.state.amount} onChange={this.onAmountChange} />
                        </td>
                    }
                    <td className="shopItemListTableElementRow" width="4%">
                        {this.props.shoppingCart ? 
                            <button className="shopItemListTableElementButton shopItemShoppingCart" onClick={this.onClick}>-</button> 
                            :
                            <button className="shopItemListTableElementButton" onClick={this.onClick}>+</button>  
                        }
                    </td>
                </tr>);
        } else {
            return (
                <tr className="shopItemListTableElement">
                    <td className="shopItemListTableElementRow" width="14%">Keine Waren</td>
                    <td className="shopItemListTableElementRow" width="14%"></td>
                    <td className="shopItemListTableElementRow" width="14%"></td>
                    <td className="shopItemListTableElementRow" width="14%"></td>
                    <td className="shopItemListTableElementRow" width="14%"></td>       
                    <td className="shopItemListTableElementRow" width="14%"></td>
                    <td className="shopItemListTableElementRow" width="12%">
                        <input className="shopItemListTableElementAmount shopHidden" readOnly/>
                    </td>
                    <td className="shopItemListTableElementRow" width="4%"> </td>
                </tr>);
        }
    }
}

register(ShopController);