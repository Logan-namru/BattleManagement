using UnityEngine;
using UnityEngine.Events;

namespace LSS
{
	public class LSS_Manager : MonoBehaviour
    {
		//TODO:should maybe think about putting the line renderer logic in a different class so that this class is only concerned with interaction.

		[SerializeField, Tooltip("Camera from which you want a selection to be made")] 
		private Camera selectionCamera;

		[Header("[--- LINE RENDERING ---]")]
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField, Tooltip("This is an optional alternate reference to an object you want the line renderer to follow rather than the mouse cursor. Only set this if you don't want to use mouse selection/input. IE: If you've created your own cursor object that you're using with this system.")] 
		private RectTransform altCursorObject;
		[Space(10f)]

		[Header("[--- SETTINGS ---]")]
		[Range(0f, 1f)]
		public float SelectionIndicationDuration = 0.2f;
		[Space(10f)]

		private LSS_SelectableObject interactable_currentlyArmed;
		public LSS_SelectableObject CurrentlArmedInteractable => interactable_currentlyArmed;


        private LSS_SelectableObject interactable_currentlyHoveredOver;
		public LSS_SelectableObject CurrentlyHoveredOver => interactable_currentlyHoveredOver;

		/// <summary>
		/// Selectively allows only certain interactables to be interacted with if they're on this layer
		/// </summary>
		private int m_currentInteractionLayer = 0;
		/// <summary>
		/// Selectively allows only certain interactables to be interacted with if they're on this layer
		/// </summary>
		public int CurrentInteractionLayer => m_currentInteractionLayer;


		[Header("[--- TRUTH ---]")]
		public bool AllowingMouseInput = true;
		public bool AmRenderingLine = true;
		[Tooltip("When enabled, this manager will deselect currently armed interactable when you click on a valid opposing interactable for it")]
		public bool DeselectInteractableOnOpposingClick = true;

		[Space(10f)]


		[Header("[--- EVENTS ---]")]
		public UnityEvent Event_MouseEnteredInteractable;
        public UnityEvent Event_HighlightedInteractable;
		public UnityEvent Event_MouseDownOnInteractable;
        public UnityEvent Event_SelectedInteractable;
		public UnityEvent Event_DeSelectedInteractable;
		public UnityEvent Event_OnMouseExit;
		public UnityEvent Event_UnHighlightedInteractable;

		//[Header("[--- OTHER ---]")]
		Vector3[] lrPositions;

		[Header("[--- TRUTH ---]")]
		public bool AmPaused = false;


		private void OnDisable()
		{
			Event_MouseEnteredInteractable.RemoveAllListeners();
			Event_HighlightedInteractable.RemoveAllListeners();
			Event_MouseDownOnInteractable.RemoveAllListeners();
			Event_SelectedInteractable.RemoveAllListeners();
			Event_DeSelectedInteractable.RemoveAllListeners();
			Event_OnMouseExit.RemoveAllListeners();
			Event_UnHighlightedInteractable.RemoveAllListeners();
		}

		private void Awake()
	    {

		}

		void Start()
        {
			lrPositions = new Vector3[2];
			lineRenderer.enabled = false;

		}

		public bool ForceAbsolute = false;
		void Update()
        {
			if (AmPaused)
			{
				return;
			}

			if ( AmRenderingLine && interactable_currentlyArmed != null )
			{
				Vector3 v = interactable_currentlyArmed.transform.position;
				lrPositions[0] = new Vector3( v.x, v.y, -1f );

				if( altCursorObject == null )
				{
					v = selectionCamera.ScreenToWorldPoint( Input.mousePosition );
				}
				else
				{
					if( !ForceAbsolute )
					{
						v = selectionCamera.ScreenToWorldPoint( altCursorObject.position );

					}
					else
					{
						v = altCursorObject.position;
					}
				}
				lrPositions[1] = new Vector3(v.x, v.y, -1f);


				lineRenderer.SetPositions( lrPositions );
			}

			if ( AllowingMouseInput && Input.GetMouseButtonDown(0) )
			{
				if (interactable_currentlyHoveredOver == null && interactable_currentlyArmed != null) //If mouse is clicked, there's an aircraft considered 'armed', but no aircraft was clicked on...
				{
					DeselectCurrentlySelectedInteractable();
				}
			}
		}

		#region PUBLIC API METHODS --------------------------------/////////////////////////////////////
		public void SetCurrentInteractionLayer( int layer )
		{
			if( layer < 0 )
			{
				Debug.LogError( $"LOGAN'S INTERACTION SYSTEM ERROR! You supplied a negative integer ('{layer}') for " +
					$"SetCurrentInteractionLayer(). Returning early..." );
				return;
			}

			m_currentInteractionLayer = layer;
		}

		public void HandleCursorEnterOnInteractable( LSS_SelectableObject interactable_passed )
		{
			NAMRU_Debug.Log( $"{name} > {nameof(HandleCursorEnterOnInteractable)}({nameof(interactable_passed)}: '{interactable_passed.name}')" );
			Event_MouseEnteredInteractable.Invoke();
			interactable_currentlyHoveredOver = interactable_passed;

			if ( interactable_currentlyArmed == null )
			{
				if( interactable_passed.MyInteractionLayer == m_currentInteractionLayer )
				{
					NAMRU_Debug.Log($"{nameof(interactable_passed)} was on same interaction layer as manager.{nameof(m_currentInteractionLayer)}. invoking {nameof(Event_HighlightedInteractable)}...");
					Event_HighlightedInteractable.Invoke();

					NAMRU_Debug.Log( $"Now calling {nameof(interactable_passed)}.{interactable_passed.UAction_HighlightMe}()..." );

					if( interactable_passed.UAction_HighlightMe != null ) //doing this check now bc if the scenario starts and immediately the cursor is over the passed selectable on it's start, the unity action won't work and will error..
					{
						interactable_passed.UAction_HighlightMe();
					}
				}

			}
			else if( interactable_currentlyArmed.OpposingInteractionLayers.Contains(interactable_passed.MyInteractionLayer) )
			{
				NAMRU_Debug.Log( $"opposing highlight '{interactable_passed}'" );
				interactable_passed.UAction_HighlightMe();
			}
		}

		/// <summary>
		/// Should be called by foreign script whenever user input should be considered to be attempting selection of 
		/// an interactable.
		/// </summary>
		/// <param name="interactable_passed"></param>
		public void HandleSelectionAttemptOnInteractable(LSS_SelectableObject interactable_passed )
		{
			if ( interactable_currentlyArmed == null )
			{
				if( interactable_passed.MyInteractionLayer == m_currentInteractionLayer )
				{
					SelectCurrentlySelectedInteractable( interactable_passed );
				}
			}
			else if ( interactable_currentlyArmed != interactable_passed )
			{
				if( interactable_passed.MyInteractionLayer == m_currentInteractionLayer )
				{
					DeselectCurrentlySelectedInteractable();
				}
				else if( interactable_currentlyArmed.OpposingInteractionLayers.Contains(interactable_passed.MyInteractionLayer) )
				{
					SelectOpposingSelectableOnCurrentlySelected( interactable_passed );
				}
			}

			Event_MouseDownOnInteractable.Invoke();

		}

		/// <summary>
		/// Should be called by foreign script whenever user input should be considered to be attempting de-selection of 
		/// an interactable.
		/// </summary>
		/// <param name="interactable_passed"></param>
		public void HandleCursorExittOnInteractable(LSS_SelectableObject interactable_passed )
		{
			Event_OnMouseExit.Invoke();

			interactable_currentlyHoveredOver = null;

			if( interactable_currentlyArmed == null || 
				interactable_currentlyArmed.OpposingInteractionLayers.Contains(interactable_passed.MyInteractionLayer) )
			{
				interactable_passed.UAction_UnHighlightMe();
				Event_UnHighlightedInteractable.Invoke();
			}
		}

		/*public void HandleMouseup(I_LSM_Interactable interactable_passed) //will bring this back if I think I have a use for it...
		{

		}*/

		#endregion

		#region HANDLER METHODS ---------------------------------/////////////////////////////////
		void SelectCurrentlySelectedInteractable(LSS_SelectableObject interactable_passed )
		{
			if( interactable_passed != null )
			{
				interactable_currentlyArmed = interactable_passed;
				interactable_currentlyArmed.UAction_UnHighlightMe(); //May look redundant but this is to be sure the highlight is happening.

				// Tthe following prevents the line renderer from flashing onscreen briefly.
				Vector3 v = interactable_passed.transform.position;
				lrPositions[0] = new Vector3(v.x, v.y, -1f);
                lrPositions[1] = new Vector3(v.x, v.y, -1f);
                lineRenderer.SetPositions(lrPositions);

                lineRenderer.enabled = true;

				Event_UnHighlightedInteractable.Invoke();
			}
		}

		void SelectOpposingSelectableOnCurrentlySelected(LSS_SelectableObject interactable_passed )
		{
			interactable_passed.UAction_SelectedMeAsOpposing();
			interactable_currentlyArmed.MyOpposingObject = interactable_passed;
			interactable_currentlyArmed.UAction_SelectedMyOpposing();

			if( DeselectInteractableOnOpposingClick )
			{
				DeselectCurrentlySelectedInteractable();
			}
			// make event??
		}

		public void DeselectCurrentlySelectedInteractable()
		{
			if( interactable_currentlyArmed != null )
			{
				interactable_currentlyArmed.UAction_UnHighlightMe();
				Event_DeSelectedInteractable.Invoke();
			}

			interactable_currentlyArmed = null;
			lineRenderer.enabled = false;
		}

		#endregion

		public string GetDiagnosticString()
		{
            return $"{nameof(interactable_currentlyArmed)}: {(interactable_currentlyArmed == null ? "<color=yellow>null</color>" : interactable_currentlyArmed.ToString())} \n" +
			$"{nameof(m_currentInteractionLayer)}: {m_currentInteractionLayer} \n" +
			$"{nameof(interactable_currentlyHoveredOver)}: '{(interactable_currentlyHoveredOver == null ? "null" : interactable_currentlyHoveredOver)}' \n";

        }

        public bool CheckIfKosher()
		{
			bool amKosher = true;

			if( selectionCamera == null )
			{
				amKosher = false;
				NAMRU_Debug.LogError( $"<b>'{nameof(selectionCamera)}'</b> reference is null inside <b>LIS_Manager</b>" );
			}

			if ( lineRenderer == null )
			{
				amKosher = false;
				NAMRU_Debug.LogError($"<b>'{nameof(lineRenderer)}'</b> reference is null inside <b>LIS_Manager</b>");
			}

			return amKosher;
		}
    }
}