using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailController : NoteController {

    public bool IsHighlighted { get; set; }
    private Renderer rend;
    private Color hitColor;
    private Color origColor;

    protected override void Awake()
    {
        base.Awake();
        rend = transform.parent.GetComponentInChildren<Renderer>();
        hitColor = new Color(1f, 1f, 1f);
        origColor = rend.material.color;
    }

    protected override void Start()
    {
        base.Start();
        _noteData.IsTail = true;
    }

    public void TurnOn()
    {
        TurnHighlightOn();
    }

    public void TurnHighlightOn()
    {
        if (rend) rend.material.color = hitColor;
    }

    public void TurnHighlightOff()
    {
        if (rend) rend.material.color = origColor;
    }

}
