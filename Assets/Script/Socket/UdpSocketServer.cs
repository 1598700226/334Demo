using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using System.Threading;
using System;

public class UdpSocketServer : MonoBehaviour
{
    public int port = 5000;

    private UdpClient udpServer;
    private IPEndPoint endPoint;
    private Thread threadReceive;
    private Thread threadSend;

    public static string sendData = null;
    public static string receiveData = null;

    // Start is called before the first frame update
    void Start()
    {
        udpServer = new UdpClient(port); // Choose an available port
        endPoint = new IPEndPoint(IPAddress.Any, port);
        threadReceive = new Thread(ReceiveData);
        threadReceive.Start();
        threadSend = new Thread(SendData);
        threadSend.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ReceiveData()
    {
        while (true)
        {
            // 判断缓冲区是否有数据
            if (udpServer.Available > 0)
            {
                try
                {
                    //  Grab the data.
                    byte[] data = udpServer.Receive(ref endPoint);
/*                    receiveData = Encoding.UTF8.GetString(data);
                    Debug.Log("【UdpSocketServer】ReceiveData message：" + receiveData);*/

                    string byteString = "Byte Array Values: ";
                    foreach (byte b in data)
                    {
                        byteString += b.ToString() + " ";
                    }
                    Debug.Log("【UdpSocketServer】ReceiveData byteString：" + byteString);
                }
                catch (System.Exception e)
                {
                    Debug.Log("【UdpSocketServer】ReceiveData ex：" + e.ToString());
                }
            }
        }
    }

    private void SendData() 
    {
        while (true)
        {
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

    private void SocketQuit()
    {
        if (threadReceive != null)
        {
            threadReceive.Abort();
            threadReceive.Interrupt();
        }
        if (threadSend != null)
        {
            threadSend.Abort();
            threadSend.Interrupt();
        }
        if (udpServer != null)
        {
            udpServer.Close();
        }
    }
}
