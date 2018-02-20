using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

    private LevelController levelController;
    private InteractionBehaviour interactionBehaviour;
    Renderer rend;
    Color hitColor;
    Color origColor;
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
        //StartCoroutine(AutoValidTouchShutOff()); // 
    }

    //IEnumerator AutoValidTouchShutOff()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    validTouch = false;
    //}

    public void ReportExit()
    {
        //Debug.Log("ReportExit");
        rend.material.color = origColor;
        //validTouch = false;
    }

    void OnContactStay()
    {
        if (!validTouch)
        {
            levelController.ReportMissedHit();
        }
    }

}
