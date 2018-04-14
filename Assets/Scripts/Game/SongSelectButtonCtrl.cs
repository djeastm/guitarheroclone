using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongSelectButtonCtrl : MonoBehaviour {

    MenuController menuController;
    public int id;

    public AudioSource _buttonSound;
    public AudioClip _buttonHover;

    private void Awake()
    {
        menuController = GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>();
    }
    public void SelectSong()
    {
        menuController.SelectSong(id);
    }

    public void HoverSound()
    {
        _buttonSound.PlayOneShot(_buttonHover);
    }
}
