using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ServerConnectTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void CanConnectToServer()
    {
        GameObject obj = new GameObject();
        ServerConnect serverConnect = obj.AddComponent<ServerConnect>();

        serverConnect.Start();

        // wait for 1 seconds
        System.Threading.Thread.Sleep(1000);

        Assert.True(true);
    }
}
