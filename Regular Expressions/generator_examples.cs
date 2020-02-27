using System;
using System.Collections.Generic;

public class GeneratorExample {

    public static IEnumerable<int> SimpleExample() {
        yield return 4;
        yield return 8;
        yield return 15;
        yield return 16;
        yield return 23;
        yield return 42;
    }

    public static void Main() {
        var e = SimpleExample().GetEnumerator();
        while (e.MoveNext()) {
            Console.WriteLine(e.Current);
        }

        foreach (var i in SimpleExample()) {
            Console.WriteLine(i);
        }
    }
}