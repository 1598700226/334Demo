using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class TestTrackEndPoint : MonoBehaviour
{

    GameObject endPoint; // 末端点
    StringBuilder position_x = new StringBuilder();
    StringBuilder position_y = new StringBuilder();
    StringBuilder position_z = new StringBuilder();

    // Start is called before the first frame update
    void Start()
    {
        endPoint = GameObject.Find("EndPoint");
        StartCoroutine(SamplePositionEvery200ms());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            position_x.Clear();
            position_y.Clear();
            position_z.Clear();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("position_x: " + position_x.ToString());
            Debug.Log("position_y: " + position_y.ToString());
            Debug.Log("position_y: " + position_z.ToString());
        }
    }

    IEnumerator SamplePositionEvery200ms()
    {
        while (true)
        {
            Vector3 pos = endPoint.transform.position; // 获取该物体的当前坐标
            position_x.Append(pos.x).Append(",");
            position_y.Append(pos.y).Append(",");
            position_z.Append(pos.z).Append(",");
            // 等待 0.2 秒（200 ms）
            yield return new WaitForSeconds(0.2f);
        }
    }
}
