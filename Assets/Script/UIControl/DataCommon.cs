using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataCommon : MonoBehaviour
{
    public TMP_InputField input_ControlBoxIp;
    public TMP_InputField input_URControlIp;
    public TMP_InputField input_GripperSensorIp;
    public TMP_InputField input_ForceSensorIp;
    public TMP_InputField input_DetectIp;

    public Button btn_ControlBoxConn;
    public Button btn_URControlConn;
    public Button btn_GripperSensorConn;
    public Button btn_ForceSensorConn;
    public Button btn_DetectConn;

    private GameObject SocketGameObject;

    private Color btnColorGreen = new Color(0.5f, 1f, 0.5f);
    private Color btnColorRed = new Color(1f, 0.5f, 0.5f);

    void Start()
    {
        btn_ControlBoxConn.onClick.AddListener(ControlBoxConn);
        btn_URControlConn.onClick.AddListener(URControlConn);
        btn_GripperSensorConn.onClick.AddListener(GripperSensorConn);
        btn_ForceSensorConn.onClick.AddListener(ForceSensorConn);
        btn_DetectConn.onClick.AddListener(DetectConn);

        SocketGameObject = GameObject.Find("Socket");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ControlBoxConn() 
    {
        ControlBoxTcpSocketClient client = SocketGameObject.GetComponent<ControlBoxTcpSocketClient>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            client.SocketQuit();
            // 更改按钮的背景文本和颜色
            btn_ControlBoxConn.GetComponentInChildren<TMP_Text>().text = "Connect";
            btn_ControlBoxConn.GetComponent<Image>().color = btnColorGreen;
        }
        else
        {
            client.ip = input_ControlBoxIp.text;
            client.TcpConnectToServer();
            // 更改按钮的背景文本和颜色
            btn_ControlBoxConn.GetComponentInChildren<TMP_Text>().text = "Close";
            btn_ControlBoxConn.GetComponent<Image>().color = btnColorRed;
        }
    }

    private void URControlConn()
    {
        URControlUdpSocketClient client = SocketGameObject.GetComponent<URControlUdpSocketClient>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            client.SocketQuit();
            // 更改按钮的背景文本和颜色
            btn_URControlConn.GetComponentInChildren<TMP_Text>().text = "Connect";
            btn_URControlConn.GetComponent<Image>().color = btnColorGreen;
        }
        else
        {
            client.ip = input_URControlIp.text;
            client.UdpConnectToServer();
            // 更改按钮的背景文本和颜色
            btn_URControlConn.GetComponentInChildren<TMP_Text>().text = "Close";
            btn_URControlConn.GetComponent<Image>().color = btnColorRed;
        }
    }

    private void GripperSensorConn()
    {
        GripperUdpSocketClient client = SocketGameObject.GetComponent<GripperUdpSocketClient>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            client.SocketQuit();
            // 更改按钮的背景文本和颜色
            btn_GripperSensorConn.GetComponentInChildren<TMP_Text>().text = "Connect";
            btn_GripperSensorConn.GetComponent<Image>().color = btnColorGreen;
        }
        else
        {
            client.ip = input_GripperSensorIp.text;
            client.UdpConnectToServer();
            // 更改按钮的背景文本和颜色
            btn_GripperSensorConn.GetComponentInChildren<TMP_Text>().text = "Close";
            btn_GripperSensorConn.GetComponent<Image>().color = btnColorRed;
        }
    }

    private void ForceSensorConn()
    {
        ForceSensorUdpSocketServer client = SocketGameObject.GetComponent<ForceSensorUdpSocketServer>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            client.SocketQuit();
            // 更改按钮的背景文本和颜色
            btn_ForceSensorConn.GetComponentInChildren<TMP_Text>().text = "Connect";
            btn_ForceSensorConn.GetComponent<Image>().color = btnColorGreen;
        }
        else
        {
            client.ip = input_ForceSensorIp.text;
            client.UdpServerBuildConnect();
            // 更改按钮的背景文本和颜色
            btn_ForceSensorConn.GetComponentInChildren<TMP_Text>().text = "Close";
            btn_ForceSensorConn.GetComponent<Image>().color = btnColorRed;
        }
    }

    private void DetectConn()
    {
        ObjectDetectTcpSocketClient client = SocketGameObject.GetComponent<ObjectDetectTcpSocketClient>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            client.SocketQuit();
            // 更改按钮的背景文本和颜色
            btn_DetectConn.GetComponentInChildren<TMP_Text>().text = "Connect";
            btn_DetectConn.GetComponent<Image>().color = btnColorGreen;
        }
        else
        {
            client.ip = input_DetectIp.text;
            client.TcpConnectToServer();
            // 更改按钮的背景文本和颜色
            btn_DetectConn.GetComponentInChildren<TMP_Text>().text = "Close";
            btn_DetectConn.GetComponent<Image>().color = btnColorRed;
        }
    }
}
