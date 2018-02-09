using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Level
{
	public string title;
	public string artist;
	public float offset;
	public int resolution;
	public AudioClip[] musicFiles;
	public TextAsset chartFile;
	public Difficulty[] difficulties;
}

public class MenuController : MonoBehaviour {

	LevelController levelController;

	public GameObject mainMenu;
	public GameObject mainMenuContentPane;
	public GameObject pauseMenu;
	public GameObject difficultyMenu;

	public GameObject songSelectButtonPrefab;
	public GameObject diffSelectButtonPrefab;

	public Level[] levels;
	private Level _currLevel;

	private bool isPaused;
	private bool isLevelStarted;

	void Awake()
	{
		levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
	}

	void Start () {
		for (int i = 0; i < levels.Length; i++) {
			GameObject songSelectButton = Instantiate(songSelectButtonPrefab);
			songSelectButton.transform.SetParent(mainMenuContentPane.transform);
			songSelectButton.GetComponentInChildren<Text>()
				.text = levels[i].title + " - " + levels[i].artist;
			songSelectButton.GetComponent<SongSelectButtonCtrl>().id = i;
		}
	}

	private void Pause()
	{
		Time.timeScale = 0.0f;
		pauseMenu.SetActive(true);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		levelController.SetAudioPlaying(false);
		isPaused = true;
	}

	public void Resume()
	{
		Time.timeScale = 1.0f;
		pauseMenu.SetActive(false);
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		levelController.SetAudioPlaying(true);
		isPaused = false;
	}

	private void Update()
	{
		if (isLevelStarted && Input.GetButtonDown("Cancel"))
		{
			if (isPaused) Resume(); else Pause();
		}
	}

	public void SelectSong(int l)
	{
		mainMenu.SetActive(false);

		_currLevel = levels[l];
		
		for (int i = 0; i < _currLevel.difficulties.Length; i++)
		{
			//Debug.Log(_currLevel.difficulties[i]);
			GameObject diffSelectButton = Instantiate(diffSelectButtonPrefab);
			diffSelectButton.transform.SetParent(difficultyMenu.transform);
			diffSelectButton.GetComponentInChildren<Text>()
				.text = _currLevel.difficulties[i].ToString();
			diffSelectButton.GetComponent<DiffSelectButtonCtrl>().difficulty = _currLevel.difficulties[i];
		}
		difficultyMenu.SetActive(true);
		
	}

	public void SelectDifficulty(Difficulty diff)
	{
		levelController.StartLevel(_currLevel, diff);
		isLevelStarted = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
		difficultyMenu.SetActive(false);
	}

	public void ReturnToMainMenu()
	{
		Time.timeScale = 1.0f;
		// Just restart the scene
		SceneManager.LoadSceneAsync(0);
	}

	public void Restart()
	{
		Resume();
		levelController.RestartLevel();
	}

}
