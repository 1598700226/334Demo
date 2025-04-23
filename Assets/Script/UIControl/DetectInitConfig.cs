using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DetectInitConfig : MonoBehaviour
{
    [Header("UI")]
    [Space(10)]
    public TMP_Text txt_posx;
    public TMP_Text txt_posy;
    public TMP_Text txt_posz;
    public TMP_InputField input_posx;
    public TMP_InputField input_posy;
    public TMP_InputField input_posz;
    public Button btn_ResetConfig;

    [Header("GameObject")]
    [Space(10)]
    public GameObject detectModel;
    public GameObject SocketGameObject;
    public GameObject URGameObject;
    public Vector3 initPosition;
    public Quaternion initRotation;

    [Header("设置")]
    [Space(10)]
    public bool isInverseX;     // 默认false
    public bool isInverseY;     // 默认true
    public bool isInverseZ;     // 默认false

    // Start is called before the first frame update
    void Start()
    {
        initPosition.x = detectModel.transform.position.x;
        initPosition.y = detectModel.transform.position.y;
        initPosition.z = detectModel.transform.position.z;
        initRotation = detectModel.transform.rotation;
        input_posx.text = initPosition.x.ToString("F5");
        input_posy.text = initPosition.y.ToString("F5");
        input_posz.text = initPosition.z.ToString("F5");

        btn_ResetConfig.onClick.AddListener(ResetConfig);
    }

    // Update is called once per frame
    void Update()
    {
        txt_posx.text = detectModel.transform.position.x.ToString("F5");
        txt_posy.text = detectModel.transform.position.y.ToString("F5");
        txt_posz.text = detectModel.transform.position.z.ToString("F5");

        ObjectDetectTcpSocketClient client = SocketGameObject.GetComponent<ObjectDetectTcpSocketClient>();
        bool isConnect = client.isConnected;
        if (isConnect)
        {
            VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
            if (virtualURControl.gripper_catch_object != null)
            {

            }
            else
            {
                Vector3 posInc = client.positionIncrement;
                float inc_x = isInverseX ? -posInc.x : posInc.x;
                float inc_y = isInverseY ? -posInc.y : posInc.y;
                float inc_z = isInverseZ ? -posInc.z : posInc.z;
                // 标定板坐标系对应unity坐标系 x-z y-x z-y
                Vector3 nowPos = Vector3.zero;
                nowPos.x = initPosition.x + inc_y / 1000.0f;
                nowPos.y = initPosition.y + inc_z / 1000.0f;
                nowPos.z = initPosition.z + inc_x / 1000.0f;
                detectModel.transform.position = nowPos;
                detectModel.transform.rotation = initRotation;
            }
        }
    }

    private void ResetConfig()
    {
        try
        {
            float x = float.Parse(input_posx.text);
            float y = float.Parse(input_posy.text);
            float z = float.Parse(input_posz.text);
            initPosition = new Vector3(x, y, z);
            detectModel.transform.position = initPosition;
            detectModel.transform.rotation = initRotation;

            ObjectDetectTcpSocketClient client = SocketGameObject.GetComponent<ObjectDetectTcpSocketClient>();
            client.isInit = false;
        }
        catch (Exception e)
        {
            Debug.LogError("重置出现错误:" + e.Message);
        }
    }
}
