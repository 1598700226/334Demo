using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandControlToolControl : MonoBehaviour
{
    public Button btn_serialport_search;
    public TMP_Dropdown dropdowm_portname;
    public TMP_Dropdown dropdown_baudrate;
    public Button btn_serialport_open;
    public Button btn_serialport_close;
    public TMP_Text receiveTextShow;
    public TMP_Text HandControlFpsText;
    SerialPortControl serialPortControl;

    public bool isOpen = false;
    public int controlMode = 0;
    public Vector3 positionInc = new Vector3(0, 0, 0);
    public Vector3 rotationInc = new Vector3(0, 0, 0);
    public float gripperDis = 0.0f;

    public GameObject catchObject = null;

    private void Update()
    {
        string receivedData = serialPortControl.receivedData;
        string fps = serialPortControl.receiveFps.ToString("F2");
        if (!string.IsNullOrEmpty(receivedData))
        {
            HandControlFpsText.text = fps;
            string[] data = receivedData.Split(" ");
            if (data.Length < 10)
                return;
            if (data[0] != "HC" && data[data.Length - 1] != "END")
                return;

            controlMode = int.Parse(data[1]);
            positionInc.x = -float.Parse(data[2]);
            positionInc.y = float.Parse(data[4]);
            positionInc.z = -float.Parse(data[3]);
            rotationInc.x = float.Parse(data[5]);
            rotationInc.y = float.Parse(data[7]);
            rotationInc.z = float.Parse(data[6]);
            gripperDis = float.Parse(data[8]);

            StringBuilder sb = new StringBuilder();
            sb.Append(data[0]).Append("  ")
                .Append(data[1]).Append("  ").Append("\r\n")
                .Append(data[2]).Append("  ")
                .Append(data[3]).Append("  ")
                .Append(data[4]).Append("  ").Append("\r\n")
                .Append(data[5]).Append("  ")
                .Append(data[6]).Append("  ")
                .Append(data[7]).Append("  ").Append("\r\n")
                .Append(data[8]).Append("  ")
                .Append(data[9]).Append("  ");
            receiveTextShow.text = sb.ToString();
            serialPortControl.receivedData = "";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        serialPortControl = GameObject.Find("HandControl").GetComponent<SerialPortControl>();
        btn_serialport_search.onClick.AddListener(ControlBoxSerialportSearch);
        btn_serialport_open.onClick.AddListener(ControlBoxSerialportOpen);
        btn_serialport_close.onClick.AddListener(ControlBoxSerialportClose);
    }

    void ControlBoxSerialportSearch()
    {
        serialPortControl = GameObject.Find("HandControl").GetComponent<SerialPortControl>();
        string[] availablePorts = serialPortControl.GetAvailablePorts();
        dropdowm_portname.ClearOptions();
        dropdowm_portname.AddOptions(new List<string>(availablePorts));
        dropdowm_portname.value = 0;
        dropdowm_portname.RefreshShownValue();
    }

    void ControlBoxSerialportOpen()
    {
        try
        {
            serialPortControl = GameObject.Find("HandControl").GetComponent<SerialPortControl>();
            string portname = dropdowm_portname.options[dropdowm_portname.value].text;
            int baudrate = int.Parse(dropdown_baudrate.options[dropdown_baudrate.value].text);
            serialPortControl.OpenSerialPorts(portname, baudrate);
            isOpen = true;
        }
        catch (Exception ex)
        {
            Debug.Log("¡¾DataCommon¡¿HandControl SerialportOpen error\n" + ex);
            isOpen = false;
        }
    }

    void ControlBoxSerialportClose()
    {
        serialPortControl = GameObject.Find("HandControl").GetComponent<SerialPortControl>();
        serialPortControl.CloseSerialPort();
        isOpen = false;
    }

    public void SendCatchObject()
    {
        if(catchObject != null)
            serialPortControl.sendData = "HC STATUS0 CATCH1 OBJECT0 END\r\n";
    }

    public void SendReleaseObject()
    {
        if (catchObject == null)
            serialPortControl.sendData = "HC STATUS0 CATCH0 OBJECT0 END\r\n";
    }
}
