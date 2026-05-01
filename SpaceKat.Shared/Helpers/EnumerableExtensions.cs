using System;
using System.Collections.Generic;

namespace SpaceKat.Shared.Helpers;

public static class EnumerableExtensions
{
    public static void Iter<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}
