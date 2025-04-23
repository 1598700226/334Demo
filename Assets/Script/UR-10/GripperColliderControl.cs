using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripperColliderControl : MonoBehaviour
{
    // 虚拟机械臂的名称
    public string arm_name = "UR-10";
    public string gripper_1_name = "gripper_1";
    public string gripper_2_name = "gripper_2";
    public string collider_1_name = "Collider_1";
    public string collider_2_name = "Collider_2";
    private GameObject arm;

    void Start()
    {
        arm = GameObject.Find(arm_name);
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("【GripperColliderControl OnCollisionEnter】" + gameObject.name + ", Enter:" + collision.gameObject.name);
        switch (gameObject.name)
        {
            case "Collider_1":
                arm.GetComponent<VirtualURControl>().GripperTrigger(gripper_1_name, collision.gameObject, VirtualURControl.GripperStatusEnum.Enter);
                break;
            case "Collider_2":
                arm.GetComponent<VirtualURControl>().GripperTrigger(gripper_2_name, collision.gameObject, VirtualURControl.GripperStatusEnum.Enter);
                break;
            default:
                Debug.Log("【GripperColliderControl OnCollisionEnter】 OnCollisionEnter 无效的gameObject.name, 请确认碰撞体的名字");
                break;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log("【GripperColliderControl OnCollisionExit】" + gameObject.name + ", Exit:" + collision.gameObject.name);
        switch (gameObject.name)
        {
            case "Collider_1":
                arm.GetComponent<VirtualURControl>().GripperTrigger(gripper_1_name, collision.gameObject, VirtualURControl.GripperStatusEnum.Exit);
                break;
            case "Collider_2":
                arm.GetComponent<VirtualURControl>().GripperTrigger(gripper_2_name, collision.gameObject, VirtualURControl.GripperStatusEnum.Exit);
                break;
            default:
                Debug.Log("【GripperColliderControl OnCollisionExit】 无效的gameObject.name, 请确认碰撞体的名字");
                break;
        }
    }
}
