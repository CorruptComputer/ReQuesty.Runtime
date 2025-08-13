using ReQuesty.Runtime.Abstractions;

namespace ReQuesty.Runtime.Tests.Abstractions;

public class RequestHeadersTests
{
    [Fact]
    public void Defensive()
    {
        RequestHeaders instance = [];
        Assert.Throws<ArgumentNullException>(() => instance.Add(null!, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Add("name", (string[])null!));
        instance.Add("name", []);
        instance.Add("name", new List<string>());
        instance.Add(new KeyValuePair<string, IEnumerable<string>>("name", []));
        Assert.Throws<ArgumentNullException>(() => instance[null!]);
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null!));
        Assert.Throws<ArgumentNullException>(() => instance.Remove(null!, "value"));
        Assert.Throws<ArgumentNullException>(() => instance.Remove("name", null!));
        Assert.Throws<ArgumentNullException>(() => instance.AddAll(null!));
        instance.ContainsKey(null!);
    }
    [Fact]
    public void AddsToNonExistent()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" }
        };
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void TryAddsToNonExistent()
    {
        RequestHeaders instance = [];
        bool result = instance.TryAdd("name", "value");
        Assert.True(result);
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void AddsToExistent()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        Assert.Equal(new[] { "value", "value2" }, instance["name"]);
    }
    [Fact]
    public void TryAddsToExistent()
    {
        RequestHeaders instance = [];
        bool result = instance.TryAdd("name", "value");
        Assert.True(result);
        result = instance.TryAdd("name", "value2");
        Assert.False(result);
        Assert.Equal(new[] { "value" }, instance["name"]);
    }
    [Fact]
    public void AddsSingleValueHeaderToExistent()
    {
        RequestHeaders instance = new()
        {
            { "Content-Type", "value" },
            { "Content-Type", "value2" }
        };
        Assert.Equal(new[] { "value2" }, instance["Content-Type"]);
    }
    [Fact]
    public void TryAddsSingleValueHeaderToExistent()
    {
        RequestHeaders instance = [];
        instance.TryAdd("Content-Type", "value");
        instance.TryAdd("Content-Type", "value2");
        Assert.Equal(new[] { "value" }, instance["Content-Type"]);
    }
    [Fact]
    public void RemovesValue()
    {
        RequestHeaders instance = [];
        instance.Remove("name", "value");
        instance.Add("name", "value");
        instance.Add("name", "value2");
        instance.Remove("name", "value");
        Assert.Equal(new[] { "value2" }, instance["name"]);
        instance.Remove("name", "value2");
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
    }
    [Fact]
    public void Removes()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        Assert.True(instance.Remove("name"));
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.False(instance.Remove("name"));
    }
    [Fact]
    public void RemovesKVP()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        Assert.True(instance.Remove(new KeyValuePair<string, IEnumerable<string>>("name", new[] { "value", "value2" })));
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.False(instance.Remove("name"));
    }
    [Fact]
    public void Clears()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        instance.Clear();
        Assert.Throws<KeyNotFoundException>(() => instance["name"]);
        Assert.Empty(instance.Keys);
    }
    [Fact]
    public void GetsEnumerator()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        using IEnumerator<KeyValuePair<string, IEnumerable<string>>> enumerator = instance.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal("name", enumerator.Current.Key);
        Assert.Equal(new[] { "value", "value2" }, enumerator.Current.Value);
        Assert.False(enumerator.MoveNext());
    }
    [Fact]
    public void Updates()
    {
        RequestHeaders instance = new()
        {
            { "name", "value" },
            { "name", "value2" }
        };
        RequestHeaders instance2 = [];
        instance2.AddAll(instance);
        Assert.NotEmpty(instance["name"]);
    }
}