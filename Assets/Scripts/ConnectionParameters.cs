#nullable enable

using System;
using System.Collections.Generic;
using System.Web;
using UnityEngine;

/// <summary>
/// Utility functions to handle query string parameters.
/// </summary>
public static class ConnectionParameters
{
    /// <summary>
    /// Creates a key/value dict from a url with a query string, e.g. "http://127.0.0.1?key=value".
    /// </summary>
    /// <param name="url">Url string. See Application.absoluteURL.</param>
    /// <returns>Dictionary of parsed query string keys and values.</returns>
    static public Dictionary<string, string> GetConnectionParameters(string url)
    {
        var output  = new Dictionary<string, string>();

        if (url != null && url.Length > 0)
        {
            var splitUrl = url.Split('?');
            if (splitUrl.Length == 2)
            {
                var queryString = splitUrl[1];
                var paramsCollection = HttpUtility.ParseQueryString(queryString);

                foreach (var key in paramsCollection.AllKeys)
                {
                    if (key != null && key.Length > 0)
                    {
                        output[key] = paramsCollection[key];
                    }
                }
            }
        }

        return output;
    }

    /// <summary>
    /// Get a valid server hostname from query parameters.
    /// </summary>
    /// <param name="queryParams">See GetConnectionParameters().</param>
    /// <returns>Hostname string. Returns null if the input does not contain a valid hostname.</returns>
    public static string? GetServerHostname(Dictionary<string, string> queryParams)
    {
        if (queryParams == null) return null;

        if (queryParams.TryGetValue("server_hostname", out string serverHostnameString))
        {
            if (Uri.CheckHostName(serverHostnameString) != UriHostNameType.Unknown)
            {
                return serverHostnameString;
            }
            else
            {
                Debug.LogError($"Invalid server_hostname: '{serverHostnameString}'");
            }
        }

        return null;
    }

    /// <summary>
    /// Try to parse a port string.
    /// Checks if the port can be parsed into an integer between 0 and 65535 inclusively.
    /// </summary>
    /// <param name="portString">String to parse.</param>
    /// <param name="result">Output port.</param>
    /// <returns>True if the string could be parsed.</returns>
    public static bool TryParsePortString(string portString, out int result)
    {
        result = 0;
        if (int.TryParse(portString, out int port) &&
            port >= 0 &&
            port <= 65535)
        {
            result = port;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get a valid server port range from query parameters.
    /// The 'server_port_range' parameter defines two ports, e.g. "server_port_range=2222-3333".
    /// The 'server_port' defines a single port, e.g. "server_port=2222".
    /// If both parameters are defined, 'server_port_range' has priority over 'server_port'.
    /// </summary>
    /// <param name="queryParams">See GetConnectionParameters().</param>
    /// <returns>Port range. Always in increasing order. Returns null if the input does not contain a valid port.</returns>
    public static (int startPort, int endPort)? GetServerPortRange(Dictionary<string, string> queryParams)
    {
        if (queryParams == null) return null;

        // Parse "server_port_range".
        if (queryParams.TryGetValue("server_port_range", out string serverPortRangeString))
        {
            string[] parts = serverPortRangeString.Split('-');
            if (parts.Length == 2 &&
                TryParsePortString(parts[0], out int startPort) &&
                TryParsePortString(parts[1], out int endPort))
            {
                if (startPort <= endPort)
                {
                    return (startPort, endPort);
                }
                else
                {
                    return (endPort, startPort);
                }
            }
            else
            {
                Debug.LogError($"Invalid server_port_range: '{serverPortRangeString}'. Expected format: 'server_port_range=2222-3333'.");
            }
        }
        // Parse "server_port".
        else if (queryParams.TryGetValue("server_port", out string serverPortString))
        {
            if (TryParsePortString(serverPortString, out int port))
            {
                return (port, port);
            }
            else
            {
                Debug.LogError($"Invalid server_port: '{serverPortString}'. Expected format: 'server_port=2222'.");
            }
        }

        return null;
    }
}
