using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour {

	public static LoadingScreen instance;
	
	public bool isLoading = false;

	public Transform[] loadingTiles;
	
	void Start () {
	
		DontDestroyOnLoad(gameObject);
		
		instance = this;
	
	}

	// Display the tiles and load the next scene
	public IEnumerator GoToScene (string sceneName) {
	
		isLoading = true;
		
		for(int i = 0; i < loadingTiles.Length; i++) {
		
			loadingTiles[i].DOScale(Vector3.one, 0.08f);
			
			yield return new WaitForSeconds(0.015f);
		
		}
		
		yield return new WaitForSeconds(0.7f);
		
		SceneManager.LoadScene(sceneName);
		
	}
	
	// Hide the tiles
	public IEnumerator ReturnFromLoad () {
	
		isLoading = false;
		
		for(int i = 0; i < loadingTiles.Length; i++) {
		
			loadingTiles[i].DOScale(Vector3.zero, 0.08f);
			
			yield return new WaitForSeconds(0.015f);
		
		}
		
	}
	
}
