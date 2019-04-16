using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleGenerator : MonoBehaviour {

	private SudokuBoard board;
	
	public InputField inputField;

	void Awake () {
		
		board = GetComponent<SudokuBoard>();
		
	}
	
	void Update () {
	
		// Create starting panel if right lick is pressed
		if(Input.GetMouseButtonDown(2)) {
			Ray ray = board.mainCamera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 1000)) {
				SudokuBoard_NumberPanel hitPanel = hit.transform.GetComponent<SudokuBoard_NumberPanel>();
				if(hitPanel.interactive) {
					hitPanel.interactive = false;
					hitPanel.borderSprite.enabled = true;
				} else {
					hitPanel.interactive = true;
					hitPanel.borderSprite.enabled = false;
				}
			}
		}
	
	}
	
	public void Generate () {
		
		string genString = "";
		
		for(int i = 0; i < board.numberPanels.Length; i++) {
			if(board.numberPanels[i].interactive) {
				genString += board.numberPanels[i].currentNumber;
			} else {
				switch(board.numberPanels[i].currentNumber) {
					case 1: genString += 'q'; break;
					case 2: genString += 'w'; break;
					case 3: genString += 'e'; break;
					case 4: genString += 'r'; break;
					case 5: genString += 't'; break;
					case 6: genString += 'y'; break;
					case 7: genString += 'u'; break;
					case 8: genString += 'i'; break;
					case 9: genString += 'o'; break;
				}
			}
		}
		
		if(genString.Length != 81) {
			return;
		}
		
		inputField.text = genString;
		
	}
	
}
