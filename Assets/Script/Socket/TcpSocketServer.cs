using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;

public class TcpSocketServer : MonoBehaviour
{
    public string ip = "127.0.0.1";             //ip地址
    public int port = 5001;                     //端口值
    public string sendData = null;              //用以发送的信息
    public string receiveData = null;           //用以接收的信息

    private TcpListener tcpListener;
    private TcpClient connectedClient;
    private Thread listenerThread;
    private Thread threadReceive;
    private int reveiceMaxNum = 1024;
    private Thread threadSend;

    void Start()
    {
        // 开启监听
        tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        tcpListener.Start();

        // 开启等待客户端加入线程
        listenerThread = new Thread(ListenForClients);
        listenerThread.Start();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 等待客户端的连接 并且创建与之通信的Socket
    /// </summary>
    void ListenForClients()
    {
        try
        {
            while (true)
            {
                TcpClient newConnectedClient = tcpListener.AcceptTcpClient();
                if (connectedClient != null)
                {
                    connectedClient.Close();
                }
                connectedClient = newConnectedClient;

                //开启一个新线程，执行接收消息方法
                if (threadReceive != null && threadReceive.IsAlive)
                {
                    threadReceive.Abort();
                }
                threadReceive = new Thread(ReceiveData);
                threadReceive.IsBackground = true;
                threadReceive.Start();

                //开启一个新线程，执行发送消息方法
                if (threadSend != null && threadSend.IsAlive)
                {
                    threadSend.Abort();
                }
                threadSend = new Thread(SendData);
                threadSend.IsBackground = true;
                threadSend.Start();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("【TcpSocketServer】ListenForClients ex：" + e.ToString());
        }
    }

    /// <summary>
    /// 服务器端不停的接收客户端发来的消息
    /// </summary>
    void ReceiveData()
    {
        NetworkStream stream = connectedClient.GetStream();
        try
        {
            while (true)
            {
                if (connectedClient != null)
                {
                    byte[] data = new byte[reveiceMaxNum];
                    int receiveLength = stream.Read(data, 0, data.Length);
                    receiveData = Encoding.UTF8.GetString(data, 0, receiveLength);
                    Debug.Log("【TcpSocketServer】ReceiveData msg：" + receiveData);
                }
            }
        }
        catch (System.Exception e)
        {
            stream.Close();
            Debug.Log("【TcpSocketServer】ReceiveData ex：" + e.ToString());
        }
    }

    /// <summary>
    /// 服务器端不停的向客户端发送消息
    /// </summary>
    void SendData()
    {
        NetworkStream stream = connectedClient.GetStream();
        try
        {
            while (true)
            {
                if (connectedClient != null)
                {
                    if (!string.IsNullOrEmpty(sendData))
                    {
                        byte[] sendDataBytes = Encoding.UTF8.GetBytes(sendData);
                        stream.Write(sendDataBytes, 0, sendDataBytes.Length);
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            stream.Close();
            Debug.Log("【TcpSocketServer】sendData ex：" + e.ToString());
        }
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
        if (listenerThread != null)
        {
            listenerThread.Abort();
            listenerThread.Interrupt();
        }
        if (connectedClient != null)
        {
            connectedClient.Close();
        }
        if (tcpListener != null)
        {
            tcpListener.Stop();
        }
    }

    private void OnDisable()
    {
        SocketQuit();
    }

    private void OnApplicationQuit()
    {
        SocketQuit();
    }
}
