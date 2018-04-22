using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class TimeSignature
{
    public int tick;
    public int value;
    public float assignedTime;
}

public class ChartReader : MonoBehaviour
{	
    public Chart _chart;
    public Transform[] _notePrefabs;
    public Transform[] _buttonPrefabs;
    public Transform _tailPrefab;
    int _fretboardScale;
    public Transform _fretboardPrefab;
    public Transform _buttonSpawnParent;

    public Chart ReadChart(TextAsset chartFile, int speed, Difficulty diff)
    {
        _fretboardScale = speed;
        _chart = ParseChart(chartFile.text.ToString());
        SpawnNotes(_chart, diff);
        SpawnButtons();
        return _chart;        
    }	

    Chart ParseChart(string text)
    {
        MatchCollection sectionNameMatches = Regex.Matches(text, @"\[(.*)\]");
        List<string> sectionNames = new List<string>();
        foreach (Match m in sectionNameMatches)
        {
            sectionNames.Add(m.Groups[1].Value);				
        }

        string[] sectionsArr = Regex.Split(text, @"(?:}\r\n)*\[.*\](?:\r\n{)");
        List<string> sections = new List<string>();
        foreach (string s in sectionsArr) if (s.Length > 0) sections.Add(s);

        Chart c = new Chart();
        // Section 1 - Song Metadata
        string[] nameData = sections[0].Trim().Split('\n');
        c.Name = nameData[0].Trim().Split('=')[1].Trim();
        c.Artist = nameData[1].Trim().Split('=')[1].Trim();
        c.Charter = nameData[2].Trim().Split('=')[1].Trim();		
        c.Offset = float.Parse(nameData[3].Trim().Split('=')[1].Trim());
        // Offset might be used differently here than by GH
        // It is the amount of ticks to push up the tickStart of the notes
        c.Resolution = int.Parse(nameData[4].Trim().Split('=')[1].Trim());
        c.Player2 = nameData[5].Trim().Split('=')[1].Trim();
        c.Difficulty = int.Parse(nameData[6].Trim().Split('=')[1].Trim());
        c.PreviewStart = float.Parse(nameData[7].Trim().Split('=')[1].Trim());
        c.PreviewEnd = float.Parse(nameData[8].Trim().Split('=')[1].Trim());
        c.Genre = nameData[9].Trim().Split('=')[1].Trim();
        c.MediaType = nameData[10].Trim().Split('=')[1].Trim();
        c.MusicStream = nameData[11].Trim().Split('=')[1].Trim();
        c.bpms = new List<BPM>();
        c.timeSignatures = new List<TimeSignature>();

        // Section 2 - Synctrack data, like time signature and beats per second (x1000)
        string[] syncData = sections[1].Trim().Split('\n');

        for (int i = 0; i < syncData.Length; i++)
        {
            if (syncData[i].Trim().Split('=')[1].Trim().Split(' ')[0] == "B")
            {
                // BPM change
                int tick = int.Parse(syncData[i].Trim().Split('=')[0].Trim());
                int value = int.Parse(syncData[i].Trim().Split('=')[1].Trim().Split(' ')[1]);
                BPM bpm = new BPM(tick, value);

                c.bpms.Add(bpm);
            }
            else
            {
                // Time signature change
                int tick = int.Parse(syncData[i].Trim().Split('=')[0].Trim());
                int value = int.Parse(syncData[i].Trim().Split('=')[1].Trim().Split(' ')[1]);
                TimeSignature ts = new TimeSignature
                {
                    tick = tick,
                    value = value
                };
                c.timeSignatures.Add(ts);
            }
        }

        for (int i = 0; i < c.bpms.Count; i++)
        {
            c.bpms[i].assignedTime = TickToTime(c, c.bpms[i].tick, c.Resolution);
        }

        // Section 3 (index2) is ignored 

        // Section 4 (index3) is the first guitar track
        c.Notes = new Dictionary<Difficulty, List<Note>>();
        
        for (int i = 3; i < sectionNames.Count; i++)
        {            
            c.Notes.Add(ParseDifficulty(sectionNames[i]), ParseNotes(c, sections[i]));
        }

        return c;
    }

    private Difficulty ParseDifficulty(string unparsed)
    {		
        if (unparsed.StartsWith("Easy")) return Difficulty.Easy;
        if (unparsed.StartsWith("Medium")) return Difficulty.Medium;
        if (unparsed.StartsWith("Hard")) return Difficulty.Hard;
        if (unparsed.StartsWith("Expert")) return Difficulty.Expert;

        return Difficulty.Expert;
    }

    List<Note> ParseNotes(Chart c, string section)
    {
        string[] notesData = section.Trim().Split('\n');
        
        List<Note> notes = new List<Note>();
        int id = 0;
        for (int i = 0; i < notesData.Length; i++)
        {
            Note n = new Note();
            if (notesData[i].Length > 3) // skip blanks
            {
                string[] noteData = notesData[i].Trim().Split('=');
                
                n.id = id++;
                n.tickStart = int.Parse(noteData[0].Trim());

                n.type = noteData[1].Trim().Split(' ')[0];
                if (n.type != "N") continue; // Ignore things other than straight notes (S or E) for now
                                             // N is note, S is starpower phrase, E is event (?)
                n.button = int.Parse(noteData[1].Trim().Split(' ')[1]);
                // 0-4 is green-orange buttons, 5 is force flag (?), 6 is tap note, 7 is open note
                if (n.button > 4) continue; // Ignore 5, 6, or 7 notes
                n.tickLength = int.Parse(noteData[1].Trim().Split(' ')[2]);
                n.secLength = TickToTime(c, n.tickLength, c.Resolution);                
                n.secStart = TickToTime(c, n.tickStart, c.Resolution);
                notes.Add(n);
            }
        }
        return notes;
    }

    // Spawn all notes
    void SpawnNotes(Chart c, Difficulty diff)
    {
        
        List<Note> notes = c.Notes[diff];
        foreach (Note note in notes)
        {           
            SpawnNote(c, note);
        }
        
    }

    //Spawn single note
    float SpawnNote(Chart c, Note note)
    {
        // Subtract the offset from the note's tickStart position
        // Thus a negative offset pushes the start of the notes farther away from the player
        Vector3 point = new Vector3(0f, 0f, note.secStart - TickToTime(c, (int)c.Offset, c.Resolution));
        SpawnPrefab(note, _notePrefabs[note.button], point);
        return point.z;
    }

    void SpawnPrefab(Note note, Transform prefab, Vector3 point)
    {
        Transform noteTransform = Instantiate(prefab);
        noteTransform.SetParent(transform);
        noteTransform.position = new Vector3(prefab.position.x, prefab.position.y, point.z);
        Transform noteCollider = noteTransform.GetChild(0);
        //noteCollider.gameObject.AddComponent<NoteController>();
        noteCollider.GetComponent<NoteController>().InitializeNote(note, false);
        if (note.secLength > 0) // There's a held note, so spawn a 'tail' on the note
        {
            Transform tail = Instantiate(_tailPrefab);
            Color tailColor = noteTransform.gameObject.GetComponentInChildren<Renderer>().material.color;
            tail.GetComponentInChildren<Renderer>().material.color = tailColor;

            tail.SetParent(noteTransform.transform);
            // We want to push the tail back by half to line up with the end of the note
            tail.position = new Vector3(noteTransform.position.x, noteTransform.position.y,
                noteTransform.position.z + (note.secLength / 2f) + (tail.localScale.z /2f));
            // Then we reshape our prefab to make a tail of the correct Length
            // First we handle the collider
            Transform tailCollider = tail.GetChild(0);
            tailCollider.localScale = new Vector3(tail.localScale.x, tail.localScale.y, 0.5f + note.secLength);
            tailCollider.localPosition = new Vector3(tailCollider.localPosition.x, tailCollider.localPosition.y, tailCollider.localPosition.z + 0.25f);
            // Then we shrink the renderer (to make it thinner)
            Transform tailRenderer = tail.GetChild(1);
            tailRenderer.localScale += new Vector3(-tail.localScale.x * 0.5f, -tail.localScale.y * 0.5f, note.secLength);
            Destroy(tailCollider.gameObject.GetComponent<NoteController>());
            TailController tc = tailCollider.gameObject.AddComponent<TailController>();
            tc.InitializeNote(note, true);
            noteCollider.GetComponent<NoteController>().AttachTail(tc);
            tail.SetParent(noteTransform.GetChild(2)); // Attach to tail spawn point
        }
        if (noteTransform.position.z < Camera.main.farClipPlane)
        {
            foreach (Renderer r in noteTransform.GetComponentsInChildren<Renderer>()) { 
                r.enabled = true;
            }
        }
    }
    #region Alexander Ong's code with license
    // This function is taken without much change from the Moonscraper Guitar Hero Chart Editor
    // By Alexander "FireFox" Ong
    float TickToTime(Chart c, int tick, int resolution)
    {
        double time = 0;
        BPM prevBPM = c.bpms[0];

        foreach (BPM bpm in c.bpms)
        {
            BPM bpmInfo = bpm;

            if (bpmInfo == null)
                continue;

            if (bpmInfo.tick > tick)
            {
                break;
            }
            else
            {
                time += DisToTime(prevBPM.tick, bpmInfo.tick, resolution, prevBPM.value / 1000.0f, _fretboardScale);
                prevBPM = bpmInfo;
            }
        }

        time += DisToTime(prevBPM.tick, tick, resolution, prevBPM.value / 1000.0f, _fretboardScale);

        return (float)time;
    }

    // This function is taken without much change from the Moonscraper Guitar Hero Chart Editor
    // By Alexander "FireFox" Ong
    public static double DisToTime(int tickStart, int tickEnd, float resolution, float bpm, int fretboardScale)
    {
        return fretboardScale * (tickEnd - tickStart) / resolution * 60 / bpm;
    }
    
    /*BSD 3-Clause License

Copyright(c) 2016-2017, Alexander Ong
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*/
    #endregion
    private void SpawnButtons()
    {
        foreach (Transform prefab in _buttonPrefabs)
        {
            Transform button = Instantiate(prefab);
            button.SetParent(_buttonSpawnParent);
            button.position = new Vector3(prefab.position.x, prefab.position.y, 0f);
        }
    }
}
