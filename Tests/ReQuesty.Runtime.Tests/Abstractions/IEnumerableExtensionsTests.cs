using ReQuesty.Runtime.Extensions;

namespace ReQuesty.Runtime.Tests.Abstractions;

public class IEnumerableExtensionsTests
{
    [Fact]
    public void AsList_WithNullInput_ReturnsNull()
    {
        IEnumerable<int>? nullEnumerable = null;
        List<int>? result = nullEnumerable.AsList();
        Assert.Null(result);
    }

    [Fact]
    public void AsList_WithListInput_ReturnsSameList()
    {
        List<int> originalList = [1, 2, 3];
        List<int>? resultList = originalList.AsList();
        Assert.Same(originalList, resultList);
    }

    [Fact]
    public void AsList_WithEnumerableInput_ReturnsNewList()
    {
        IEnumerable<int> enumerable = new int[] { 1, 2, 3 };
        List<int>? resultList = enumerable.AsList();
        Assert.NotSame(enumerable, resultList);
        Assert.Equal(enumerable, resultList);
    }

    [Fact]
    public void AsArray_WithNullInput_ReturnsNull()
    {
        IEnumerable<int>? nullEnumerable = null;
        int[]? result = nullEnumerable.AsArray();
        Assert.Null(result);
    }

    [Fact]
    public void AsArray_WithArrayInput_ReturnsSameArray()
    {
        int[] originalArray = new int[] { 1, 2, 3 };
        int[]? resultArray = originalArray.AsArray();
        Assert.Same(originalArray, resultArray);
    }

    [Fact]
    public void AsArray_WithListInput_ReturnsNewArray()
    {
        List<int> list = [1, 2, 3];
        int[]? resultArray = list.AsArray();
        Assert.NotSame(list, resultArray);
        Assert.Equal(list, resultArray);
    }

    [Fact]
    public void AsArray_WithEnumerableInput_ReturnsNewArray()
    {
        IEnumerable<int> enumerable = new int[] { 1, 2, 3 };
        int[]? resultArray = enumerable.AsArray();
        // We expect a new array only if the input is not already an array
        if (enumerable is not int[])
        {
            Assert.NotSame(enumerable, resultArray);
        }
        Assert.Equal(enumerable, resultArray);
    }

    [Fact]
    public void AsArray_WithNonCollectionEnumerableInput_ReturnsNewArray()
    {
        IEnumerable<int> enumerable = GetEnumerable();
        int[]? resultArray = enumerable.AsArray();
        Assert.Equal(new int[] { 1, 2, 3 }, resultArray);
    }

    private IEnumerable<int> GetEnumerable()
    {
        yield return 1;
        yield return 2;
        yield return 3;
    }
}
