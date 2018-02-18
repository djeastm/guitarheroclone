using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ChartReader))]
public class LevelController : MonoBehaviour {

    private Level level;
    private Difficulty diff;

    public int speed = 8;
    private int score = 0;
    private int totalPossScore = 0;

    public int hitScoreIncrease = 10;
    public int errorPenalty = 5;
    public float errorPenaltyTime = 0.5f;
    public Text scoreText;
    private bool _isRunning;

    private Vector3 lastPositionHit;


    public bool ErrorPenalty { get; set; }

    private void Init()
    {
        score = 0;
        scoreText.text = "0.00%";

        transform.position = Vector3.zero;

        foreach (AudioSource s in gameObject.GetComponents<AudioSource>())
        {
            Destroy(s);
        }
    }

    public void StartLevel(Level level, Difficulty diff)
    {
        this.level = level;
        this.diff = diff;
        Init();
        List<Note> notes = GetComponent<ChartReader>().ReadChart(level.chartFile, speed, diff);

        totalPossScore = 0;
        foreach (Note n in notes)
        {
            totalPossScore += (int) n.length;
        }

        foreach (AudioClip clip in level.musicFiles)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.clip = clip;
            s.Play();
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
        if (_isRunning) transform.Translate(-Vector3.forward * speed * Time.deltaTime);
    }

    // Placeholder scoring system for testing
    public void ReportNoteHit(/*Vector3 position*/)
    {
        //lastPositionHit = position;
        score += hitScoreIncrease;
        UpdateScoreText();
    }

    public void ReportFretboardHit(/*Vector3 position*/)
    {
        //if ()
        ErrorPenalty = true;
        score -= errorPenalty;
        UpdateScoreText();
        StartCoroutine(ErrorPenaltyTimer());
        
    }

    private IEnumerator ErrorPenaltyTimer()
    {
        yield return new WaitForSeconds(errorPenaltyTime);
        ErrorPenalty = false;
    }

    public void HeldNoteIncreaseScore()
    {
        score++;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        float ratio = ((score / (float) totalPossScore));
        if (ratio < 0) ratio = 0;
        string scoreStr = string.Format("{0:0.00%}%", ratio);
        scoreText.text = scoreStr;
    }

}
