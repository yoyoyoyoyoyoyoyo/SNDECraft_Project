using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lab4;

/**
 * This class deals selecting units and unit groups
 * The selection code is clunky and has some redundant parts as it evolved organically from a different example.
 * I haven't had the time to rewrite it yet, but I'll clean it up eventiually.
 * We'll create more intelligent unit movement behaviours later
 */ 

namespace Lab4 {
	public class UnitSelectMover : MonoBehaviour {

		public LayerMask mask;						// Mask for the raycast placement
		public Transform target;					// Target destination
		private List<AIWithPathfinding> selectedAI; // List of selected units
		private Vector3 start_box;					//Start position for selection box
		private Vector3 end_box;					//End position for selection box
		public Texture selectionImage;				//Image for selection box
		private Rect boundbox;						//Bounding box for the units in 3D world coordinates
		private Rect screenBox;						//Bounding box for the units in screen coordinates
		private bool drawBox = false;				//Flag for drawing the box
		private bool settingPatrol = false;
		//private bool settingAttackMove = false;
		
		// This variable will store the location of wherever we first click before dragging.
		
		/** Determines if the target position should be updated every frame or only on double-click */
		
		Camera cam;
		
		public void Start () {
			//Cache the Main Camera
			cam = Camera.main;
			selectedAI = new List<AIWithPathfinding>();
		}
		
		public void OnGUI () {

			if (start_box != -Vector3.one)
			{
				GUI.color = new Color(1, 1, 1, 0.25f);
				if (drawBox)
					GUI.DrawTexture(screenBox, selectionImage);
				//GUI.Box(screenBox, "Selection");
			}
	
			//If the user releases the right mouse button
			if (cam != null && Input.GetMouseButtonUp(1)) {
				//...remove dead units from the selection group
				UpdateSelection();
				//...and move the selected units to that position
				if(settingPatrol)
				{
					StartPatrolling();
					settingPatrol = false;
				}
				else
				{
					MoveUnit();
					//settingAttackMove = false;
				}

			//If left mound button up
			}else if (cam != null && Input.GetMouseButtonUp(0)) {
				//Select the unit(s)
				SelectUnit();
				//and disable the selection box
				drawBox = false;
			}
		}


		//Select a unit or deselect all units
		public void SelectUnit() {
			RaycastHit hit;
			//Cast a ray
			if (Physics.Raycast	(cam.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, mask) && hit.point != target.position) {
				string hitMask = LayerMask.LayerToName(hit.transform.gameObject.layer);
				//if ground is hit, deselect all units
				if (hitMask == "Ground"){
					SelectAll(false);
					selectedAI.Clear();
				//Otherwise select the unit that was clicked on
				}else if (hitMask == "Unit" ) {
					SelectAll(false);
					selectedAI.Clear();
					AIWithPathfinding ai = hit.transform.GetComponent<AIWithPathfinding>();
					//ai.target = target;
					ai.selected = true;
					selectedAI.Add(ai);
					SelectAll(true);
					print ("Object selected");
				}
			}

		}

		public void StartPatrolling()
		{
			RaycastHit hit;
			//Cast a ray
			if (Physics.Raycast	(cam.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, mask) && hit.point != target.position) {
				string hitMask = LayerMask.LayerToName(hit.transform.gameObject.layer);
				print ("Hit "+hit.transform.name+" from layer "+hitMask);
				//If the ray hit the groud, move all selected units
				if (hitMask == "Ground" && selectedAI.Count != 0){
					target.position = hit.point;
					foreach (AIWithPathfinding ai in selectedAI){
						ai.StartPatrolling (target.position);
						ai.FindPath();
					}
				}
			}
		}
	
		//Move selected units
		public void MoveUnit () {
			//Fire a ray through the scene at the mouse position and place the target where it hits
			RaycastHit hit;
			//Cast a ray
			if (Physics.Raycast	(cam.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, mask) && hit.point != target.position) {
				string hitMask = LayerMask.LayerToName(hit.transform.gameObject.layer);
				print ("Hit "+hit.transform.name+" from layer "+hitMask);
				//If the ray hit the groud, move all selected units
				if (hitMask == "Ground" && selectedAI.Count != 0){
					target.position = hit.point;
					foreach (AIWithPathfinding ai in selectedAI){
						/*if(settingAttackMove)
						{
							ai.StartAttackMoving();
						}
						else
						{*/
							ai.IssueUserCommand();
						//}
						ai.target = target;
						ai.FindPath();
					}
				}
			}
		}

		/** 
		 * Remove all null references from list of select units
		 * Null referecnes would be from player units that were destroyed after they were selected
		 */
		void UpdateSelection(){
			//Pass a lambda expression to the RemoveAll method
			selectedAI.RemoveAll(a => a == null);
		}

		/**
		 * Regular update
		 */ 
		void Update() {
			//Start drawing selection box on left mouse down
			if(Input.GetMouseButtonDown(0)) {
				settingPatrol = false;
				//settingAttackMove = false;
				start_box = Input.mousePosition;
				drawBox = true;
			}

			//If user presses "s", top the current movement
			if (Input.GetKey (KeyCode.S)){
				settingPatrol = false;
				//settingAttackMove = false;
				StopAll();
			}

			if(Input.GetKey (KeyCode.H)){
				settingPatrol = false;
				//settingAttackMove = false;
				HoldPositionAll();
			}

			if(Input.GetKey (KeyCode.P)){
				settingPatrol = true;
				//settingAttackMove = false;
			}

			/*if(Input.GetKey (KeyCode.A))
			{
				settingAttackMove = true;
				settingPatrol = false;
			}*/

			//Keep drawing selection box on left mouse down
			if(Input.GetMouseButton(0)){
				end_box = Input.mousePosition;
				makeBox();
				//printy(GUIUtility.ScreenToGUIPoint(Input.mousePosition));
				//print(boundbox);
			}

			//On mouse button up, select all unts within the box
			if(Input.GetMouseButtonUp(0)) {
				//Disable box
				drawBox = false;
				//Get end box corner
				end_box = Input.mousePosition;
				//Create box
				makeBox();

				//Iterate through all selectable objects and check which ones are in the box
				GameObject[] csel = GameObject.FindGameObjectsWithTag("Selectable");
				for (int i = 0; i < csel.Length; i++) {
					//Convert object position to screen coordinated
					Vector3 objectlocation = Camera.main.WorldToScreenPoint(new Vector3(csel[i].transform.position.x,csel[i].transform.position.y,csel[i].transform.position.z));
					
					//If the object falls inside the screen box set its state to selected so we can use it later
					if(boundbox != null && boundbox.Contains(objectlocation)) {
						//csel[i].SendMessage("setisSelected", true);    
						AIWithPathfinding ai = csel[i].GetComponent<AIWithPathfinding>();
						//ai.target = target;
						selectedAI.Add(ai);
					}
					SelectAll(true);
				}
			}
		}

		//Create a selection box
		private void makeBox() {
			//Ensures the bottom left and top right values are correct
			//regardless of how the user boxes units
			float xmin = Mathf.Min(start_box.x, end_box.x);
			float ymin = Mathf.Min(start_box.y, end_box.y);

			float width = Mathf.Max(start_box.x, end_box.x) - xmin;
			float height = Mathf.Max(start_box.y, end_box.y) - ymin;
			boundbox = new Rect(xmin, ymin, width, height);

			//Create box in sreen coordinated
			screenBox = new Rect(start_box.x, Screen.height-start_box.y, end_box.x-start_box.x, start_box.y-end_box.y);
		}

		//Stop all selected units
		private void StopAll() {
			foreach (AIWithPathfinding ai in selectedAI){
				ai.Stop();
			}
		}

		private void HoldPositionAll()
		{
			foreach (AIWithPathfinding ai in selectedAI){
				ai.HoldPosition();
			}
		}

		//Set all units in selectd group as selected
		private void SelectAll(bool status) {
			foreach (AIWithPathfinding ai in selectedAI){
				ai.highlight.SetActive(status);
				ai.selected = status;
			}
		}
	}
}
					               
