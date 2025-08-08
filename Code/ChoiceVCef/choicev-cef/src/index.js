import React from 'react';
import { createRoot } from 'react-dom/client';
import './style/index.css';
import App from './App';
import InputController from './InputController';
import OutputController from './OutputController';
import AuthenticationController from './AuthenticationController';

import './fonts/digital-7.ttf';

//CefElements
import AudioPlayer from './CefElements/AudioPlayer/AudioPlayer';
import CefFile from './CefElements/CefFile/CefFile';
import DecryptScreen from './CefElements/DecryptScreen/DecryptScreen';
import Notification from './CefElements/Notification/Notification';
import InventoryController from './CefElements/Inventory/InventoryController';
import MenuController from './CefElements/Menu/MenuController';
import CombinationLockController from './CefElements/CombinationLock/CombinationLockController'
import MedicalAnalyse from './CefElements/MedicalAnalyse/MedicalAnalyse';
import CompanyPanelController from './CefElements/CompanyPanel/CompanyPanelController';
import SmartphoneController from './CefElements/Smartphone/SmartphoneController';
import IDCardController from './CefElements/IDCard/IDCardController';
import ColorPickerController from './CefElements/ColorPicker/ColorPickerController';
import VoiceRangeController from './CefElements/VoiceRange/VoiceRangeController';
import GasStationController from './CefElements/GasStation/GasStationController';
import HUDController from './CefElements/HUD/HUDController';
import MapController from './CefElements/Map/MapController';
import StaticPhoneController from './CefElements/StaticPhone/StaticPhoneController';
import ShopController from './CefElements/Shop/ShopController';
import TaximeterController from './CefElements/Taximeter/TaximeterController';
import MechanicGameController from './CefElements/MechanicGame/MechanicGameController';
import StrengthMiniGame from './CefElements/SmallMiniGames/StrengthMiniGame';

import FishingController from './CefElements/FishingGame/FishingGame';
import FlipBookController from './CefElements/FlipBook/FlipController';
import DebugInformationController from './CefElements/DebugInformation/DebugInformationController';
import AnimalTalk from "./CefElements/AnimalTalk/AnimalTalk";

import BreakOpenMinigame from './CefElements/Minigames/BreakOpen/BreakOpenMinigame';

var ws = null;
export var url = "http://www.choicev-cef.net/src/cef/";

var input = new InputController();
var output = new OutputController();

var auth = new AuthenticationController(input);

input.registerEvent("CLOSE_CONNECTION", () => {
    ws.close();
});

Number.prototype.mod = function(n) {
    return ((this%n)+n)%n;
};

const container = document.getElementById('root');
const root = createRoot(container);
root.render(<App input={input} output={output} auth={auth} />);

if("alt" in window) {
    window.alt.emit("HAS_LOADED");
    
    window.alt.on('INIT_WEBSOCKET', (id, url, loginToken) => {
        openWebSocketConnection(id, url, loginToken);
    });

    window.alt.on('CEF_EVENT', (data) => {
        input.onEventTrigger(data.Event, data);
    });
}

// //Catch event sent by index.html from Client!
// document.addEventListener("INIT_WEBSOCKET", (e) => {
//     openWebSocketConnection(e.detail);
// });

// document.addEventListener("CEF_EVENT", (e) => {
//     input.onEventTrigger(e.detail.Event, e.detail);
// });

function openWebSocketConnection(id, url, loginToken) {
    if(ws != null && ws.readyState != 1 && ws.readyState != 0) {
        return;
    }

    if(ws != null) {
        ws.close();
    }

    ws = new WebSocket(url);
    input.initWebSocket(ws);
    output.initWebSocket(ws, id, loginToken);

    ws.onopen = (ev) => {
        output.sendToServer("INIT_WEBSOCKET", {});
    }

    ws.onerror = (ev) => {
        console.error('Error on websocket connect');
    };
}

Number.prototype.map = function (in_min, in_max, out_min, out_max) {
    return (this - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

//setTimeout(() => {
//    input.onEventTrigger("OPEN_BREAKOPEN", {
//        backgroundOpenImg: "door_opened",
//        backgroundClosedImg: "door_closed",
//        height: "80vh",
//        width: "48.4vh",
//        end: 30,
//        
//        crowbarHeight: 17.5,
//        crowbarX: 7.5,
//        crowbarY: 50,
//        crowbarDirection: "LtR",
//        crowbarChargeInDistance: 10,
//        
//        xCrowbarBarrier: 29.5,
//        yCrowbarBarrier: null,
//        
//        locksSize: 5,
//        lockLineDist: 7,
//        locks: [
//            {"position": 0}
//        ]
//    });
//}, 100);

//setTimeout(() => {
//    input.onEventTrigger("OPEN_ANIMAL_TALK", {
//        categories: [
//            {"name": "Tiere", "words": ["Hund", "Katze", "Maus", "Elefant"]},
//            {"name": "Farben", "words": ["Rot", "Blau", "Grün", "Gelb"]},
//        ]
//    });
//}, 100);
//
//setTimeout(() => {
//    input.onEventTrigger("ANIMAL_TALK_FOCUS_TOGGLE", {
//        focus: true,
//    });
//}, 500);
//
//setTimeout(() => {
//    input.onEventTrigger("ANIMAL_TALK_FOCUS_TOGGLE", {
//        focus: false,
//    });
//}, 4000);

//setTimeout(() => {
//    input.onEventTrigger("VOICE_SOLUTION_MUTE", {
//        State: true,
//        Icon: "teamspeak",
//    });
//}, 100)
//
//setTimeout(() => {
//    input.onEventTrigger("CREATE_VOICERANGE", {
//        Range: 0,
//    });
//}, 100)

//setTimeout(() => {
//    input.onEventTrigger("UPDATE_CHANNEL_HUD_CLIENT", {
//        sendingChannels: ["27 Mhz", "29 Mhz"],
//        receivingChannels: ["23 Mhz", "45Mhz"],
//    });
//}, 100)

//setTimeout(() => {
//    input.onEventTrigger("OPEN_COMPANY_ID_CARD", {
//        data: [
//            {name: "type", data: "NORMAL"},
//            {name: "icon", data: "red"},
//            {name: "name", data: "Vincent Dante Racone"},
//            {name: "rank", data: "Chief of Police"},
//            {name: "company", data: "Los Santos Police Department"},
//            {name: "address", data: "Am Plaza 5b"},
//        ]
//    });
//}, 100)

//setTimeout(() => {
//    input.onEventTrigger("OPEN_COMPANY_ID_CARD", {
//        data: [
//            {name: "type", data: "DEPARTMENT"},
//            {name: "icon", data: "lspd"},
//            {name: "name", data: "Vincent Dante Racone"},
//            {name: "birthday", data: "17.08.1919"},
//            {name: "number", data: "PD-62"},
//            {name: "hiringDay", data: "16.07.2019"},
//            {name: "rank", data: "Chief of Police"},
//            {name: "company", data: "Los Santos Police Department"},
//            {name: "address", data: "Am Plaza 5b"},
//
//        ]
//    });
//}, 100)


//setTimeout(() => {
//    input.onEventTrigger("OPEN_COMPANY_ID_CARD", {
//        data: [
//            {name: "type", data: "BADGE"},
//            {name: "icon", data: "lspd"},
//            {name: "name", data: "Vincent Dante Racone"},
//            {name: "birthday", data: "17.08.1919"},
//            {name: "number", data: "PD-62"},
//            {name: "hiringDay", data: "16.07.2019"},
//            {name: "rank", data: "Chief of Police"},
//            {name: "company", data: "Los Santos Police Department"},
//            {name: "address", data: "Am Plaza 5b"},
//        ]
//    });
//}, 100)

//setTimeout(() => {
//    input.onEventTrigger("OPEN_PDF_URL", {
//        url: "079d904e-1ca0-4107-a3e3-550bd305df3b.pdf"
//    });
//}, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_VARIABLE_FILE", {
//         backgroundImage: "http://choicev-cef.net/src/cef/cefFile/variableFile/TEST.png",
//         width: "50vh",
//         height: "70vh",
//         debugMode: true,
//         isCopy: false,
//         data: [
//             {type: "VARIABLE", identifier: "S1", x: "10vh", y: "10%", text: "TEST TEXT", fontSize: "1vh", width: "20%", height: "3%"},

//             {type: "SAVE_BUTTON", identifier: "SAVE_BUTTON", x: "100%", y: "15%", fontSize: "1vh", width: "60%"},

//             {type: "SIGNATURE", identifier: "SIG1", x: "0%", y: "50%", fontSize: "1vh", width: "50%", signatureInfo: "Signatur", signatureText: "Max Mustermann"}
//         ]
//     });
// }, 100);


// setTimeout(() => {
//     input.onEventTrigger("SET_DEBUG_INFO", {
//         key: "TEST",
//         value: "<h1 style='background-color:Tomato;'>Tomato</h1>"
//     });
// }, 100);


// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TERMINAL_HUD", {
//         tokens: 10
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_CERTIFICATE", {
//         data: [
//             { name: "title", data: "Doktortitel in der Medizin" },
//             { name: "name", data: "Vincent Dante Racone" },
//             { name: "text", data: "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labor Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labor" },
//             { name: "signDate", data: "17.09.2022" },
//             { name: "signName", data: "Michaella Vitoria Reed" },
//         ]
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TERMINAL_HUD", {
//         tokens: 10
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("SHOW_MECHANIC_GAME", {
//         col: 5,
//         row: 5,
//         maxDepth: 2,
//         parts: [
//             //JSON.stringify({id: 0, img: "mech1x2", pos: [{x: 0, y:0}, {x:1, y:0}], iden: false, depth: 0}),
//             //JSON.stringify({id: 1, img: "mech1x1", pos: [{x:1,y:1}], iden: false, depth: 0}),

//             JSON.stringify({id:0, img: "2x2", stash: false, pos: [{x: 0,y: 0}, {x: 1,y: 0}, {x: 0,y: 1}, {x: 1, y: 1}], iden: false, depth: 0}),
//             //JSON.stringify({id: 1, img: "mech1x2", pos: [{x: 0, y: 2}, {x: 0, y: 3}], iden: false, depth: 0}),
//             JSON.stringify({id: 1, img: "tube1x2_U", stash: false, pos: [{x: 2, y: 0}, {x: 3, y: 0}], iden: true, depth: 0}),
//             JSON.stringify({id: 2, img: "tube1x2_U", stash: false, pos: [{x: 2, y: 1}, {x: 2, y: 2}], iden: true, depth: 1}),
//             //JSON.stringify({id: 2, img: "mech1x2", pos: [{x: 1, y: 3}, {x: 2, y: 3}], iden: false, depth: 0}),
//             JSON.stringify({id: 3, img: "light1x1_R", stash: true, pos: [{x: 3, y: 2}], iden: true, depth: 0}),
//             JSON.stringify({id: 3, img: "light1x1_L", stash: true, pos: [{x: 3, y: 2}], iden: true, depth: 0}),

//             //JSON.stringify({id: 3, img: "light1x1_R", stash: false, pos: [{x: 3, y: 2}, {x: 2, y: 2}, {x: 2, y: 1}], iden: false, depth: 0}),

//             // JSON.stringify({id: 2, img: "mech1x2", pos: null, iden: true, depth: 1}),
//             // JSON.stringify({id: 3, img: "mech1x1", pos: null, iden: true, depth: 1}),
//         ],
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_MECHANIC_GAME_PART", {
//         partId: 0,
//         action: "IDENTIFY",
//     });
// }, 1000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_MECHANIC_GAME_PART", {
//         partId: 1,
//         action: "IDENTIFY",
//     });
// }, 2000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_MECHANIC_GAME_PART", {
//         partId: 2,
//         action: "REMOVE",
//     });
// }, 3000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_MECHANIC_GAME_PART", {
//         partId: 3,
//         action: "MOVE_BACK_IN",
//     });
// }, 2000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_MECHANIC_GAME_PART", {
//         partId: 1,
//         action: "MOVE_TO_STASH",
//     });
// }, 5000);



// setTimeout(() => {
//     input.onEventTrigger("OPEN_PRESCRIPTION", {
        
//     });
// }, 100);


// setTimeout(() => {
//     input.onEventTrigger("OPEN_VEHICLE_REGISTRATION_CARD", {
//         startDate: "1 JAN 2000",
//         expDate: "1 JAN 2020",
//         licNumber: "45324",
//         owner: "VINCENT DANTE RACONE",
//         chassisNumber: "859834754",
//         numberPlate: "GH7TZ3",
//         vehicleName: "Coquette",
//         vehicleProducer: "Invetero",
//         dmvSignature: "Keith Meyer",
//         ownerSignature: "Vincent Dante Racone"
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_COLOR", {
//         ColorTyp: 7,
//         Color: "#1c1f21",
//         ColorArr: [ "#1c1f21", "#272a2c", "#312e2c", "#35261c", "#4b321f", "#5c3b24", "#6d4c35", "#6b503b", "#765c45", "#7f684e", "#99815d", "#a79369", "#af9c70", "#bba063", "#d6b97b", "#dac38e", 
//         "#9f7f59", "#845039", "#682b1f", "#61120c", "#640f0a", "#7c140f", "#a02e19", "#b64b28", "#a2502f", "#aa4e2b", "#626262", "#808080", "#aaaaaa", "#c5c5c5", "#463955", "#5a3f6b", 
//         "#763c76", "#ed74e3", "#eb4b93", "#f299bc", "#04959e", "#025f86", "#023974", "#3fa16a", "#217c61", "#185c55", "#b6c034", "#70a90b", "#439d13", "#dcb857", "#e5b103", "#e69102", 
//         "#f28831", "#fb8057", "#e28b58", "#d1593c", "#ce3120", "#ad0903", "#880302", "#1f1814", "#291f19", "#2e221b", "#37291e", "#2e2218", "#231b15", "#020202", "#706c66", "#9d7a50", ],
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_SHOP", {
//         title: "<strong>I</strong>nternational<b> D</b>elivery and <b>O</b>rder <b>S</b>ystem",
//         bannerColor: "#307CB8",
//         quantDiscConst: 2,
//         optionSets: [
//             JSON.stringify({
//                 id: 1,
//                 description: "Fahrzeugfarbe",
//                 options: [{name: "Sportwagen", extraPrice: 50}, {name: "Einsatzwagen", extraPrice: 75}]
//             }),
//         ],

//         items: [
//             JSON.stringify({type: 0, configId: 0, name: "Kasten Pißwasser", weight: "6.5", price: "14", maxAmount: "∞", category: "FOOD", optionsSet: -1}),
//             JSON.stringify({type: 0, configId: 1, name: "Kasten Erdinger", weight: "6.5", price: "22", maxAmount: "∞", category: "FOOD", optionsSet: -1}),
//             JSON.stringify({type: 0, configId: 7, name: "Flasche Wasser", weight: "0.2", price: "2", maxAmount: "∞", category: "FOOD", optionsSet: -1}),

//             JSON.stringify({type: 0, configId: 2, name: "Schraubenschlüssel", weight: "1.5", price: "16", maxAmount: "∞", category: "TOOLS", optionsSet: -1}),
//             JSON.stringify({type: 0, configId: 3, name: "Gießkanne", weight: "1.0", price: "25", maxAmount: "∞", category: "TOOLS", optionsSet: -1}),

//             JSON.stringify({type: 0, configId: 4, name: "Fahrzeug-Hülle", weight: "2.5", price: "100", maxAmount: "∞", category: "CAR_REPAIR", optionsSet: 1}),
//             JSON.stringify({type: 0, configId: 5, name: "Sportwagen-Reifen", weight: "1.5", price: "16", maxAmount: "∞", category: "CAR_REPAIR", optionsSet: -1}),
//             JSON.stringify({type: 0, configId: 6, name: "Notfallwaffen-Hülle", weight: "2.5", price: "25", maxAmount: "∞", category: "CAR_REPAIR", optionsSet: -1}),
//         ]
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_STATIC_PHONE", {
//        number: 555555123456,
//        contacts: [
//            JSON.stringify({name: "Erk Racone", number: 555555123456}),
//            JSON.stringify({name: "Jesko Downing", number: 555555123457}),
//            JSON.stringify({name: "Erk Racone", number: 555555123456}),
//            JSON.stringify({name: "Jesko Downing", number: 555555123457}),
//            JSON.stringify({name: "Erk Racone", number: 555555123456}),
//            JSON.stringify({name: "Jesko Downing", number: 555555123457}),
//            JSON.stringify({name: "Erk Racone", number: 555555123456}),
//            JSON.stringify({name: "Jesko Downing", number: 555555123457}),
//        ]
//     });
// }, 100);

// // setTimeout(() => {
//     input.onEventTrigger("TOGGLE_MAP", {
//         x: -515,
//         y: 4426,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("START_TAXOMETER", {
//         price: 100,
//         rate: 10000,
//         distance: 0,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TAXOMETER", {
//         distance: 1,
//     });
// }, 1000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TAXOMETER", {
//         distance: 2.5,
//     });
// }, 2000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TAXOMETER", {
//         distance: 4.12,
//     });
// }, 3000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_TAXOMETER", {
//         distance: 6.78,
//     });
// }, 4000);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_FOOD_HUD", {
//         hunger: 10,
//         thirst: 10,
//         energy: 10,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("UPDATE_CAR_HUD", {
//         milage: 0.5,
//         fuelMax: 100,
//         fuel: 50,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_SOCIAL_SECURITY_CARD", {
//         number: 83242123456,
//         name: "Erk Racone",
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_US_CUSTOMS_FILE", {
//         data: [
//             { name: "admission", data: "1678096-78"},
//             { name: "flightNumber", data: "A76 - JZH"},
//             { name: "enterDate", data: "16.12.2020"},
//             { name: "fullName", data: "Erk Racone"},
//             { name: "birthday", data: "17.08.1919"},
//             { name: "citizenship", data: "U.S Bürger"},
//             { name: "number", data: "555 555 176524"},
//             { name: "goverment", data: ""},
//             { name: "drugs", data: false},
//             { name: "crime", data: true},
//             { name: "terror", data: true},
//             { name: "work", data: false},
//             { name: "child", data: true},
//             { name: "visa", data: false},
//             { name: "signature", data: ""},    
//         ],

//         isCopy: false,
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_PERICO_CUSTOMS_FILE", {
//         data: [
//             { name: "visaNumber", data: "1678096-78"},
//             { name: "flightNumber", data: "A76 - JZH"},
//             { name: "enterDate", data: "16.12.2020"},
//             { name: "fullName", data: "Erk Racone"},
//             { name: "birthday", data: "17.08.1919"},
//             { name: "citizenship", data: "U.S Bürger"},
//             { name: "number", data: "555 555 176524"},
//             { name: "visaAmount", data: "$100"},

//             { name: "time", data: "10 Jahre"},
//             { name: "visit", data: "Urlaub"},

//             { name: "work", data: false},
//             { name: "workMaybe", data: ""},

//             { name: "food", data: true},
//             { name: "foodMaybe", data: ""},

//             { name: "money", data: true},
//             { name: "moneyMaybe", data: ""},

//             { name: "signature", data: ""},    
//         ],

//         isCopy: false,
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_INVOICE_FILE", {
//         companyName: "ACLS",

//         charName: "Erk Racone",
//         street: "Am Plaza 3",
//         city: "656754 Los Santos",

//         date: "09.06.2020",
//         signDate: "",
//         tax: 0.00,

//         products: [
//             JSON.stringify({count: 1, price: 75.50, name: "Motorwartung"}),
//             JSON.stringify({count: 3, price: 25, name: "Scheibenwechsel"}),
//         ],

//         paymentInfo: "",
//         additionalInfo: "",
//         sellerSignature: "",
//         buyerSignature: "",

//         isCopy: false,
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_GASSTATION", {
//         StationType: 1,

//         Fuel: 0,
//         FuelMax: 600,
//         FuelName: "Kerosin",
//         FuelPrice: 0.45,
    
//         ShowCash: true,
//         ShowBank: true,
//         ShowComp: true,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_VOICERANGE", {
//         Range: 0,
//     });
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_DRIVERS_LICENSE", {
//         dlNumber: "DL12345678",
//         expDate: "16.07.2019",
//         vehicleClass: "PKW",
//         lastName: "Shepherd",
//         firstName: "Maximilian Lincoln",
//         dateOfBirth: "16.07.2019",  
//         gender: "M",
//         hairColor: "Schwarz",
//         eyeColor: "Blau",
//         issueDate: "16.07.2019",
//         issuer: "Mira Hope",
//         signature: "Maximilian L. Shepherd",
//     });
// }, 100)

// setTimeout(() => {
//     input.onEventTrigger("EQUIP_PHONE", {
//         version: 1,
//         number: 111,
//         background: 2,
//         ringtone: 1,
//         contacts: [
//             JSON.stringify({id: 0, favorit: true, number: 123, name: "Samuel Perez", note: "West Side Hustlers", email: "erk@choicev.net"}),
//             JSON.stringify({id: 1, favorit: false, number: 456, name: "Vincent Dante Racone", note: "Merryweather", email: "erk@choicev.net"}),
//             JSON.stringify({id: 2, favorit: true, number: 789, name: "Samuel Keaton", note: "FBI", email: "erk@choicev.net"}),
//         ],
//         settings: JSON.stringify({volume: 3, hiddenNumber: true, flyMode: false, silent: true}),
//     })
// }, 100);


// setTimeout(() => {
//     input.onEventTrigger("OPEN_PHONE", {

//     })
// }, 300);


// setTimeout(() => {
//     input.onEventTrigger("OPEN_SEARCH_WARRANT_FILE", {
//         logo: "",
//         property: "",
//         suspicion: "",
//         place: "",
//         signature: "",
//         signDate: "",
//         isCopy: false,
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_ARREST_WARRANT_FILE", {
//         logo: "DistrictCourt",
//         name: "Erk Racone",
//         offenses: "Mord, Folter",
//         bail: "gestattet",
//         bailCost: "$ 6000",
//         bailLength: "72 Hafteinheiten",
//         aka: "Velvet Thunder",
//         address: "Am Plaza 5b",
//         hairColor: "Green",
//         vehicles: "LS5457, Schwarzer Feltzer",
//         info: "Trägt meistens eine grüne Sonnebrille",
//         signature: "",
//         signDate: "",
//         isCopy: false,
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("OPEN_PRISON_FILE", {
//         logo: "City",
//         name: "",
//         birthday: "",
//         gender: "",
//         offense: "",
//         sentenceLength: "",
//         sentenceDate: "",
//         sentenceMin: "",
//         sentenceMax: "",
//         offenseInfo: "",
//         sentenceInfo: "",
//         judgeSignature: "",
//         prisonSignature: "",
//     })
// }, 100);


// setTimeout(() => {
//     input.onEventTrigger("OPEN_COMPANY_PANEL", {
//         companyName: "LSPD",
//         rankPanelAccess: true,
//         employees: [
//             JSON.stringify({id: 0, onDuty: true, firstName: "Blfredo", lastName: "Schmidtmüller", rank: "Chief of Police", bank: 12345678, salary: 2000.12, todayDuty: "3h"}),
//             JSON.stringify({id: 1, onDuty: true, firstName: "Ark", lastName: "Racone", rank: "Officer", bank: 12345678, salary: 200, todayDuty: "3h"}),
//         ],

//         ranks: [
//             JSON.stringify({name: "Chief of Police", salary: 123.45, permissions: ["HIRE_EMPLOYEE", "FIRE_EMPLOYEE"]}),
//             JSON.stringify({name: "Operations Commander", salary: 123.45, permissions: ["FIRE_EMPLOYEE"]}),
//             JSON.stringify({name: "Detective Commander", salary: 123.45, permissions: ["HIRE_EMPLOYEE"]}),
//             JSON.stringify({name: "Officer", salary: 123.45, permissions: []}),
//         ],

//         permissions: [
//             JSON.stringify({id: "HIRE_EMPLOYEE", name: "Personen einstellen"}),
//             JSON.stringify({id: "FIRE_EMPLOYEE", name: "Personen feuern"}),
//             JSON.stringify({id: "ISSUE_RECEIPT", name: "Rechnung ausstellen"}),
//             JSON.stringify({id: "USE_COMPANY_BANK", name: "Firmenkontozugriff"}),
//             JSON.stringify({id: "CHANGE_RANKS", name: "Ränge bearbeiten"}),
//             JSON.stringify({id: "GIVE_EMPLOYEE_RANKS", name: "Ränge zuweisen"}),
//             JSON.stringify({id: "USE_GARADE", name: "Garagenzugriff"}),
//         ],

//         taxes: [
//             JSON.stringify({id: 9, tax: 0.05, amount: 45, message: "Rechnung Id: 13", date: "29.09.2020", automatic: true}),
//             JSON.stringify({id: 10, tax: 0.05, amount: 45, message: "Rechnung Id: 14", date: "29.09.2020", automatic: false}),
//         ],
//     })
// }, 0);

// setTimeout(() => {
//     input.onEventTrigger("MEDICAL_ANALYSE", {
//         injuries: [
//             JSON.stringify({id: 0, severness: 0, seed: 368, bodyPart: "torso"}),
//             JSON.stringify({id: 0, severness: 6, seed: 438, bodyPart: "head"}),
//             JSON.stringify({id: 0, severness: 5, seed: 948468, bodyPart: "leftArm"}),
//             JSON.stringify({id: 0, severness: 6, seed: 6468, bodyPart: "rightLeg"}),
//             JSON.stringify({id: 0, severness: 4, seed: 9468, bodyPart: "torso"}),
//             JSON.stringify({id: 0, severness: 2, seed: 838, bodyPart: "torso"}),
//             JSON.stringify({id: 0, severness: 3, seed: 14568, bodyPart: "rightLeg"}),
//             JSON.stringify({id: 0, severness: 1, seed: 446468, bodyPart: "leftLeg"}),
//             JSON.stringify({id: 0, severness: 0, seed: 54855468, bodyPart: "rightArm"}),
//             //JSON.stringify({severness: 3, seed: 8876, bodyPart: "head"}),
//         ]
//     })
// }, 10);

// setTimeout(() => {
//     input.onEventTrigger("TEST", {
        
//     })
// }, 100);

// setTimeout(() => {
//    input.onEventTrigger("CREATE_MENU", {
//        name: "TestMenu",
//        subtitle: "Was möchtest du tun?", 
//        elements: [
//            JSON.stringify({id: 0, type: "static", name: "Test1", right:"Spoiler1254534534 53452423423", className: "green", description:"Eine Beschreibung die ziemlich lang ist und nicht aufhört"}),
//            JSON.stringify({id: 2, type: "click", name: "Spoiler1254534534 53452423423", right:"Info12345 34343", className: "yellow", description: "Beschreibung", evt: "MENU_CLICK"}),
//            JSON.stringify({id: 3, type: "file", name: "Test4", description: "Checken", className: "red", evt: "INPUT_CHECKED"}),
//            JSON.stringify({id: 1, type: "input", name: "Spoiler1254534534", right:"Info", className: "normal", description: "Eine Beschreibung die ziemlich lang ist und nicht aufhört", input: "Placeholder", event: "MENU_INPUT_CLICK", options: ["Option 1", "Option 2"]}),
//            JSON.stringify({id: 2, type: "click", name: "Test3", right:"Info", className: "yellow", description: "Beschreibung", evt: "MENU_CLICK"}),
//            JSON.stringify({id: 3, type: "check", name: "Test4", description: "Checken", className: "red", check: true, evt: "INPUT_CHECKED"}),
//            JSON.stringify({id: 4, type: "menu", name: "Test5", description:"Die zweite Beschreibung", className: "normal",
//                menuData: {
//                    elements: [
//                        JSON.stringify({id: 5, type: "static", name: "UnterMenu1", right:"Info", className: "normal", description:"Eine Beschreibung die ziemlich lang ist und nicht aufhört"}),
//                        JSON.stringify({id: 6, type: "menu", name: "TestElement3", description:"Die zweite Beschreibung", className: "normal",
//                            menuData: {
//                                elements: [
//                                    JSON.stringify({id: 7, type: "static", name: "UnterMenu1", right:"Info1234324fdsffs32423423878764", className: "normal", description:"Eine Beschreibung die ziemlich lang ist und nicht aufhört"}),
//                                    JSON.stringify({id: 8, type: "click", name: "UnterMenu2", right:"Info", description:"", className: "normal", evt: "SUB_MENU_CLICK"}),
//                                    JSON.stringify({id: 9, type: "list", name: "Test6", description:"Liste zum auswählen", className: "normal", evt: "LIST:_ENTER", elements: ["Info1234324fdsffs324234234", "2", "Item", "Sieben"]}),
//                                ]
//                             }
//                        }),
//                    ]
//                }
//            }),
//            JSON.stringify({id: 9, type: "list", name: "Spoiler 122423423", description:"Liste zum auswählen", className: "normal", evt: "LIST:_ENTER", elements: ["Blista-Flügel (Primärfarbe)", "2", "Item", "Sieben"]}),
//            JSON.stringify({id: 2, type: "click", name: "Test3", right:"Info", className: "yellow", description: "Beschreibung", evt: "MENU_CLICK"}),
//            JSON.stringify({id: 10, type: "stats", name: "Test7", description:"Stats kriegen", className: "normal", evt: "LIST:_ENTER"}),
//            JSON.stringify({id: 10, type: "stats", name: "Test7", description:"Stats kriegen", className: "normal", evt: "LIST:_ENTER"}),
//            JSON.stringify({id: 10, type: "stats", name: "Test7", description:"Stats kriegen", className: "normal", evt: "LIST:_ENTER"}),
//        ]
//    })
// , 100);

// setTimeout(() => {
//    input.onEventTrigger("COMBINATION_LOCK_OPEN", {
//         combination: "01234",
//         id: 154,
//     })
// }, 100);


//  setTimeout(() => {
//      input.onEventTrigger("LOAD_DOUBLE_INVENTORY", {
//          idLeft: 1,
//          maxWeightLeft: 14,
//          //itemsRight:["{\"configId\":47,\"name\":\"Waffenlauf\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.5,\"amount\":1,\"description\":\"Komponente für ein/eine: Rifle\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":44,\"name\":\"Waffenkörper\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":1.7000000476837158,\"amount\":1,\"description\":\"Komponente für ein/eine: Bullpup Gewehr\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":true}"],
//          itemsRight:["{\"configId\":47,\"name\":\"Waffenlauf\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.5,\"amount\":10,\"description\":\"Komponente für ein/eine: Rifle\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}"],

//          idRight: 2,
//          maxWeightRight: 4,
//          rightName: "Archiv-Regal-1",
//          showRightSearchbar: true,
//          itemsLeft: ["{\"configId\":1,\"name\":\"Fahrzeugschlüssel\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.15000000596046448,\"amount\":1,\"description\":\"Gehört zu einem Sanchez mit Kennzeichen: 23876C1F\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}"],
//          //itemsLeft: ["{\"configId\":1,\"name\":\"Fahrzeugschlüssel\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.15000000596046448,\"amount\":1,\"description\":\"Gehört zu einem Sanchez mit Kennzeichen: 23876C1F\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":36,\"name\":\"Notiz\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.009999999776482582,\"amount\":1,\"description\":\"Titel: Title\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":true}","{\"configId\":36,\"name\":\"Notiz\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.009999999776482582,\"amount\":1,\"description\":\"Titel: Titel4343\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":true}","{\"configId\":49,\"name\":\"Waffenmagazinehalter\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.5,\"amount\":1,\"description\":\"Komponente für ein/eine: Rifle\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":50,\"name\":\"Waffenreceiver\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.699999988079071,\"amount\":1,\"description\":\"Komponente für ein/eine: Rifle\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":51,\"name\":\"Waffenvisier\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.30000001192092896,\"amount\":1,\"description\":\"Komponente für ein/eine: Rifle\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":46,\"name\":\"Waffenschaft\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":1.2999999523162842,\"amount\":1,\"description\":\"Komponente für ein/eine: Bullpup Gewehr\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":false}","{\"configId\":27,\"name\":\"Kleidungsset\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":1.0,\"amount\":1,\"description\":\"Merryweather Parade Outfit\",\"isEquipped\":true,\"equipSlot\":\"clothes\",\"useable\":true}","{\"configId\":36,\"name\":\"Notiz\",\"quality\":-1,\"category\":\"bauernhof\",\"weight\":0.009999999776482582,\"amount\":1,\"description\":\"Titel: Test\",\"isEquipped\":false,\"equipSlot\":\"\",\"useable\":true}"],
    
//         })
// }, 100);

//  setTimeout(() => {
//      input.onEventTrigger("LOAD_SINGLE_INVENTORY", {
//          id: 1,
//          maxWeight: 30,
//          items: [
//              JSON.stringify({configId: 0, name: "Schraubenschlüssel", quality: 0, category: "bauernhof", description: "Und wie schaut das mit einer etwas längeren Beschreibung aus.", weight: 0.5, amount: 10, useable: false, equipSlot: "pistol", isEquipped: true}),
//              JSON.stringify({configId: 2, name: "Ein Spitzhacke",  quality: 0, category: "bauernhof", description: "Test-Beschreibung", weight: 0.5, amount: 12, useable: true, equipSlot: "mask", isEquipped: false}),
//              JSON.stringify({configId: 1, name: "Geiler Fisch",  quality: 1, category: "fisch", description: "Test-Beschreibung", weight: 0.5, amount: 12, useable: false}),
//         ],
//         cash: 300.50,
//         duty: "Polizei",
//         info: "Minijob gestartet",
//         giveItemTarget: 1,

//         painPercent: 0.5,
//         parts: [
//             JSON.stringify({name: "Kopf", type: "stechende Schmerzen", painLevel: 1, multiple: false}),
//             JSON.stringify({name: "linken Bein", type: "stumpfe Schmerzen", painLevel: 6, multiple: true}),
//         ]
//      })
//  }, 100); 

//  setTimeout(() => {
//      input.onEventTrigger("PLAY_SOUND", {
//          name: "Bell.ogg",
//          volume: 1,
//          loop: false,
//      })
//  }, 1000);


//  setTimeout(() => {
//      input.onEventTrigger("PLAY_DISTANCE_SOUND", {
//          source: "http://localhost:80/radio.mp3?target=https://azuracast.choicev-cef.net:8010",
//          volume: 0.1,
//          loop: false,
//          soundPos: {X: 0, Y: 0, Z: 0},
//          playerPos: {X: 0, Y: 0, Z: 0},
//          maxDistance: 10,
//      })
//  }, 1000);

//  setTimeout(() => {
//     input.onEventTrigger("PLAY_SPATIAL_SOUND", {
//         name: "Bell.ogg",
//         relativePos: {X: 2.967033, Y: 5.24395, Z: -0.5897},
//         forwardVec: {X: 0.5997277, Y: 0.80020416},
//         volume: 1,
//         maxDistance: 100,
//     })
//  }, 300);
 
//  setTimeout(() => {
//     input.onEventTrigger("PLAY_SPATIAL_SOUND", {
//         name: "Bell.ogg",m
//         relativePos: {X: 5, Y: -4},
//         forwardVec: {X: 0.25, Y: 1},
//         volume: 1,
//         maxDistance: 10,
//     })
//  }, 100);


// setTimeout(() => {
//     input.onEventTrigger("OPEN_NOTE", {
//         text: "Text",
//         title: "Title",
//         readOnly: true
//     })
// }, 100);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_NOTIFICATION", {
//         title: "Text",
//         message: "Du bist zwar aus dem Gefängnis raus, aber keine Insassen-akte ist noch nicht gelöscht. Melde dich bei den Wärtern. Es waren noch Besitztümer des Insassen in der Zelle.",
//         imgName: "Bone.png",
//         type: "Info",
//         replaceCategory: "TEST1"
//     })
// }, 200);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_NOTIFICATION", {
//         title: "Text",
//         message: "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren,",
//         imgName: "Bone.png",
//         type: "Danger",
//     })
// }, 1300);

// setTimeout(() => {
//     input.onEventTrigger("CREATE_NOTIFICATION", {
//         title: "Text",
//         message: "Lorem ipsum dolor sit amet, consetetur sadips",
//         imgName: "Bone.png",
//         type: "Warning",
//         replaceCategory: "TEST1"
//     })
// }, 1000);
