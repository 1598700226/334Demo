using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

	private float mouseXAmount, mouseYAmount;
	public Transform cam;
    public float rSpeed = 10f;
    public float tSpeed = 100f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            cam.Translate(0, 0, 100f);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            cam.Translate(0, 0, -100f);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            cam.Translate(0, 0, tSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.C))
        {
            cam.Translate(0, 0, -tSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A)){
			transform.Rotate(0,0, rSpeed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.E)){
			transform.Rotate(0,0,-rSpeed * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.LeftArrow)){
			transform.Rotate(0, rSpeed * Time.deltaTime, 0);
		}
		if(Input.GetKey(KeyCode.RightArrow)){
			transform.Rotate(0,-rSpeed * Time.deltaTime, 0);
		}		
		if(Input.GetKey(KeyCode.UpArrow)){
			transform.Rotate(rSpeed * Time.deltaTime, 0,0);
		}
		if(Input.GetKey(KeyCode.DownArrow)){
			transform.Rotate(-rSpeed * Time.deltaTime, 0,0);
		}		
		if(Input.GetKey(KeyCode.Z)){
            cam.Rotate(-rSpeed * Time.deltaTime, 0,0);
		}
		if(Input.GetKey(KeyCode.S)){
            cam.Rotate(rSpeed * Time.deltaTime, 0,0);
		}
		if(Input.GetKey(KeyCode.Q)){
            cam.Rotate(0, -rSpeed * Time.deltaTime, 0);
		}
		if(Input.GetKey(KeyCode.D)){
            cam.Rotate(0, rSpeed * Time.deltaTime, 0);
		}
	}
}
