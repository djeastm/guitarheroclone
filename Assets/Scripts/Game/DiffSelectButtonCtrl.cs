using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffSelectButtonCtrl : MonoBehaviour {
    
    MenuController menuController;

    public Difficulty difficulty;

    public AudioSource _buttonSound;
    public AudioClip _buttonHover;

    private void Awake()
    {
        menuController = GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>();
    }
    public void SelectDifficulty()
    {
        menuController.SelectDifficulty(difficulty);
    }

    public void HoverSound()
    {
        _buttonSound.PlayOneShot(_buttonHover);
    }


}
