﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FragmentServerWV
{

    /// <summary>
    /// A simple key-value pair configuration class that also supports line comments
    /// </summary>
    /// <remarks>
    /// INI lite
    /// </remarks>
    public sealed class SimpleConfiguration
    {

        private readonly Dictionary<string, string> configurationValues;


        /// <summary>
        /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> that represents the loaded configuration
        /// </summary>
        public ReadOnlyDictionary<string, string> Values => new ReadOnlyDictionary<string, string>(configurationValues);

        /// <summary>
        /// Creates a new instance of the Configuration class
        /// </summary>
        public SimpleConfiguration()
        {
            configurationValues = new Dictionary<string, string>();
            if (File.Exists("settings.txt"))
            {
                string[] lines = File.ReadAllLines("settings.txt");
                foreach (string line in lines)
                    if (line.Trim() != "" && !line.Trim().StartsWith("#"))
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                            configurationValues.Add(parts[0].Trim().ToLower(), parts[1].Trim());
                    }
            }
        }

        /// <summary>
        /// Retrieves a value if it exists, otherwise returns <paramref name="default"/>
        /// </summary>
        /// <param name="name">The name of the configuration value</param>
        /// <param name="default">The value to return if <paramref name="name"/> doesn't exist</param>
        /// <returns><see cref="string"/></returns>
        public string Get(string name, string @default = "") => configurationValues.ContainsKey(name) ? configurationValues[name] : @default;

    }

}
