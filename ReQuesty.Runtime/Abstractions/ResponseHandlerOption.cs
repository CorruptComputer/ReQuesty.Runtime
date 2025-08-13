﻿namespace ReQuesty.Runtime.Abstractions;

/// <summary>
///   Defines the <see cref="IRequestOption"/> for holding a <see cref="IResponseHandler"/>
/// </summary>
public class ResponseHandlerOption : IRequestOption
{
    /// <summary>
    ///   The <see cref="IResponseHandler"/> to use for a request
    /// </summary>
    public IResponseHandler? ResponseHandler { get; set; }
}
