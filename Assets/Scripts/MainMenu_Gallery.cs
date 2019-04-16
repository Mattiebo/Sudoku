using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenu_Gallery : MonoBehaviour {

	private MainMenu mainMenu;
	
	public GameObject galleryObject;
	public CanvasGroup galleryGroup;
	public Image galleryImage;
	
	public List<int> galleryIndexes  = new List<int>(0);
	
	private int currentImage = 0;
	
	private void Awake () {
		mainMenu = GetComponent<MainMenu>();
	}
	
	// Fade on and display the gallery
	public void OpenGallery () {
		
		galleryObject.SetActive(true);
		galleryGroup.DOFade(1f, 0.5f);
		
	}
	
	// Fade out and close the menu
	private IEnumerator CloseGalleryRoutine () {
		
		galleryGroup.DOFade(0f, 0.5f);
		
		yield return new WaitForSeconds(0.5f);
		
		galleryObject.SetActive(false);
		
	}
	
	// Close function accessible by uGUI
	public void CloseGallery () {
		StartCoroutine("CloseGalleryRoutine");
	}
	
	
	// Uses a puzzle group and puzzle index to change the gallery image
	public void ChangeImage (int groupNum, int puzzleNum) {
		int imageNum = 1 + (groupNum*9) + puzzleNum;
		Sprite newImage = Resources.Load<Sprite>("Gallery Images/" + imageNum) as Sprite;
		galleryImage.sprite = newImage;
		currentImage = (groupNum*9)+puzzleNum;
	}
	
	// Used by the left and right buttons to switch to the next or previous image
	public void NextOrPreviousImage (bool isNextImage) {
	
		int switchNum = 0;
		
		for(int i = 0; i < galleryIndexes.Count; i++) {
			if(galleryIndexes[i] == currentImage) {
				if(isNextImage) {
					switchNum = i+1;
					if(switchNum >= galleryIndexes.Count) {
						switchNum = 0;
					}
				} else {
					switchNum = i-1;
					if(switchNum < 0) {
						switchNum = galleryIndexes.Count-1;
					}
				}
			}
		}
		
		int puzzleNum = (int)(galleryIndexes[switchNum]%9);
		int groupNum = (int)(galleryIndexes[switchNum]/9);
		
		ChangeImage(groupNum, puzzleNum);
	
	}
	
}
