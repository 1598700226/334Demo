using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HapticToolControl : MonoBehaviour
{
    public Button btn_serialport_search;
    public TMP_Dropdown dropdowm_portname;
    public TMP_Dropdown dropdown_baudrate;
    public Button btn_serialport_open;
    public Button btn_serialport_close;
    public TMP_Text receiveTextShow;
    public TMP_Text HapticFpsText;
    public Image[] HapticImageArrays;
    public float[] HapticValues = new float[9];
    public float SendDataDelayTime = 0.1f; // 单位秒


    SerialPortControl serialPortControl;
    int requireDataLength = 5; // 固定长度
    Color LerpColor1 = new Color(0.9f, 1, 0.9f);
    Color LerpColor2 = new Color(0.1f, 1, 0.1f);
    float maxHapticValues = 1.5f;       // 触觉传感器最大的值;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            serialPortControl.sendData = "9\r\n";
        }

        string receivedData = serialPortControl.receivedData;
        string fps = serialPortControl.receiveFps.ToString("F2");
        if (!string.IsNullOrEmpty(receivedData))
        {
            HapticFpsText.text = fps;

            string[] data = receivedData.Split(" ");
            for (int i = 0; i < 9; i++)
            {
                HapticValues[i] = float.Parse(data[i]);
                Color color = Color.Lerp(LerpColor1, LerpColor2, HapticValues[i] / maxHapticValues);
                HapticImageArrays[i].color = color;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(HapticValues[0].ToString("F2")).Append("  ")
                .Append(HapticValues[1].ToString("F2")).Append("  ")
                .Append(HapticValues[2].ToString("F2")).Append("  ").Append("\r\n")
                .Append(HapticValues[3].ToString("F2")).Append("  ")
                .Append(HapticValues[4].ToString("F2")).Append("  ")
                .Append(HapticValues[5].ToString("F2")).Append("  ").Append("\r\n")
                .Append(HapticValues[6].ToString("F2")).Append("  ")
                .Append(HapticValues[7].ToString("F2")).Append("  ")
                .Append(HapticValues[8].ToString("F2")).Append("  ");
            receiveTextShow.text = sb.ToString();

            serialPortControl.receivedData = "";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        serialPortControl = GameObject.Find("HapticToolControl").GetComponent<SerialPortControl>();
        btn_serialport_search.onClick.AddListener(ControlBoxSerialportSearch);
        btn_serialport_open.onClick.AddListener(ControlBoxSerialportOpen);
        btn_serialport_close.onClick.AddListener(ControlBoxSerialportClose);
    }

    void ControlBoxSerialportSearch()
    {
        serialPortControl = GameObject.Find("HapticToolControl").GetComponent<SerialPortControl>();
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
            serialPortControl = GameObject.Find("HapticToolControl").GetComponent<SerialPortControl>();
            string portname = dropdowm_portname.options[dropdowm_portname.value].text;
            int baudrate = int.Parse(dropdown_baudrate.options[dropdown_baudrate.value].text);
            serialPortControl.OpenSerialPorts(portname, baudrate);
            StartCoroutine(DelaySendDataAction(SendDataDelayTime));
        }
        catch (Exception ex)
        {
            Debug.Log("【DataCommon】HapticData SerialportOpen error\n" + ex);
        }
    }

    void ControlBoxSerialportClose()
    {
        serialPortControl = GameObject.Find("HapticToolControl").GetComponent<SerialPortControl>();
        serialPortControl.CloseSerialPort();
        StopCoroutine(DelaySendDataAction(SendDataDelayTime));
    }

    IEnumerator DelaySendDataAction(float delay)
    {
        while (true) // 循环调用
        {
            serialPortControl.sendData = "9\r\n";
            // 等待指定的时间
            yield return new WaitForSeconds(delay);
        }
    }
}
