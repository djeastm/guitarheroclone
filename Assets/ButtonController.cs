using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {

    LevelController levelController;
    //InteractionManager interactionManager;
    private bool hit;
    private float length = 0f;
    Renderer rend;
    Color hitColor;

    void Awake () {
        levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
        //interactionManager = GameObject.FindGameObjectWithTag("InteractionManager").GetComponent<InteractionManager>();
        rend = GetComponent<Renderer>();
        hitColor = new Color(1f, 1f, 1f);
    }

    private void Update()
    {
        // Make the note visible right before it comes into the camera's view
        if (transform.position.z < Camera.main.farClipPlane) GetComponent<Renderer>().enabled = true;
        // Destroy the note when it's done and any tail has passed
        if (transform.position.z < (-1 - length)) Destroy(gameObject);
    }

    public void SetLength(float length)
    {
        // Gets called when created by ChartReader so that tails won't disappear too soon when destroyed
        this.length = length;
    }

    private void OnTriggerStay(Collider other)
    {
        rend.material.color = hitColor;
        if (!hit)
        {
            levelController.ReportNoteHit();
            hit = true;
        }

        levelController.HeldNoteIncreaseScore();
    }

    //private void OnCollisionStay(Collision collision)
    //{
    //	rend.material.color = hitColor;
    //	if (!hit)
    //	{
    //		levelController.ReportNoteHit();
    //		hit = true;
    //	}

    //	levelController.HeldNoteIncreaseScore();

    //}
}
