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

    private LevelController _levelController;

    public GameObject _mainMenu;
    public GameObject _mainMenuContentPane;
    public GameObject _pauseMenu;
    public GameObject _difficultyMenu;

    public GameObject _endLevelMenu;
    public Text _endLevelOverallScore;
    public Text _endLevelPercentage;
    public Text _endLevelNoteStreak;

    public Material[] _skyboxes;
    public GameObject[] _particleSystemsPrefabs;

    public GameObject _songSelectButtonPrefab;
    public GameObject _diffSelectButtonPrefab;

    public Level[] _levels;
    private Level _currLevel;

    private bool _isPaused;
    private bool _isLevelRunning;

    void Awake()
    {
        _levelController = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelController>();
    }

    void Start () {
        for (int i = 0; i < _levels.Length; i++) {
            GameObject songSelectButton = Instantiate(_songSelectButtonPrefab);
            songSelectButton.transform.SetParent(_mainMenuContentPane.transform);
            songSelectButton.transform.GetChild(0).GetChild(0).GetComponent<Text>()
                .text = _levels[i].artist;
            songSelectButton.transform.GetChild(0).GetChild(2).GetComponent<Text>()
                .text = _levels[i].title;
            songSelectButton.GetComponent<SongSelectButtonCtrl>().id = i;
        }
    }

    private void Pause()
    {
        Time.timeScale = 0.0f;
        _pauseMenu.SetActive(true);
        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
        _levelController.SetAudioPlaying(false);
        _isPaused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        _pauseMenu.SetActive(false);
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        _levelController.SetAudioPlaying(true);
        _isPaused = false;
    }

    private void Update()
    {
        if (_isLevelRunning && Input.GetButtonDown("Cancel"))
        {
            if (_isPaused) Resume(); else Pause();
        }
    }

    public void SelectSong(int l)
    {
        _mainMenu.SetActive(false);

        _currLevel = _levels[l];
        
        for (int i = 0; i < _currLevel.difficulties.Length; i++)
        {            
            GameObject diffSelectButton = Instantiate(_diffSelectButtonPrefab);
            diffSelectButton.transform.SetParent(_difficultyMenu.transform.GetChild(0));
            diffSelectButton.GetComponentInChildren<Text>()
                .text = _currLevel.difficulties[i].ToString();
            diffSelectButton.GetComponent<DiffSelectButtonCtrl>().difficulty = _currLevel.difficulties[i];
        }
        _difficultyMenu.SetActive(true);
        
    }

    public void SelectDifficulty(Difficulty diff)
    {
        // Change skybox
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        RenderSettings.skybox = _skyboxes[(int) diff];

        // Change particle effects
        GameObject particleSystem = Instantiate(_particleSystemsPrefabs[(int)diff]);
        particleSystem.transform.SetParent(Camera.main.transform);
        
        _levelController.StartLevel(_currLevel, diff);
        _isLevelRunning = true;
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        _difficultyMenu.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1.0f;
        // Just restart the scene
        SceneManager.LoadSceneAsync(0);
    }

    public void Restart()
    {        
        _levelController.RestartLevel();
        Resume();
    }

    public void EndLevel(PlayedLevelData playedLevelData)
    {
        _isLevelRunning = false;

        _endLevelOverallScore.text = "Score: " + playedLevelData.overallScore;        
        _endLevelPercentage.text = "Notes Hit: " + playedLevelData.notesHit
            + " / " + playedLevelData.totalNumberNotes 
            + "   " + playedLevelData.percentage;
        _endLevelNoteStreak.text = "Longest note streak: " + playedLevelData.noteStreak;
        
        _endLevelMenu.SetActive(true);
    }

}
