using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Text;

public class URControlUdpSocketClient : MonoBehaviour
{
    public int port = 1500;
    public string ip = "127.0.0.1";

    private UdpClient udpServer;
    private IPEndPoint endPoint;
    private Thread threadReceive;
    private Thread threadSend;
    public bool isConnected = false;

    public string sendData = null;
    public string receiveData = null;
    public byte[] sendBytes = null;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool UdpConnectToServer()
    {
        SocketQuit();

        try
        {
            udpServer = new UdpClient();
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            threadReceive = new Thread(ReceiveData);
            threadReceive.Start();
            threadSend = new Thread(SendData);
            threadSend.Start();
            Debug.Log("【URControlUdpSocketClient】connnect success...");
            isConnected = true;
            return isConnected;
        }
        catch (System.Exception e)
        {
            Debug.LogError("【URControlUdpSocketClient】connnect fail!!! ex：" + e.ToString());
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

/*            // 判断缓冲区是否有数据
            if (udpServer.Available > 0)
            {
                try
                {
                    //  Grab the data.
                    byte[] data = udpServer.Receive(ref endPoint);
                    receiveData = Encoding.UTF8.GetString(data);
                    Debug.Log("【URControlUdpSocketClient】ReceiveData message：" + receiveData);
                }
                catch (System.Exception e)
                {
                    Debug.Log("【URControlUdpSocketClient】ReceiveData ex：" + e.ToString());
                }
            }*/
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
                    byte[] data = Encoding.UTF8.GetBytes(sendData);
                    udpServer.Send(data, data.Length, endPoint);
                    sendData = null;
                }
                catch (System.Exception e)
                {
                    Debug.Log("【URControlUdpSocketClient】SendData ex：" + e.ToString());
                }
            }

            if (sendBytes != null && sendBytes.Length == 6 * 8) {
                try
                {
                    udpServer.Send(sendBytes, sendBytes.Length, endPoint);
                    sendBytes = null;
                }
                catch (System.Exception e)
                {
                    Debug.Log("【URControlUdpSocketClient】SendData ex：" + e.ToString());
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
        if (threadReceive != null) {
            threadReceive.Abort();
            threadReceive.Interrupt();
            threadReceive = null;
        }
        if (threadSend != null) {
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
