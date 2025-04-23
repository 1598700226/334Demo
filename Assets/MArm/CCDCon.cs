using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCDCon : MonoBehaviour  //机械臂控制脚本
{


    //CCD控制

    [SerializeField]
    Transform pbone;//骨骼承台节点

    [SerializeField]
    int depth = 0;//节点深度  - 脚本所在的物体是承台，往下depth数量的节点为机械臂

    [SerializeField]
    int ccdDepth = 5;//CCD迭代次数

    [SerializeField]
    float rotateTime = 3.0f;//旋转匹配时间

    float nowTime = 0;//当前时间

    List<Transform> bones;//骨骼节点

    List<Transform> arms;//臂节点

    Transform bonePoint;//骨骼末端点

    List<Quaternion> baseRotation;//初始旋量

    List<Quaternion> armRotation;//机械臂节点的初始旋量

    //状态相关  - 夹取相关

    public enum ArmState
    {
        rotate, //正在旋转
        stati //已静止的 可以被操作的
    }

    ArmState armState = ArmState.stati;

    public ArmState AState {
        get {
            return armState;
        }
    }

    bool onPhyCheck = false;//用于开启物理碰撞检测
    bool isCatch = false;//是否夹取到了目标 ---  用于在Update位置匹配完成后指示下一步操作
    catchObj catchTar = null;//夹取目标

    public bool IsCatch {
        get {
            return isCatch;
        }
    }

    [SerializeField]
    Transform catchPoint; //末端位点 - 用于夹取物体后移动时的位置匹配

    [SerializeField]
    Transform boneEndPoint;//骨骼末端位置 - 用于放置物体时，判断是否能放到目标位点上

    Vector3 delVec;//与末端位点的差量
    Vector3 roVec;//转轴向量
    float cproAngle;//每秒需要旋转的角度


    [System.Serializable]
    class pointRoClamp
    {
        public bool onClamp;
        public float maxro;
        public float minro;
    }


    [SerializeField]
    List<pointRoClamp> roclamps;


    //动画相关

    Animator animator;

    int openHash = Animator.StringToHash("open");
    int closeHash = Animator.StringToHash("close");
    int stopHash = Animator.StringToHash("stop");
    int resetHash = Animator.StringToHash("reset");

    private void Awake()
    {
        arms = new List<Transform>();
        bones = new List<Transform>();
        baseRotation = new List<Quaternion>();
        armRotation = new List<Quaternion>();

        animator = gameObject.GetComponent<Animator>();

        Transform pre = gameObject.transform.GetChild(0);//往下走1，跳开FPoint
        Transform preb = pbone.transform.GetChild(0);

        catchTar = null;

        armRotation.Add(gameObject.transform.rotation);

        for (int i = 0; i < depth; ++i) {
            pre = pre.GetChild(0);
            preb = preb.GetChild(0);

            arms.Add(pre);
            bones.Add(preb);
            baseRotation.Add(preb.transform.localRotation);
            armRotation.Add(pre.transform.localRotation);
        }

        bonePoint = preb.GetChild(0);//骨骼末端点

    }


    Vector3 bonetopoint;//骨骼指向末端点向量
    Vector3 bonetotar;//骨骼指向目标点向量
    Vector3 vec;
    float angle;//旋转角度
    Vector3 axis;//转轴

    public void ccdgo(Vector3 p, catchObj tar = null)  //解算CCD尝试够到世界坐标p位置
    {

        if (armState == ArmState.rotate)//如果正在旋转应忽略
            return;

        if (isCatch == (tar != null)) //未抓住物体必须有目标，已抓住物体必须无目标进行放置
            return;

        //骨骼承台对准
        pbone.LookAt(new Vector3(p.x, pbone.transform.position.y, p.z), Vector3.up);


        //

        if (catchTar == null) { //夹取物体
            catchTar = tar;//设置夹取目标
            animator.SetTrigger(openHash);//打开爪子

        }
        else { //放置物体
            catchTar.Cld.enabled = false;//开始移动夹取物体时，关闭碰撞
            p += catchTar.PutOverVec;//增加悬浮高度

            //计算位置&旋转匹配数据
            delVec = catchTar.transform.position - catchPoint.position;
            cproAngle = Vector3.Angle(transform.forward, pbone.forward) / rotateTime;
            roVec = Vector3.Cross(transform.forward, pbone.forward).normalized;
        }


        //骨骼节点初始化旋转
        for (int i = 0; i < depth; ++i)
            bones[i].transform.localRotation = baseRotation[i];

        //

        //CCD解算骨骼节点
        for (int i = 0; i < ccdDepth; ++i) { //迭代次数

            for (int j = 0; j < depth; ++j) { //依次计算每根骨骼的旋量

                //节点选择
                if (Vector3.Distance(p, gameObject.transform.position) < 2.0f && j == 0) { //如果目标点位置过近，CCD计算跳过第一个节点
                    continue;
                }

                vec = bones[j].InverseTransformPoint(bonePoint.position);//转入本地坐标
                bonetopoint = new Vector3(vec.x, 0, vec.z).normalized;//Y轴强制归0，化为平面内旋转

                vec = bones[j].InverseTransformPoint(p);//转入本地坐标
                bonetotar = new Vector3(vec.x, 0, vec.z).normalized;//Y轴强制归0，化为平面内旋转

                angle = Vector3.Angle(bonetopoint, bonetotar);

                axis = Vector3.Cross(bonetopoint, bonetotar).normalized;

                bones[j].Rotate(axis, angle);


                ////节点旋向限制
                //if (j == 0) {
                //    //第一时间对Y轴旋转夹一个Clamp
                //    bones[0].localEulerAngles = new Vector3(0, Mathf.Clamp(bones[0].localEulerAngles.y, -60.0f, 130.0f), 0);
                //    //这里如果节点没错的话，那么 x z local旋转肯定是0，不用 bones[0].localEulerAngles.x/.z 这样写
                //}
                //else if (j == 1 || j == 2) {
                //    bones[j].localEulerAngles = new Vector3(0, Mathf.Clamp(bones[j].localEulerAngles.y, 0, 120.0f), 0);
                //}
                //else if (j == 3) {
                //    bones[3].localEulerAngles = new Vector3(0, Mathf.Clamp(bones[3].localEulerAngles.y, -20.0f, 20.0f), 0);
                //}

                if (roclamps.Count > j && roclamps[j].onClamp) {
                    bones[j].localEulerAngles = new Vector3(0, Mathf.Clamp(bones[j].localEulerAngles.y, roclamps[j].minro, roclamps[j].maxro), 0);
                }
            }


        }

        //操作机械臂节点匹配

        //机械臂直接匹配
        //gameObject.transform.rotation = pbone.rotation;

        //for (int i = 0; i < depth; ++i)
        //    arms[i].rotation = bones[i].rotation;


        //设置状态 Update 插值匹配


        if (IsCatch && Vector3.Distance(p, boneEndPoint.position) > 0.1f) {//受Clamp影像，放置目标时，预先判断能否放到指定位置
            //如果放不到 Reset机械臂
            catchTar.Rbdy.isKinematic = false;//刚体恢复
            catchTar.Cld.enabled = true;
            catchTar = null;
            isCatch = false;
            animator.SetTrigger(resetHash);//爪子恢复默认状态
            armState = ArmState.stati;
            ccdReset();//重置机械臂姿态

            Debug.Log("无法完成放置！！！");
        }
        else {//正常开启Update旋转的初始化

        armState = ArmState.rotate;
        nowTime = 0;

        armRotation[0] = gameObject.transform.rotation;//记录承台旋量

        for (int i = 1; i <= depth; ++i)
            armRotation[i] = arms[i - 1].localRotation;

        }
    }


    public void ccdReset() //复位
    {
        if (armState == ArmState.rotate) return;//如果正在旋转应忽略

        //对准承台
        pbone.rotation = gameObject.transform.rotation;

        //骨骼复位
        for (int i = 0; i < depth; ++i)
            bones[i].localRotation = baseRotation[i];

        //设置状态 Update 插值匹配

        armState = ArmState.rotate;
        nowTime = 0;

        armRotation[0] = gameObject.transform.rotation;//记录承台旋量

        for (int i = 1; i <= depth; ++i)
            armRotation[i] = arms[i - 1].localRotation;
    }

    float lerp;
    private void Update()
    {
        if (!onPhyCheck&&armState == ArmState.rotate) {//正在旋转匹配的
            nowTime += Time.deltaTime;
            if (nowTime >= rotateTime) { //匹配已经完成

                //完成匹配 对齐姿态
                gameObject.transform.rotation = pbone.rotation;

                for (int i = 0; i < depth; ++i)
                    arms[i].localRotation = bones[i].localRotation;


                //根据 isCatch 进行下一步操作
                if (catchTar) { //有夹取目标需要进行下一步操作

                    if (isCatch) { //应该释放夹取目标
                        //catchTar.parent = null;//释放目标
                        catchTar.Rbdy.isKinematic = false;//刚体恢复
                        catchTar.Cld.enabled = true;
                        catchTar = null;
                        isCatch = false;
                        animator.SetTrigger(resetHash);//爪子恢复默认状态
                        armState = ArmState.stati;
                        ccdReset();//重置机械臂姿态

                        

                    }
                    else {//应该完成夹取动作
                        
                        //夹取动画&物理检测
                        animator.SetTrigger(closeHash);//执行合抓动画
                        onPhyCheck = true;//开启物理检测

                    }
                }else
                    armState = ArmState.stati;//不需要下一步 重置状态为静止

            }
            else { //正在旋转匹配
                lerp = nowTime / rotateTime;

                gameObject.transform.rotation = Quaternion.Slerp(armRotation[0], pbone.rotation, lerp);

                for (int i = 1; i <= depth; ++i)
                    arms[i - 1].localRotation = Quaternion.Slerp(armRotation[i], bones[i - 1].localRotation, lerp);

                //抓取目标的位置跟随匹配
                if (isCatch && catchTar) {
                    catchTar.transform.RotateAround(transform.position, roVec, Time.deltaTime*cproAngle);
                    catchTar.transform.position = catchPoint.position + delVec;

                }
            }
        }

        

        
        
    }

    private void OnTriggerStay(Collider other) //碰撞到夹取目标物体
    {
        if (!isCatch&&onPhyCheck&&other.CompareTag("obj")&&catchTar.Cld == other) {
            isCatch = true;
            onPhyCheck = false;
            animator.SetTrigger(stopHash);
            armState = ArmState.stati;
            catchTar.Rbdy.isKinematic = true;//刚体静态化
           
        }
    }


    public void catchCheck() //Close执行到最后 是否夹取到
    {
        if (onPhyCheck && !isCatch) { //抓取失败
            isCatch = false;
            catchTar = null;
            onPhyCheck = false;
            animator.SetTrigger(resetHash);
            armState = ArmState.stati;
            ccdReset();
            Debug.Log("夹取失败！！！");
        }

    }




}
