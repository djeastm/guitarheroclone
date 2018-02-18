using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractionBehaviour))]
public class ButtonController : MonoBehaviour {

    LevelController levelController;
    InteractionBehaviour interactionBehaviour;
    private bool hit;
    private float length = 0f;
    Renderer rend;
    Color hitColor;

    void Awake () {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        rend = GetComponentInChildren<Renderer>();
        hitColor = new Color(1f, 1f, 1f);
    }

    void Start()
    {
	    //Debug.Log(interactionButton.OnPress.Method);
        interactionBehaviour.OnContactStay += OnContactStay;
    }

    private void Update()
    {
        // Make the note visible right before it comes into the camera's view
        if (transform.position.z < Camera.main.farClipPlane) GetComponentInChildren<Renderer>().enabled = true;
        // Destroy the note when it's done and any tail has passed
        if (transform.position.z < (-1 - length)) Destroy(gameObject);
    }

    public void SetLength(float length)
    {
        // Gets called when created by ChartReader so that tails won't disappear too soon when destroyed
        this.length = length;
    }
    
    public void OnContactStay()
    {
		// Don't do anything if there's a penalty timer going
	    //if (levelController.ErrorPenalty) return;

		
		// Otherwise, go to each of the interaction controllers
		//foreach (InteractionController i in interactionBehaviour.contactingControllers)
		//{
		//	// Go through each contactbone
	 //       foreach (ContactBone cb in i.contactBones)
	 //       {
		       
		//        // ignore the palms
		//		if (!cb.name.Contains("Palm")) 
		//        {
		//	        // ... and find out what the finger is striking
		//	        bool hitFretboard = false;
		//	        foreach (IInteractionBehaviour b in i.contactingObjects)
		//	        {
		//		        // these can either be the Fretboard or one of the Buttons
		//		        if (!b.name.StartsWith("Fretboard"))
		//		        {
					        // If it's a button, and the button hasn't been hit before
					        // register it as a hit
					        rend.material.color = hitColor;
					        if (!hit)
					        {
						        levelController.ReportNoteHit();
						        hit = true;
					        }

					        // If it's already been struck, then give a bonus
					        // for holding it out
					        levelController.HeldNoteIncreaseScore();
		//		        }
		//		        else hitFretboard = true;
		//		        // They hit the fretboard during this frame
		//	        }
		//	        if (hitFretboard) levelController.ReportFretboardHit();
		//		}
	 //       }
		//}
    }
}
