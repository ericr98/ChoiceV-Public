import React from 'react';
import { register } from '../../App';

import MenuShow from './MenuContent/Menu'

export default class MenuController extends React.Component {
    constructor(props) {
        super(props)
        this.state = {
            menuData: null,
        }
        
        //this.menuShow = React.createRef();

        this.onCreateMenu = this.onCreateMenu.bind(this);
        this.onSubMenuAddData = this.onSubMenuAddData.bind(this);
        this.onCloseMenu = this.onCloseMenu.bind(this);
        this.onMenuEvent = this.onMenuEvent.bind(this);
        
        this.onUpdateMenu = this.onUpdateMenu.bind(this);

        this.props.input.registerEvent("CREATE_MENU", this.onCreateMenu);
        this.props.input.registerEvent("ADD_MENU_DATA", this.onSubMenuAddData);
        
        this.props.input.registerEvent("UPDATE_MENU", this.onUpdateMenu);

        this.props.input.registerEvent("CLOSE_CEF", this.onCloseMenu);
        this.props.input.registerEvent("CLOSE_MENU", this.onCloseMenu);
    }

    onCreateMenu(data) {
        this.onCloseMenu();
        
        this.setState({
            menuData: null,
        }, () => {
            this.setState({
                menuData: data,
            });
        });
    }

    onSubMenuAddData(data) {
        this.refs.menuShow.addSubMenuData(data);
    }
    
    onUpdateMenu(data) {
        if (this.state.menuData == null) {
            return;
        }
        
        this.refs.menuShow.updateMenuItem(data);
    }

    onCloseMenu(data) {
        if(this.state.menuData != null) {
            this.setState({
                menuData: null,
            });
        }
    }
    
    onMenuEvent(data, closeMenu) {
        data.closeMenu = closeMenu;
        this.props.output.sendToServer("MENU_EVENT", data, closeMenu, "MENU");
    }

    render() {
        if(this.state.menuData == null) {
            return null;
        } else {
            return <MenuShow ref="menuShow" data={this.state.menuData} sendEvent={this.onMenuEvent}/>;
        }
    }
}

register(MenuController);