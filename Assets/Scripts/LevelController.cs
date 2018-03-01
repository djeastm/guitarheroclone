using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ChartReader))]
public class LevelController : MonoBehaviour {

    private Level level;
    private Difficulty diff;
    private Chart chart;

    public int speed = 8;
    private float score = 0; // numerator
    private int totalPossScore = 0; // denominator 

    public float heldNoteBonus = 0.005f;
    public bool allowErrors = true;
    public float errorPenaltyPercentDrop = 0.005f; // half a percent off per miss
    //public float errorPenaltyTime = 0.5f;
    public Text scoreText;
    public Slider scoreSlider;
    public Slider timeSlider;
    private bool _isRunning;

    private List<AudioSource> songAudioSources;


    //public bool ErrorPenalty { get; set; }

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

    public void StartLevel(Level level, Difficulty diff)
    {
        this.level = level;
        this.diff = diff;
        Init();
        ChartReader chartReader = GetComponent<ChartReader>();
        chart = chartReader.ReadChart(level.chartFile, speed, diff);
        
        totalPossScore = chart.Notes[diff].Count;

        foreach (AudioClip clip in level.musicFiles)
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
        StartLevel(this.level, this.diff);
    }

    public void SetAudioPlaying(bool play)
    {
        foreach (AudioSource s in gameObject.GetComponents<AudioSource>()) 
        {
            if (play) s.UnPause(); else s.Pause();
        }
    }


    void Update () {
        // Move the "highway" towards the player
        if (_isRunning)
        {
            transform.Translate(-Vector3.forward * speed * Time.deltaTime);
            if (songAudioSources[0] != null)
                timeSlider.value = songAudioSources[0].time / songAudioSources[0].clip.length;
        }
    }

    // Placeholder scoring system for testing
    public void ReportNoteHit(/*Vector3 position*/)
    {

        score++;
        UpdateScoreVisuals();
    }

    public void ReportMissedHit()
    {
        if (allowErrors) {
            //ErrorPenalty = true;
            //Debug.Log("Invalid touch");
            score = (score * (1-errorPenaltyPercentDrop));
            
            if (score < 1) score = 0;
            UpdateScoreVisuals();
                //StartCoroutine(ErrorPenaltyTimer());
        }

    }

    //private IEnumerator ErrorPenaltyTimer()
    //{
    //    yield return new WaitForSeconds(errorPenaltyTime);
    //    ErrorPenalty = false;
    //}

    public void HeldNoteIncreaseScore()
    {
        // Holding notes adds a little to your score to make up 
        // for missing notes
        score += heldNoteBonus;
        UpdateScoreVisuals();
    }

    private void UpdateScoreVisuals()
    {
        float ratio = (score / totalPossScore);
        string scoreStr = string.Format("{0:0.00%}", ratio);
        
        scoreText.text = scoreStr;
        scoreSlider.value = ratio;
    }

}
