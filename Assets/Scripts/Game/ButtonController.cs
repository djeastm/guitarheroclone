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

    private int sensitivityCounter = 1;    
    
    bool validTouch;    

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

    public void ReportStrike()
    {
        rend.material.color = hitColor;
        validTouch = true;        
    }

    public void ReportExit()
    {        
        rend.material.color = origColor;        
    }

    void OnContactStay()
    {
        if (sensitivityCounter % _sensitivity == 0)
        {
            if (!validTouch)
            {
                //Debug.Log("Invalid touch");
                levelController.ReportInvalidButtonPress();
            }
        }
        sensitivityCounter++;
        
    }

}
