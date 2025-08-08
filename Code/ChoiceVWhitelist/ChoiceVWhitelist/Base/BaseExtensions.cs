using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;
using System.Globalization;
using AltV.Net.Data;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using ChoiceVServer.Base;

namespace ChoiceVServer.Base {
    static class BaseExtensions {

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
    }

    public static class DictionaryExtensions {
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
            int tries = 0;
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

    public static class EnumerableExtensions {
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
        public static string Truncate(this string value, int maxLength) {
            if(string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
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
            Vector2 point = new Vector2(r.Next((int)minEdge.X, (int)maxEdge.X), r.Next((int)minEdge.Y, (int)maxEdge.Y));

            while(!IsInPolygon(polygon, point)) {
                point = new Vector2(r.Next((int)minEdge.X, (int)maxEdge.X), r.Next((int)minEdge.Y, (int)maxEdge.Y));
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
    }
}