using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualURMotionPlaningControl : MonoBehaviour
{
    [Header("UI")]
    [Space(10)]
    public TMP_Text txt_EndPointsNowPos;
    public TMP_InputField input_MoveSpeed;
    public Button btn_BackInitPos;
    public Button btn_Running;

    public Button btn_SelectTargetPos;
    public TMP_Text txt_SelectTargetPos;
    public TMP_InputField input_TargetPosx;
    public TMP_InputField input_TargetPosy;
    public TMP_InputField input_TargetPosz;
    public TMP_Dropdown drop_MoveMode; // 0 是pick夹取 1是place放置
    public Toggle toggle_isAuto; // 是否自动抓取模式
    public Button btn_Execute;


    [Header("场景对象")]
    [Space(10)]
    public GameObject URGameObject;
    public float GripperDelayTime = 3.0f; // 夹爪执行现实对应的延迟
    public float GripperYoffSet = 0.1f;   // y向偏移距离，单位m

    [Header("绘制的选点")]
    private string gameObjec_name = "DrawSelectTargetPoints";
    private GameObject drawObject;
    private List<GameObject> drawChildObject;
    public float sphereRadius = 0.01f;
    public float offset = 0.0f;
    public Material Material_blue;

    //***********************************脚本私有对象***********************************//
    private Vector3 SelectTargetPosition;
    private bool isRunning;
    private bool isSelectPoint;
    private bool isAutoRunning;
    public List<Vector3> AutoTargetPosition = new List<Vector3>();
    private List<int> AutoMoveType = new List<int>(); //自动执行的类型，0是夹爪紧，1是夹爪松，2是机械臂移动
    private bool isCoroutineRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        btn_BackInitPos.onClick.AddListener(BackInitPosBtn);
        btn_SelectTargetPos.onClick.AddListener(SelectTargetPosBtn);
        btn_Execute.onClick.AddListener(ExecuteBtn);

        input_MoveSpeed.onEndEdit.AddListener(MoveSpeedEndEdit);
        input_TargetPosx.onEndEdit.AddListener(TargetPosxEndEdit);
        input_TargetPosy.onEndEdit.AddListener(TargetPosyEndEdit);
        input_TargetPosz.onEndEdit.AddListener(TargetPoszEndEdit);

        drop_MoveMode.onValueChanged.AddListener(DropMoveModeValueChanged);

        drawObject = new GameObject(gameObjec_name);
        drawChildObject = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
        string txtNowPos = virtualURControl.newPosition.x.ToString("F4") + ","
                            + virtualURControl.newPosition.y.ToString("F4") + ","
                            + virtualURControl.newPosition.z.ToString("F4") + ",";
        txt_EndPointsNowPos.text = txtNowPos;

        if (toggle_isAuto.isOn)
        {
            if (isAutoRunning)
            {
                btn_Running.image.color = Color.red;
                btn_Execute.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = "Stop";
                if (AutoMoveType.Count <= 0)
                {
                    isAutoRunning = false;
                    btn_Running.image.color = Color.green;
                    btn_Execute.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = "Execute";
                }
                else
                { 
                    switch (AutoMoveType[0]) 
                    {
                        case 0:
                            virtualURControl.gripperMoveMode = 0;
                            if (!isCoroutineRunning)
                            {
                                isCoroutineRunning = true;
                                StartCoroutine(DelayGripperAction(GripperDelayTime));
                            }
                            break;
                        case 1:
                            virtualURControl.gripperMoveMode = 1;
                            if (!isCoroutineRunning)
                            {
                                isCoroutineRunning = true;
                                StartCoroutine(DelayGripperAction(GripperDelayTime));
                            }
                            break;
                        case 2:
                            if (Vector3.Distance(virtualURControl.newPosition, AutoTargetPosition[0]) < 0.001f)
                            {
                                virtualURControl.isReceiveNewPositon = false;
                                AutoMoveType.RemoveAt(0);
                                AutoTargetPosition.RemoveAt(0);
                            }
                            else
                            {
                                virtualURControl.moveTargetPositon = AutoTargetPosition[0];
                                virtualURControl.isReceiveNewPositon = true;
                            }
                            break;
                        default: break;
                    }
                }
            }
            else
            {
                btn_Running.image.color = Color.green;
                btn_Execute.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = "Execute";
            }
        }
        else
        {
            isRunning = virtualURControl.isReceiveNewPositon;
            if (isRunning)
            {
                btn_Running.image.color = Color.red;
                btn_Execute.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = "Stop";
            }
            else
            {
                btn_Running.image.color = Color.green;
                btn_Execute.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = "Execute";
            }
        }

        if (isSelectPoint)
        {
            // 左键按下
            if (Input.GetMouseButtonDown(0))
            {
                Physics.queriesHitBackfaces = true;
                // 判断是否在UI上
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 hitPoint = hit.point;
                    // 画出点，设置颜色和大小
                    GameObject redSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建一个球体
                    redSphere.transform.position = hitPoint;
                    redSphere.transform.parent = drawObject.transform;
                    Renderer sphereRenderer = redSphere.GetComponent<Renderer>();
                    Destroy(redSphere.GetComponent<SphereCollider>());
                    sphereRenderer.material = Material_blue;
                    redSphere.transform.localScale = new Vector3(sphereRadius * 3, sphereRadius * 3, sphereRadius * 3);

                    drawChildObject.Add(redSphere);
                    SelectTargetPosition = redSphere.transform.position;
                    string txtSelectPos = SelectTargetPosition.x.ToString("F4") + ","
                        + SelectTargetPosition.y.ToString("F4") + ","
                        + SelectTargetPosition.z.ToString("F4") + ",";
                    txt_SelectTargetPos.text = txtSelectPos;
                    input_TargetPosx.text = SelectTargetPosition.x.ToString("F4");
                    input_TargetPosy.text = SelectTargetPosition.y.ToString("F4");
                    input_TargetPosz.text = SelectTargetPosition.z.ToString("F4");

                    isSelectPoint = false;
                }

                // 如果是自动模式，先规划点和动作：
                // 1.松开夹爪
                // 2.移动到物体中点上方
                // 3.移动到物体中点处
                // 4.夹紧夹爪
                // 5.移动到中点上方
                // 6.移动到目标点上方
                // 7.移动到目标点处
                // 8.松开夹爪
                // 9.移动到目标点上方
                if (toggle_isAuto.isOn)
                {
                    AutoMoveType.Clear();
                    AutoTargetPosition.Clear();
                    GameObject parentObject = GameObject.Find("MidPoint");
                    //Transform childTransform = parentObject.transform.Find("MidPoint");
                    Vector3 childWorldPosition = parentObject.transform.position;
                    SelectTargetPosition.y = childWorldPosition.y;

                    AutoMoveType.Add(1);
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(childWorldPosition.x, childWorldPosition.y + GripperYoffSet, childWorldPosition.z));
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(childWorldPosition.x, childWorldPosition.y, childWorldPosition.z));
                    AutoMoveType.Add(0);
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(childWorldPosition.x, childWorldPosition.y + GripperYoffSet, childWorldPosition.z));
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(SelectTargetPosition.x, SelectTargetPosition.y + GripperYoffSet, SelectTargetPosition.z));
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(SelectTargetPosition.x, SelectTargetPosition.y, SelectTargetPosition.z));
                    AutoMoveType.Add(1);
                    AutoMoveType.Add(2);
                    AutoTargetPosition.Add(new Vector3(SelectTargetPosition.x, SelectTargetPosition.y + GripperYoffSet, SelectTargetPosition.z));
                }
            }

            // 右键按下, 关闭示教功能
            if (Input.GetMouseButtonDown(1))
            {
                isSelectPoint = false;
            }
        }
    }

    void BackInitPosBtn()
    {
        VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
        virtualURControl.moveTargetPositon = virtualURControl.beginPosition;
        virtualURControl.isReceiveNewPositon = true;
        isRunning = virtualURControl.isReceiveNewPositon;
    }

    void SelectTargetPosBtn()
    {
        isSelectPoint = true;
        // 删除旧的物体
        foreach (Transform childTransform in drawObject.transform)
        {
            Destroy(childTransform.gameObject);
        }
        drawChildObject.Clear();
    }

    void ExecuteBtn()
    {
        VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
        if (toggle_isAuto.isOn)
        {
            if (isAutoRunning)
            {
                virtualURControl.moveTargetPositon = virtualURControl.newPosition;
                virtualURControl.isReceiveNewPositon = true;
                AutoTargetPosition.Clear();
                AutoMoveType.Clear();
                isAutoRunning = false;
            }
            else
            {
                if (AutoMoveType.Count <= 0)
                    return;

                isAutoRunning = true;
            }
        }
        else
        {
            AutoTargetPosition.Clear();
            AutoMoveType.Clear();
            if (isRunning)
            {
                virtualURControl.moveTargetPositon = virtualURControl.newPosition;
                virtualURControl.isReceiveNewPositon = true;
            }
            else
            {
                if (SelectTargetPosition != Vector3.zero)
                {
                    virtualURControl.moveTargetPositon = SelectTargetPosition;
                    virtualURControl.isReceiveNewPositon = true;
                }
            }
        }
    }

    void MoveSpeedEndEdit(string finalValue)
    {
        VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
        try
        {
            float speed = float.Parse(finalValue);
            virtualURControl.moveSpeed_newPosition = speed;
        }
        catch(Exception e)
        {
            Debug.Log("【MoveSpeedEndEdit】输入格式有误, " + e.Message);
        }
    }

    void TargetPosxEndEdit(string finalValue)
    {
        try
        {
            float posx = float.Parse(finalValue);
            SelectTargetPosition.x = posx;
            string txtSelectPos = SelectTargetPosition.x.ToString("F4") + ","
                + SelectTargetPosition.y.ToString("F4") + ","
                + SelectTargetPosition.z.ToString("F4") + ",";
            txt_SelectTargetPos.text = txtSelectPos;

            drawChildObject[drawChildObject.Count - 1].transform.position = SelectTargetPosition;
        }
        catch (Exception e)
        {
            Debug.Log("【TargetPosxEndEdit】输入格式有误, " + e.Message);
        }
    }

    void TargetPosyEndEdit(string finalValue)
    {
        try
        {
            float posy = float.Parse(finalValue);
            SelectTargetPosition.y = posy;
            string txtSelectPos = SelectTargetPosition.x.ToString("F4") + ","
                + SelectTargetPosition.y.ToString("F4") + ","
                + SelectTargetPosition.z.ToString("F4") + ",";
            txt_SelectTargetPos.text = txtSelectPos;

            drawChildObject[drawChildObject.Count - 1].transform.position = SelectTargetPosition;
        }
        catch (Exception e)
        {
            Debug.Log("【TargetPosyEndEdit】输入格式有误, " + e.Message);
        }
    }

    void TargetPoszEndEdit(string finalValue)
    {
        try
        {
            float posz = float.Parse(finalValue);
            SelectTargetPosition.z = posz;
            string txtSelectPos = SelectTargetPosition.x.ToString("F4") + ","
                + SelectTargetPosition.y.ToString("F4") + ","
                + SelectTargetPosition.z.ToString("F4") + ",";
            txt_SelectTargetPos.text = txtSelectPos;

            drawChildObject[drawChildObject.Count - 1].transform.position = SelectTargetPosition;
        }
        catch (Exception e)
        {
            Debug.Log("【TargetPoszEndEdit】输入格式有误, " + e.Message);
        }
    }

    void DropMoveModeValueChanged(int value)
    {
        VirtualURControl virtualURControl = URGameObject.GetComponent<VirtualURControl>();
        virtualURControl.gripperMoveMode = value;
    }


    // 协程，实现延迟3秒的操作
    IEnumerator DelayGripperAction(float delay)
    {
        // 等待指定的时间
        yield return new WaitForSeconds(delay);
        // 延迟后的操作
        Debug.Log($"{delay}秒延迟完成！");
        AutoMoveType.RemoveAt(0);
        isCoroutineRunning = false;
    }
}
