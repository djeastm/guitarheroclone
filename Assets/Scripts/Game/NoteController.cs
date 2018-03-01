using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public struct NoteData
{
    public int id;
    public float secStart;
    public float secLength;
    public bool hit;
    public bool isTail;
    public int framesHit;
}

public class NoteController : MonoBehaviour
{
    protected LevelController levelController;
    InteractionBehaviour interactionBehaviour;

    public Note note;
    public bool hit;
    protected bool isAtButton;
    protected ButtonController triggeringButton;
    protected float length;
    private TailController tail;
    protected NoteData noteData;

    void Awake () {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();
    }

    protected virtual void Start()
    {   
        interactionBehaviour.OnContactStay += OnContactStay;
        noteData = new NoteData
        {
            id = note.id,
            secLength = note.secLength,
            secStart = note.secStart
        };
    }

    protected virtual void Update()
    {
        // Make the note visible right before it comes into the camera's view
        if (transform.position.z < Camera.main.farClipPlane) GetComponentInChildren<Renderer>().enabled = true;
        // Destroy the note when it's done and any tail has passed
        if (transform.position.z < (-1 - length))
        {
            levelController.ReportDestroyedNote(noteData);
            Destroy(gameObject);
        }
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

    public virtual void OnContactStay()
    {

        if (!hit && isAtButton)
        {           
            // TODO: Clean this up
            noteData.hit = true;
            hit = true;
            if (noteData.secLength > 0)
                this.tail.GetComponent<TailController>().IsEnabled = true;

            levelController.ReportNoteHit(noteData);
            triggeringButton.ReportStrike(); // so button can glow
            //if (tickLength <= 0) Destroy(gameObject); // TODO: Animate
            //} else if (isAtButton)
            //{
            //    levelController.ReportTailContact();
            //}
        }
        else if (!hit)
        {
            levelController.ReportInvalidButtonPress();
        }
    }
    
    public void SetLength(float length)
    {
        // Gets called when created by ChartReader so that tails won't disappear too soon when destroyed
        this.length = length;
        noteData.secLength = length;
    }

    public void AttachTail(TailController addedTail)
    {
        tail = addedTail;
    }
}
