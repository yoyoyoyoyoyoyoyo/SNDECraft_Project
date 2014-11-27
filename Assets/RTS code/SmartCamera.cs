using UnityEngine;
using System.Collections;

public class SmartCamera : MonoBehaviour {

	public Transform lookAt, pivot;
	private Quaternion defaultOrientation;
	private int rotationDirection;

	public float sensitivity = 35f;
	public float maxFOV = 70f;
	public float minFOV = 35f;

	// Use this for initialization
	void Start () {
		transform.LookAt (lookAt);
		defaultOrientation = transform.rotation;
		rotationDirection = 1;
	}
	
	// Update is called once per frame
	void Update () {

		float rotVal = 45;

		float angle = Quaternion.Angle (defaultOrientation, transform.rotation);

		if(Input.GetKey (KeyCode.LeftBracket) && angle <= rotVal)
		{
			transform.RotateAround(lookAt.position, Vector3.up, rotVal * Time.deltaTime);
			rotationDirection = -1;
		}
		else if(Input.GetKey (KeyCode.RightBracket) && angle <= rotVal)
		{
			transform.RotateAround(lookAt.position, Vector3.up, -rotVal * Time.deltaTime);
			rotationDirection = 1;
		}

		if(!Input.GetKey (KeyCode.LeftBracket) && !Input.GetKey (KeyCode.RightBracket))
		{
			transform.RotateAround(lookAt.position, Vector3.up, rotationDirection * angle * Time.deltaTime);
		}

		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		Vector3 disp = new Vector3(x,0,z);
		//transform.position += disp;
		lookAt.position += disp;

		/*float zoomVal = -Input.GetAxis ("CamRotate");
		transform.RotateAround(pivot.position, Vector3.right, zoomVal * Time.deltaTime);
		*/

		transform.LookAt (lookAt);
	}
}
