﻿using System.Collections;
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
    public Chart chart;
    public Transform[] notePrefabs;
    public Transform[] buttonPrefabs;
    int fretboardScale;
    private float fretboardTime;
    public Transform fretboardPrefab;
    public Transform buttonSpawnParent;

    public Chart ReadChart(TextAsset chartFile, int speed, Difficulty diff)
    {
        fretboardScale = speed;
        chart = ParseChart(chartFile.text.ToString());
        List<Note> notes = SpawnNotes(chart, diff);
        fretboardTime = TickToTime(chart, notes[notes.Count - 1].tickStart, chart.Resolution) + TickToTime(chart, (int)chart.Offset, chart.Resolution);
        SpawnFretboard(fretboardPrefab,Vector3.zero,fretboardTime);
        SpawnButtons();
        return chart;
        
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
        int id = 1;
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
    List<Note> SpawnNotes(Chart c, Difficulty diff)
    {
        List<Note> notes = c.Notes[diff];
        foreach (Note note in notes)
        {
            SpawnNote(c, note);
        }

        return notes;
    }

    //Spawn single note
    float SpawnNote(Chart c, Note note)
    {
        // Subtract the offset from the note's tickStart position
        Vector3 point = new Vector3(0f, 0f, note.secStart - TickToTime(c, (int)c.Offset, c.Resolution));
        // When spawning the prefab, convert the tickLength from ticks to time
        SpawnPrefab(note, notePrefabs[note.button], point, note.secLength);
        return point.z;
    }

    void SpawnPrefab(Note noteData, Transform prefab, Vector3 point, float length)
    {
        Transform note = Instantiate(prefab);
        note.SetParent(transform);
        note.position = new Vector3(prefab.position.x, prefab.position.y, point.z);
        note.gameObject.AddComponent<NoteController>();
        note.GetComponent<NoteController>().note = noteData;
        if (length > 0) // There's a held note, so spawn a 'tail' on the note
        {
            Transform tail = Instantiate(prefab);
            tail.SetParent(note.transform);
            // We want to push the tail back by half to line up with the end of the note
            tail.position = new Vector3(note.position.x, note.position.y, point.z + length / 2f);
            // Then we reshape our note prefab to make a tail of the correct tickLength
            tail.localScale += new Vector3(-tail.localScale.x * 0.5f, -tail.localScale.y * 0.5f, length);
            // We tell the note about its tail and the tickLength, so they can be destroyed correctly           
            TailController tc = tail.gameObject.AddComponent<TailController>();
            tc.note = noteData;
            note.GetComponent<NoteController>().AttachTail(tc);
            note.GetComponent<NoteController>().SetLength(length);
            tail.SetParent(note);
        }		
        if (note.position.z < Camera.main.farClipPlane) note.GetComponentInChildren<Renderer>().enabled = true;
    }

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
                time += DisToTime(prevBPM.tick, bpmInfo.tick, resolution, prevBPM.value / 1000.0f, fretboardScale);
                prevBPM = bpmInfo;
            }
        }

        time += DisToTime(prevBPM.tick, tick, resolution, prevBPM.value / 1000.0f, fretboardScale);

        return (float)time;
    }

    // This function is taken without much change from the Moonscraper Guitar Hero Chart Editor
    // By Alexander "FireFox" Ong
    public static double DisToTime(int tickStart, int tickEnd, float resolution, float bpm, int fretboardScale)
    {
        return fretboardScale * (tickEnd - tickStart) / resolution * 60 / bpm;
    }


    private void SpawnFretboard(Transform prefab, Vector3 point, float length = 0f)
    {
        Transform fretboard = Instantiate(prefab);
        fretboard.SetParent(transform);
        fretboard.position = new Vector3(fretboard.position.x, fretboard.position.y, point.z + length / 2f);
        fretboard.localScale = new Vector3(fretboard.localScale.x, fretboard.localScale.y, length/10f);

    }

    private void SpawnButtons()
    {
        foreach (Transform prefab in buttonPrefabs)
        {
            Transform button = Instantiate(prefab);
            button.SetParent(buttonSpawnParent);
            button.position = new Vector3(prefab.position.x, prefab.position.y, 0f);
        }
    }
}
