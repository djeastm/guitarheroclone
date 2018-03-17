using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

public struct NoteData
{
    public int id;
    public float secStart;
    public float secLength;
    public bool IsHit { get; set; }
    public bool IsTail { get; set; }
    public int framesHit;
}

public class NoteController : MonoBehaviour
{
    protected LevelController levelController;

    public NoteData _noteData;    
    protected ButtonController _triggeringButton;
    private bool hasEnteredButton;
    public TailController Tail { get; set; }
    
    private bool _hasExitedButton;
    private bool _hasPassedBy;

    protected virtual void Awake()
    {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
    }

    // Called by ChartReader when instantiating note
    public void InitializeNote(Note note)
    {
        _noteData = new NoteData
        {
            id = note.id,
            secLength = note.secLength,
            secStart = note.secStart
        };
    }

    public void AttachTail(TailController addedTail)
    {
        Tail = addedTail;
    }

    public void OnHit()
    {        
        _noteData.IsHit = true;
        
        if (Tail) {            
            Tail.TurnOn();
        } else
        {
            // TODO: Do something interesting with hit notes
        }
    }

    public NoteData GetNoteData()
    {
        return _noteData;
    }

    public void SetNoteData(NoteData noteData)
    {
        _noteData = noteData;
    }

    
    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        // Make the note visible right before it comes into the camera's view
        if (transform.position.z < Camera.main.farClipPlane) {
            foreach (Renderer r in transform.parent.GetComponentsInChildren<Renderer>())
            {
                r.enabled = true;
            }
        }

        // Note has passed by without being hit
        if (transform.position.z < -1)
            if (!_hasPassedBy && !_noteData.IsHit && !_noteData.IsTail)
            {
                _hasPassedBy = true;
                levelController.OnNotePassedByWithoutHit();
            }

        // Destroy the note when it's done and any tail has passed
        if (transform.position.z < (-1.5f - _noteData.secLength))
        {            
            Destroy(transform.parent.gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<ButtonController>())
        {
            if (!hasEnteredButton && !_noteData.IsHit)
            {
                hasEnteredButton = true;
                _triggeringButton = other.GetComponentInParent<ButtonController>();
                _triggeringButton.OnNoteEnter(this);
            }            
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<ButtonController>())
        {
            if (_triggeringButton && !_hasExitedButton)
            {
                _hasExitedButton = true;
                _triggeringButton.OnNoteExit(this);
            }
        }
    }

}
