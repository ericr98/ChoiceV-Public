import React from 'react';
import { register } from './../../App';
import { url } from './../../index';
import { Howl, Howler } from 'howler';

//const audioContext = new (window.AudioContext || window.webkitAudioContext)();
var audioContext = new AudioContext();

export default class AudioPlayer extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            audioplayers: [],
        };

        //this.onPlaySound = this.onPlaySound.bind(this);
        //this.onPlayerSpatialSound = this.onPlayerSpatialSound.bind(this);
        //this.onStopSound = this.onStopSound.bind(this);

        this.onPlayNormalSound = this.onPlayNormalSound.bind(this);

        this.onPlayDistanceSound = this.onPlayDistanceSound.bind(this);
        this.onStopDistanceSound = this.onStopDistanceSound.bind(this);
        this.onUpdateSoundVolume = this.onUpdateSoundVolume.bind(this);
        this.onUpdateSoundSource = this.onUpdateSoundSource.bind(this);
        this.onResumePauseSound = this.onResumePauseSound.bind(this);
        this.onUpdateSoundDistance = this.onUpdateSoundDistance.bind(this);

        this.onUpdatePlayerPosition = this.onUpdatePlayerPosition.bind(this);
        this.onUpdatePlayerSoundMuffled = this.onUpdatePlayerSoundMuffled.bind(this);

        this.onSetSourceIdentifierSondMetadata = this.onSetSourceIdentifierSondMetadata.bind(this);

        this.props.input.registerEvent("PLAY_SOUND", this.onPlaySound);
        this.props.input.registerEvent("PLAY_SPATIAL_SOUND", this.onPlayerSpatialSound);
        this.props.input.registerEvent("STOP_SOUND", this.onStopSound);

        this.props.input.registerEvent("PLAY_NORMAL_SOUND", this.onPlayNormalSound);

        this.props.input.registerEvent("PLAY_DISTANCE_SOUND", this.onPlayDistanceSound);
        this.props.input.registerEvent("STOP_DISTANCE_SOUND", this.onStopDistanceSound);
        this.props.input.registerEvent("UPDATE_SOUND_VOLUME", this.onUpdateSoundVolume);
        this.props.input.registerEvent("UPDATE_SOUND_SOURCE", this.onUpdateSoundSource);
        this.props.input.registerEvent("UPDATE_SOUND_DISTANCE", this.onUpdateSoundDistance);
        this.props.input.registerEvent("RESUME_PAUSE_SOUND", this.onResumePauseSound);

        this.props.input.registerEvent("SET_SOURCE_IDENTIFIER", this.onSetSourceIdentifierSondMetadata);

        this.props.input.registerEvent("UPDATE_PLAYER_SOUND_POSITION", this.onUpdatePlayerPosition);
        this.props.input.registerEvent("UPDATE_PLAYER_SOUND_MUFFLED", this.onUpdatePlayerSoundMuffled);

        this.state = {
            playerPos: {x: 0, y: 0, z: 0},
            audioplayers: [],
            sourceInformation: [],
        }
    }

    onSetSourceIdentifierSondMetadata(data) {
        var el = this.state.sourceInformation.find(el => el.name === data.name);

        if (el != null) {
            el.track = data.track;
            el.artist = data.artist;
        } else {
            this.setState({
                sourceInformation: [...this.state.sourceInformation, {
                    name: data.name,
                    track: data.track,
                    artist: data.artist
                }]
            });
        }
    }

    onPlayNormalSound(data) {
        var soundId = data.soundId;
        var volume = data.volume;
        var sourceIdentifier = data.sourceIdentifier;
        var source = data.source;
        var loop = data.loop;

        const [audio, _, __, ___] = this.createAudio("NORMAL", {x: 0, y: 0, z: 0}, -1, volume, source, loop);

        this.setState({
            audioplayers: [...this.state.audioplayers, {
                type: "NORMAL",
                soundId: soundId,
                sourceIdentifier: sourceIdentifier,
                audio: audio,
                volume: volume,
                maxDistance: 0,
                soundPos: {x: 0, y: 0, z: 0},
                source: source,
                loop: loop,
                isMuffled: false,
                sourceNode: null,
                ambientPan: null,
                muffleFilter: null
            }]
        });
    }

    onPlayDistanceSound(data) {
        var soundId = data.soundId;
        var soundPos = {x: data.soundPos.X, y: data.soundPos.Y, z: data.soundPos.Z};
        var playerPos = {x: data.playerPos.X, y: data.playerPos.Y, z: data.playerPos.Z};
        var maxDistance = data.maxDistance;
        var volume = data.volume;
        var source = data.source;
        var sourceIdentifier = data.sourceIdentifier;

        this.setState({
            playerPos: playerPos
        }, () => {
            const [audio, sourceNode, ambientPan, filterNode] = this.createAudio("DISTANCE", soundPos, maxDistance, volume, source, data.loop, () => {
                audio.pause();
                audio.src = "";
                this.setState({audioplayers: this.state.audioplayers.filter(el => el.soundId !== data.soundId)});
                
                if ("alt" in window) {
                    window.alt.emit("DELETE_DISTANCE_SOUND_EVENT", data.soundId);
                }
            });

            this.setState({
                audioplayers: [...this.state.audioplayers, {
                    type: "DISTANCE",
                    soundId: soundId,
                    sourceIdentifier: sourceIdentifier,
                    audio: audio,
                    volume: volume,
                    maxDistance: maxDistance,
                    soundPos: soundPos,
                    source: source,
                    loop: data.loop,
                    isMuffled: false,
                    sourceNode: sourceNode,
                    ambientPan: ambientPan,
                    muffleFilter: filterNode
                }]
            })
        })
    }

    createAudio(type, soundPos, maxDistance, volume, source, loop, onEnded) {
        const audio = new Audio(source);
        audio.loop = loop;
        
        if (onEnded != null) {
            audio.onended = onEnded;
        }

        var sourceNode = null;
        var ambientPan = null;
        var filterNode = null;

        if (type === "DISTANCE") {
            sourceNode = audioContext.createMediaElementSource(audio);

            filterNode = audioContext.createBiquadFilter();
            filterNode.type = 'lowpass';
            filterNode.frequency.value = 24000;

            ambientPan = audioContext.createStereoPanner();

            sourceNode.connect(ambientPan);
            ambientPan.connect(filterNode);
            filterNode.connect(audioContext.destination);
        }

        audio.volume = this.getVolumeValue({type: type, soundPos: soundPos, volume: volume, maxDistance: maxDistance});
        audio.crossOrigin = "anonymous";

        audio.load();
        audio.play();

        return [audio, sourceNode, ambientPan, filterNode];
    }

    onStopDistanceSound(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);
        if (el != null) {
            el.audio.pause();
            el.audio.src = "";
            el.audio = null;
            this.setState({audioplayers: this.state.audioplayers.filter(el => el.soundId !== data.soundId)});
        }
    }

    onUpdateSoundVolume(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);
        if (el != null) {
            el.volume = data.volume;
            el.audio.volume = this.getVolumeValue(el);
        }
    }

    onUpdateSoundSource(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);
        if (el != null) {
            el.source = data.source;
            el.sourceIdentifier = data.sourceIdentifier;

            el.audio.src = data.source;
            if (data.withResume) {
                this.onResumePauseSound({soundId: data.soundId, resume: true})
            }
            // el.audio.src = data.source;
            // el.audio.load();

            // if(data.withResume){
            //     el.audio.play();
            // }
        }
    }

    onUpdateSoundDistance(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);
        if (el != null) {
            el.maxDistance = data.newDistance;
            el.audio.volume = this.getVolumeValue(el);
        }

    }

    onResumePauseSound(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);
        if (el != null) {
            if (data.resume) {
                //const [audio, sourceNode, ambientPan, filterNode] = this.createAudio(el.type, el.soundPos, el.maxDistance, el.volume, el.source, el.loop);
                //el.audio = audio;
                // if(el.type === "DISTANCE") {
                //     el.sourceNode = sourceNode;
                //     el.ambientPan = ambientPan;
                //     el.muffleFilter = filterNode;
                // }
                el.audio.load();
                el.audio.play();
            } else {
                el.audio.pause();
                //el.src = "";
                //el.audio = null;
            }
        }
    }

    getVolumeValue(el) {
        if (el.type === "NORMAL") {
            return el.volume;
        }

        var relativePos = {x: el.soundPos.x - this.state.playerPos.x, y: el.soundPos.y - this.state.playerPos.y};
        var dist = Math.sqrt(relativePos.x * relativePos.x + relativePos.y * relativePos.y);


        //inverse reduction
        var refDistance = el.maxDistance / 5;
        var rolloffFactor = 0.75;
        var rollOffRefdistance = el.maxDistance / 7.5;
        var muffleRollOffFactor = 0.9;
        var distance = dist;

        if (dist > el.maxDistance) {
            return 0;
        }

        var volumeValue = el.volume;
        if (!el.isMuffled) {
            var mod = refDistance / (refDistance + rolloffFactor * (Math.max(distance, refDistance) - refDistance));
            volumeValue = el.volume * mod;
        } else {
            var muffleMod = rollOffRefdistance / (rollOffRefdistance + muffleRollOffFactor * (Math.max(distance, rollOffRefdistance) - rollOffRefdistance));
            volumeValue = el.volume * muffleMod * 0.75;
        }

        //console.log("volumeValue: " +  Math.min(el.volume,  volumeValue.toFixed(10)));
        return Math.max(0, Math.min(el.volume, volumeValue.toFixed(2)));
    }

    getMuffleFrequency(el) {
        var relativePos = {x: el.soundPos.x - this.state.playerPos.x, y: el.soundPos.y - this.state.playerPos.y};
        var dist = Math.sqrt(relativePos.x * relativePos.x + relativePos.y * relativePos.y);

        return 1500 * Math.max(0, -Math.pow(dist * 1.25 / el.maxDistance, 2) + 1)
    }

    getDistance(el) {
        var relativePos = {
            x: el.soundPos.x - this.state.playerPos.x,
            y: el.soundPos.y - this.state.playerPos.y,
            z: el.soundPos.z - this.state.playerPos.z
        };
        return Math.sqrt(relativePos.x * relativePos.x + relativePos.y * relativePos.y + relativePos.z * relativePos.z);
    }

    onUpdatePlayerPosition(data) {
        this.setState({
            playerPos: data.position,
            forwardVec: data.forwardVec
        }, () => {
            this.state.audioplayers.forEach(el => {
                if (el.audio != null && el.type === "DISTANCE") {
                    if (this.getDistance(el) > 2) {
                        var relativePos = {
                            x: el.soundPos.x - this.state.playerPos.x,
                            y: el.soundPos.y - this.state.playerPos.y
                        };
                        var normVec = {x: 0.0, y: 1}
                        var forwardVec = this.state.forwardVec;
                        var rotationAngle = Math.atan2(normVec.y, normVec.x) - Math.atan2(forwardVec.y, forwardVec.x)

                        var s = Math.sin(rotationAngle);
                        var c = Math.cos(rotationAngle);

                        var xnew = relativePos.x * c - relativePos.y * s;
                        var ynew = relativePos.x * s + relativePos.y * c;

                        var panAngle = Math.atan2(xnew, ynew);

                        var panValue = Math.sin(panAngle).toFixed(2);

                        el.ambientPan.pan.value = panValue;
                    } else {
                        el.ambientPan.pan.value = 0;
                    }

                    el.audio.volume = this.getVolumeValue(el);
                }
            });
        });
    }


    onUpdatePlayerSoundMuffled(data) {
        var el = this.state.audioplayers.find(el => el.soundId === data.soundId);

        if (el != null) {
            el.isMuffled = data.isMuffled;

            if (data.isMuffled) {
                el.muffleFilter.frequency.value = this.getMuffleFrequency(el);
            } else {
                if (el.muffleFilter != null) {
                    el.muffleFilter.frequency.value = 24000;
                }
            }
        }
    }

    render() {
        var nearest = null;
        var nearestDistance = 1000000;
        this.state.audioplayers.forEach(el => {
            if (el.type === "NORMAL") {
                nearest = el;
                nearestDistance = 0;
            } else {
                var dist = this.getDistance(el);
                if (dist < el.maxDistance && dist < nearestDistance) {
                    nearest = el;
                    nearestDistance = dist;
                }
            }
        });

        if (nearest != null && (nearest.type == "NORMAL" || this.getDistance(nearest) < nearest.maxDistance)) {
            var nearestInformation = this.state.sourceInformation.find(el => el.name === nearest.sourceIdentifier);
            if (nearestInformation != null) {
                return (
                    <div class="noSelect" style={{
                        position: "fixed",
                        right: "1.5vh",
                        top: "1.5vh",
                        color: "white",
                        fontFamily: "Lato",
                        letterSpacing: "2px"
                    }}>
                        𝅘𝅥𝅮 {nearestInformation.track} - {nearestInformation.artist}
                    </div>
                );
            } else {
                return null;
            }
        } else {
            return null;
        }
    }
}


register(AudioPlayer);