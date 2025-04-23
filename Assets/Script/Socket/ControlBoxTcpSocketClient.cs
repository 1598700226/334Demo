using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System;

public class ControlBoxTcpSocketClient : MonoBehaviour
{
    public string ip = "127.0.0.1";             //ip地址
    public int port = 5000;                     //端口值
    public byte[] sendData = null;              //用以发送的信息

    private TcpClient tcpClient;
    private Thread threadReceive;
    private int reveiceMaxNum = 1024;
    private Thread threadSend;
    public bool isConnected = false;

    /// <summary>
    /// 手控器相关
    /// </summary>
    private bool isInit = false;                        // 是否位置初始化
    private Vector3 lastPostion = Vector3.zero;         // 接收到手控器上次位置
    private Vector3 nowPostion = Vector3.zero;          // 接收到手控器当前位置
    public Vector3 positionIncrement = Vector3.zero;    // 位置增量
    public bool virtualArm_used = false;                // 虚拟机械臂数据是否使用
    private float gripperLastValue = 0.0f;                     // 夹爪
    public float gripperNowValue = 0.0f;                       // 夹爪
    public float gripperIncrement = 0.0f;                      // 夹爪增量

    void Start()
    {

    }

    // Update is called once per frame
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
/*                    receiveData = Encoding.UTF8.GetString(data, 0, receiveLength);
                    Debug.Log("【ControlBoxTcpSocketClient】ReceiveData msg：" + receiveData);
                    // 接收手控器的数据
                    string byteString = "Byte Array Values: ";
                    foreach (byte b in data)
                    {
                        byteString += b.ToString() + " ";
                        Debug.Log("【ControlBoxTcpSocketClient】ReceiveData byteString：" + receiveData);
                    }*/
                    
                    for (int i = 0; i <= 96; i++)
                    {
                        if (data[i] == 0xAA && data[i + 1] == 0xBB && data[i + 2] == 0xEE && data[i + 3] == 0xEE)
                        {
                            byte[] dataBuff1 = new byte[] { data[i + 4], data[i + 5], data[i + 6], data[i + 7] };
                            byte[] dataBuff2 = new byte[] { data[i + 8], data[i + 9], data[i + 10], data[i + 11] };
                            byte[] dataBuff3 = new byte[] { data[i + 12], data[i + 13], data[i + 14], data[i + 15] };
                            byte[] dataBuff4 = new byte[] { data[i + 16], data[i + 17], data[i + 18], data[i + 19] };
                            byte[] dataBuff5 = new byte[] { data[i + 20], data[i + 21], data[i + 22], data[i + 23] };
                            byte[] dataBuff6 = new byte[] { data[i + 24], data[i + 25], data[i + 26], data[i + 27] };
                            byte[] dataBuff7 = new byte[] { data[i + 28], data[i + 29], data[i + 30], data[i + 31] };

                            float t_x = BitConverter.ToSingle(dataBuff1, 0);
                            float t_y = BitConverter.ToSingle(dataBuff2, 0);
                            float t_z = BitConverter.ToSingle(dataBuff3, 0);
                            float r_x = BitConverter.ToSingle(dataBuff4, 0);
                            float r_y = BitConverter.ToSingle(dataBuff5, 0);
                            float r_z = BitConverter.ToSingle(dataBuff6, 0);
                            float gripper = BitConverter.ToSingle(dataBuff7, 0);
                            Debug.Log("【ControlBoxTcpSocketClient】ReceiveData tx" + t_x + " ty:" + t_y + " tz:" + t_z + " rx:" + r_x + " ry:" + r_y + "rz:" + r_z + "gripper:" + gripper);

                            // 更新虚拟机械臂末端点的位置
                            if (isInit)
                            {
                                nowPostion = new Vector3(t_x, t_y, t_z);
                                positionIncrement = AxisConvert(nowPostion - lastPostion);
                                gripperNowValue = gripper;
                                gripperIncrement = gripperNowValue - gripperLastValue;

                                if (virtualArm_used) {
                                    lastPostion = nowPostion;
                                    gripperLastValue = gripperNowValue;
                                    virtualArm_used = false;
                                }
                            }
                            else
                            {
                                isInit = true;
                                nowPostion = new Vector3(t_x, t_y, t_z);
                                lastPostion = new Vector3(t_x, t_y, t_z);
                                gripperNowValue = gripper;
                                gripperLastValue = gripper;
                            }
                        }
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
        if (tcpClient != null) {
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

    /**
     * 转换为虚拟机械臂末端点实际移动的坐标
     */
    private Vector3 AxisConvert(Vector3 posIncrement) {
        // 手控器单位是cm，转换成m
        return new Vector3(posIncrement.z / 100, posIncrement.y / 100, posIncrement.x / 100);
    }
}
