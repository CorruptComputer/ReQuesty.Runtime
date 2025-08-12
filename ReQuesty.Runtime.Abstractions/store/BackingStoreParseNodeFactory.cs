using ReQuesty.Runtime.Abstractions.Serialization;

namespace ReQuesty.Runtime.Abstractions.Store;

/// <summary>
///   Proxy implementation of <see cref="IAsyncParseNodeFactory"/> for the <see cref="IBackingStore">backing store</see> that automatically sets the state of the backing store when deserializing.
/// </summary>
public class BackingStoreParseNodeFactory(IAsyncParseNodeFactory concrete) : ParseNodeProxyFactory(
    concrete,
    (x) =>
        {
            if (x is IBackedModel backedModel && backedModel.BackingStore != null)
            {
                backedModel.BackingStore.InitializationCompleted = false;
            }
        },
    (x) =>
        {
            if (x is IBackedModel backedModel && backedModel.BackingStore != null)
            {
                backedModel.BackingStore.InitializationCompleted = true;
            }
        }
    );
