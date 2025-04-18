﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


// Sets the script to be executed later than all default scripts
// This is helpful for UI, since other things may need to be initialized before setting the UI
[DefaultExecutionOrder(1000)]

public class TitleUIHandler : MonoBehaviour
{

	void Start()
	{
		
	}

	// Go to the Solar System scene
	public void GoToSolarSystemScene()
	{
		SceneManager.LoadScene(1);
	}


	// Go to the Orbit scene
	public void GoToOrbitScene()
	{
		SceneManager.LoadScene(2);
	}

	public void Exit()
	{
		// Save any persistent data here

#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#else
		Application.Quit();
#endif
	}
}
