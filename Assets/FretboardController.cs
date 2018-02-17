using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FretboardController : MonoBehaviour {

	LevelController levelController;
	private bool hit;
	Renderer rend;
	private Color origColor;
	Color hitColor;
	// Use this for initialization
	void Awake () {
		levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
		//interactionManager = GameObject.FindGameObjectWithTag("InteractionManager").GetComponent<InteractionManager>();
		rend = GetComponent<Renderer>();
		origColor = rend.material.color;
		hitColor = new Color(1f, 1f, 1f);
	}

	//private void OnTriggerEnter(Collider other)
	//{
	//	rend.material.color = hitColor;
		
	//	levelController.ReportFretboardHit();

	//}

	//private void OnTriggerExit(Collider other)
	//{
	//	rend.material.color = origColor;
	//}
}
