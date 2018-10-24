using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}

