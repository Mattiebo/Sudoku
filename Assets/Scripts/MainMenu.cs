using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenu : MonoBehaviour {
	
	[System.Serializable]
	public struct PuzzlesPanel {
		public RectTransform panel;
		public Button[] puzzleButtons;
		public GameObject panelButton;
		
		public Vector2 startPosition;
		public Image[] unlockImage;
		public bool[] imageIsUnlocked;
	}

	public PuzzlesPanel[] revealPanels;
	
	public Color suspendedButtonColour;
	
	private int currentPanel= -1;
	private int currentPuzzle = -1;
	
	public Text puzzleNameText;
	
	public RectTransform[] difficultyTitles;
	
	public RectTransform menuAnchor;
	
	public GameObject loadingScreenObject;
	
	private List<GameObject> previewNumbers = new List<GameObject>(0);
	public RectTransform previewAnchor;
	public GameObject previewNumberObject;
	
	private MainMenu_Gallery gallery;
	
	private void Awake () {
	
		gallery = GetComponent<MainMenu_Gallery>();
	
		// Generate a loading screen if one does not already exist
		GameObject lsObj = GameObject.Find("Load Canvas (Clone)");
		if(lsObj == null) {
			GameObject.Instantiate(loadingScreenObject);
		}
		
		// Get start positions of each panel and get number text references
		for(int i = 0; i < revealPanels.Length; i++) {
			revealPanels[i].startPosition = revealPanels[i].panel.anchoredPosition;
			
			revealPanels[i].unlockImage = new Image[9];
			revealPanels[i].imageIsUnlocked = new bool[9];
			for(int x = 0; x < 9; x++) {
				revealPanels[i].unlockImage[x] = revealPanels[i].puzzleButtons[x].transform.GetChild(0).GetComponent<Image>();
			}
		}
		
		// Unlock the image for any complete puzzles
		string unlocksString = PlayerPrefs.GetString("Unlocks String", "100000000000000000000000000000000000000000000000000000000000000000000000000000000");
		PlayerPrefs.SetString("Unlocks String", unlocksString);
		int awakePuzzleGroup = 0;
		int awakePuzzleCount = 0;
		for(int i = 0; i < unlocksString.Length; i++) {
			if(unlocksString[i] == '1') {
				revealPanels[awakePuzzleGroup].unlockImage[awakePuzzleCount].gameObject.SetActive(true);
				revealPanels[awakePuzzleGroup].imageIsUnlocked[awakePuzzleCount] = true;
			
				// Add this to the gallery indexes
				gallery.galleryIndexes.Add(i);
			}
			awakePuzzleCount++;
			if(awakePuzzleCount >= 9) {
				awakePuzzleGroup++;
				awakePuzzleCount = 0;
			}
		}
		
		// If there is a puzzle currently suspended, colour that tile yellow
		if(PlayerPrefs.GetInt("Suspended Puzzle Group", -1) >= 0) {
			Button suspendButton = revealPanels[PlayerPrefs.GetInt("Suspended Puzzle Group")].puzzleButtons[PlayerPrefs.GetInt("Suspended Puzzle Number")];
			suspendButton.targetGraphic.color = suspendedButtonColour;
		}
		
		// If this is coming back from another screen, close the load screen
		if(LoadingScreen.instance != null) {
			if(LoadingScreen.instance.isLoading) {
				StartCoroutine(LoadingScreen.instance.ReturnFromLoad());
			}
		}
		
	}
	
	// Reset Playerprefs when the logo is pressed 30 times
	private int saveResetCount = 0;
	public void IncrementReset () {
	
		saveResetCount++;
		if(saveResetCount == 30) {
			PlayerPrefs.SetString("Unlocks String", "100000000000000000000000000000000000000000000000000000000000000000000000000000000");
			StartCoroutine(LoadingScreen.instance.GoToScene("Main Menu"));
		}
	
	}
	
	void Start () {
	
		// If a new puzzle has been completed, display the new image
		if(PuzzleData.newUnlockGroup != -1 && PuzzleData.newUnlockPuzzle != -1) {
			gallery.OpenGallery();
			gallery.ChangeImage(PuzzleData.newUnlockGroup, PuzzleData.newUnlockPuzzle);
			PuzzleData.newUnlockGroup = -1;
			PuzzleData.newUnlockPuzzle = -1;
		}
	
	}
	
	// Move the panel to the centre of the menu and increase the sizeof
	// If the index is -1, return to default layout
	public void FocusOnPanel (int panelIndex) {
		StartCoroutine("FocusOnPanelRoutine", panelIndex);
	}
	
	private IEnumerator FocusOnPanelRoutine (int panelIndex) {
	
		if(currentPanel == -1 && panelIndex == -1) {
			yield break;
		}
		
		// If going to a panel menu, hide titles and other panels
		if(panelIndex != -1) {
		
			// Move title cards off-screen
			for(int i = 0; i < difficultyTitles.Length; i++) {
				difficultyTitles[i].DOAnchorPos(new Vector2(-500, difficultyTitles[i].anchoredPosition.y), 0.15f);
				yield return new WaitForSeconds(0.04f);
			}
			
			// Scale all other panels down
			for(int i = 0; i < revealPanels.Length; i++) {
				if(i != panelIndex) {
					yield return new WaitForSeconds(0.02f);
					revealPanels[i].panel.DOScale(Vector3.zero, 0.2f);
				}
			}
			
			yield return new WaitForSeconds(0.05f);
			
		}
		
		// Set up targets for the panel
		Vector2 targetPosition = new Vector2(-20, 0);
		Vector3 targetScale = Vector3.one;
		PuzzlesPanel pp = revealPanels[0];
		bool enablePanelButton = false;
		float fadeTarget = 1f;
		if(panelIndex == -1) {
			pp = revealPanels[currentPanel];
			targetPosition = pp.startPosition;
			targetScale = Vector3.one*0.325f;
			enablePanelButton = true;
			fadeTarget = 0f;
		} else {
			pp = revealPanels[panelIndex];
		}
		
		// Move the panel to position
		pp.panel.DOAnchorPos(targetPosition, 0.3f);
		pp.panel.DOScale(targetScale, 0.3f);
		
		// Disable/Enable the panel button
		pp.panelButton.SetActive(enablePanelButton);
		
		// Enable/Disable the puzzle buttons
		foreach(Button b in pp.puzzleButtons) {
			b.interactable = !enablePanelButton;
		}
		
		// Store this panel as the current panel
		currentPanel = panelIndex;
		
		// If returning to normal menu, set up titles and panels
		if(panelIndex == -1) {
			
			yield return new WaitForSeconds (0.15f);
			
			// Scale other panels on
			for(int i = revealPanels.Length-1; i >= 0; i--) {
				if(i != panelIndex) {
					yield return new WaitForSeconds(0.02f);
					revealPanels[i].panel.DOScale(Vector3.one*0.325f, 0.2f);
				}
			}
			
			// Move title cards on-screen
			for(int i = difficultyTitles.Length-1; i >= 0; i--) {
				difficultyTitles[i].DOAnchorPos(new Vector2(-400, difficultyTitles[i].anchoredPosition.y), 0.15f);
				yield return new WaitForSeconds(0.04f);
			}
			
			// Move menu to show "home" panel
			menuAnchor.DOAnchorPos(new Vector2(0, 0), 0.4f);
			
		}
		
	}
	
	// Select a puzzle based on the index. This then references STA_PuzzleSetup to get the starting board and solution
	// If the puzzle is already finished, open the gallery instead
	public void SelectPuzzle (int puzzleIndex) {
	
		currentPuzzle = puzzleIndex;
	
		if(!revealPanels[currentPanel].imageIsUnlocked[currentPuzzle]) {
			// Puzzle is not complete, display info
		
			// Change the puzzle name text
			puzzleNameText.text = GeneratePuzzleName(currentPanel, currentPuzzle);
			
			// Show the starting numbers on the grid
			GeneratePreviewBoard(currentPanel, currentPuzzle);
			
			// Move menu to show "play" panel
			menuAnchor.DOAnchorPos(new Vector2(-380, 0), 0.4f);
		} else {
			// Puzzle is complete, open gallery
			
			gallery.ChangeImage(currentPanel, currentPuzzle);
			gallery.OpenGallery();
		}
		
	}
	
	// Generate a name for the current puzzle
	
	public static string GeneratePuzzleName (int thisPanel, int thisPuzzle) {
		
		string puzzleName = "";
		if(thisPanel == 0 || thisPanel == 1 || thisPanel == 2) {
			puzzleName = "<color=#6EBB72FF>EASY</color>";
		} else if(thisPanel == 3 || thisPanel == 4 || thisPanel == 5) {
			puzzleName = "<color=#658BCCFF>MEDIUM</color>";
		} else if(thisPanel == 6 || thisPanel == 7 || thisPanel == 8) {
			puzzleName = "<color=#BD514FFF>HARD</color>";
		}
		puzzleName += " <color=#E7E7E7FF>Puzzle " + (thisPanel+1) + "-" + (thisPuzzle+1) + "</color>";
		
		return puzzleName;
		
	}
	
	// Get the puzzle data and add a preview on the menu board
	private void GeneratePreviewBoard (int thisPanel, int thisPuzzle) {
	
		// Clear the preview grid
		for(int i = 0; i < previewNumbers.Count; i++) {
			if(previewNumbers[i] != null) {
				GameObject.Destroy(previewNumbers[i]);
			}
		}
		previewNumbers = new List<GameObject>(0);
		
		// Go through the puzzle string and add preview numbers
		string puzzleString = PuzzleData.puzzleStrings[(thisPanel*9)+thisPuzzle];
		for(int i = 0; i < puzzleString.Length; i++) {
			
			int previewNum = -1;
			
			switch(puzzleString[i]) {
				case 'q': previewNum = 1; break;
				case 'w': previewNum = 2; break;
				case 'e': previewNum = 3; break;
				case 'r': previewNum = 4; break;
				case 't': previewNum = 5; break;
				case 'y': previewNum = 6; break;
				case 'u': previewNum = 7; break;
				case 'i': previewNum = 8; break;
				case 'o': previewNum = 9; break;
			}
			
			if(previewNum != -1) {
				GameObject obj = Instantiate(previewNumberObject);
				obj.transform.SetParent(previewAnchor);
				obj.transform.localScale = Vector3.one;
				float xPos = (i%9)*35.5f;
				float yPos = (i/9)*35.5f;
				obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, -yPos);
				obj.GetComponent<Text>().text = "" + previewNum;
				obj.SetActive(true);
				previewNumbers.Add(obj);
			}
	
		}
		
	}
	
	// Get a current puzzle number and load the board scene
	public void PlayCurrentPuzzle () {
		
		PuzzleData.puzzleGroup = currentPanel;
		PuzzleData.puzzleNumber = currentPuzzle;
		
		StartCoroutine(LoadingScreen.instance.GoToScene("Puzzle Board"));
		
	}
	
	
	
	// DEBUG
	// Go through each row and column and make sure there are numbers 1-9 for each one (to make sure the puzzle is completable)
	private void CheckPuzzleCompletable (int puzzleNum) {
	
		string puzzleString = PuzzleData.puzzleStrings[puzzleNum];
		
		// CHECK ROWS
		
		bool[] rowIsCorrect = new bool[9];
		for(int i = 0; i < 9; i++) {
			bool[] rowHasNumber = new bool[9];
			
			for(int x = 0; x < 9; x++) {
				char thisChar = puzzleString[(i*9)+x];
				
				switch(thisChar) {
					case 'q': thisChar = '1'; break;
					case 'w': thisChar = '2'; break;
					case 'e': thisChar = '3'; break;
					case 'r': thisChar = '4'; break;
					case 't': thisChar = '5'; break;
					case 'y': thisChar = '6'; break;
					case 'u': thisChar = '7'; break;
					case 'i': thisChar = '8'; break;
					case 'o': thisChar = '9'; break;
				}
				
				int finalNum = int.Parse(""+thisChar);
				
				rowHasNumber[finalNum-1] = true;
				
			}
			
			bool hasAllNumbers = true;
			foreach(bool b in rowHasNumber) {
				if(b == false) {
					hasAllNumbers = false;
				}
			}
			
			rowIsCorrect[i] = hasAllNumbers;
			
		}
		
		bool rowsAreCorrect = true;
		
		foreach(bool b in rowIsCorrect) {
			if(b == false) {
				rowsAreCorrect = false;
			}
		}
		
		// CHECK COLUMNS
		
		bool[] columnIsCorrect = new bool[9];
		for(int i = 0; i < 9; i++) {
			bool[] columnHasNumber = new bool[9];
			
			for(int x = 0; x < 9; x++) {
				char thisChar = puzzleString[i+(x*9)];
				
				switch(thisChar) {
					case 'q': thisChar = '1'; break;
					case 'w': thisChar = '2'; break;
					case 'e': thisChar = '3'; break;
					case 'r': thisChar = '4'; break;
					case 't': thisChar = '5'; break;
					case 'y': thisChar = '6'; break;
					case 'u': thisChar = '7'; break;
					case 'i': thisChar = '8'; break;
					case 'o': thisChar = '9'; break;
				}
				
				int finalNum = int.Parse(""+thisChar);
				
				columnHasNumber[finalNum-1] = true;
				
			}
			
			bool hasAllNumbers = true;
			foreach(bool b in columnHasNumber) {
				if(b == false) {
					hasAllNumbers = false;
				}
			}
			
			columnIsCorrect[i] = hasAllNumbers;
			
		}
		
		bool columnsAreCorrect = true;
		
		foreach(bool b in columnIsCorrect) {
			if(b == false) {
				columnsAreCorrect = false;
			}
		}
		
		string endText = " IS COMPLETABLE";
		
		if(!rowsAreCorrect || !columnsAreCorrect) {
			endText = " IS INVALID";
		}
		
		Debug.Log("PUZZLE " + puzzleNum + endText);
		
	}
	
}
