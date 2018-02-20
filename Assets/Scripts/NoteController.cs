using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public class NoteController : MonoBehaviour
{
    private LevelController levelController;
    InteractionBehaviour interactionBehaviour;
    private float length = 0f;
    
    private bool hit;
    private bool isAtButton;
    private ButtonController triggeringButton;

    void Awake () {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
    }

    void Start()
    {
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

    public void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<ButtonController>() != null)
        {
            if (!hit) triggeringButton = other.GetComponentInParent<ButtonController>();
            isAtButton = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (triggeringButton != null) triggeringButton.ReportExit();
    }

    public void OnContactStay()
    {
        
        if (!hit && isAtButton) 
        {
            levelController.HeldNoteIncreaseScore();
            levelController.ReportNoteHit();
            triggeringButton.ReportStrike(); // so button can glow
            hit = true;
            //if (length <= 0) Destroy(gameObject); // TODO: Animate
        } else if (isAtButton)
        {
            levelController.HeldNoteIncreaseScore();
        }
        else if (!hit)
        {
            levelController.ReportMissedHit();
        }
    }
}
