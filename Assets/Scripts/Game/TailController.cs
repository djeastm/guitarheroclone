using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailController : NoteController {
    
    public bool IsEnabled { get; set; }
    public int FramesHit { get; set; }

    protected override void Start()
    {
        base.Start();
        noteData.isTail = true;
    }

    public override void OnContactStay()
    {
        if (IsEnabled & isAtButton)
        {
            noteData.framesHit += this.FramesHit;

            levelController.ReportTailContact(noteData);
            FramesHit++;
        }
    }
}
