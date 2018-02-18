using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public class FretboardController : MonoBehaviour {
	
	InteractionBehaviour interactionBehaviour;
	private bool hit;
	Renderer rend;
	public Color hitColor;
	private Color origColor;

	// Use this for initialization
	void Awake () {
		interactionBehaviour = GetComponent<InteractionBehaviour>();
		rend = GetComponent<Renderer>();
		origColor = rend.material.color;
	}

	//void Start()
	//{
	//	interactionBehaviour.OnContactBegin += OnContactBegin;
	//	interactionBehaviour.OnContactEnd += OnContactEnd;
	//}
	//private void OnContactBegin()
	//{
	//	rend.material.color = hitColor;
	//}

	//private void OnContactEnd()
	//{
	//	rend.material.color = origColor;
	//}
	
}
