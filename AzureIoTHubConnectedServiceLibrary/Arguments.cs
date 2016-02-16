// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See license.txt file in the project root for full license information.

using System;

namespace AzureIoTHubConnectedService
{
    internal static class Arguments
    {
        /// <summary>
        /// Validates that the argument with the given is non-null, and throws an exception otherwise.
        /// </summary>
        /// <typeparam name="T">The argument type</typeparam>
        /// <param name="value">The argument value to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private IServiceProvider _serviceProvider;
        ///   function foo(IServiceProvider serviceProvider) {
        ///     this._serviceProvider = Arguments.ValidateNotNull(serviceProvider, "serviceProvider");
        ///   }
        /// </remarks>
        public static T ValidateNotNull<T>(T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }

        /// <summary>
        /// Validates that the argument with the given is non-null and contains more than just whitespace, 
        /// and throws an exception otherwise.
        /// </summary>
        /// <param name="value">The argument value to validate.</param>
        /// <param name="name">The name of the argument, for the exception.</param>
        /// <returns>The validated argument value.</returns>
        /// <remarks>
        /// Example usage:
        ///   private string _myString;
        ///   function foo(string myString) {
        ///     this._myString = Arguments.ValidateNotNullOrWhitespace(myString, "myString");
        ///   }
        /// </remarks>
        public static string ValidateNotNullOrWhitespace(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }
    }
}
