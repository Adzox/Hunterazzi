using System;
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

