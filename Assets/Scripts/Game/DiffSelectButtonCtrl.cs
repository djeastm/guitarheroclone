using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiffSelectButtonCtrl : MonoBehaviour {
	
	MenuController menuController;

	public Difficulty difficulty;

	private void Awake()
	{
		menuController = GameObject.FindGameObjectWithTag("MenuController").GetComponent<MenuController>();
	}
	public void SelectDifficulty()
	{
		menuController.SelectDifficulty(difficulty);
	}
}
