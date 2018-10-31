﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions {

    public static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second) {
        using (var e1 = first.GetEnumerator())
        using (var e2 = second.GetEnumerator()) {
            while (e1.MoveNext() && e2.MoveNext()) {
                yield return new KeyValuePair<T1, T2>(e1.Current, e2.Current);
            }
        }
    }

    public static void ZipDo<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second, Action<T1, T2> action) {
        using (var e1 = first.GetEnumerator())
        using (var e2 = second.GetEnumerator()) {
            while (e1.MoveNext() && e2.MoveNext()) {
                action(e1.Current, e2.Current);
            }
        }
    }

    public static void Add<T>(this Queue<T> queue, T item) {
        queue.Enqueue(item);
    }

    public static void Add(this Texture2D texture2D, Texture2D other, float blendFactor) {
        if (texture2D.width == other.width && texture2D.height == other.height) {
            float blend = Mathf.Clamp01(blendFactor);
            int width = texture2D.width;
            int height = texture2D.height;
            Color[] colors = new Color[texture2D.width * texture2D.height];
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    colors[x + y * width] = texture2D.GetPixel(x, y) * (1 - blend) + other.GetPixel(x, y) * blend;
                }
            }
            texture2D.SetPixels(colors);
        }
    }

    public static void Mult(this Texture2D texture2D, Texture2D other, float blendFactor) {
        if (texture2D.width == other.width && texture2D.height == other.height) {
            float blend = Mathf.Clamp01(blendFactor);
            int width = texture2D.width;
            int height = texture2D.height;
            Color[] colors = new Color[texture2D.width * texture2D.height];
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    colors[x + y * width] = texture2D.GetPixel(x, y) * other.GetPixel(x, y);
                }
            }
            texture2D.SetPixels(colors);
        }
    }

    public static void AddPixel(this Texture2D texture2D, int x, int y, Color color) {
        texture2D.SetPixel(x, y, (texture2D.GetPixel(x, y) + color) / 2);
    }

    public static void MultPixel(this Texture2D texture2D, int x, int y, Color color) {
        texture2D.SetPixel(x, y, texture2D.GetPixel(x, y) * color / 2);
    }

    #region Coroutine Join

    private class WaitForAllShared {
        public int done;
    }

    private static IEnumerator IncrementDone(MonoBehaviour mb, IEnumerator enumerator, WaitForAllShared shared) {
        yield return mb.StartCoroutine(enumerator);
        shared.done++;
    }

    public static IEnumerator WaitForAllParams(this MonoBehaviour mb, params IEnumerator[] enumerable) {
        return mb.WaitForAll(enumerable);
    }

    public static IEnumerator WaitForAll(this MonoBehaviour mb, IEnumerable<IEnumerator> enumerable) {
        WaitForAllShared shared = new WaitForAllShared();
        int count = 0;
        foreach (IEnumerator enumerator in enumerable) {
            mb.StartCoroutine(IncrementDone(mb, enumerator, shared));
            count++;
        }
        yield return new WaitUntil(() => shared.done == count);
    }

    #endregion
}

