using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System;

public class ForceSensorUdpSocketServer : MonoBehaviour
{
    public int port = 2000;
    public string ip = "192.168.1.106";

    private UdpClient udpServer;
    private IPEndPoint endPoint;
    private Thread threadReceive;
    private Thread threadSend;
    public bool isConnected = false;

    public string sendData = null;
    public string receiveData = null;
    public byte[] sendBytes = null;
    public byte[] receiveBytes = null;
    
    // 接收时间记录
    private long lastReceviceTime = 0;
    public float receiveFps = 0;

    // Start is called before the first frame update
    void Start()
    {
/*        udpServer = new UdpClient(port); // Choose an available port
        endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        threadReceive = new Thread(ReceiveData);
        threadReceive.Start();
        threadSend = new Thread(SendData);
        threadSend.Start();*/
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool UdpServerBuildConnect() 
    {
        SocketQuit();

        try
        {
            udpServer = new UdpClient(port); // Choose an available port
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            threadReceive = new Thread(ReceiveData);
            threadReceive.Start();
            threadSend = new Thread(SendData);
            threadSend.Start();
            Debug.Log("【ForceSensorUdpSocketServer】build Server success.");
            isConnected = true;
            return isConnected;
        }
        catch (System.Exception e)
        {
            Debug.LogError("【ForceSensorUdpSocketServer】connnect fail!!! ex：" + e.ToString());
            isConnected = false;
            return isConnected;
        }
    }

    private void ReceiveData()
    {
        while (true)
        {
            if (!isConnected)
                continue;
            // 判断缓冲区是否有数据
            if (udpServer.Available > 0)
            {
                try
                {
                    //  Grab the data.
                    byte[] data = udpServer.Receive(ref endPoint);
                    receiveBytes = data;

                    long nowTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    receiveFps = (float)(1000.0 / (nowTime - lastReceviceTime));
                    lastReceviceTime = nowTime;
                }
                catch (System.Exception e)
                {
                    Debug.Log("【ForceSensorUdpSocketServer】ReceiveData ex：" + e.ToString());
                }
            }
        }
    }

    private void SendData() 
    {
        while (true)
        {
            if (!isConnected)
                continue;
            // 判断缓冲区是否有数据
            if (!string.IsNullOrEmpty(sendData))
            {
                try
                {
                    //  Grab the data.
                    byte[] data = Encoding.UTF8.GetBytes(sendData);
                    udpServer.Send(data, data.Length, endPoint);
                }
                catch (System.Exception e)
                {
                    Debug.Log("【UdpSocketServer】SendData ex：" + e.ToString());
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        SocketQuit();
    }

    void OnDestroy()
    {
        SocketQuit();
    }

    public void SocketQuit()
    {
        if (threadReceive != null)
        {
            threadReceive.Abort();
            threadReceive.Interrupt();
            threadReceive = null;
        }
        if (threadSend != null)
        {
            threadSend.Abort();
            threadSend.Interrupt();
            threadSend = null;
        }
        if (udpServer != null)
        {
            udpServer.Close();
            udpServer = null;
        }
        isConnected = false;
    }
}
