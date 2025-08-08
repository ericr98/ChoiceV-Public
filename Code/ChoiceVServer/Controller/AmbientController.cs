using AltV.Net.Data;
using ChoiceVServer.Base;
using System;
using System.Collections.Generic;

namespace ChoiceVServer.Controller {
    public class AmbientController : ChoiceVScript {
        private static readonly List<Object> Planes = new List<Object>();

        private const int HeightMin = 800;
        private const int HeightMax = 1200;

        private const int PlaneCount = 6;

        public AmbientController() {
            //InvokeController.AddTimedInvoke("Plane-Generator", (ivk) => {
            //    generatePlane();
            //}, TimeSpan.FromMinutes(1), true);
        }

        private static readonly string[] planeProps = new string[] { "prop_med_jet_01", "p_cs_mp_jet_01_s", "p_med_jet_01_s" };

        public static void generatePlane() {

            foreach(var plane in Planes) {
                if(plane.Deleted) {
                    Planes.Remove(plane);
                }
            }

            if(Planes.Count < PlaneCount) {
                var r = new Random();
                var planeName = r.Next(planeProps.Length);

                var xOrY = r.NextDouble() > 0.5;
                var negOrPos = r.NextDouble() > 0.5;

                var x = 0.0f;
                var y = 0.0f;


                if(xOrY) {
                    x = (float)(r.NextDouble() * 8000) - 4000;

                    if(negOrPos) {
                        y = -9000;
                    } else {
                        y = 9000;
                    }
                } else {
                    y = (float)(r.NextDouble() * 18000) - 9000f;

                    if(negOrPos) {
                        x = -4000;
                    } else {
                        x = 4000;
                    }
                }

                var z = r.Next(HeightMin, HeightMax);
                var createPos = new Position(x, y, z);

                var to = (Position.Zero - createPos);
                to.Z = z;

                var newPlane = ObjectController.createObject(planeProps[planeName], createPos, new Rotation(0, 0, (float)bearing(createPos.X, createPos.Y, to.X, to.Y)), 100000);

                InvokeController.AddTimedInvoke("StartPlane", (ivk) => {
                    ObjectController.moveObject(newPlane, to, true, 0.65f, 0.65f, 0.65f, true);
                    //ObjectController.setObjectRotation(newPlane, new Rotation(0, 0, (float)bearing(createPos.X, createPos.Y, to.X, to.Y)));
                }, TimeSpan.FromSeconds(2), false);

                Logger.logDebug(LogCategory.System, LogActionType.Created, $"Plane started: model: {planeProps[planeName]}. At: X: {createPos.X}, Y:{createPos.Y}, Z: {createPos.Z}. To: X: {to.X}, Y: {to.Y}, Z {to.Z}");
            }
        }

        private static double bearing(double a1, double a2, double b1, double b2) {
            const double TWOPI = 6.2831853071795865;
            const double RAD2DEG = 57.2957795130823209;
            double theta = Math.Atan2(b1 - a1, a2 - b2);
            if(theta < 0.0)
                theta += TWOPI;
            return RAD2DEG * theta;
        }
    }
}
