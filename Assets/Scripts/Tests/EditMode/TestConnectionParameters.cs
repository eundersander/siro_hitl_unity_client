using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Habitat.Tests.EditMode
{
    public class TestConnectionParameters
    {
        [Test]
        public void TestGetConnectionParameters()
        {
            string url;
            Dictionary<string, string> parameters;

            // Canonical case.
            url = "test?varA=true&varB=false";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 2);
            Assert.IsTrue(parameters.ContainsKey("varA"));
            Assert.IsTrue(parameters.ContainsKey("varB"));
            Assert.AreEqual(parameters["varA"], "true");
            Assert.AreEqual(parameters["varB"], "false");

            // No connection parameter.
            url = "test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);

            // Nothing after "?".
            url = "test?";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);

            // "?" only.
            url = "?";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);

            // Multiple "?".
            url = "test?varA=true&varB=false?varC=true";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);

            // Incomplete argument.
            url = "test?varA=true&varB";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 1);
            Assert.AreEqual(parameters["varA"], "true");

            // Incomplete value.
            url = "test?varA=true&varB=";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 2);
            Assert.AreEqual(parameters["varA"], "true");
            Assert.AreEqual(parameters["varB"], "");

            // Incomplete key.
            url = "test?varA=true&=false";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 1);
            Assert.AreEqual(parameters["varA"], "true");

            // Two identical keys.
            url = "test?varA=true&varA=false";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 1);
            Assert.AreEqual(parameters["varA"], "true,false");

            // Incomplete key and parameter.
            url = "test?varA=true&=";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 1);
            Assert.AreEqual(parameters["varA"], "true");

            // Empty string.
            url = "";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);

            // Null string.
            url = null;
            parameters = ConnectionParameters.GetConnectionParameters(url);
            Assert.AreEqual(parameters.Count, 0);
        }

        [Test]
        public void TestGetServerHostname()
        {
            string url;
            string host;
            Dictionary<string, string> parameters;

            // Valid cases.
            LogAssert.ignoreFailingMessages = false;

            // Canonical case.
            url = "test?server_hostname=HOST&server_port=1111&test=true";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, "HOST");

            // No hostname.
            url = "test?test=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, null);

            // No parameter.
            url = "test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, null);

            // IP hostname.
            url = "test?server_hostname=127.0.0.1";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, "127.0.0.1");

            // Null input
            host = ConnectionParameters.GetServerHostname(null);
            Assert.AreEqual(host, null);

            // Invalid cases.
            LogAssert.ignoreFailingMessages = true;

            // Hostname with port. This is considered invalid.
            url = "test?server_hostname=127.0.0.1:1111";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, null);

            // Invalid hostname.
            url = "test?server_hostname=test<>";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            host = ConnectionParameters.GetServerHostname(parameters);
            Assert.AreEqual(host, null);

            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void TestGetServerPort()
        {
            string url;
            int? port;
            Dictionary<string, string> parameters;

            // Valid cases.
            LogAssert.ignoreFailingMessages = false;

            // Canonical case.
            url = "test?server_hostname=HOST&server_port=1111&test=true";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, 1111);

            // No port.
            url = "test?test=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, null);

            // No parameter.
            url = "test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, null);

            // Null input
            port = ConnectionParameters.GetServerPort(null);
            Assert.AreEqual(port, null);

            // Invalid cases.
            LogAssert.ignoreFailingMessages = true;

            // Invalid port.
            url = "test?server_port=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, null);

            // Negative port.
            url = "test?server_port=-100";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, null);

            // Port > 65535.
            url = "test?server_port=1000000";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            port = ConnectionParameters.GetServerPort(parameters);
            Assert.AreEqual(port, null);

            LogAssert.ignoreFailingMessages = false;
        }
    }
}
