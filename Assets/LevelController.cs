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
	public Text scoreText;
	private bool _isRunning;

	void Awake () {

	}

	void Start()
	{
		
	}


	private void Init()
	{
		score = 0;
		scoreText.text = "Score";

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
		GetComponent<ChartReader>().ReadChart(level.chartFile, speed, diff);

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
	public void ReportNoteHit()
	{
		score += 10;
		UpdateScoreText();
	}

	public void HeldNoteIncreaseScore()
	{
		score++;
		UpdateScoreText();
	}

	private void UpdateScoreText()
	{
		scoreText.text = ""+score;
	}

}
