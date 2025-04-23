using System.Collections.Generic;
using UnityEngine;

public class RobotArmControl : MonoBehaviour
{
    // 旋转全只允许一个Y轴
    public float rotateSpeed = 20.0f;
    public int ccdIterations = 5;

    // 机械臂的部件
    GameObject armBase;
    GameObject arm_1;
    GameObject arm_2;
    GameObject arm_3;
    GameObject arm_4;
    List<GameObject> arms;

    GameObject clampLeft;
    GameObject clampRight;
    bool clampLeftIsTrigger = false;
    bool clampRightIsTrigger = false;
    GameObject endPoint;

    // 机械臂参数
    List<Quaternion> baseRotation;//初始旋量
    Vector3 baseLocation;//初始位置

    // 当前状态
    ArmStatus armStatus;
    enum ArmStatus { 
        Rotated,
        Idle,
        ReceivePos,
        ReceivePosRotatedEnd
    }
    // 夹爪
    public enum ArmClampStatus {
        LeftEnter,
        RightEnter,
        LeftStay,
        RightStay,
        LeftExit,
        RightExit
    }
    GameObject catchObj_leftClamp = null;
    GameObject catchObj_rightClamp = null;
    GameObject catchObj = null;
    bool isCatching = false;
    Vector3 catchPoint = Vector3.zero;
    Quaternion catchRotation = Quaternion.identity;
    int catchingPower = 0;
    int maxCatchingPower = 100;

    Vector3 receivePos;

    // Start is called before the first frame update
    void Start()
    {
        arms = new List<GameObject>();
        armBase = GameObject.Find("Test_MArm");
        arm_1 = GameObject.Find("MA_1");
        arm_2 = GameObject.Find("MA_2");
        arm_3 = GameObject.Find("MA_3");
        arm_4 = GameObject.Find("MA_4");
        endPoint = GameObject.Find("Point001");
        clampLeft = GameObject.Find("SL");
        clampRight = GameObject.Find("SR");

        baseRotation = new List<Quaternion>
        {
            armBase.transform.localRotation,
            arm_1.transform.localRotation,
            arm_2.transform.localRotation,
            arm_3.transform.localRotation,
            arm_4.transform.localRotation
        };
        baseLocation = armBase.transform.localPosition;

        armStatus = ArmStatus.Idle;
        arms.Add(armBase);
        arms.Add(arm_1);
        arms.Add(arm_2);
        arms.Add(arm_3);
        arms.Add(arm_4);
    }

    // Update is called once per frame
    void Update()
    {
        if (armStatus == ArmStatus.ReceivePos)
        {

            for (int j = 0; j < arms.Count; j++)
            {
                if (Vector3.Distance(endPoint.transform.position, receivePos) < 0.5)
                {
                    armStatus = ArmStatus.Idle;
                    return;
                }

                GameObject itemArm = arms[j];
                Vector3 endPointPos = itemArm.transform.InverseTransformPoint(endPoint.transform.position);
                Vector3 targetPos = itemArm.transform.InverseTransformPoint(receivePos);
                endPointPos.y = targetPos.y = 0;
                endPointPos = endPointPos.normalized;
                targetPos = targetPos.normalized;

                float angle = Vector3.Angle(endPointPos, targetPos);
                if (angle > 1)
                {
                    angle = 1;
                }

                if (angle < -1)
                {
                    angle = -1;
                }
                Vector3 axis = Vector3.Cross(endPointPos, targetPos);
                itemArm.transform.Rotate(axis, angle);
            }
        }
        else {
            // a d
            armStatus = armBaseRotateControl(armStatus);
            // w s
            armStatus = arm1RotateControl(armStatus);
            // q e
            armStatus = arm2RotateControl(armStatus);
            // z c
            armStatus = arm3RotateControl(armStatus);
            // up down
            armStatus = arm4RotateControl(armStatus);
            // left right
            armStatus = clampRotateControl(armStatus);
        }

        if (isCatching && catchObj != null) {
            catchObj.transform.SetPositionAndRotation(endPoint.transform.position + catchPoint, endPoint.transform.rotation * catchRotation);
        }
    }

    // 基座旋转 只能绕y轴
    ArmStatus armBaseRotateControl(ArmStatus nowStatus) {
        if (Input.GetKey(KeyCode.A))
        {
            armBase.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;  
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            armBase.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    ArmStatus arm1RotateControl(ArmStatus nowStatus)
    {
        if (Input.GetKey(KeyCode.W))
        {
            arm_1.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        if (Input.GetKey(KeyCode.S))
        {
            arm_1.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    ArmStatus arm2RotateControl(ArmStatus nowStatus)
    {
        if (Input.GetKey(KeyCode.Q))
        {
            arm_2.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        if (Input.GetKey(KeyCode.E))
        {
            arm_2.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    ArmStatus arm3RotateControl(ArmStatus nowStatus)
    {
        if (Input.GetKey(KeyCode.Z))
        {
            arm_3.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        if (Input.GetKey(KeyCode.C))
        {
            arm_3.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    ArmStatus arm4RotateControl(ArmStatus nowStatus)
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            arm_4.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            arm_4.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    ArmStatus clampRotateControl(ArmStatus nowStatus)
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // 加紧后不能继续旋转 防止穿模
            if (!isCatching) {
                clampLeft.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
                clampRight.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            }

            if (isCatching && catchingPower < maxCatchingPower) {
                Debug.Log("catching power:" + catchingPower);
                catchingPower++;
                clampLeft.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
                clampRight.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            }

            return ArmStatus.Idle;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (isCatching) {
                catchingPower--;
            }
            clampLeft.transform.Rotate(0f, -rotateSpeed * Time.deltaTime, 0f);
            clampRight.transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
            return ArmStatus.Idle;
        }

        return nowStatus;
    }

    public void ReceivePositionMessage(Vector3 pos)
    {
        Debug.Log("receive:" + pos);
        receivePos = pos;
        armStatus = ArmStatus.ReceivePos;
    }

    public void ArmClampIsTrigger(ArmClampStatus armClamp, GameObject catchObj) {
        if (!catchObj.CompareTag("obj"))
        {
            return;
        }

        if (armClamp.Equals(ArmClampStatus.LeftEnter))
        {
            clampLeftIsTrigger = true;
            catchObj_leftClamp = catchObj;
        } 
        if (armClamp.Equals(ArmClampStatus.RightEnter))
        {
            clampRightIsTrigger = true;
            catchObj_rightClamp = catchObj;
        }
        if (armClamp.Equals(ArmClampStatus.LeftExit))
        {
            clampLeftIsTrigger = false;
            catchObj_leftClamp = null;
        }
        if (armClamp.Equals(ArmClampStatus.RightExit))
        {
            clampRightIsTrigger = false;
            catchObj_rightClamp = null;
        }

        if (clampLeftIsTrigger && clampRightIsTrigger &&
            catchObj_leftClamp != null && catchObj_rightClamp != null &&
            catchObj_leftClamp.Equals(catchObj_rightClamp))
        {
            Debug.Log("抓住了物体:" + catchObj_leftClamp.name);
            isCatching = true;

            if (catchObj.transform.parent != null && catchObj.transform.parent.CompareTag("obj"))
            {
                this.catchObj = catchObj.transform.parent.gameObject;
                catchPoint = this.catchObj.transform.position - endPoint.transform.position;
                catchRotation = Quaternion.Inverse(endPoint.transform.rotation) * this.catchObj.transform.rotation;
            }
            else {
                this.catchObj = catchObj;
                catchPoint = this.catchObj.transform.position - endPoint.transform.position;
                catchRotation = Quaternion.Inverse(endPoint.transform.rotation) * this.catchObj.transform.rotation;
            }


            
        }
        else {
            if (isCatching) {
                Debug.Log("松开了物体:" + this.catchObj.name);
                isCatching = false;
                this.catchObj = null;
                catchingPower = 0;
            }
        }

    }

    public void BeginPositionMessage() {

        armBase.transform.localRotation = baseRotation[0];
        arm_1.transform.localRotation = baseRotation[1];
        arm_2.transform.localRotation = baseRotation[2];
        arm_3.transform.localRotation = baseRotation[3];
        arm_4.transform.localRotation = baseRotation[4];
        armBase.transform.localPosition = baseLocation;
    }
}
