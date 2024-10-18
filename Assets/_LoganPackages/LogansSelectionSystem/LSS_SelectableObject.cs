using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LSS_SelectableObject : MonoBehaviour
{
	public int MyInteractionLayer = 0;
	public List<int> OpposingInteractionLayers;
	public LSS_SelectableObject MyOpposingObject;
	public bool SelectOpposing { get; set; }

	public UnityAction UAction_HighlightMe;
	public UnityAction UAction_UnHighlightMe;


	public UnityAction UAction_SelectedMeAsOpposing;
	public UnityAction UAction_SelectedMyOpposing;


	void Start()
    {

    }

	private void OnMouseEnter()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleCursorEnterOnInteractable(this);
	}

	/*private void OnMouseDown()
	{
		GameManager.Instance.LogansInteractionSystemManager.HandleMouseDownOnInteractable(this);

	}*/

	private void OnMouseUp()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleSelectionAttemptOnInteractable(this);

	}

	private void OnMouseExit()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleCursorExittOnInteractable(this);

	}
}
