namespace ReQuesty.Runtime.Abstractions.Store;

/// <summary>
///   This class is used to register the backing store factory.
/// </summary>
public class BackingStoreFactorySingleton
{
    /// <summary>
    ///   The backing store factory singleton instance.
    /// </summary>
    public static IBackingStoreFactory Instance { get; set; } = new InMemoryBackingStoreFactory();
}
