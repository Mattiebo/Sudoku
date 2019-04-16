using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroLoad : MonoBehaviour {

	public Transform loadingCircle;
	
	void Start () {
		StartCoroutine("LoadMainMenu");
	}

	void Update () {
		
		loadingCircle.Rotate(new Vector3(0, 0, -20) * Time.deltaTime);
		
	}
	
	// Load the main menu
	IEnumerator LoadMainMenu () {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main Menu");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
	
}
