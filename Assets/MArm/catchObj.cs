using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class catchObj : MonoBehaviour
{
    Collider cld;
    Rigidbody rbdy;

    [SerializeField]
    float catchOverHeight = 0f;//相对于轴心的悬浮中心高度

    [SerializeField]
    float putOverHeight = 0f;//相对放置位置需要悬浮的高度


    public Collider Cld {
        get {
            return cld;
        }
    }

    public Rigidbody Rbdy {
        get {
            return rbdy;
        }
    }

    public Vector3 CatchCenterPoint { //获取物体被夹取的中心位点 世界坐标
        get {
            return transform.position + catchOverHeight * transform.up;
        }
    }

    public Vector3 PutOverVec { //中心相对放置位点应当悬浮的高度向量
        get {
            return Vector3.up * putOverHeight;
        }
    }


    private void Awake()
    {
        cld = gameObject.GetComponent<Collider>();
        rbdy = gameObject.GetComponent<Rigidbody>();
    }


}
