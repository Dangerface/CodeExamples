using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

[DefaultExecutionOrder(-9)] 
public class GetTime : MonoBehaviour
{
    public DateTime CurrentTime;
    private GetNet getnetscript;
    private Authentication authscript;
    public bool gotTime = false;
    
    void Update()
    {
        getnetscript = GetComponent<GetNet>();
        authscript = GetComponent<Authentication>();
        
        if(!gotTime && getnetscript.isConnected && authscript.signedIn)
        {
            CurrentTime = GetNetworkTime();
            StaticBaseTime.CurrentDateTime = CurrentTime;
            gotTime = true;
        }
    }

    public static DateTime GetNetworkTime()
    {
        const string ntpServer = "pool.ntp.org";
        var ntpData = new byte[48];
        ntpData[0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

        var addresses = Dns.GetHostEntry(ntpServer).AddressList;
        var ipEndPoint = new IPEndPoint(addresses[0], 123);
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(ipEndPoint);
        socket.Send(ntpData);
        socket.Receive(ntpData);
        socket.Close();

        ulong intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | (ulong)ntpData[43];
        ulong fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | (ulong)ntpData[47];

        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
        var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
        var networkUniDateTime =networkDateTime;
        
        return networkUniDateTime;
        
    }
}