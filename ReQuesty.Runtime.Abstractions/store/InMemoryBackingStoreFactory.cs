namespace ReQuesty.Runtime.Abstractions.Store;

/// <summary>
///   This class is used to create instances of <see cref="InMemoryBackingStore" />.
/// </summary>
public class InMemoryBackingStoreFactory : IBackingStoreFactory
{
    /// <summary>
    ///   Creates a new instance of <see cref="IBackingStore"/>
    /// </summary>
    /// <returns></returns>
    public IBackingStore CreateBackingStore()
    {
        return new InMemoryBackingStore();
    }
}
