using HapticGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualURControl : MonoBehaviour
{

    // 机械臂的部件
    [Header("机械臂的部件")]
    [Space(10)]
    public string arm_1_name = "1";
    public string arm_2_name = "2";
    public string arm_3_name = "3";
    public string arm_4_name = "4";
    public string arm_5_name = "5";
    public string arm_6_name = "6";
    public string gripper_1_name = "gripper_1";
    public string gripper_2_name = "gripper_2";
    public float gripper_keyboard_speed = 0.001f;
    public string end_point_name = "EndPoint";
    GameObject arm_1;
    GameObject arm_2;
    GameObject arm_3;
    GameObject arm_4;
    GameObject arm_5;
    GameObject arm_6;
    GameObject gripper_1;
    GameObject gripper_2;
    GameObject endPoint; // 末端点

    // 机械臂初始状态
    [Header("机械臂初始状态")]
    [Space(10)]
    public Vector3 beginPosition;
    public Quaternion beginRotation;
    public bool isReceiveNewPositon = false;
    public float moveSpeed_newPosition = 0.05f;
    public Vector3 moveTargetPositon;
    public Vector3 newPosition;
    public Quaternion newRotation;

    [Header("机械臂关节初始状态")]
    [Space(10)]
    public bool isBackToBegin = false;
    public bool send_to_actual_UR = false;
    public double arm_1_init_angle = 180.0;
    public bool arm_1_angle_inverse = false;
    public double arm_2_init_angle = -90.0;
    public bool arm_2_angle_inverse = false;
    public double arm_3_init_angle = 115.0;
    public bool arm_3_angle_inverse = false;
    public double arm_4_init_angle = -120.0;
    public bool arm_4_angle_inverse = false;
    public double arm_5_init_angle = 270.0;
    public bool arm_5_angle_inverse = false;
    public double arm_6_init_angle = 180;
    public bool arm_6_angle_inverse = false;

    public double gripper_min_value = double.MaxValue;  // 收到手控器夹爪的最小值数据，用于归一化
    public double gripper_max_value = double.MinValue;  // 收到手控器夹爪的最大值数据，用于归一化
    public double hhrgripper_min_value = double.MaxValue;  // 收到手控器夹爪的最小值数据，用于归一化
    public double hhrgripper_max_value = double.MinValue;  // 收到手控器夹爪的最大值数据，用于归一化
    public double touchgripper_min_value = double.MaxValue;  // 收到手控器夹爪的最小值数据，用于归一化
    public double touchgripper_max_value = double.MinValue;  // 收到手控器夹爪的最大值数据，用于归一化

    // Touch 手控器末端点
    [Header("Touch 手控器末端点")]
    [Space(10)]
    public bool connectTouch = false;
    public GameObject TouchX;
    private Vector3 touch_newPosition = new Vector3();
    private bool touch_isInitPostion = false;
    private Vector3 touch_oldPosition = new Vector3();
    private Vector3 touch_newRotation = new Vector3();
    private bool touch_isInitRotation = false;
    private Vector3 touch_oldRotation = new Vector3();

    // 机械臂夹爪状态和抓到的物体
    public enum GripperStatusEnum
    {
        Enter,
        Exit
    }
    GameObject gripper_1_catch_object = null;
    GameObject gripper_2_catch_object = null;
    public GameObject gripper_catch_object = null;
    public int gripperMoveMode; // -1是停止，0是逐渐夹紧，1是逐渐松开

    // 机械臂外设参数
    public float moveSpeed_keyboard = 1f;
    public float rotationSpeed_keyboard = 1f;
    // 与外部手控器的通信
    private GameObject armControlSocket;
    public bool send_hand_force_sensors = false;
    // 与自制手控器的通信
    public GameObject hhrHCControl;

    // UI显示控件
    [Header("UI显示控件")]
    [Space(10)]
    public Slider slider_gripper_force;
    public Slider slider_acc_x;
    public Slider slider_acc_y;
    public Slider slider_acc_z;
    public TMP_Text gripper_fps;

    // Start is called before the first frame update
    void Start()
    {
        arm_1 = GameObject.Find(arm_1_name);
        arm_2 = GameObject.Find(arm_2_name);
        arm_3 = GameObject.Find(arm_3_name);
        arm_4 = GameObject.Find(arm_4_name);
        arm_5 = GameObject.Find(arm_5_name);
        arm_6 = GameObject.Find(arm_6_name);
        gripper_1 = GameObject.Find(gripper_1_name);
        gripper_2 = GameObject.Find(gripper_2_name);
        endPoint = GameObject.Find(end_point_name);

        newPosition = endPoint.transform.position;
        newRotation = endPoint.transform.rotation;
        beginPosition = endPoint.transform.position;
        beginRotation = endPoint.transform.rotation;

        armControlSocket = GameObject.Find("Socket");
        TouchX = GameObject.Find("TouchX");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 positionIncrement = Vector3.zero;
        if (isBackToBegin) {
            isBackToBegin = false;
            UpdateArmStatus(beginPosition, beginRotation);
            newPosition = beginPosition;
            newRotation = beginRotation;
        }

        // 判断当前鼠标是否在UI上
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // 根据按键更新待移动点在世界坐标系下的位置
            if (Input.GetKey(KeyCode.W))
            {
                positionIncrement += Vector3.right * moveSpeed_keyboard * Time.deltaTime;
                newPosition += Vector3.right * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            if (Input.GetKey(KeyCode.S))
            {
                positionIncrement -= Vector3.right * moveSpeed_keyboard * Time.deltaTime;
                newPosition -= Vector3.right * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            if (Input.GetKey(KeyCode.A))
            {
                positionIncrement -= Vector3.forward * moveSpeed_keyboard * Time.deltaTime;
                newPosition -= Vector3.forward * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            if (Input.GetKey(KeyCode.D))
            {
                positionIncrement += Vector3.forward * moveSpeed_keyboard * Time.deltaTime;
                newPosition += Vector3.forward * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                positionIncrement += Vector3.up * moveSpeed_keyboard * Time.deltaTime;
                newPosition += Vector3.up * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                positionIncrement -= Vector3.up * moveSpeed_keyboard * Time.deltaTime;
                newPosition -= Vector3.up * moveSpeed_keyboard * Time.deltaTime;
                UpdateArmStatus(newPosition, newRotation);
            }
            // 左键 <- 张开夹爪
            // 右键 -> 闭合夹爪
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                // Debug.Log(Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition));
                if (Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) <= 1.2f)
                {
                    UpdateGripperStatus(gripper_1, gripper_keyboard_speed);
                    UpdateGripperStatus(gripper_2, gripper_keyboard_speed);
                }
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                // Debug.Log(Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition));
                if (Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) >= 0.3f)
                {
                    UpdateGripperStatus(gripper_1, -gripper_keyboard_speed);
                    UpdateGripperStatus(gripper_2, -gripper_keyboard_speed);
                }
            }
            if (Input.GetKey(KeyCode.B))
            {
                connectTouch = true;
            }
            if (Input.GetKey(KeyCode.P))
            {
                connectTouch = false;
            }
            // 更新姿态
            bool isUpdataEndPointRotation = UpdataEndPointRotation(ref newRotation);
            if (isUpdataEndPointRotation)
            {
                UpdateArmStatus(newPosition, newRotation);
            }
        }

        // 监听手控器的消息
        bool data_used = armControlSocket.GetComponent<ControlBoxTcpSocketClient>().virtualArm_used;
        if (!data_used) {
            positionIncrement = armControlSocket.GetComponent<ControlBoxTcpSocketClient>().positionIncrement;
            if (!positionIncrement.Equals(Vector3.zero))
            {
                positionIncrement.x = -positionIncrement.x;
                positionIncrement.z = -positionIncrement.z;
                newPosition += (0.2f * positionIncrement);
                UpdateArmStatus(newPosition, newRotation);
            }

            float gripperIncrement = armControlSocket.GetComponent<ControlBoxTcpSocketClient>().gripperIncrement;
            if (gripperIncrement != 0)
            {
                if (gripperIncrement > 0 && Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) <= 1.2f)
                {
                    UpdateGripperStatus(gripper_1, 0.05f * gripperIncrement);
                    UpdateGripperStatus(gripper_2, 0.05f * gripperIncrement);
                }

                if (gripperIncrement < 0 && Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) > 0.3f)
                {
                    UpdateGripperStatus(gripper_1, 0.05f * gripperIncrement);
                    UpdateGripperStatus(gripper_2, 0.05f * gripperIncrement);
                }
            }

            // 如果最新数据已经消费了，则更新标识符
            armControlSocket.GetComponent<ControlBoxTcpSocketClient>().virtualArm_used = true;
        }

        // 监听自制手控器消息
        bool hhrHCEnable = hhrHCControl.GetComponent<HandControlToolControl>().isOpen;
        if (hhrHCEnable)
        {
            int hhrHCMode = hhrHCControl.GetComponent<HandControlToolControl>().controlMode;
            float hhrHCGripperDis = hhrHCControl.GetComponent<HandControlToolControl>().gripperDis;
            if (hhrHCMode == 0)
            {
                //
                positionIncrement = hhrHCControl.GetComponent<HandControlToolControl>().positionInc;
                if (!positionIncrement.Equals(Vector3.zero))
                {
                    newPosition = beginPosition + (0.005f * positionIncrement);
                }
            }

            UpdateArmStatus(newPosition, newRotation);
            // 虚拟夹爪当前的位置控制
            if (hhrHCGripperDis > 0)
            {
                UpdateGripperStatus(gripper_1, new Vector3(0, 1.1f, 0.1f + Math.Clamp(HHRGripperNormalizedGripperData(hhrHCGripperDis, 0f, 0.4f), 0f, 0.4f)));
                UpdateGripperStatus(gripper_2, new Vector3(0, 1.1f, -0.2f - Math.Clamp(HHRGripperNormalizedGripperData(hhrHCGripperDis, 0f, 0.4f), 0f, 0.4f)));
            }
            // 判断是否夹住了物体
            GameObject hhrHCCatchObject = hhrHCControl.GetComponent<HandControlToolControl>().catchObject;
            if (gripper_catch_object != null && hhrHCCatchObject == null)
            {
                hhrHCControl.GetComponent<HandControlToolControl>().catchObject = gripper_catch_object;
                hhrHCControl.GetComponent<HandControlToolControl>().SendCatchObject();
            }
            else if (gripper_catch_object == null && hhrHCCatchObject != null)
            {
                hhrHCControl.GetComponent<HandControlToolControl>().catchObject = null;
                hhrHCControl.GetComponent<HandControlToolControl>().SendReleaseObject();
            }
        }

        // 监听TouchX的消息
        if (connectTouch) { 
            if (TouchX != null && !TouchX.GetComponent<HapticPlugin>().DeviceIdentifier.Equals("Not Connected")) {
                // 末端点位置
                touch_newPosition = TouchX.GetComponent<HapticPlugin>().CurrentPosition;
                if (!touch_isInitPostion && touch_newPosition != Vector3.zero) {
                    touch_oldPosition = touch_newPosition;
                    touch_isInitPostion = true;
                }
                positionIncrement = new Vector3(touch_newPosition.z - touch_oldPosition.z,
                    touch_newPosition.y - touch_oldPosition.y,
                    touch_newPosition.x - touch_oldPosition.x);
            
                this.newPosition += 0.002f * positionIncrement;
                UpdateArmStatus(this.newPosition, this.newRotation);
                touch_oldPosition = touch_newPosition;

                // 末端点旋转
                touch_newRotation = TouchX.GetComponent<HapticPlugin>().GimbalAngles;
                if (!touch_isInitRotation && touch_newRotation != Vector3.zero)
                {
                    touch_oldRotation = touch_newRotation;
                    touch_isInitRotation = true;
                }
                Vector3 newRotation = touch_newRotation - touch_oldRotation;
                // 虚拟夹爪当前的位置控制
                UpdateGripperStatus(gripper_1, new Vector3(0, 1.1f, 0.1f + Math.Clamp(TouchXNormalizedGripperData(touch_newRotation.z, 0f, 0.45f), 0f, 0.45f)));
                UpdateGripperStatus(gripper_2, new Vector3(0, 1.1f, -0.2f - Math.Clamp(TouchXNormalizedGripperData(touch_newRotation.z, 0f, 0.45f), 0f, 0.45f)));
                touch_oldRotation = touch_newRotation;
            }
        }

        // 监听是否有新位置点传入
        if(isReceiveNewPositon)
        {
            newPosition = Vector3.MoveTowards(newPosition, moveTargetPositon, moveSpeed_newPosition * Time.deltaTime);

            UpdateArmStatus(newPosition, newRotation);
            if (Vector3.Distance(newPosition, moveTargetPositon) < 0.001f)
            {
                isReceiveNewPositon = false;
            }
        }

        // 夹爪状态外部控制
        if (gripperMoveMode >= 0)
        {
            if (gripperMoveMode == 0)
            {
                if (Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) >= 0.3f)
                {
                    UpdateGripperStatus(gripper_1, -gripper_keyboard_speed);
                    UpdateGripperStatus(gripper_2, -gripper_keyboard_speed);
                }
                else
                    gripperMoveMode = -1;
            }
            else if (gripperMoveMode == 1)
            {
                if (Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition) <= 1.2f)
                {
                    UpdateGripperStatus(gripper_1, gripper_keyboard_speed);
                    UpdateGripperStatus(gripper_2, gripper_keyboard_speed);
                }
                else
                    gripperMoveMode = -1;
            }
        }

        // 更新UI界面的力反馈
        UpdateUIValueWhenTouchXCatchObject(positionIncrement.x / Time.deltaTime, 
            positionIncrement.y / Time.deltaTime, 
            positionIncrement.z / Time.deltaTime);

        // Socket发送消息
        /*        float gripperNowValue = armControlSocket.GetComponent<ControlBoxTcpSocketClient>().gripperNowValue;
                SendGripperData(NormalizedGripperData(gripperNowValue));*/
        SendGripperData(NormalizedGripperData(Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition)));
        SendVirtualURData();
        SendForceSensorData();
    }

    /// <summary>
    /// 更新机械臂的各个位置
    /// </summary>
    /// <param name="newPosition">相对于UR-10坐标系下的位置</param>
    public void UpdateArmStatus(Vector3 newPosition, Quaternion newRotation) {
        /***************************************模式一：只考虑位置，不考虑末端点姿态********************************************//*
        double angle_1 = UpdateArmRotation(newPosition, arm_1, "y");
        if (arm_1_angle_inverse)
        {
            angle_1 = -angle_1;
        }
        arm_1_init_angle += angle_1;

        double angle_2 = UpdateArmRotation(newPosition, arm_2, "y");
        if (arm_2_angle_inverse)
        {
            angle_2 = -angle_2;
        }
        arm_2_init_angle += angle_2;

        double angle_3 = UpdateArmRotation(newPosition, arm_3, "y");
        if (arm_3_angle_inverse)
        {
            angle_3 = -angle_3;
        }
        arm_3_init_angle += angle_3;

        *//*        double angle_4 = UpdateArmRotation(newPosition, arm_4, "y");
                if (arm_4_angle_inverse)
                {
                    angle_4 = -angle_4;
                }
                arm_4_init_angle += angle_4;

                double angle_5 = UpdateArmRotation(newPosition, arm_5, "z");
                if (arm_5_angle_inverse)
                {
                    angle_5 = -angle_5;
                }
                arm_5_init_angle += angle_5;

                double angle_6 = UpdateArmRotation(newPosition, arm_6, "y");
                if (arm_6_angle_inverse)
                {
                    angle_6 = -angle_6;
                }
                arm_6_init_angle += angle_6;*/


        /***************************************模式二：考虑位置，并且与初始姿态角相同********************************************/
        int itetation_num = 20;
        while (itetation_num-- > 0)
        {

            if (Vector3.Distance(endPoint.transform.position, newPosition) < 0.00001 &&
                Math.Abs(Quaternion.Dot(endPoint.transform.rotation, newRotation)) < 0.1)
            {
                return;
            }

            double angle_1 = UpdateArmRotation(newPosition, arm_1, "y");
            if (arm_1_angle_inverse)
            {
                angle_1 = -angle_1;
            }
            arm_1_init_angle += angle_1;

            double angle_2 = UpdateArmRotation(newPosition, arm_2, "y");
            if (arm_2_angle_inverse)
            {
                angle_2 = -angle_2;
            }
            arm_2_init_angle += angle_2;

            double angle_3 = UpdateArmRotation(newPosition, arm_3, "y");
            if (arm_3_angle_inverse)
            {
                angle_3 = -angle_3;
            }
            arm_3_init_angle += angle_3;
          
            // 计算相对旋转
            Quaternion relativeRotation = Quaternion.Inverse(newRotation) * endPoint.transform.rotation;
            // 将相对旋转转化为欧拉角度
            Vector3 relativeEulerAngles = relativeRotation.eulerAngles;
            // 打印相对旋转角度
            //Debug.Log("Relative Rotation: " + relativeEulerAngles);

            float relative_x = relativeEulerAngles.x;
            float relative_y = relativeEulerAngles.y;
            float relative_z = relativeEulerAngles.z;
            if (relativeEulerAngles.x > 180)
            {
                relative_x = relativeEulerAngles.x - 360;
            }
            UpdateArmRotation(relative_x, arm_5, "z");
            if (arm_5_angle_inverse)
            {
                relative_x = -relative_x;
            }
            arm_5_init_angle += relative_x;

            if (relativeEulerAngles.y > 180)
            {
                relative_y = relativeEulerAngles.y - 360;
            }
            UpdateArmRotation(relative_y, arm_6, "y");
            if (arm_6_angle_inverse)
            {
                relative_y = -relative_y;
            }
            arm_6_init_angle += relative_y;

            if (relativeEulerAngles.z > 180)
            {
                relative_z = relativeEulerAngles.z - 360;
            }
            UpdateArmRotation(-relative_z, arm_4, "y");
            if (arm_4_angle_inverse)
            {
                relative_z = -relative_z;
            }
            arm_4_init_angle += relative_z;
        }
    }

    /// <summary>
    /// 更新夹爪的位置
    /// </summary>
    /// <param name="gripper">夹爪对象</param>
    /// <param name="increment">增量数值</param>
    public void UpdateGripperStatus(GameObject gripper, float increment)
    {
        Vector3 vector3 = new Vector3(0, increment, 0);
        gripper.transform.Translate(vector3);       // translate默认是在局部坐标系下进行的
    }

    /// <summary>
    /// 更新夹爪的位置
    /// </summary>
    /// <param name="gripper">夹爪对象</param>
    /// <param name="postion">局部坐标系下的位置</param>
    public void UpdateGripperStatus(GameObject gripper, Vector3 postion)
    {
        // 局部坐标系
        gripper.transform.localPosition = postion;
    }

    /// <summary>
    /// 旋转指定的机械臂关节
    /// </summary>
    /// <param name="newPosition">世界位置</param>
    /// <param name="arm">关节</param>
    /// <param name="type">旋转轴,x、y、z</param>
    private double UpdateArmRotation(Vector3 newPosition, GameObject arm, string type)
    {
        // 世界坐标系转到本地机械臂关节的坐标系
        Vector3 endPointPos = arm.transform.InverseTransformPoint(endPoint.transform.position);
        Vector3 targetPos = arm.transform.InverseTransformPoint(newPosition);

        Vector3 rotationAixs = Vector3.zero;
        float angle = 0f;
        // 确定旋转轴，计算投影角
        if (type.ToLower().Equals("x"))
        {
            endPointPos.x = targetPos.x = 0;
            endPointPos = endPointPos.normalized;
            targetPos = targetPos.normalized;
            angle = Vector3.Angle(endPointPos, targetPos);
            rotationAixs = Vector3.Cross(endPointPos, targetPos);
        }
        if (type.ToLower().Equals("y"))
        {
            endPointPos.y = targetPos.y = 0;
            endPointPos = endPointPos.normalized;
            targetPos = targetPos.normalized;
            angle = Vector3.Angle(endPointPos, targetPos);
            rotationAixs = Vector3.Cross(endPointPos, targetPos);
        }
        if (type.ToLower().Equals("z"))
        {
            endPointPos.z = targetPos.z = 0;
            endPointPos = endPointPos.normalized;
            targetPos = targetPos.normalized;
            angle = Vector3.Angle(endPointPos, targetPos);
            rotationAixs = Vector3.Cross(endPointPos, targetPos);
        }
        // 将欧拉角转换为四元数
        if (angle != 0) {
            Quaternion rotation = Quaternion.AngleAxis(angle, rotationAixs);
            // 应用旋转到关节
            arm.transform.rotation *= rotation;

            // 根据转轴确认角度的正负，不能提前更改
            if (rotationAixs.x < 0f || rotationAixs.y < 0f || rotationAixs.z < 0f)
            {
                angle = -angle;
            }
            return angle;
        }
        return 0.0;
    }

    private void UpdateArmRotation(float angle, GameObject arm, string type)
    {
        Vector3 angles = arm.transform.localEulerAngles;
        // 确定旋转轴，计算投影角
        if (type.ToLower().Equals("x"))
        {
            angles.x += angle;
        }
        if (type.ToLower().Equals("y"))
        {
            angles.y += angle;
        }
        if (type.ToLower().Equals("z"))
        {
            angles.z += angle;
        }
        arm.transform.localEulerAngles = angles;
    }

    /// <summary>
    /// 收到夹爪触发的事件，通过外部脚本调用触发
    /// </summary>
    /// <param name="gripper">夹爪对象</param>
    /// <param name="catchObject">抓到的物体</param>
    public void GripperTrigger(string gripper_name, GameObject catchObject, GripperStatusEnum status) {
        if (gripper_name.Equals(gripper_1_name)) {
            if (status.Equals(GripperStatusEnum.Enter))
            {
                gripper_1_catch_object = catchObject;
            }
            if (status.Equals(GripperStatusEnum.Exit))
            {
                gripper_1_catch_object = null;
            }
        }

        if (gripper_name.Equals(gripper_2_name))
        {
            if (status.Equals(GripperStatusEnum.Enter))
            {
                gripper_2_catch_object = catchObject;
            }
            if (status.Equals(GripperStatusEnum.Exit))
            {
                gripper_2_catch_object = null;
            }
        }

        // 判断是否是一个对象，确认抓取对象
        if (gripper_1_catch_object != null && gripper_1_catch_object.Equals(gripper_2_catch_object))
        {
            Debug.Log("抓到物体！！！");
            gripper_catch_object = catchObject;
            gripper_catch_object.GetComponent<Rigidbody>().useGravity = false;
            gripper_catch_object.GetComponent<Rigidbody>().isKinematic = true;
            gripper_catch_object.transform.SetParent(endPoint.transform, true);
        }
        else {
            if (gripper_catch_object != null) {
                gripper_catch_object.transform.SetParent(null, true);
                gripper_catch_object.GetComponent<Rigidbody>().useGravity = true;
                gripper_catch_object.GetComponent<Rigidbody>().isKinematic = false;
                gripper_catch_object = null;
            }
        }
    }

    private void UpdateUIValueWhenTouchXCatchObject(float acc_x, float acc_y, float acc_z) {
        if (gripper_catch_object != null)
        {
            float distance = Vector3.Distance(gripper_1.transform.localPosition, gripper_2.transform.localPosition);
            slider_gripper_force.value = Math.Clamp(1.2f - distance, 0.05f , 1f);
            slider_acc_x.value = Math.Clamp(Math.Abs(acc_x), 0f, 1f);
            slider_acc_y.value = Math.Clamp(Math.Abs(acc_y), 0.1f, 1f);
            slider_acc_z.value = Math.Clamp(Math.Abs(acc_z), 0f, 1f);

            Debug.LogFormat("UI gripperForce: {0} acc_x:{1} acc_y:{2} acc_z:{3}", distance, acc_x, acc_y, acc_z);
        }
        else {
            slider_gripper_force.value = 0;
            slider_acc_x.value = 0;
            slider_acc_y.value = 0;
            slider_acc_z.value = 0;
        }
    }
    
    /// <summary>
    /// 发送虚拟UR机械臂的各个关节角度
    /// </summary>
    private void SendVirtualURData() {
        if (send_to_actual_UR)
        {
            // 转成弧度
            double arm_1_angle = arm_1_init_angle / 180.0 * Math.PI;
            double arm_2_angle = arm_2_init_angle / 180.0 * Math.PI;
            double arm_3_angle = arm_3_init_angle / 180.0 * Math.PI;
            double arm_4_angle = arm_4_init_angle / 180.0 * Math.PI;
            double arm_5_angle = arm_5_init_angle / 180.0 * Math.PI;
            double arm_6_angle = arm_6_init_angle / 180.0 * Math.PI;

            byte[] arm_1_byteArray = BitConverter.GetBytes(arm_1_angle);
            byte[] arm_2_byteArray = BitConverter.GetBytes(arm_2_angle);
            byte[] arm_3_byteArray = BitConverter.GetBytes(arm_3_angle);
            byte[] arm_4_byteArray = BitConverter.GetBytes(arm_4_angle);
            byte[] arm_5_byteArray = BitConverter.GetBytes(arm_5_angle);
            byte[] arm_6_byteArray = BitConverter.GetBytes(arm_6_angle);

            List<byte> combinedList = new List<byte>();
            combinedList.AddRange(arm_1_byteArray);
            combinedList.AddRange(arm_2_byteArray);
            combinedList.AddRange(arm_3_byteArray);
            combinedList.AddRange(arm_4_byteArray);
            combinedList.AddRange(arm_5_byteArray);
            combinedList.AddRange(arm_6_byteArray);

            armControlSocket.GetComponent<URControlUdpSocketClient>().sendBytes = combinedList.ToArray();

            string byteString = "Byte Array Values: ";
            foreach (byte b in combinedList.ToArray())
            {
                byteString += b.ToString() + " ";
            }
            //Debug.Log("【VirtualURControl】SendVirtualURData byteString：" + byteString);
        }
    }

    public void SendForceSensorData() 
    { 
        if (send_hand_force_sensors) 
        {
            byte[] receiveBytes = armControlSocket.GetComponent<ForceSensorUdpSocketServer>().receiveBytes;
            gripper_fps.text = armControlSocket.GetComponent<ForceSensorUdpSocketServer>().receiveFps.ToString("F2");
            if (receiveBytes == null || receiveBytes.Length == 0)
            {
                return;
            }

            List<byte> combinedList = new List<byte>
            {
                // 控制箱规定起始字符
                0xAA,
                0xBB,
                0x04,
                0xF4
            };

            // 获取传感器的值
            float force_x = BitConverter.ToSingle(receiveBytes, 0);
            float force_y = BitConverter.ToSingle(receiveBytes, 4);
            float force_z = BitConverter.ToSingle(receiveBytes, 8);
            float torque_x = BitConverter.ToSingle(receiveBytes, 12);
            float torque_y = BitConverter.ToSingle(receiveBytes, 16);
            float torque_z = BitConverter.ToSingle(receiveBytes, 20);
            Debug.Log($"【力传感器数据】 force_x:{force_x}, force_y:{force_y}, force_z:{force_z}, \n" +
                $"torque_x:{torque_x}, torque_y:{torque_y}, torque_z:{torque_z}");

            // 二次处理
            float force_gripper_out = 0;
            float force_x_rotate_out = 0; 
            float force_y_rotate_out = 0;
            float force_z_rotate_out = 0;
            float force_x_out = -force_x;
            float force_y_out = -force_z;
            float force_z_out = -force_y;

            // 转成byte[] 并传输
            byte[] b1 = BitConverter.GetBytes(force_gripper_out);
            byte[] b2 = BitConverter.GetBytes(force_y_rotate_out);
            byte[] b3 = BitConverter.GetBytes(force_z_rotate_out);
            byte[] b4 = BitConverter.GetBytes(force_x_rotate_out);
            byte[] b5 = BitConverter.GetBytes(force_z_out);
            byte[] b6 = BitConverter.GetBytes(force_y_out);
            byte[] b7 = BitConverter.GetBytes(force_x_out);
            combinedList.AddRange(b1);
            combinedList.AddRange(b2);
            combinedList.AddRange(b3);
            combinedList.AddRange(b4);
            combinedList.AddRange(b5);
            combinedList.AddRange(b6);
            combinedList.AddRange(b7);

            // 控制箱规定结束
            combinedList.Add(0x45);
            combinedList.Add(0x4E);
            combinedList.Add(0x44);

            armControlSocket.GetComponent<ControlBoxTcpSocketClient>().sendData = combinedList.ToArray();
            armControlSocket.GetComponent<ForceSensorUdpSocketServer>().receiveBytes = null;
        }
    }

    /// <summary>
    /// 发送夹爪的数据
    /// </summary>
    private void SendGripperData(float gripper_value) {
        float placeHolderValue = 0.0f;

        byte[] arm_1_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_2_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_3_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_4_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_5_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_6_byteArray = BitConverter.GetBytes(placeHolderValue);
        byte[] arm_7_byteArray = BitConverter.GetBytes(gripper_value);

        List<byte> combinedList = new List<byte>();
        combinedList.AddRange(arm_1_byteArray);
        combinedList.AddRange(arm_2_byteArray);
        combinedList.AddRange(arm_3_byteArray);
        combinedList.AddRange(arm_4_byteArray);
        combinedList.AddRange(arm_5_byteArray);
        combinedList.AddRange(arm_6_byteArray);
        combinedList.AddRange(arm_7_byteArray);

        armControlSocket.GetComponent<GripperUdpSocketClient>().sendBytes = combinedList.ToArray();
    }

    /// <summary>
    /// 将夹爪的数据归一化到(0, 60)
    /// </summary>
    private float NormalizedGripperData(float gripper_value, float min_value = 0f, float max_value = 60f) {
        if (gripper_value < gripper_min_value) {
            gripper_min_value = gripper_value;
        }

        if (gripper_value > gripper_max_value) {
            gripper_max_value = gripper_value;
        }

        if (gripper_max_value == gripper_min_value) {
            return min_value;
        }

        double value = max_value - (((gripper_value - gripper_min_value) / (gripper_max_value - gripper_min_value)) * (max_value - min_value) + min_value);
        return ((float)value);
    }

    private float TouchXNormalizedGripperData(float gripper_value, float min_value = 0f, float max_value = 60f)
    {
        if (gripper_value < touchgripper_min_value)
        {
            touchgripper_min_value = gripper_value;
        }

        if (gripper_value > touchgripper_max_value)
        {
            touchgripper_max_value = gripper_value;
        }

        if (touchgripper_max_value == touchgripper_min_value)
        {
            return min_value;
        }

        double value = max_value - (((gripper_value - touchgripper_min_value) / (touchgripper_max_value - touchgripper_min_value)) * (max_value - min_value) + min_value);
        return ((float)value);
    }

    private float HHRGripperNormalizedGripperData(float gripper_value, float min_value = 0f, float max_value = 60f)
    {
        if (gripper_value < hhrgripper_min_value)
        {
            hhrgripper_min_value = gripper_value;
        }

        if (gripper_value > hhrgripper_max_value)
        {
            hhrgripper_max_value = gripper_value;
        }

        if (hhrgripper_max_value == hhrgripper_min_value)
        {
            return min_value;
        }

        double value = (((gripper_value - hhrgripper_min_value) / (hhrgripper_max_value - hhrgripper_min_value)) * (max_value - min_value) + min_value);
        return ((float)value);
    }

    /// <summary>
    /// 根据案件更新末端的姿态角
    /// </summary>
    /// <returns></returns>
    public bool UpdataEndPointRotation(ref Quaternion nowRotation) {
        Vector3 eularRotation = nowRotation.eulerAngles;
        bool isUpdate = false;

        if (Input.GetKey(KeyCode.Keypad6)) 
        {
            eularRotation.x += rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }
        if (Input.GetKey(KeyCode.Keypad4))
        {
            eularRotation.x -= rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }
        if (Input.GetKey(KeyCode.Keypad8))
        {
            eularRotation.z += rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }
        if (Input.GetKey(KeyCode.Keypad2))
        {
            eularRotation.z -= rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }
        if (Input.GetKey(KeyCode.Keypad7))
        {
            eularRotation.y += rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }
        if (Input.GetKey(KeyCode.Keypad9))
        {
            eularRotation.y -= rotationSpeed_keyboard * Time.deltaTime;
            isUpdate = true;
        }

        nowRotation = Quaternion.Euler(eularRotation);
        return isUpdate;
    }
}
