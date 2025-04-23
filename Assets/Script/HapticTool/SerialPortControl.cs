using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEngine;

public class SerialPortControl : MonoBehaviour
{
    public string receivedData;
    public string sendData;

    private SerialPort serialPort;
    private Thread threadReceive;

    // 接收时间记录
    private long lastReceviceTime = 0;
    public float receiveFps = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(sendData))
        {
            SendData(sendData);
            sendData = null;
        }
    }

    public void SendData(string data)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.WriteLine(data); // 发送数据
            Debug.Log("【SerialPortControl】Sent to serial port: " + data);
        }
    }

    private void ReceiveData()
    {
        StringBuilder tempData = new StringBuilder();
        while (true)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string data = serialPort.ReadExisting(); // Read the data in the buffer
                if (string.IsNullOrEmpty(data))
                {
                    continue;
                }
                else
                {
                    tempData.Append(data);
                }

                if (data.Contains("\r\n"))
                {
                    receivedData = tempData.ToString();
                    Debug.Log($"【SerialPortControl】Received from serial port: length:{receivedData.Length} data:{receivedData}");
                    tempData.Clear();

                    long nowTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    receiveFps = (float)(1000.0 / (nowTime - lastReceviceTime));
                    lastReceviceTime = nowTime;
                }
            }
        }
    }

    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool OpenSerialPorts(string portName, int baudRate)
    {
        try
        {
            // 确保串口不是已经打开的状态
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            // 创建新的串口连接
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            threadReceive = new Thread(ReceiveData);
            threadReceive.IsBackground = true;
            threadReceive.Start();
            Debug.Log("【SerialPortControl】Serial port opened successfully.");
            Debug.Log($"【SerialPortControl】 Read Buffer Size: {serialPort.ReadBufferSize} bytes");
            Debug.Log($"【SerialPortControl】 Write Buffer Size: {serialPort.WriteBufferSize} bytes");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("【SerialPortControl】Error opening serial port: " + ex.Message);
            return false;
        }
    }

    void OnDisable()
    {
        CloseSerialPort();
    }

    public void CloseSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("【SerialPortControl】Serial port closed.");
        }

        if (threadReceive != null)
        {
            threadReceive.Abort();
            threadReceive.Interrupt();
            threadReceive = null;
        }
    }
}
