namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Proxy factory that allows the composition of before and after callbacks on existing factories.
/// </summary>
/// <param name="concrete">The concrete factory to wrap.</param>
/// <param name="onBefore">The callback to invoke before the deserialization of any model object.</param>
/// <param name="onAfter">The callback to invoke after the deserialization of any model object.</param>
public abstract class ParseNodeProxyFactory(IAsyncParseNodeFactory concrete, Action<IParsable> onBefore, Action<IParsable> onAfter) : IAsyncParseNodeFactory
{
    /// <summary>
    ///   The valid content type for the <see cref="ParseNodeProxyFactory"/> instance
    /// </summary>
    public string ValidContentType { get { return _concrete.ValidContentType; } }

    private readonly IAsyncParseNodeFactory _concrete = concrete ?? throw new ArgumentNullException(nameof(concrete));
    private readonly Action<IParsable> _onBefore = onBefore;
    private readonly Action<IParsable> _onAfter = onAfter;

    /// <summary>
    ///   Wires node to before and after actions.
    /// </summary>
    /// <param name="node">A parse node to wire.</param>
    private void WireParseNode(IParseNode node)
    {
        Action<IParsable>? originalBefore = node.OnBeforeAssignFieldValues;
        Action<IParsable>? originalAfter = node.OnAfterAssignFieldValues;
        node.OnBeforeAssignFieldValues = (x) =>
        {
            _onBefore?.Invoke(x);
            originalBefore?.Invoke(x);
        };
        node.OnAfterAssignFieldValues = (x) =>
        {
            _onAfter?.Invoke(x);
            originalAfter?.Invoke(x);
        };
    }

    /// <summary>
    ///   Create a parse node from the given stream and content type.
    /// </summary>
    /// <param name="content">The stream to read the parse node from.</param>
    /// <param name="contentType">The content type of the parse node.</param>
    /// <param name="cancellationToken">The cancellation token for the task</param>
    /// <returns>A parse node.</returns>
    public async Task<IParseNode> GetRootParseNodeAsync(string contentType, Stream content,
        CancellationToken cancellationToken = default)
    {
        if (_concrete is not IAsyncParseNodeFactory asyncConcrete)
        {
            throw new Exception("IAsyncParseNodeFactory is required for async operations");
        }
        IParseNode node = await asyncConcrete.GetRootParseNodeAsync(contentType, content, cancellationToken).ConfigureAwait(false);
        WireParseNode(node);
        return node;
    }
}
