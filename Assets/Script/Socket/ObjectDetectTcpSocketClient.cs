using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ObjectDetectTcpSocketClient : MonoBehaviour
{
    public string ip = "127.0.0.1";             //ip地址
    public int port = 12345;                     //端口值
    public byte[] sendData = null;              //用以发送的信息

    private TcpClient tcpClient;
    private Thread threadReceive;
    private int reveiceMaxNum = 1024;
    private Thread threadSend;
    public bool isConnected = false;

    /// <summary>
    /// 手控器相关
    /// </summary>
    public bool isInit = false;                        // 是否位置初始化
    private Vector3 initPostion = Vector3.zero;         // 接收目标的初始位置
    private Vector3 nowPostion = Vector3.zero;          // 接收目标的当前位置
    public Vector3 positionIncrement = Vector3.zero;    // 位置增量

    [System.Serializable]
    public class DetectPoint
    {
        public float x;
        public float y;
        public float z;
    }

    void Start()
    {
        isInit = false;
    }

    void Update()
    {
        
    }

    public bool TcpConnectToServer()
    {
        SocketQuit();

        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(ip), port);

            threadReceive = new Thread(ReceiveData);
            threadReceive.IsBackground = true;
            threadReceive.Start();

            threadSend = new Thread(SendData);
            threadSend.IsBackground = true;
            threadSend.Start();

            Debug.Log("【ControlBoxTcpSocketClient】connnect success...");
            isConnected = true;
            return isConnected;
        }
        catch (System.Exception e)
        {
            Debug.LogError("【ControlBoxTcpSocketClient】connnect fail!!! ex：" + e.ToString());
            isConnected = false;
            return isConnected;
        }
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
        if (tcpClient != null)
        {
            tcpClient.Close();
            tcpClient = null;
        }
        isConnected = false;
    }

    private void OnDisable()
    {
        SocketQuit();
    }

    private void OnApplicationQuit()
    {
        SocketQuit();
    }

    void SendData()
    {
        NetworkStream stream = tcpClient.GetStream();
        try
        {
            while (true)
            {
                if (tcpClient != null && isConnected)
                {
                    if (sendData != null)
                    {
                        stream.Write(sendData, 0, sendData.Length);
                        sendData = null;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            stream.Close();
            Debug.Log("【ControlBoxTcpSocketClient】sendData ex：" + e.ToString());
        }
    }

    void ReceiveData()
    {
        NetworkStream stream = tcpClient.GetStream();
        try
        {
            while (true)
            {
                if (tcpClient != null && isConnected)
                {
                    byte[] data = new byte[reveiceMaxNum];
                    int receiveLength = stream.Read(data, 0, data.Length);
                    string receiveData = Encoding.UTF8.GetString(data, 0, receiveLength);
                    DetectPoint detectPoint = JsonUtility.FromJson<DetectPoint>(receiveData);
                    Debug.Log("Object Detect:" + receiveData);
                    // 过滤脏数据
                    if (detectPoint == null)
                    {
                        continue;
                    }
                    if (detectPoint.x == 0 && detectPoint.y == 0 && detectPoint.z == 0)
                    {
                        continue;
                    }

                    if (isInit)
                    {
                        nowPostion = new Vector3(detectPoint.x, detectPoint.y, detectPoint.z);
                        positionIncrement = nowPostion - initPostion;
                    }
                    else
                    {
                        initPostion = new Vector3(detectPoint.x, detectPoint.y, detectPoint.z);
                        positionIncrement = Vector3.zero;
                        isInit = true;
                    }
                }
            }

        }
        catch (System.Exception e)
        {
            stream.Close();
            Debug.Log("【ControlBoxTcpSocketClient】ReceiveData ex：" + e.ToString());
        }
    }
}
