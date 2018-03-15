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
    public int _speed;
    public int _scoreMultiplier;
    public int _starPowerNotesReqd;
    public float _perFrameTailBonus;
    public float _perFrameTailPenalty;
    public float _missedNotePenalty;

    //References
    public Text _percentageText;
    public Text _scoreText;
    public Slider _scoreSlider;
    public Slider _timeSlider;
    
    private Level _level;
    private Difficulty _difficulty;
    private Chart chart;
    private bool _isRunning;
    private List<AudioSource> songAudioSources;

    ////Scoring
    private float _score; // Overall score
    private int _notesHit;
    private int _totalNumberNotes;
    private float _starPowerNotesHit;
    private int _currentNoteStreak;
    private int _longestNoteStreak;

    private void Init()
    {
        ToggleLevelUI(true);

        _score = 0;
        _notesHit = 0; ;
        _totalNumberNotes = 0;
        _starPowerNotesHit = 0;
        _currentNoteStreak = 0;
        _longestNoteStreak = 0;

        _percentageText.text = "0.00%";
        _scoreText.text = "0";
        _scoreSlider.value = 0;

        // Reset position, fretboard and notes
        transform.position = Vector3.zero;
        foreach (Transform t in transform.GetChildren())
        {
            Destroy(t.gameObject);
        }

        // End any previously playing sounds
        songAudioSources = gameObject.GetComponents<AudioSource>().ToList();
        foreach (AudioSource s in songAudioSources)
        {
            Destroy(s);
        }
        songAudioSources.Clear();

    }

    public void StartLevel(Level lev, Difficulty diff)
    {
        _level = lev;
        _difficulty = diff;
        Init();

        ChartReader chartReader = GetComponent<ChartReader>();
        chart = chartReader.ReadChart(_level.chartFile, _speed, diff);
        List<Note> thisChartNotes = chart.Notes[diff];

        _totalNumberNotes = thisChartNotes.Count;

        foreach (AudioClip clip in _level.musicFiles)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.clip = clip;
            s.Play();
            songAudioSources.Add(s);
        }
        _isRunning = true;

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

    public void OnSingleNoteSuccess()
    {
        AddToNoteStreak();
        UpdatePercentage(1);
        UpdateStarPowerScore(1);
        UpdateOverallScore(1);
    }

    public void OnHeldNote()
    {
        UpdateStarPowerScore(_perFrameTailBonus);
        UpdateOverallScore(_perFrameTailBonus);
    }

    public void OnInvalidTouch()
    {
        UpdateStarPowerScore(-_perFrameTailPenalty);
    }

    public void OnNotePassedByWithoutHit()
    {
        CancelNoteStreak();
        UpdateStarPowerScore(-_missedNotePenalty);
    }

    private void AddToNoteStreak()
    {
        _currentNoteStreak++;
        _longestNoteStreak = Mathf.Max(_longestNoteStreak, _currentNoteStreak);
    }

    private void CancelNoteStreak()
    {
        _currentNoteStreak = 0;
    }

    private void UpdatePercentage(int amount)
    {
        _notesHit += amount;

        float percentage = (float)_notesHit / _totalNumberNotes;

        percentage = Mathf.Clamp(percentage, 0, 1);

        UpdatePercentageVisual(percentage);
    }

    private void UpdateStarPowerScore(float amount)
    {
        _starPowerNotesHit += amount;

        _starPowerNotesHit = Mathf.Clamp(_starPowerNotesHit, 0, _starPowerNotesReqd);

        float starPowerRatio = _starPowerNotesHit / _starPowerNotesReqd;

        starPowerRatio = Mathf.Clamp(starPowerRatio, 0, 1);

        UpdateStarPowerVisual(starPowerRatio);
    }

    private void UpdateOverallScore(float amount)
    {
        _score += amount * _scoreMultiplier;
        if (_score < 0) _score = 0;
        UpdateOverallScoreVisual(_score);
    }

    private void UpdatePercentageVisual(float percentage)
    {
        string scoreStr = string.Format("{0:0.00%}", percentage);
        _percentageText.text = scoreStr; // Final percent complete
    }

    private void UpdateStarPowerVisual(float starPowerRatio)
    {
        _scoreSlider.value = starPowerRatio; // Star Power
    }

    private void UpdateOverallScoreVisual(float overallScore)
    {
        string scoreStr = "" + overallScore;
        _scoreText.text = scoreStr;
    }

    void Update()
    {
        // Move the fretboard towards the player
        if (_isRunning)
        {
            transform.Translate(-Vector3.forward * _speed * Time.deltaTime);
            if (songAudioSources[0] != null)
                _timeSlider.value = songAudioSources[0].time / songAudioSources[0].clip.length;

            // Check for song complete by seeing if only the fretboard is left in the scene
            // I.e. all of the notes have been destroyed. Then wait a second before showing 
            // the end level menu
            if (transform.childCount < 2) StartCoroutine(WaitForEndLevel());
        }
    }

    IEnumerator WaitForEndLevel()
    {
        yield return new WaitForSeconds(1);
        ShowEndLevelMenu();
    }

    private void ToggleLevelUI(bool active)
    {
        //Clear Screen
        _scoreText.gameObject.SetActive(active);
        _scoreSlider.gameObject.SetActive(active);
        _timeSlider.gameObject.SetActive(active);
        _percentageText.gameObject.SetActive(active);
    }

    private void ShowEndLevelMenu()
    {
        _isRunning = false;
        ToggleLevelUI(false);        

        //Calculate final scores and package data
        PlayedLevelData playedLevelData = new PlayedLevelData
        {
            overallScore = _score,
            notesHit = _notesHit,
            totalNumberNotes = _totalNumberNotes,
            percentage = (float)_notesHit / _totalNumberNotes,
            noteStreak = _longestNoteStreak
        };        

        // Go to final screen and menu
        GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>().EndLevel(playedLevelData);
    }
}

