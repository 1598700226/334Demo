using Emgu.CV.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MeasurentPointControl : MonoBehaviour
{
    [Header("UI对象")]
    public Button selectPointsButton;
    public TMP_Text txt_selectPointNum;
    public Button calculateLineButton;
    public TMP_Text txt_LineLength;
    public Button savePlaneOneButton;
    public TMP_Text txt_PlaneOneNum;
    public Button savePlaneTwoButton;
    public TMP_Text txt_PlaneTwoNum;
    public Button calculatePlaneAngleButton;
    public TMP_Text txt_PlaneAngle;

    [Space(10)]
    [Header("点的位置列表")]
    public List<Vector3> drawPointList;
    public bool isSelectPoints = false;

    [Space(10)]
    [Header("Point Draw设置")]
    // Point Draw
    private string gameObjec_name = "DrawMeasurementSelectPoints";
    private GameObject drawPointGameObject;
    public float sphereRadius = 0.01f;
    public float offset = 0.0f;
    [Header("红色材质")]
    public Material Material_Red;

    /**************脚本内部共享变量***********/
    private int selectPointNum = 0;
    private float lineLength = 0.0f;
    private int planeOneNum = 0;
    private int planeTwoNum = 0;
    private float planeAngle = 0.0f;
    private Vector3 planeOneNormal = Vector3.zero;
    private Vector3 planeTwoNormal = Vector3.zero;

    void updateTxtUI()
    {
        // UI更新
        txt_selectPointNum.text = selectPointNum.ToString();
        txt_LineLength.text = lineLength.ToString("F3");
        txt_PlaneOneNum.text = planeOneNum.ToString();
        txt_PlaneTwoNum.text = planeTwoNum.ToString();
        txt_PlaneAngle.text = planeAngle.ToString("F3");
    }

    // Start is called before the first frame update
    void Start()
    {
        drawPointList = new List<Vector3>();
        isSelectPoints = false;
        drawPointGameObject = new GameObject(gameObjec_name);

        selectPointsButton.onClick.AddListener(SelectPointsBtnClick);
        calculateLineButton.onClick.AddListener(CalculateLineBtnClick);
        savePlaneOneButton.onClick.AddListener(SavePlaneOneBtnClick);
        savePlaneTwoButton.onClick.AddListener(SavePlaneTwoBtnClick);
        calculatePlaneAngleButton.onClick.AddListener(CalculatePlaneAngleBtnClick);

        selectPointNum = 0;
        lineLength = 0.0f;
        planeOneNum = 0;
        planeTwoNum = 0;
        planeAngle = 0.0f;
        updateTxtUI();
    }

    // Update is called once per frame
    void Update()
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
                if (isSelectPoints)
                {
                    drawPointList.Add(hitPoint);

                    // 画出点，设置颜色和大小
                    GameObject redSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere); // 创建一个球体
                    redSphere.transform.position = hitPoint;
                    redSphere.transform.parent = drawPointGameObject.transform;
                    Renderer sphereRenderer = redSphere.GetComponent<Renderer>();
                    Destroy(redSphere.GetComponent<SphereCollider>());
                    sphereRenderer.material = Material_Red;
                    redSphere.transform.localScale = new Vector3(sphereRadius * 3, sphereRadius * 3, sphereRadius * 3);
                }
            }
        }

        // 右键按下, 关闭示教功能
        if (Input.GetMouseButtonDown(1))
        {
            if (isSelectPoints)
            {
                isSelectPoints = false;
            }
        }
    }

    void SelectPointsBtnClick()
    {
        isSelectPoints = true;
        // 删除旧的物体
        foreach (Transform childTransform in drawPointGameObject.transform)
        {
            Destroy(childTransform.gameObject);
        }

        drawPointList.Clear();
        selectPointNum = 0;
        lineLength = 0;
        updateTxtUI();
    }

    void CalculateLineBtnClick()
    {
        if (drawPointList.Count < 2)
        {
            Debug.LogError("【MeasurementPointControl】 CalculateLineBtnClick 选择的点少于两个");
            return;
        }
        lineLength = Vector3.Distance(drawPointList[0], drawPointList[1]);
        updateTxtUI();
    }

    void SavePlaneOneBtnClick()
    {
        planeOneNum = drawPointList.Count;
        planeOneNormal = GetPlaneNormal(drawPointList);

        drawPointList.Clear();
        selectPointNum = 0;
        updateTxtUI();
    }

    void SavePlaneTwoBtnClick()
    {
        planeTwoNum = drawPointList.Count;
        planeTwoNormal = GetPlaneNormal(drawPointList);

        drawPointList.Clear();
        selectPointNum = 0;
        updateTxtUI();
    }

    void CalculatePlaneAngleBtnClick()
    {
        float dotProduct = Vector3.Dot(planeOneNormal, planeTwoNormal);
        // 使用反余弦函数计算夹角，并将其从弧度转换为角度
        float angle = Mathf.Acos(dotProduct / (planeOneNormal.magnitude * planeTwoNormal.magnitude)) * Mathf.Rad2Deg;
        // 确保角度是锐角
        if (angle > 90)
        {
            angle = 180 - angle;
        }
        planeAngle = angle;
        updateTxtUI();
    }

    public Vector3 GetPlaneNormal(List<Vector3> points)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 point in points)
        {
            sum += point;
        }
        Vector3 centroid = sum / points.Count;
        Vector3 ns_svd = PointCloudHandle.CalNormalVector(points, centroid);
        Plane plane = new(ns_svd, centroid);

        // 计算平面度，暂时无用
        float planeQuality = 0;
        foreach (Vector3 point in points)
        {
            planeQuality += Mathf.Abs(plane.GetDistanceToPoint(point));
        }
        planeQuality /= points.Count;
        return ns_svd;
    }

}
