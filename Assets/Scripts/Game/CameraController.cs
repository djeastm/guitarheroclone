using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	
	public float speed = 1f;
	
	void Start () {
		
	}
	
	void Update () {
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
	}
}
