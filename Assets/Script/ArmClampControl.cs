using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmClampControl : MonoBehaviour
{
    GameObject arm;
    void Start()
    {
        arm = GameObject.Find("Test_MArm");
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(gameObject.name + ",Enter:" + collision.gameObject.name);
        switch (gameObject.name)
        {
            case "Box002":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.LeftEnter, collision.gameObject);
                break;
            case "Box003":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.RightEnter, collision.gameObject);
                break;
            default:
                break;
        }
    }

/*    private void OnCollisionStay(Collision collision)
    {
        switch (gameObject.name)
        {
            case "Box002":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.LeftStay, collision.gameObject);
                break;
            case "Box003":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.RightStay, collision.gameObject);
                break;
            default:
                break;
        }
    }*/
    private void OnCollisionExit(Collision collision)
    {
        Debug.Log(gameObject.name +",Exit:" + collision.gameObject.name);
        switch (gameObject.name)
        {
            case "Box002":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.LeftExit, collision.gameObject);
                break;
            case "Box003":
                arm.GetComponent<RobotArmControl>().ArmClampIsTrigger(RobotArmControl.ArmClampStatus.RightExit, collision.gameObject);
                break;
            default:
                break;
        }
    }
}
