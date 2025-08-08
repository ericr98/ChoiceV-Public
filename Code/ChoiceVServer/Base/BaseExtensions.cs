using AltV.Net.Data;
using ChoiceVServer.Base;
using ChoiceVServer.Model.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace ChoiceVServer.Base {
    public static class BaseExtensions {
        public static int ExtractNumber(this string text) {
            Match match = Regex.Match(text, @"(\d+)");
            if(match == null) {
                return 0;
            }

            int value;
            if(!int.TryParse(match.Value, out value)) {
                return 0;
            }

            return value;
        }

        public static DateTime FromUnixTime(long unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static long ToUnixTime(this DateTime dateTime) {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
        }


        public static Vector3 ToEulerAngles(this Quaternion q) {
            var x = q.X;
            var y = q.Y;
            var z = q.Z;
            var w = q.W;
            var xx = x * x;
            var yy = y * y;
            var zz = z * z;
            var ww = w * w;
            var ls = xx + yy + zz + ww;
            var st = x * w - y * z;
            var sv = ls * 0.499f;
            var rd = 180.0f / (float)Math.PI;
            if(st > sv) {
                return new Vector3(90, (float)Math.Atan2(y, x) * 2.0f * rd, 0);
            } else if(st < -sv) {
                return new Vector3(-90, (float)Math.Atan2(y, x) * -2.0f * rd, 0);
            } else {
                return new Vector3(
                    (float)Math.Asin(2.0f * st) * rd,
                    (float)Math.Atan2(2.0f * (y * w + x * z), 1.0f - 2.0f * (xx + yy)) * rd,
                    (float)Math.Atan2(2.0f * (x * y + z * w), 1.0f - 2.0f * (xx + zz)) * rd);

                //// Store the Euler angles in radians
                //Vector3 pitchYawRoll = new Vector3();

                //double sqw = q.W * q.W;
                //double sqx = q.X * q.X;
                //double sqy = q.Y * q.Y;
                //double sqz = q.Z * q.Z;

                //// If quaternion is normalised the unit is one, otherwise it is the correction factor
                //double unit = sqx + sqy + sqz + sqw;
                //double test = q.X * q.Y + q.Z * q.W;

                //if(test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
                //{
                //    // Singularity at north pole
                //    pitchYawRoll.X = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                //    pitchYawRoll.Z = Convert.ToSingle(Math.PI) * 0.5f;  // Pitch
                //    pitchYawRoll.Y = 0f;                                // Roll
                //    return pitchYawRoll;
                //} else if(test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
                //  {
                //    // Singularity at south pole
                //    pitchYawRoll.X = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                //    pitchYawRoll.Z = Convert.ToSingle(-Math.PI) * 0.5f; // Pitch
                //    pitchYawRoll.Y = 0f;                                // Roll
                //    return pitchYawRoll;
                //} else {
                //    pitchYawRoll.X = (float)Math.Atan2(2f * q.Y * q.W - 2f * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
                //    pitchYawRoll.Z = (float)Math.Asin(2f * test / unit);                                             // Pitch
                //    pitchYawRoll.Y = (float)Math.Atan2(2f * q.X * q.W - 2f * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll
                //}

                //return pitchYawRoll;
            }
        }

        private static double copysign(double x, double y) => Math.Sign(x) == Math.Sign(y) ? x : -x;

        public static object PopulateJson(this object obj, string data, JsonSerializerSettings serializerSettings = null) {
            if(obj is not null) {
                JsonConvert.PopulateObject(data, obj, serializerSettings);
            }

            return obj;
        }

        /// <summary>
        /// Transforms any given object to a JSON string. Use for saving in e.g. Database
        /// </summary>
        public static string ToJson(this object obj) {
            switch(obj) {
                case null:
                    return string.Empty;
                case Position position: {
                        Vector3 serializablePosition = position;
                        return JsonConvert.SerializeObject(serializablePosition, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    }
                case Rotation rotation: {
                        Vector3 serializableRotation = rotation;
                        return JsonConvert.SerializeObject(serializableRotation, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    }
                default:
                    return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            }
        }

        public static string ToJsonWithIgnore(this object obj, JsonIgnoreContractResolver resolver) {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, ContractResolver = resolver });
        }

        public static string ToJson(this characterstyle style) {
            return JsonConvert.SerializeObject(style, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        /// <summary>
        /// Transforms a given JSON string back to an given object type. Returns generated object.
        /// </summary>
        /// <typeparam name="T">Variable object type.</typeparam>
        public static T FromJson<T>(this string s) {
            if(string.IsNullOrEmpty(s)) {
                return default;
            }

            //Rotation can not be converted from JSON it seems.
            if(typeof(T) == typeof(Rotation)) {
                var rotation = JsonConvert.DeserializeObject<Vector3>(s);
                return (T)(object)new Rotation(rotation.X, rotation.Y, rotation.Z);
            }

            return JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>
        /// Generates API Vector3 object from JSON string. Use e.g. for transforming database positions to the game.
        /// </summary>
        public static Position FromJson(this string s) {
            if(string.IsNullOrEmpty(s) || s == "null") {
                return Constants.EmptyVector;
            }

            return JsonConvert.DeserializeObject<Position>(s);
        }

        //https://github.com/codebrainz/color-names/tree/master/output
        public static string getColorName(this Color c) {
            Dictionary<Color, KnownColor> allColors = new Dictionary<Color, KnownColor>();
            foreach(KnownColor kc in Enum.GetValues(typeof(KnownColor))) {
                if((int)kc < 30) {
                    continue;
                }

                Color known = Color.FromKnownColor(kc);
                allColors.Add(known, kc);
            }

            var closest = GetClosestColor(allColors.Keys.ToArray(), c);

            return allColors[closest].ToString();
        }

        private static Color GetClosestColor(Color[] colorArray, Color baseColor) {
            var colors = colorArray.Select(x => new { Value = x, Diff = GetDiff(x, baseColor) }).ToList();
            var min = colors.Min(x => x.Diff);
            return colors.Find(x => x.Diff == min).Value;
        }

        private static int GetDiff(Color color, Color baseColor) {
            int a = color.A - baseColor.A,
                r = color.R - baseColor.R,
                g = color.G - baseColor.G,
                b = color.B - baseColor.B;
            return a * a + r * r + g * g + b * b;
        }

        public static Position Round(this Position position) {
            return new Position((float)Math.Round(position.X, 2), (float)Math.Round(position.Y, 2), (float)Math.Round(position.Z, 2));
        }

        public static Rotation Round(this Rotation position) {
            return new Rotation((float)Math.Round(position.Roll, 2), (float)Math.Round(position.Pitch, 2), (float)Math.Round(position.Yaw, 2));
        }

        public static byte ToByte(this int i) {
            return Convert.ToByte(i);
        }
    }

    public class JsonIgnoreContractResolver : DefaultContractResolver {
        private readonly HashSet<string> ignoreProps;
        public JsonIgnoreContractResolver(IEnumerable<string> propNamesToIgnore) {
            this.ignoreProps = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if(this.ignoreProps.Contains(property.PropertyName)) {
                property.ShouldSerialize = _ => false;
            }
            return property;
        }
    }

    public static class DictionaryExtensions {
        public static T GetData<T>(this IDictionary<string, string> dictionary, string dataName, T defaultValue = default(T)) {
            try {
                if(dictionary == null)
                    return defaultValue;
                if(!dictionary.ContainsKey(dataName))
                    return defaultValue;

                var tmp = dictionary[dataName];
                if(tmp == null)
                    return defaultValue;
                if(typeof(T).IsPrimitive || (typeof(T) == typeof(String))) {
                    return (T)Convert.ChangeType(tmp, typeof(T), CultureInfo.InvariantCulture);
                } else {
                    return JsonConvert.DeserializeObject<T>(tmp);
                }
            } catch { return defaultValue; }
        }

        public static void SetData(this IDictionary<string, string> dictionary, string key, object value) {
            if(dictionary == null)
                return;

            if(value.GetType().IsPrimitive || (value is String))
                dictionary[key] = Convert.ToString(value, CultureInfo.InvariantCulture);
            else
                dictionary[key] = JsonConvert.SerializeObject(value, Formatting.None);
        }

        public delegate bool Predicate<TKey, TValue>(KeyValuePair<TKey, TValue> d);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RemoveWhere<TKey, TValue>(
            this Dictionary<TKey, TValue> hashtable, Predicate<TKey, TValue> p) {
            foreach(KeyValuePair<TKey, TValue> value in hashtable.ToList().Where(value => p(value)))
                hashtable.Remove(value.Key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RemoveWhere<TKey, TValue>(
            this ConcurrentDictionary<TKey, TValue> hashtable, Predicate<TKey, TValue> p) {
            foreach(KeyValuePair<TKey, TValue> value in hashtable.ToList().Where(value => p(value)))
                hashtable.Remove(value.Key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> hashtable, TKey key) where TValue : class {
            TValue valOut;
            if(hashtable.TryGetValue(key, out valOut))
                return valOut;
            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> hashtable, TKey key, TValue value) where TValue : class {
            TValue valOut;
            if(hashtable.TryGetValue(key, out valOut))
                return valOut;
            hashtable.Add(key, value);
            return value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) {
            TValue value;
            if((key == null) || !dict.ContainsKey(key))
                return false;
            return dict.TryRemove(key, out value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value) {
            if(key == null)
                return false;
            if(dict.ContainsKey(key))
                dict.Remove(key);
            int tries = 0;
            while(!dict.TryAdd(key, value)) {
                tries++;
                if(tries > 10) {
                    //Global.Logger.Warn("ConCurrent add for {0} failed after 10 tries", key);
                    return false;
                }
                Thread.Sleep(100);
            }
            return true;
        }
    }

    public static class ShiftList {
        public static List<T> ShiftLeft<T>(this List<T> list, int shiftBy) {
            if(list.Count <= shiftBy) {
                return list;
            }

            var result = list.GetRange(shiftBy, list.Count - shiftBy);
            result.AddRange(list.GetRange(0, shiftBy));
            return result;
        }

        public static List<T> ShiftRight<T>(this List<T> list, int shiftBy) {
            if(list.Count <= shiftBy) {
                return list;
            }

            var result = list.GetRange(list.Count - shiftBy, shiftBy);
            result.AddRange(list.GetRange(0, list.Count - shiftBy));
            return result;
        }
    }

    public static class EnumerableExtensions {
        public static bool containsAscSequence(this List<int> list, int min, int max) {
            if(list.Count == 1) {
                return list[0] == min && list[0] == max;
            }

            var checkArray = new bool[max + 1];

            for(int i = 0; i < list.Count(); i++) {
                var el = list[i];

                if(el == 0) {
                    checkArray[0] = true;
                } else {
                    if(checkArray[el - 1] && !checkArray[el]) {
                        checkArray[el] = true;

                        if(checkArray.All(t => t)) {
                            return true;
                        }
                    } else {
                        checkArray = new bool[max + 1];
                    }
                }
            }

            return false;
        }


        //Could be more efficient, but its release day, and i dont have more time to think about it
        public static bool containsSequence(this List<int> list, List<int> otherList) {
            for(var i = 0; i < list.Count; i++) {
                if(list[i] == otherList[0]) {
                    if(checkSubList(list, otherList, i)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool checkSubList(List<int> list, List<int> otherList, int index) {
            for(var i = 1; i < otherList.Count; i++) {
                if(list.Count <= index + i || list[index + i] != otherList[i]) {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> enumerable, Func<TSource, TSource, bool> comparer) {
            return enumerable.Distinct(new LambdaComparer<TSource>(comparer));
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> enumerable, IEnumerable<TSource> other, Func<TSource, TSource, bool> comparer) {
            return enumerable.Intersect<TSource>(other, new LambdaComparer<TSource>(comparer));
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
            Contract.Requires(enumerable != null);
            Contract.Requires(action != null);

            if(enumerable is T[]) {
                ForEach((T[])enumerable, action);
                return;
            }

            if(enumerable is IReadOnlyList<T>) {
                ForEach((IReadOnlyList<T>)enumerable, action);
                return;
            }

            if(enumerable is IList<T>) {
                ForEach((IList<T>)enumerable, action);
                return;
            }

            foreach(var item in enumerable)
                action(item);
        }

        public static void ForEach<T>(this IReadOnlyList<T> list, Action<T> action) {
            Contract.Requires(list != null);
            Contract.Requires(action != null);

            for(int i = 0; i < list.Count; i++)
                action(list[i]);
        }

        private static void ForEach<T>(this IList<T> list, Action<T> action) {
            Contract.Requires(list != null);
            Contract.Requires(action != null);

            for(int i = 0; i < list.Count; i++)
                action(list[i]);
        }

        public static void ForEach<T>(this T[] array, Action<T> action) {
            Contract.Requires(array != null);
            Contract.Requires(action != null);

            for(int i = 0; i < array.Length; i++)
                action(array[i]);
        }

        public static float Map(this float value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng) {
            if(source == null) throw new ArgumentNullException("source");
            if(rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng) {
            List<T> buffer = source.ToList();
            for(int i = 0; i < buffer.Count; i++) {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
        public static List<T> GetRandomElements<T>(this IEnumerable<T> list, int elementsCount) {
            return list.OrderBy(x => Guid.NewGuid()).Take(elementsCount).ToList();
        }
    }

    public class LambdaComparer<T> : IEqualityComparer<T> {
        private readonly Func<T, T, bool> _lambdaComparer;
        private readonly Func<T, int> _lambdaHash;

        public LambdaComparer(Func<T, T, bool> lambdaComparer) :
            this(lambdaComparer, o => 0) {
        }

        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash) {
            if(lambdaComparer == null)
                throw new ArgumentNullException("lambdaComparer");
            if(lambdaHash == null)
                throw new ArgumentNullException("lambdaHash");

            _lambdaComparer = lambdaComparer;
            _lambdaHash = lambdaHash;
        }

        public bool Equals(T x, T y) {
            return _lambdaComparer(x, y);
        }

        public int GetHashCode(T obj) {
            return _lambdaHash(obj);
        }
    }

    public static class StringExt {
        public static string CutToLength(this string value, int maxLength) {
            if(string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 2) + "..";
        }
    }
}

namespace System {
    public delegate void EventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);

    public static class TypeMethods {
        public static bool TryCast<T>(this object obj, out T result) {
            if(obj is T) {
                result = (T)obj;
                return true;
            }

            result = default(T);
            return false;
        }
    }


    public static class PolygonMethods {
        public static void getBounds(Vector2[] polygon, ref Vector2 minEdge, ref Vector2 maxEdge) {
            float minX = 0, minY = 0, maxX = 0, maxY = 0;

            foreach(var p in polygon) {
                if(p.Y < minY) minY = p.Y;
                if(p.Y > maxY) maxY = p.Y;

                if(p.X < minX) minX = p.X;
                if(p.X > maxX) maxX = p.X;
            }

            minEdge = new Vector2(minX, minY);
            maxEdge = new Vector2(maxX, maxY);
        }

        public static Vector2 getEnclosedPoint(Vector2[] polygon) {
            if(polygon.Length < 3) {
                return Vector2.Zero;
            }

            Vector2 minEdge = new Vector2();
            Vector2 maxEdge = new Vector2();

            getBounds(polygon, ref minEdge, ref maxEdge);

            var r = new Random();
            Vector2 point = new Vector2(r.NextFloat(minEdge.X, maxEdge.X), r.NextFloat(minEdge.Y, maxEdge.Y));

            while(!IsInPolygon(polygon, point)) {
                point = new Vector2(r.NextFloat(minEdge.X, maxEdge.X), r.NextFloat(minEdge.Y, maxEdge.Y));
            }

            return point;
        }

        public static bool IsInPolygon(Vector2[] poly, Vector2 p) {
            Vector2 p1, p2;
            bool inside = false;

            if(poly.Length < 3) {
                return inside;
            }

            var oldPoint = new Vector2(
                poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for(int i = 0; i < poly.Length; i++) {
                var newPoint = new Vector2(poly[i].X, poly[i].Y);

                if(newPoint.X > oldPoint.X) {
                    p1 = oldPoint;
                    p2 = newPoint;
                } else {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - (long)p1.Y) * (p2.X - p1.X)
                    < (p2.Y - (long)p1.Y) * (p.X - p1.X)) {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }

        public static float NextFloat(this Random random, float min, float max) {
            return (float)random.NextDouble() * (max - min) + min;
        }
    }

    public class ExtendedDictionary<TKey, TValue> {
        public delegate void ExtendedDictionaryValueChanged(TKey key, TValue value);
        public delegate bool ExtendedDictionaryValueRemoved(TKey key);

        public Dictionary<TKey, TValue> Items;

        public ExtendedDictionary(Dictionary<TKey, TValue> items) {
            Items = items;
        }

        [JsonIgnore]
        public ExtendedDictionaryValueChanged OnValueChanged;

        [JsonIgnore]
        public ExtendedDictionaryValueRemoved OnValueRemoved;

        public TValue this[TKey key] {
            get {
                try {
                    return Items[key];
                } catch(Exception e) {
                    Logger.logException(e);
                    return default;
                }
            }
            set {
                if(Items.ContainsKey(key)) {
                    var current = Items[key];
                    if(current == null || !current.Equals(value)) {
                        Items[key] = value;
                        if(OnValueChanged != null) {
                            OnValueChanged.Invoke(key, value);
                        }
                    }
                } else {
                    Items[key] = value;

                    if(OnValueChanged != null) {
                        OnValueChanged.Invoke(key, value);
                    }
                }
            }
        }

        public T get<T>(TKey key) {
            if(hasKey(key)) {
                return (T)Convert.ChangeType(Items[key], typeof(T));
            } else {
                return default(T);
            }
        }

        public bool remove(TKey key) {
            if(Items.Remove(key)) {
                if(OnValueRemoved != null) {
                    return OnValueRemoved.Invoke(key);
                } else {
                    return false;
                }
            }

            return false;
        }

        public bool hasKey(TKey key) {
            return Items.ContainsKey(key);
        }

        public IEnumerable<TKey> getKeys() {
            return Items.Keys;
        }

        public string ToJson() {
            return Items.ToJson();
        }

        public int getCount() {
            return Items.Count;
        }
    }
}