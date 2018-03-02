using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap.Unity;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

[RequireComponent(typeof(ChartReader))]
public class LevelController : MonoBehaviour
{

    public const int FPS = 60;

    //Adjustables
    public int speed = 8;
    public int lookbackNotes = 100;
    public float heldNoteBonus = 0.005f;    
    public float errorPenaltyPercentDrop = 0.005f; // half a percent off per miss
    
    //References
    public Text scoreText;
    public Slider scoreSlider;
    public Slider timeSlider;
    
    //Debugging
    public bool allowErrors = true;

    private Level _level;
    private Difficulty _difficulty;
    private Chart chart;
    private bool _isRunning;
    private List<AudioSource> songAudioSources;

    ////Scoring
    private float score; // Overall
    private int _notesHit;    
    private int _totalNumberNotes;
    private float _secondsHit;
    private float _totalTailSeconds;

    // Used for last X amount of notes (i.e. Recent Performance)
    private Queue<NoteData> _recentNotes;
    private float[] _secondsRSQTable;
    private float _recentScoreNotesHit;
    private float _recentScoreNotesTotal;
    private float _recentScoreSecondsHit;
    private float _recentScoreSecondsTotal;    

    private void Init()
    {
        score = 0;
        scoreText.text = "0.00%";

        transform.position = Vector3.zero;

        songAudioSources = gameObject.GetComponents<AudioSource>().ToList();
        foreach (AudioSource s in songAudioSources)
        {
            Destroy(s);
        }

    }

    public void StartLevel(Level lev, Difficulty diff)
    {
        _level = lev;
        _difficulty = diff;
        Init();

        ChartReader chartReader = GetComponent<ChartReader>();
        chart = chartReader.ReadChart(_level.chartFile, speed, diff);
        List<Note> thisChartNotes = chart.Notes[diff];

        _totalNumberNotes = thisChartNotes.Count;
        _totalTailSeconds = CalculateTotalTailSeconds(thisChartNotes);

        _recentNotes = new Queue<NoteData>();

        float[] secondsTable = new float[_totalNumberNotes];
        int i = 0;

        foreach (Note n in thisChartNotes)
        {            
            secondsTable[i] = n.secLength;
            i++;
        }

        _secondsRSQTable = CreateRSQTable(_totalNumberNotes, secondsTable);
        
        foreach (AudioClip clip in _level.musicFiles)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.clip = clip;
            s.Play();
            songAudioSources.Add(s);
        }
        _isRunning = true;
        
    }

    private float CalculateTotalTailSeconds(List<Note> notes)
    {
        float totalSeconds = 0;
        foreach (Note n in notes) totalSeconds += n.secLength;
        return totalSeconds;
    }

    // Range Sum Query
    private float[] CreateRSQTable(int n, float[] elements)
    {
        for (int i = 1; i < n; i++)
        {
            elements[i] += elements[i-1];
        }
        return elements;
    }

    private float RangeSumQuery(float[] table, int i, int j)
    {        
        return (i > 0) ? table[j] - table[i - 1] : 0;
    }

    public void RestartLevel()
    {
        StartLevel(this._level, this._difficulty);
    }

    public void SetAudioPlaying(bool play)
    {
        foreach (AudioSource s in gameObject.GetComponents<AudioSource>()) 
        {
            if (play) s.UnPause(); else s.Pause();
        }
    }    

    public void ReportNoteHit(NoteData nd)
    {
        _notesHit++;
        UpdatePerformanceScore(nd);
        UpdateScore();
    }

    public void ReportInvalidButtonPress()
    {
        UpdatePerformanceScore(new NoteData());
        if (allowErrors) {
            //Debug.Log("Invalid touch");
            _secondsHit = (int) (_secondsHit * (1 - errorPenaltyPercentDrop));
            if (_secondsHit < 1) _secondsHit = 0;
            UpdateScore();
        }
    }

    public void ReportDestroyedNote(NoteData nd)
    {
        UpdatePerformanceScore(nd);
        UpdateScore();
    }

    public void ReportTailContact(NoteData nd)
    {
        // Holding notes adds a little to your score to make up 
        // for missing notes
        score += heldNoteBonus;
        UpdatePerformanceScore(nd);
        UpdateScore();
    }

    private void UpdatePerformanceScore(NoteData nd)
    {
        //Debug.Log(nd.id);
        if (nd.id == 0) return; // Don't want non-notes (i.e. missed hits) counted

        _recentScoreNotesTotal = (nd.id < lookbackNotes)? nd.id : lookbackNotes;

        int j = nd.id;
        int i = j - lookbackNotes;
        if (i < 1) i = 1;

        _recentScoreSecondsTotal = RangeSumQuery(_secondsRSQTable, i, j);

        _recentScoreNotesHit = 0;
        _recentScoreSecondsHit = 0;        

        _recentNotes.Enqueue(nd);

        foreach (NoteData n in _recentNotes)
        {
            //Debug.Log(n.id+" : "+n.secStart + " : " + n.hit + " : " + n.isTail);            
            if (n.hit)
            {
                _recentScoreNotesHit++;
                if (n.isTail)
                    _recentScoreSecondsHit += (float) n.framesHit / FPS;
            }            
        }

        if (_recentNotes.Count > lookbackNotes) _recentNotes.Dequeue();
    }

    private void UpdateScore()
    {
        float noteRatio = (float) _notesHit / _totalNumberNotes;
        float secondsRatio = (float) _secondsHit / _totalTailSeconds;

        float overallScoreRatio = (noteRatio + secondsRatio / 2);
        
        float recentScoreNoteRatio;
        float recentScoreSecondsRatio = 0;
        float recentScoreRatio;
        
        if (_recentScoreNotesTotal > 0 && _recentScoreSecondsTotal > 0)
        {
            recentScoreNoteRatio = _recentScoreNotesHit / _recentScoreNotesTotal;
            recentScoreSecondsRatio = _recentScoreSecondsHit / _recentScoreSecondsTotal;
            recentScoreRatio = (recentScoreNoteRatio + recentScoreSecondsRatio) / 2;
        }
        else if (_recentScoreNotesTotal > 0)
        {
            recentScoreNoteRatio = _recentScoreNotesHit / _recentScoreNotesTotal;
            recentScoreRatio = recentScoreNoteRatio;
        }
        else if (_recentScoreSecondsTotal > 0)
        {
            recentScoreSecondsRatio = _recentScoreSecondsHit / _recentScoreSecondsTotal;
            recentScoreRatio = recentScoreSecondsRatio;
        }
        else recentScoreRatio = 0.5f;       

        UpdateScoreVisuals(overallScoreRatio, recentScoreRatio);
    }

    private void UpdateScoreVisuals(float scoreRatio, float recentPerformanceScore)
    {        
        string scoreStr = string.Format("{0:0.00%}", scoreRatio);
        
        scoreText.text = scoreStr;
        scoreSlider.value = recentPerformanceScore;
    }

    void Update()
    {
        // Move the "highway" towards the player
        if (_isRunning)
        {
            transform.Translate(-Vector3.forward * speed * Time.deltaTime);
            if (songAudioSources[0] != null)
                timeSlider.value = songAudioSources[0].time / songAudioSources[0].clip.length;
        }
    }

}
