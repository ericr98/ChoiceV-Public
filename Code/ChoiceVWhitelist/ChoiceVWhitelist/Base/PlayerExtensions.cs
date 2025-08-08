using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Data;
using System.Numerics;

namespace ChoiceVServer.Base {
    public static class PlayerExtensions {

        #region API Method Extensions

        /// <summary>
        /// Trigger a Client Event.
        /// </summary>
        /// <param name="eventname">The Event that should be triggered</param>
        /// <param name="args">The Event event args sent to the player</param>
        public static void emitClientEvent(this IPlayer player, string eventname, params object[] args) {
            if(player != null) {
                player.Emit(eventname, args);
            }
        }


        #endregion

        #region Custom Extensions

        public static void setDimension(this IPlayer player, int dimension) {
            player.SetSyncedMetaData("DIMENSION_CHANGE", dimension);
            player.Dimension = dimension;
        }

        #endregion 
    }
}