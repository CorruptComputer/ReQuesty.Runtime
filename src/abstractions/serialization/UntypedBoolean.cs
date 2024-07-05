﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Kiota.Abstractions.Serialization
{
    /// <summary>
    /// Represents an untyped node with boolean value.
    /// </summary>
    /// <param name="value">The boolean value associated with the node.</param>
    public class UntypedBoolean(bool value) : UntypedNode
    {
        private readonly bool _value = value;
        /// <summary>
        /// Gets the value associated with untyped boolean node.
        /// </summary>
        /// <returns>The value associated with untyped boolean node.</returns>
        public new bool GetValue() => _value;
    }
}
