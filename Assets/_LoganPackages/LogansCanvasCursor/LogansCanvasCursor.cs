using NAMRU_data;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LoganPackages
{
    public class LogansCanvasCursor : MonoBehaviour
    {
        [Header("REFERENCE - INTERNAL")]
        [SerializeField] private RawImage ri_cursorGraphic;
		private RectTransform rectTrans;

        [Header("REFERENCE - EXTERNAL")]
        [SerializeField] private Canvas myCanvas;
		private RectTransform rectTrans_canvas;
		public Camera ReferenceCamera;

		[SerializeField] private Vector3 calculatedPosition;
		public Vector3 CalculatedPosition => calculatedPosition;

		[Header("SETTINGS")]
		[Tooltip("Determines whether 3d colliders are used in interaction, or 2d colliders")]
		public bool Am3D = false;
		public List<string> IntersectingLayers = new List<string>();
		

		[Header("CALCULATED")]
        private float boundX;
        private float boundY;
		[SerializeField] private int mask_intersectingLayers;
		private Transform trans_intersecting;

		[Header("[--- EVENTS ---]")]
		public LCC_Event Event_CursorEnteredInteractable;
		public LCC_Event Event_CursorExitedInteractable;

		public LCC_Event Event_CursorClicked;

		[Header("[--- TRUTH ---]")]
		public bool AmPaused = false;

		[Header("[--- DEBUG ---]")]	
		public bool AmDebugging = false;
		[SerializeField] private bool amCurrentlyIntersecting;

		private void OnEnable()
		{
			Event_CursorEnteredInteractable = new LCC_Event();
			Event_CursorExitedInteractable = new LCC_Event();
			Event_CursorClicked = new LCC_Event();
		}

		private void OnDisable()
		{
			Event_CursorEnteredInteractable.RemoveAllListeners();
			Event_CursorExitedInteractable.RemoveAllListeners();
			Event_CursorClicked.RemoveAllListeners();
		}

		private void Awake()
		{
			rectTrans = GetComponent<RectTransform>();
			rectTrans_canvas = myCanvas.GetComponent<RectTransform>();
			trans_intersecting = null;
		}

		void Start()
        {
			CheckIfKosher();
			calculatedPosition = rectTrans.anchoredPosition; //This is so that I can start the cursor in the position it's been set to in the unity scene
			CalculateBoundsAndLayerMasks();

        }

		public float dbgZ = 0f;

        void Update()
        {
			if( AmPaused )
			{
				return;
			}

			calculatedPosition += (
				(Vector3.right * Input.GetAxis(GameManager.Axis_Horizontal) * GameManager.Speed_axisControls * Time.deltaTime) +
				(Vector3.up * Input.GetAxis(GameManager.Axis_Vertical) * GameManager.Speed_axisControls * Time.deltaTime)
			);

			#region CORRECT FOR OFF-SCREEN ----------------------/////////////////////////////
			
			if ( Mathf.Abs(calculatedPosition.x) > boundX )
			{
				calculatedPosition.x = boundX * Mathf.Sign(calculatedPosition.x);
			}

			if ( Mathf.Abs(calculatedPosition.y) > boundY )
			{
				calculatedPosition.y = boundY * Mathf.Sign(calculatedPosition.y);
			}
			
			#endregion

			rectTrans.anchoredPosition = calculatedPosition;

			#region SELECTION/INTERACTION --------------////////////////////////////
			Ray ray = new Ray( rectTrans.position, Vector3.forward );

			Transform trans_hit = DetectIntersectionWithRay( ray );

			if ( AmDebugging )
			{
				Debug.DrawRay( ray.origin, ray.direction * 100f );
			}

			if ( trans_hit != null )
			{
				if( trans_intersecting == null )
				{
					if( AmDebugging )
					{
						Debug.Log( $"Just newly-intersected with: '{trans_hit.name}'" );
					}

					trans_intersecting = trans_hit;
					Event_CursorEnteredInteractable.Invoke( trans_hit );
				}
				else if( trans_intersecting != trans_hit )
				{
                    Event_CursorExitedInteractable.Invoke(trans_intersecting);
                    trans_intersecting = trans_hit;
                    Event_CursorEnteredInteractable.Invoke(trans_hit);
                }
			}
			else
			{
				if( trans_intersecting != null )
				{
					Event_CursorExitedInteractable.Invoke( trans_intersecting );
				}

				trans_intersecting = null;
			}
			#endregion

			if( Input.GetKeyDown(GameManager.Keycode_Select) )
			{
				Event_CursorClicked.Invoke( trans_intersecting );
			}
		}

		public Transform DetectIntersectionWithRay( Ray ray )
		{
			amCurrentlyIntersecting = false;
			Transform trans_hit = null;

			if ( Am3D )
			{
				RaycastHit hit;
				if ( Physics.Raycast(ray, out hit) ) //this works with 3d colliders
				{
					trans_hit = hit.transform;
					amCurrentlyIntersecting = true;
				}
			}
			else //battle mgmt is NOT set to 'Am3D', so BM uses this else block...
			{
				RaycastHit2D rcHit2d = Physics2D.Raycast( 
					ray.origin, ray.direction, 100f, mask_intersectingLayers 
					);

				if ( rcHit2d.collider != null )
				{
					trans_hit = rcHit2d.transform;
					amCurrentlyIntersecting = true;
				}
			}

			return trans_hit;
		}

		public void CalculateBoundsAndLayerMasks()
		{
			NAMRU_Debug.Log( $"{nameof(LogansCanvasCursor)}.{nameof(CalculateBoundsAndLayerMasks)}() start" );
			boundX = Mathf.Abs( myCanvas.GetComponent<RectTransform>().rect.x );
			boundY = Mathf.Abs(myCanvas.GetComponent<RectTransform>().rect.y );

			if( IntersectingLayers != null && IntersectingLayers.Count > 0 )
			{
				for( int i = 0; i < IntersectingLayers.Count; i++ )
				{
					mask_intersectingLayers = LayerMask.GetMask( IntersectingLayers[i] );
				}
			}

			NAMRU_Debug.Log( $"{nameof(LogansCanvasCursor)}.{nameof(CalculateBoundsAndLayerMasks)}() end. {nameof(boundX)}: '{boundX}', {nameof(boundY)}: '{boundY}'" );
		}

		public void ResetCursorPosition()
		{
			calculatedPosition = Vector3.zero;
			rectTrans.anchoredPosition = Vector3.zero;
		}

		[SerializeField] private Vector3 vTrySetCursor;
		[ContextMenu("z call TrySetCursor()")]
		public void TrySetCursor()
		{
			GetComponent<RectTransform>().anchoredPosition = vTrySetCursor;
		}

		[SerializeField] private Vector3 vTryGetPoint;
		[ContextMenu("z call TryGetPoint()")]
		public void TryGetPoint()
		{
			//Debug.Log( ReferenceCamera.ScreenToViewportPoint(GetComponent<RectTransform>().position) );
			Debug.Log(ReferenceCamera.ScreenToWorldPoint(GetComponent<RectTransform>().position)); //This seems right
		}

		public string GetDebugString()
		{
			string str = $"<b>CanvasCursor</b>\n" +
				$"{nameof(calculatedPosition)}: '{calculatedPosition}'\n" +
				$"{nameof(calculatedPosition)}: '{calculatedPosition}'\n" +
				$"{nameof(boundX)}: '{nameof(boundY)}'\n" +
				$"{nameof(boundY)}: '{boundY}'";

			return str;
		}

		[ContextMenu("z call CheckIfKosher()")]
		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if ( ri_cursorGraphic == null )
			{
				amKosher = false;
				Debug.LogError( $"{nameof(LogansCanvasCursor)}.{nameof(ri_cursorGraphic)} reference was null!" );
			}

			if ( myCanvas == null )
			{
				amKosher = false;
				Debug.LogError( $"{nameof(LogansCanvasCursor)}.{nameof(myCanvas)} reference was null!" );
			}

			Debug.Log( $"end of {nameof(StartSceneCanvas)}.CheckIfKosher(). amKosher: '{amKosher}'" );
			return amKosher;
		}
	}
}
