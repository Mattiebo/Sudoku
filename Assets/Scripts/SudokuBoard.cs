using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SudokuBoard : MonoBehaviour {

	public GameObject numberPanelPrefab;
	public Transform numberPanelsParent;
	
	public SudokuBoard_NumberPanel[] numberPanels;
	
	public Camera mainCamera;
	
	public Button[] numberButtons;
	public int currentNumber = 1;
	
	public AudioSource sfxClickOn;
	public AudioSource sfxClickOff;
	
	public Text puzzleNameText;
	
	public ParticleSystem confettiParticles;
	public Transform puzzleCompleteText;
	public CanvasGroup menuGroup;
	
	private bool[] rowIsComplete = new bool[9];
	private bool[] columnIsComplete = new bool[9];
	private bool puzzleComplete = false;
	private bool exitMenuActive = false;
	
	public GameObject exitMenu;
	
	private void Start () {
	
		// Generate a grid of panels based on the prefab
		numberPanels = new SudokuBoard_NumberPanel[81];
		int xCount = 0;
		int yCount = 0;
		for(int i = 0; i < 81; i++) {
			
			GameObject panel = GameObject.Instantiate(numberPanelPrefab) as GameObject;
			panel.SetActive(true);
			panel.transform.SetParent(numberPanelsParent);
			panel.transform.localScale = Vector3.one * 0.13f;
			
			numberPanels[i] = panel.GetComponent<SudokuBoard_NumberPanel>();
			
			numberPanels[i].sudokuBoard = this;
			numberPanels[i].FindRowAndColumn(i);
			
			float xPosition = -1.692f + (xCount*0.413f) + ((xCount/3)*0.041f);
			float yPosition = 1.692f - ((yCount*0.413f) + ((yCount/3)*0.041f));
			
			xCount++;
			if(xCount >= 9) {
				xCount = 0;
				yCount++;
			}
			
			panel.transform.localPosition = new Vector3(xPosition, yPosition, 0);
			
		}
		
		// Populate the grid with hidden and visible numbers based on the puzzle string
		int puzzleNum = (PuzzleData.puzzleGroup*9) + PuzzleData.puzzleNumber;
		if(puzzleNum >= 0) {
			SetupBoard(PuzzleData.puzzleStrings[puzzleNum]);
			
			// Change the puzzle name
			puzzleNameText.text = MainMenu.GeneratePuzzleName(PuzzleData.puzzleGroup, PuzzleData.puzzleNumber);
		}
		
		// Check if this puzzle is currently suspended, and if so fill it with the relevent data
		if(PlayerPrefs.GetInt("Suspended Puzzle Group", -1) == PuzzleData.puzzleGroup && PlayerPrefs.GetInt("Suspended Puzzle Number", -1) == PuzzleData.puzzleNumber) {
			string suspendString = PlayerPrefs.GetString("Suspended Puzzle Data");
			Debug.Log("suspendString: " + suspendString);
			if(suspendString.Length == numberPanels.Length) {
				for(int i = 0; i < numberPanels.Length; i++) {
					if(suspendString[i] != '0' && numberPanels[i].interactive) {
						numberPanels[i].ChangeNumber(int.Parse(suspendString[i]+""));
					}
				}
			}
		}
		
		// Remove loading screen tiles
		if(LoadingScreen.instance != null) {
			StartCoroutine(LoadingScreen.instance.ReturnFromLoad());
		}
		
	}
	
	private void Update () {
	
		// Check for inputs and raycast towards panels when screen is pressed
		if(Input.GetMouseButtonDown(0) && !puzzleComplete && !exitMenuActive) {
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 1000)) {
				SudokuBoard_NumberPanel hitPanel = hit.transform.GetComponent<SudokuBoard_NumberPanel>();
				hitPanel.ChangeNumber(currentNumber);
			}
		}
	
	}
	
	// Change the current number when one of the sidebar number buttons is pressed
	public void ChangeNumber (int newNumber) {
		
		currentNumber = newNumber;
		
		// Make number buttons interactable/non-interactable
		for(int i = 0; i < numberButtons.Length; i++) {

			if(i+1 == newNumber) {
				numberButtons[i].interactable = false;
			} else {
				numberButtons[i].interactable = true;
			}
		
		}
		
	}
	
	// Take a string of 81 characters and convert it into a sudoku board
	// Numbers represent hidden numbers and letters represent numbers shown at the start
	// Letters correspond with the key directly under the number key on a QWERTY keyboard
	private void SetupBoard (string b) {
	
		for(int i = 0; i < b.Length; i++) {
		
			switch(b[i]) {
			
				default: // Convert number to hidden number
					numberPanels[i].targetNumber = int.Parse(b[i]+"");
				break;
				
				case 'q': // Default 1
					numberPanels[i].SetStartNumber(1);
				break;
				
				case 'w': // Default 2
					numberPanels[i].SetStartNumber(2);
				break;
				
				case 'e': // Default 3
					numberPanels[i].SetStartNumber(3);
				break;
				
				case 'r': // Default 4
					numberPanels[i].SetStartNumber(4);
				break;
				
				case 't': // Default 5
					numberPanels[i].SetStartNumber(5);
				break;
				
				case 'y': // Default 6
					numberPanels[i].SetStartNumber(6);
				break;
				
				case 'u': // Default 7
					numberPanels[i].SetStartNumber(7);
				break;
				
				case 'i': // Default 8
					numberPanels[i].SetStartNumber(8);
				break;
				
				case 'o': // Default 9
					numberPanels[i].SetStartNumber(9);
				break;
			
			}
		
		}
	
	}
	
	// Based on a row and column provided by a number panel, check through the other numbers and see if the correct numbers are present
	// If all of the numbers are present, dim the numbers to reflect this
	public void CheckRowAndColumn (int rowNum, int columnNum) {
		
		// Check column
		SudokuBoard_NumberPanel[] columnPanels = new SudokuBoard_NumberPanel[0];
		bool columnIsCorrect = CheckRowOrColumn(columnNum, true, out columnPanels);
		columnIsComplete[columnNum] = columnIsCorrect;
		
		// Go through each number panel and change the columnIsCorrect variable
		foreach(SudokuBoard_NumberPanel np in columnPanels) {
			np.columnIsCorrect = columnIsCorrect;
		}
		
		// Check row
		SudokuBoard_NumberPanel[] rowPanels = new SudokuBoard_NumberPanel[0];
		bool rowIsCorrect = CheckRowOrColumn(rowNum, false, out rowPanels);
		rowIsComplete[rowNum] = rowIsCorrect;
		
		// Go through each number panel and change the rowIsCorrect variable
		foreach(SudokuBoard_NumberPanel np in rowPanels) {
			np.rowIsCorrect = rowIsCorrect;
		}
		
		// Combine the arrays and update each of the number panels to possible change the number colour
		SudokuBoard_NumberPanel[] allPanels = new SudokuBoard_NumberPanel[columnPanels.Length + rowPanels.Length];
		Array.Copy(columnPanels, 0, allPanels, 0, columnPanels.Length);
		Array.Copy(rowPanels, 0, allPanels, columnPanels.Length, rowPanels.Length);
		foreach(SudokuBoard_NumberPanel np in allPanels) {
			np.UpdateNumberColour();
		}
		
		// Check if the puzzle is now complete
		CheckPuzzleComplete();
		
	}
	
	// Used by CheckRowAndColumn as a generic way to check wither a column or a row. Returns a bool for if the section is correct, plus an array of number panels
	public bool CheckRowOrColumn (int checkNumber, bool isColumn, out SudokuBoard_NumberPanel[] theseNumberPanels) {
		
		theseNumberPanels = new SudokuBoard_NumberPanel[9];
		int correctNumbers = 0;
		
		int i = 0;
		while(i < 9) {
		
			// Find the number panel by incrementing based on if it's a row or a column
			SudokuBoard_NumberPanel np = null;
			if(isColumn) {
				np = numberPanels[checkNumber+(i*9)];
			} else {
				np = numberPanels[(checkNumber*9)+i];
			}
			theseNumberPanels[i] = np;
			
			// Check if this is the correct number
			if(np.currentNumber != -1) {
				if(np.currentNumber == np.targetNumber) {
					// Correct number found
					correctNumbers++;
				}
			}
			
			i++;
		}
		
		bool returnBool = false;
		if(correctNumbers == 9) {
			returnBool = true;
		}
		return returnBool;
		
	}
	
	// Check if the puzzle is complete, and if so play the complete routine
	private void CheckPuzzleComplete () {
	
		bool rowCheck = true;
		foreach(bool b in rowIsComplete) {
			if(b == false) {
				rowCheck = false;
			}
		}
		
		bool columnCheck = true;
		foreach(bool b in columnIsComplete) {
			if(b == false) {
				columnCheck = false;
			}
		}
		
		if(rowCheck && columnCheck) {
			StartCoroutine("PuzzleCompleteRoutine");
		}
	
	}
	
	// Play confetti, victory music and then switch back to the main menu
	private IEnumerator PuzzleCompleteRoutine () {
		
		// Set status to prevent further adjustments to the board
		puzzleComplete = true;
		menuGroup.interactable = false;
		
		// Add this level to the unlock string
		string unlocks = PlayerPrefs.GetString("Unlocks String");
		string newString = "";
		for(int i = 0; i < unlocks.Length; i++) {
			if(i == ((PuzzleData.puzzleGroup*9) + PuzzleData.puzzleNumber)) {
				newString += "1";
			} else {
				newString += unlocks[i];
			}
		}
		PlayerPrefs.SetString("Unlocks String", newString);
		PuzzleData.newUnlockGroup = PuzzleData.puzzleGroup;
		PuzzleData.newUnlockPuzzle = PuzzleData.puzzleNumber;
		yield return new WaitForSeconds(1f);
		
		// Display the "puzzle complete" text and play the confetti particle system
		confettiParticles.Play();
		puzzleCompleteText.DOScale(Vector3.one, 0.5f);
		yield return new WaitForSeconds(4f);
		
		// Exit to the main menu
		StartCoroutine(LoadingScreen.instance.GoToScene("Main Menu"));
		
	}
	
	// Open or close the exit confirmation menu
	public void ToggleExitMenu (bool openMenu) {
		exitMenu.SetActive(openMenu);
		exitMenuActive = openMenu;
	}
	
	// Exit to the main menu
	public void ExitToMainMenu () {
		StartCoroutine(LoadingScreen.instance.GoToScene("Main Menu"));
	}
	
	// Save progress on this puzle and return to the main menu
	public void SuspendPuzzle () {
	
		// Create a string of values based on the filled-in tiles
		string suspendString = "";
		foreach(SudokuBoard_NumberPanel np in numberPanels) {
			if(np.currentNumber == -1) {
				suspendString += "0";
			} else {
				suspendString += np.currentNumber;
			}
		}
		
		// Save the string of values and the puzzle number to playerprefs
		PlayerPrefs.SetString("Suspended Puzzle Data", suspendString);
		PlayerPrefs.SetInt("Suspended Puzzle Group", PuzzleData.puzzleGroup);
		PlayerPrefs.SetInt("Suspended Puzzle Number", PuzzleData.puzzleNumber);
		
		// Exit to the main menu
		ExitToMainMenu();
	
	}
	
}
