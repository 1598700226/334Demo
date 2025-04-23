using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ccdCmaCon : MonoBehaviour
{
    [SerializeField]
    CCDCon arm;


    RaycastHit hitInfo;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) { //摁下左键

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo)) {

                if (!arm.IsCatch&&hitInfo.collider.CompareTag("obj")) //夹取物体
                    arm.ccdgo(hitInfo.collider.GetComponent<catchObj>().CatchCenterPoint, hitInfo.collider.GetComponent<catchObj>());
                else //放置物体
                    arm.ccdgo(hitInfo.point, null);
            }

        }
    }
}
