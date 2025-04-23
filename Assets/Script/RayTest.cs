using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTest : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    // Start is called before the first frame update

    GameObject arm;
    void Start()
    {
        arm = GameObject.Find("Test_MArm");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                arm.GetComponent<RobotArmControl>().ReceivePositionMessage(hit.point);
                Debug.Log("Œª÷√£∫" + hit.point + "name:" + hit.collider.gameObject.name);
            }
        }
    }
}
