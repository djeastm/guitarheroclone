using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{

    public int _sensitivity;

    //References    
    public GameObject _explosion;
    private LevelController _levelController;
    private InteractionBehaviour _interactionBehaviour;
    public Transform depressingButton;

    //Audio
    private AudioSource _soundEffects;
    public AudioClip _missSound;
    //public AudioClip hitSound;

    // Position
    private Vector3 BUTTON_START_POS;
    public float _buttonDepth;

    private int _sensitivityCounter = 1;

    bool _isContacted; // Button is being contacted by the Leap Motion hands    
    
    List<NoteController> _currentNotes;

    void Awake()
    {
        _levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        _interactionBehaviour = GetComponent<InteractionBehaviour>();
        _soundEffects = GetComponent<AudioSource>();
        _currentNotes = new List<NoteController>();
        BUTTON_START_POS = transform.position;
    }

    private void Update()
    {
        if (_isContacted) PressButton();
        else { UnpressButton(); }
    }

    void Start()
    {
        _interactionBehaviour.OnContactStay += OnContactStay;
        _interactionBehaviour.OnContactBegin += OnContactBegin;
        _interactionBehaviour.OnContactEnd += OnContactEnd;
    }

    public void OnNoteEnter(NoteController noteCtrl)
    {
        _currentNotes.Add(noteCtrl);
    }

    public void OnNoteExit(NoteController noteCtrl)
    {
        if (_currentNotes.Count > 0)
        {
            _currentNotes.Remove(noteCtrl);
            _currentNotes.RemoveAll(item => item == null);
        }
    }


    //Leap Motion
    void OnContactBegin()
    {
        _isContacted = true;        
    }

    void OnContactStay()
    {        
        if (_currentNotes.Count > 0 && _currentNotes[0])
        {
            if (!_currentNotes[0].GetNoteData().IsTail)
            {
                if (!_currentNotes[0].GetNoteData().IsHit)
                {
                    _currentNotes[0].OnHit();
                    _currentNotes.Remove(_currentNotes[0]);

                    _levelController.OnSingleNoteSuccess();
                }
            } else 
            {                
                _levelController.OnHeldNote();
            }
        }
        else
        {   
            if (_sensitivityCounter % _sensitivity == 0)
            {
                _levelController.OnInvalidTouch();
                
                _soundEffects.PlayOneShot(_missSound);
            }
            _sensitivityCounter++;
            
        }        
    }

    void OnContactEnd()
    {
        _isContacted = false;
    }

    void PressButton()
    {
        Transform visualization = depressingButton;
        visualization.Translate(Vector3.down * Time.deltaTime, Space.World);

        if (visualization.position.y < BUTTON_START_POS.y - _buttonDepth)
            visualization.position = new Vector3(BUTTON_START_POS.x, BUTTON_START_POS.y - _buttonDepth, BUTTON_START_POS.z);

    }

    void UnpressButton()
    {
        Transform visualization = depressingButton;
        visualization.Translate(Vector3.up * Time.deltaTime, Space.World);

        if (visualization.position.y > BUTTON_START_POS.y)
            visualization.position = BUTTON_START_POS;

    }

    public void TriggerExplosion()
    {        
        if (_explosion)
        {
            GameObject explosion = Instantiate(_explosion);
            explosion.transform.position = 
                new Vector3(
                    transform.position.x, 
                    transform.position.y + _buttonDepth, 
                    transform.position.z
                    );
        }
    }


}
