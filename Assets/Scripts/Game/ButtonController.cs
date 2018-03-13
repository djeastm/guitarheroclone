using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

    public int _sensitivity = 300;

    //References
    private LevelController levelController;
    private InteractionBehaviour interactionBehaviour;
    private Renderer rend;
    private Color hitColor;
    private Color origColor;

    private int _sensitivityCounter = 1;    
    
    bool _validTouch;    

    void Awake()
    {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        interactionBehaviour = GetComponent<InteractionBehaviour>();

        rend = GetComponentInChildren<Renderer>();
        hitColor = new Color(1f, 1f, 1f);
        origColor = rend.material.color;
    }

    void Start()
    {
        interactionBehaviour.OnContactStay += OnContactStay;
    }

    public void OnNoteStay()
    {
        rend.material.color = hitColor;
        _validTouch = true;
    }

    public void OnNoteExit()
    {
        rend.material.color = origColor;
        _validTouch = false;
    }    

    void OnContactStay()
    {        
        if (_sensitivityCounter % _sensitivity == 0)
        {
            if (!_validTouch)
            {
                Debug.Log("Button Controller: Invalid touch");
                levelController.ReportInvalidButtonPress();
            }
        }
        _sensitivityCounter++;
        
    }

}
