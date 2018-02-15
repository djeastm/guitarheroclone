using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boundary
{
	public float xMin, xMax, yMin, yMax;
}

public class HandInteraction : MonoBehaviour
{
    new Rigidbody rigidbody;
	public float speed;
	public float tilt;
	public Boundary boundary;

	private void Awake()
	{
		rigidbody = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Mouse X");
		float moveVertical = Input.GetAxis("Mouse Y");

		Vector3 movement = new Vector3(moveHorizontal, moveVertical, 0.0f);
		rigidbody.velocity = movement * speed;

		rigidbody.position = new Vector3
		(
			Mathf.Clamp(rigidbody.position.x, boundary.xMin, boundary.xMax),			
			Mathf.Clamp(rigidbody.position.y, boundary.yMin, boundary.yMax),
			0.0f
		);

		rigidbody.rotation = Quaternion.Euler(0.0f, 0.0f, rigidbody.velocity.x * -tilt);
	}
}
