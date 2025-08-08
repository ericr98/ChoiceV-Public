import alt from 'alt';
import game from 'natives';

import { isLock } from './client.js';
import { isLoginViewOpen } from './connection.js';
import { editorOpen } from './cef.js';

let buffer = [];

let loaded = false;
export let opened = false;
let hidden = false;

let view = null;

alt.onServer("TOGGLE_CHAT", () => {
  if (view == null) {
    view = new alt.WebView("http://resource/cef/NEEDED/chat/index.html");

    view.on('chatloaded', () => {
      for (const msg of buffer) {
        addMessage(msg.name, msg.text);
      }

      loaded = true;
    })

    view.on('chatmessage', (text) => {
      alt.emitServer('chatmessage', text);

      opened = false;
      alt.toggleGameControls(true);
    })

  } else {
    view.destroy();
    view = null;
  }
});

function addMessage(name, text) {
  if (name) {
    view.emit('addMessage', name, text);
  } else {
    view.emit('addString', text);
  }
}
export function pushMessage(name, text) {
  if (!loaded) {
    buffer.push({ name, text });
  } else {
    addMessage(name, text);
  }
}

export function pushLine(text) {
  pushMessage(null, text);
}

alt.onServer('chatmessage', pushMessage);

alt.on('keyup', (key) => {
  if (!loaded)
    return;

  if (!opened && key === 0x54 && !isLock() && !isLoginViewOpen() && !editorOpen && view != null) {
    opened = true;
    view.emit('openChat', false);
    alt.toggleGameControls(false);
    view.focus();
  }
  else if (!opened && key === 0xBF && !isLock() && !isLoginViewOpen() && !editorOpen && view != null) {
    opened = true;
    view.emit('openChat', true);
    alt.toggleGameControls(false);
    view.focus();
  }
  else if (opened && key == 0x1B && view != null) {
    opened = false;
    view.emit('closeChat');
    alt.toggleGameControls(true);
    view.unfocus();
  }

  if (key == 0x76) {
    hidden = !hidden;
    game.displayHud(!hidden);
    game.displayRadar(!hidden);
    if(view != null) {
      view.emit('hideChat', hidden);
    }
  }
})

export default { pushMessage, pushLine };
