﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xbim.InformationSpecifications.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Capitalises the first character of a string.
        /// Useful when building user messages to capitalise for style.
        /// </summary>
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
    }
}
