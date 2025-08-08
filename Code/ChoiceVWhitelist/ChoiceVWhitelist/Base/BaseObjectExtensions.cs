using AltV.Net.Elements.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChoiceVServer.Base {
    static class BaseObjectExtension {
        /// <summary>
        /// Setting dynamic data for a specific client
        /// <param name="key"/>The Name of the Specific Data</param>
        /// <param name="value"/>The value that shall be saved</param>
        /// </summary>
        public static void setData(this IBaseObject obj, string key, dynamic value) {
            BaseObjectData.SetBaseObjectData(obj.NativePointer, key, value);
        }

        /// <summary>
        /// Gets a data specified by the given key
        /// <param name="key"/>The Name of the specific data</param>
        /// </summary>
        public static dynamic getData(this IBaseObject obj, string key) {
            return BaseObjectData.GetBaseObjectData(obj.NativePointer, key);
        }

        /// <summary>
        /// Gets if data has specific key
        /// <param name="key"/>The Name of the specific data</param>
        /// </summary>
        public static bool hasData(this IBaseObject obj, string key) {
            return BaseObjectData.HasBaseObjectData(obj.NativePointer, key);
        }

        /// <summary>
        /// Resets specific data
        /// <param name="key"/>The Name of the specific data</param>
        /// </summary>
        public static void resetData(this IBaseObject obj, string key) {
            BaseObjectData.ResetBaseObjectData(obj.NativePointer, key);
        }
    }

    public static class BaseObjectData {
        private static ConcurrentDictionary<IntPtr, ConcurrentDictionary<string, dynamic>> _baseObjectData = new ConcurrentDictionary<IntPtr, ConcurrentDictionary<string, dynamic>>();

        /// <summary>
        /// DO NOT USE! USE REPRESENTATIVE EXTENSION METHOD FOR ENTITY TYPE.
        /// </summary>
        public static void SetBaseObjectData(IntPtr pointer, string key, dynamic value) {
            if(_baseObjectData.ContainsKey(pointer)) {
                ResetBaseObjectData(pointer, key);
                _baseObjectData[pointer].TryAdd(key, value);
                return;
            }

            _baseObjectData.TryAdd(pointer, new ConcurrentDictionary<string, dynamic>());
            _baseObjectData[pointer].TryAdd(key, value);
        }

        /// <summary>
        /// DO NOT USE! USE REPRESENTATIVE EXTENSION METHOD FOR ENTITY TYPE.
        /// </summary>
        public static dynamic GetBaseObjectData(IntPtr pointer, string key) {
            if(_baseObjectData.ContainsKey(pointer)) {
                if(_baseObjectData[pointer].ContainsKey(key))
                    return _baseObjectData[pointer][key];
                return null;
            }

            return null;
        }

        /// <summary>
        /// DO NOT USE! USE REPRESENTATIVE EXTENSION METHOD FOR ENTITY TYPE.
        /// </summary>
        public static bool HasBaseObjectData(IntPtr pointer, string key) {
            if(_baseObjectData.ContainsKey(pointer)) {
                return _baseObjectData[pointer].ContainsKey(key);
            }

            return false;
        }


        /// <summary>
        /// DO NOT USE! USE REPRESENTATIVE EXTENSION METHOD FOR ENTITY TYPE.
        /// </summary>
        public static void ResetBaseObjectData(IntPtr pointer, string key) {
            if(_baseObjectData.ContainsKey(pointer)) {
                if(_baseObjectData[pointer].ContainsKey(key)) {
                    _baseObjectData[pointer].TryRemove(key, out _);
                }
            }
        }

        /// <summary>
        /// ONLY USE FOR REMOVING ENTITY FROM BASEOBJECTDATA DICTIONARY.
        /// </summary>
        public static void RemoveBaseObject(IntPtr pointer) {
            if(_baseObjectData.ContainsKey(pointer)) {
                _baseObjectData.Remove(pointer);
            }
        }

    }
}
