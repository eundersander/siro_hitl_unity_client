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
    /// Get a valid server port from query parameters.
    /// </summary>
    /// <param name="queryParams">See GetConnectionParameters().</param>
    /// <returns>Port int. Returns null if the input does not contain a valid port.</returns>
    public static int? GetServerPort(Dictionary<string, string> queryParams)
    {
        if (queryParams == null) return null;

        if (queryParams.TryGetValue("server_port", out string serverPortString))
        {
            if (int.TryParse(serverPortString, out int port) &&
                port >= 0 &&
                port <= 65535)
            {
                return port;
            }
            else
            {
                Debug.LogError($"Invalid server_port: '{serverPortString}'");
            }
        }

        return null;
    }
}
