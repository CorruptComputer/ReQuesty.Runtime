namespace ReQuesty.Runtime.Abstractions.Serialization;

/// <summary>
///   Defines the factory for creating parsable objects.
/// </summary>
/// <param name="node">The <see cref="IParseNode">node</see> to parse use to get the discriminator value from the payload.</param>
/// <returns>The <see cref="IParsable">parsable</see> object.</returns>
/// <typeparam name="T">The type of the parsable object.</typeparam>
public delegate T ParsableFactory<T>(IParseNode node) where T : IParsable;
