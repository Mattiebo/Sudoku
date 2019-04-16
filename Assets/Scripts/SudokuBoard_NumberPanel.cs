using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SudokuBoard_NumberPanel : MonoBehaviour {

	public SpriteRenderer borderSprite;
	public TextMesh panelNumber;
	public SudokuBoard sudokuBoard;
	
	public bool interactive = true;
	
	public int currentNumber = -1;
	public int targetNumber = -1;
	
	private int panelIndex = 0;
	private int rowNumber = 0;
	private int columnNumber = 0;
	
	public bool columnIsCorrect = false;
	public bool rowIsCorrect = false;
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.K)) {
			ChangeNumber(targetNumber);
		}
	}
	
	// Work out the row and column numbers based on the array index
	public void FindRowAndColumn (int indexNum) {
	
		panelIndex = indexNum;
		
		rowNumber = (int)indexNum/9;
		columnNumber = (int)indexNum%9;
	
	}
	
	// Add the starting number and make this panel non-interactive
	public void SetStartNumber (int startNumber) {
		
		currentNumber = startNumber;
		targetNumber = startNumber;
		panelNumber.text = "" + startNumber;
		borderSprite.enabled = true;
		interactive = false;
		
	}
	
	// Change the current number. Called when the panel is clicked.
	public void ChangeNumber (int newNumber) {
		
		if(!interactive) {
			return;
		}
	
		DOTween.Kill(panelNumber.transform);
	
		if(currentNumber == newNumber) {
			// Change to no number
			
			panelNumber.text = "";
			
			// Play the click off sound
			sudokuBoard.sfxClickOff.pitch = Random.Range(0.8f, 1.5f);
			sudokuBoard.sfxClickOff.Play();
			
			currentNumber = -1;
		
		} else {
			// Change to the new number
			
			panelNumber.text = "" + newNumber;
			panelNumber.transform.localScale = Vector3.one*1.35f;
			panelNumber.transform.DOScale(Vector3.one, 0.4f);
			
			// Play the click on sound
			sudokuBoard.sfxClickOn.pitch = Random.Range(0.9f, 1.3f);
			sudokuBoard.sfxClickOn.Play();
			
			currentNumber = newNumber;
		
		}
		
		// On the board, check the row and column to see if they are correct
		sudokuBoard.CheckRowAndColumn(rowNumber, columnNumber);
	
	}
	
	// Either make the number semi-transparent to indicate it is correct or make it opaque to indicate it is incorrect or in an incomplete row/column
	public void UpdateNumberColour () {
	
		Color c = panelNumber.color;
		
		if(columnIsCorrect || rowIsCorrect) {
			panelNumber.color = new Color(c.r, c.g, c.b, 0.5f);
		} else {
			panelNumber.color = new Color(c.r, c.g, c.b, 1f);
		}
		
	}
	
}
