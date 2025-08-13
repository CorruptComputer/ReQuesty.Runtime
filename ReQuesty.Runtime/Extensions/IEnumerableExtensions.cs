namespace ReQuesty.Runtime.Extensions;

/// <summary>
///   Extension methods for <see cref="IEnumerable{T}"/>
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    ///   Converts an <see cref="IEnumerable{T}"/> to a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="e">The enumerable to convert.</param>
    /// <returns>A <see cref="List{T}"/> containing the elements of the enumerable, or <c>null</c> if the input is <c>null</c>.</returns>
    public static List<T>? AsList<T>(this IEnumerable<T>? e)
    {
        if (e is null)
        {
            return null;
        }

        if (e is List<T> list)
        {
            return list;
        }

        return [.. e];
    }

    /// <summary>
    ///   Converts an <see cref="IEnumerable{T}"/> to an array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="e">The enumerable to convert.</param>
    /// <returns>An array containing the elements of the enumerable, or <c>null</c> if the input is <c>null</c>.</returns>
    public static T[]? AsArray<T>(this IEnumerable<T>? e)
    {
        if (e is null)
        {
            return null;
        }

        if (e is T[] array)
        {
            return array;
        }

        T[]? result = null;

        if (e is ICollection<T> collection)
        {
            // Allocate an array with the exact size
            result = AllocateOnHeap(collection.Count);
            collection.CopyTo(result, 0);
            return result;
        }

        // First pass to count the elements
        int count = 0;
        foreach (T item in e)
        {
            count++;
        }

        result = AllocateOnHeap(count);

        // Second pass to copy the elements
        count = 0;
        foreach (T? item in e)
        {
            result[count++] = item;
        }

        return result;

        static T[] AllocateOnHeap(int count)
        {
            return GC.AllocateUninitializedArray<T>(count);
        }
    }
}

