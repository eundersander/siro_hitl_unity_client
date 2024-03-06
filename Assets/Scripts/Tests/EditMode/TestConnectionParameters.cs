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
        public void TestTryParsePortString()
        {
            int port;
            bool result;

            // Valid cases.
            LogAssert.ignoreFailingMessages = false;

            // Canonical case.
            result = ConnectionParameters.TryParsePortString("1111", out port);
            Assert.AreEqual(result, true);
            Assert.AreEqual(port, 1111);

            // Null string.
            result = ConnectionParameters.TryParsePortString(null, out port);
            Assert.AreEqual(result, false);

            // Empty string.
            result = ConnectionParameters.TryParsePortString("", out port);
            Assert.AreEqual(result, false);

            // Negative port.
            result = ConnectionParameters.TryParsePortString("-55", out port);
            Assert.AreEqual(result, false);

            // Port > 65535.
            result = ConnectionParameters.TryParsePortString("100000", out port);
            Assert.AreEqual(result, false);
        }

        [Test]
        public void TestGetServerPortRange()
        {
            string url;
            (int portStart, int portEnd)? portRange;
            Dictionary<string, string> parameters;

            // Valid cases.
            LogAssert.ignoreFailingMessages = false;

            // Canonical server_port.
            url = "test?server_hostname=HOST&server_port=2222&test=true";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange.Value.portStart, 2222);
            Assert.AreEqual(portRange.Value.portEnd, 2222);

            // Canonical server_port_range.
            url = "test?server_hostname=HOST&server_port_range=2222-3333&test=true";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange.Value.portStart, 2222);
            Assert.AreEqual(portRange.Value.portEnd, 3333);

            // Both server_port and server_port_range. 'server_port_range' has priority.
            url = "test?server_port_range=2222-3333&server_port=4444";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange.Value.portStart, 2222);
            Assert.AreEqual(portRange.Value.portEnd, 3333);

            // Decreasing port range. Output is expected to be in increasing order.
            url = "test?server_port_range=3333-2222";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange.Value.portStart, 2222);
            Assert.AreEqual(portRange.Value.portEnd, 3333);

            // No port.
            url = "test?test=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // No parameter.
            url = "test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // Null input
            portRange = ConnectionParameters.GetServerPortRange(null);
            Assert.AreEqual(portRange, null);

            // Invalid cases.
            LogAssert.ignoreFailingMessages = true;

            // Invalid port.
            url = "test?server_port=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // Invalid port range.
            url = "test?server_port_range=test";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // '-' port range.
            url = "test?server_port_range=-";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);
            
            // Invalid port range portion.
            url = "test?server_port_range=test-test2";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // Negative port.
            url = "test?server_port=-100";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            // Port > 65535.
            url = "test?server_port=1000000";
            parameters = ConnectionParameters.GetConnectionParameters(url);
            portRange = ConnectionParameters.GetServerPortRange(parameters);
            Assert.AreEqual(portRange, null);

            LogAssert.ignoreFailingMessages = false;
        }
    }
}
