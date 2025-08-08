import React, { Fragment } from 'react';
import { register } from './../../App';
import './Notification.css';
import { url } from './../../index';

import { ToastContainer, toast } from "react-toastify";

import "react-toastify/dist/ReactToastify.css";

export default class Notification extends React.Component {
    constructor(props) {
        super(props);

        this.replaceList = {}
        this.props.input.registerEvent("CREATE_NOTIFICATION", this.createNotification);
    }

    createNotification = (data) => {
        if(data.replaceCategory) {
            if (this.replaceList[data.replaceCategory]) {
                if(toast.isActive(this.replaceList[data.replaceCategory].id)) {
                    toast.update(this.replaceList[data.replaceCategory].id, {
                        render: data.message,
                        theme: "colored",
                        icon: () => (
                            <img style={{width: "4vh", height: "4vh"}} src={url + "/badges/" + data.imgName} />
                        ),
                        className: 'alert ' + data.type,
                        closeButton: false,
                        pauseOnFocusLoss: false,
                        pauseOnHover: false,
                        autoClose: this.getToastDuration(data.message.length),
                    });
                    return;
                }
            }
        }

        const toastId = toast.info(data.message, {
            theme: "colored",
            icon: () => (
                <img style={{width: "4vh", height: "4vh"}} src={url + "/badges/" + data.imgName} />
            ),
            className: 'alert ' + data.type,
            closeButton: false,
            pauseOnFocusLoss: false,
            pauseOnHover: false,
            autoClose: this.getToastDuration(data.message.length),
        });

        this.replaceList[data.replaceCategory] = { id: toastId };
    }

    getToastDuration(length) {
        return Math.min(20000, Math.max(1000, Math.max(4000, length * 100)));
    }

    render() {
        return (
            <ToastContainer 
                position="bottom-left"
                className="notificationToaster"
            />
        );
    }
}

register(Notification);
