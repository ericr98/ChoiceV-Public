var data = {};

var all = document.getElementsByTagName("*");

for (var i=0, max=all.length; i < max; i++) {
    data[all[i].id] = all[i].value;
    
    var id = all[i].id;
    var value = all[i].value;
}

function update(id, value) {
    data[id] = value;

    if("alt" in window) {
        alt.emit("UpdatePedData", JSON.stringify(data));
    }
}

function updateOutfit(id, value, element) {
    const outfitButtons = document.getElementsByClassName('outfitButton');
    Array.from(outfitButtons).forEach((el) => {
        console.log(el);
        if(el.classList.contains("btn-primary")) {
            el.classList.remove("btn-primary");
            el.classList.add("btn-secondary");
        
        }
    });

    element.parentNode.classList.remove("btn-secondary");
    element.parentNode.classList.add("btn-primary");

    update(id, value);
}

const states = ['San Andreas', 'Alabama','Alaska','American Samoa','Arizona','Arkansas','California','Colorado','Connecticut','Delaware','District of Columbia','Federated States of Micronesia','Florida','Georgia','Guam','Hawaii','Idaho','Illinois','Indiana','Iowa','Kansas','Kentucky','Louisiana','Maine','Marshall Islands','Maryland','Massachusetts','Michigan','Minnesota','Mississippi','Missouri','Montana','Nebraska','Nevada','New Hampshire','New Jersey','New Mexico','New York','North Carolina','North Dakota','Northern Mariana Islands','Ohio','Oklahoma','Oregon','Palau','Pennsylvania','Puerto Rico','Rhode Island','South Carolina','South Dakota','Tennessee','Texas','Utah','Vermont','Virgin Island','Virginia','Washington','West Virginia','Wisconsin','Wyoming']
const statesToSSC = ['843','416','574','586','526','429','545','521','040','221','577','001','261','252','586','575','518','318','303','478','508','400','433','004','002','212','010','362','468','425','486','516','505','530','001','135','525 ','050','237','501','586','268','440','540','002','159','580','035','247','503','756','449','528','008','580','223','531','232','387','520']

const faceNames = ['Benjamin', 'Daniel', 'Joshua', 'Noah', 'Andrew', 'Joan', 'Alex', 'Isaac', 'Evan', 'Ethan', 'Vincent', 'Angel', 'Diego', 'Adrian', 'Gabriel', 'Michael', 'Santiago', 'Kevin', 'Louis', 'Samuel',
                        'Anthony',
                        'Hannah',
                        'Audrey',
                        'Jasmine',
                        'Giselle',
                        'Amelia',
                        'Isabella',
                        'Zoe',
                        'Ava',
                        'Camilla',
                        'Violet',
                        'Sophia',
                        'Eveline',
                        'Nicole',
                        'Ashley',
                        'Grace',
                        'Brianna',
                        'Natalie',
                        'Olivia',
                        'Elizabeth',
                        'Charlotte',
                        'Emma',
                        'Claude',
                        'Niko',
                        'John',
                        'Misty'];

function updateFather(id, value) {
    var tag = document.getElementById("fatherlabel");
    tag.innerHTML = faceNames[value];
    update(id, value);
}

const eyeColors = ["Green", "Emerald", "Light Blue", "Ocean Blue", "Light Brown", "Dark Brown", "Hazel", "Dark Gray", "Light Gray", "Pink", "Yellow", "Purple", "Blackout", "Shades of Gray", "Tequila Sunrise", "Atomic", "Warp", "ECola", "Space Ranger", "Ying Yang", "Bullseye", "Lizard", "Dragon", "Extra Terrestrial", "Goat", "Smiley", "Possessed", "Demon", "Infected", "Alien", "Undead", "Zombie"];

var fHairStyleCount = 0;
var mHairStyleCount = 0;

function updateMother(id, value) {
    var tag = document.getElementById("motherlabel");
    tag.innerHTML = faceNames[value];
    update(id, value);
}

function rotate(value) {
    alt.emit("Rotate", value);
}

function camChange(value) {
    alt.emit("SetCamera", value);
}

function genderChange(value) {
    alt.emit("SetGender", value);

    var select = document.getElementById("hairStyle");
    select.value = 0;
    if(value == "female") {
        select.max = fHairStyleCount;
    } else {
        select.max = mHairStyleCount;
    }
    setTimeout(() => {
        alt.emit("UpdatePedData", JSON.stringify(data));
    }, 500);
}

function maskChange(value) {
    alt.emit("SetMask", value);
}

function saveChar() {
    alt.emit("FinishPedCreation", JSON.stringify(data));
}

function cancel() {
    alt.emit("CancelPedCreation");
}

function selectState(state) {
    var idx = states.findIndex((el) => {return el == state});
    update("originState", state);
    update("sscPrefix", statesToSSC[idx]);
}

selectState(states[0]);
var selectBox = document.querySelector("select");
for(i in states) {
    selectBox.options.add(new Option(states[i], states[i], false, false));
}

alt.on("SET_DATA", (styleJSON, otherStyleJSON, hairStyleFCount, hairStyleMCount, hairOverlayCount) => {
    fHairStyleCount = hairStyleFCount - 1;
    mHairStyleCount = hairStyleMCount - 1;

    var select = document.getElementById("hairStyle");
    select.max = mHairStyleCount;

    var hairOverlay = document.getElementById("hairOverlay");
    //Not -1 because 0 is removal
    hairOverlay.max = hairOverlayCount;

    if(styleJSON != null) {
        var style = JSON.parse(styleJSON);
        data = style;
        Object.keys(style).forEach(key => {
            var select = document.getElementById(key);
            if(select != null) {
                select.value = style[key];
            }
        });

        document.getElementById("cancelButton").style.visibility="visible";
    }
 
    if(otherStyleJSON != null) {
        console.log(otherStyleJSON);
        var otherStyle = JSON.parse(otherStyleJSON);
        hairOverlay.value = otherStyle.overlayIdx;

        document.getElementById("title").value = otherStyle.title;
        document.getElementById("firstName").value = otherStyle.firstName;
        document.getElementById("middleNames").value = otherStyle.middleNames;
        document.getElementById("lastName").value = otherStyle.lastName;
        document.getElementById("charBirth").value = otherStyle.birthday

        var sscIdx = statesToSSC.indexOf(otherStyle.sscPrefix);
        document.getElementById("stateSelect").value = states[sscIdx];
        otherStyle["originState"] = states[sscIdx];
        
        data = { ...data, ...otherStyle };
    }

    alt.emit("UpdatePedData", JSON.stringify(data));
});