using LSS;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FriendlyAircraft : BM_Object_mono
{
	public Stats_FriendlyAircraft MyStats => AircraftManager.Instance.Stats_FriendlyAircraft;

	[Header("REFERENCE - INTERNAL")]
	private Transform trans;
	[SerializeField] private SpriteRenderer spriteRenderer_main;
	[SerializeField] private SpriteRenderer spriteRenderer_highlight;
	[SerializeField] private LineRenderer myLineRenderer;
	[SerializeField] private LSS_SelectableObject mySelectableObject;

	[Header("REFERENCE - EXTERNAL")]
	//EnemyAircraft enemyAircraft_focused;
	private Missile myMissile;

	private EnemyAircraft enemyAircraft_focused;
	public EnemyAircraft EnemyAircraft_focused => enemyAircraft_focused;


	//Logan's Interaction System
	public int MyInteractionLayer { get; set; } = 0;
	public List<int> OpposingInteractionLayers { get; set; }
	public bool SelectOpposing { get; set; }
	//private I_LIS_Interactable interactable_opposing;

	[Header("DEBUG")]
	[SerializeField, TextArea(1,7)] private string DbgString;


	private void Awake()
	{
		NAMRU_Debug.Log( $"{name} > {nameof(FriendlyAircraft)}.Awake()" );
		trans = GetComponent<Transform>();
		myMissile = Instantiate( AircraftManager.Instance.GO_Prefab_Missile, AircraftManager.Instance.transform ).GetComponent<Missile>();
		myMissile.Init( this );
		myMissile.gameObject.SetActive( false );
		NAMRU_Debug.Log( $"{name} > {nameof(FriendlyAircraft)}.Awake() end" );
	}

	void Start()
	{
		CheckIfKosher();

		spriteRenderer_highlight.enabled = false;
		OpposingInteractionLayers = new List<int>() { 1 };

		myLineRenderer.enabled = false;

		mySelectableObject.UAction_HighlightMe += HighlightedMe_action;
		mySelectableObject.UAction_UnHighlightMe += UnHighlightedMe_action;
		mySelectableObject.UAction_SelectedMeAsOpposing += SelectedMeAsOpposing_action;
		mySelectableObject.UAction_SelectedMyOpposing += SelectedMyOpposing_action;
		mySelectableObject.MyInteractionLayer = 0;
		mySelectableObject.OpposingInteractionLayers = new List<int> { 1 };
	}

	public float dotResult;
	public float distTo;
	public bool ConditionMet;

	void Update()
	{
		if( GameManager.AmPaused )
		{
			return;
		}

		DbgString = string.Empty;
		//trans.Translate( trans.forward * Speed * Time.deltaTime );
		//trans.Translate(trans.forward * Speed * Time.deltaTime, Space.Self);
		trans.Translate( trans.up * MyStats.Speed_move * Time.deltaTime, Space.World );

		if( enemyAircraft_focused != null )
		{
			LogansTransformUtilities.LookAt_2D( trans, SpatialOrientation.Y, enemyAircraft_focused.transform, 
				MyStats.Speed_rotation * Time.deltaTime);

			dotResult = LogansTransformUtilities.ShallowDot( trans.up, enemyAircraft_focused.transform.position - trans.position );
			float distTo = Vector2.Distance( LogansTransformUtilities.ShallowVector(trans.position),
				LogansTransformUtilities.ShallowVector(enemyAircraft_focused.transform.position) );

			ConditionMet = false;

			if ( dotResult > MyStats.Threshold_consideredFacing && distTo <= MyStats.Radius_closeEnoughToFire )
			{
				ConditionMet = true;

				if( myMissile != null && !myMissile.isActiveAndEnabled )
				{
					FireMissile_action();
				}
			}

			if( GameManager.UsePersistantLockOnLines )
			{
				Vector3[] lrPositions = new Vector3[2];

				lrPositions[0] = new Vector3( trans.position.x, trans.position.y, -1f );
				lrPositions[1] = new Vector3( enemyAircraft_focused.transform.position.x, enemyAircraft_focused.transform.position.y, -1f );

				myLineRenderer.SetPositions( lrPositions );
			}
		}
	}

	public void HighlightedMe_action()
	{
		spriteRenderer_highlight.enabled = true;
	}

	public void SelectedMeAsOpposing_action()
	{
		NAMRU_Debug.Log($"'{name}' SelectMeAsOpposing()" );

	}

	public void UnHighlightedMe_action()
	{
		spriteRenderer_highlight.enabled = false;
	}

	public void UnSelectedMeAsOpposing_action()
	{

	}

	public void SelectedMyOpposing_action()
	{
		NAMRU_Debug.Log( $"'{name}.{nameof(SelectedMyOpposing_action)}(). missile active?: '{myMissile.isActiveAndEnabled}'" );

		SetFocusedEnemy( mySelectableObject.MyOpposingObject.GetComponent<EnemyAircraft>() );
	}

	public void FireMissile_action()
	{
		NAMRU_Debug.Log($"{name} > {nameof(FriendlyAircraft)}.{nameof(FireMissile_action)}()");

		myMissile.gameObject.SetActive( true );
		myMissile.FireMe( enemyAircraft_focused );

		if( GameManager.Instance.PauseOnFire )
		{
			DebugManager.Instance.SetTimeScale_managed( 0f );

		}
	}

	public void RelinquishLockOn()
	{
		NAMRU_Debug.Log( $"{name} > {nameof(FriendlyAircraft)}.{nameof(RelinquishLockOn)}", BM_Enums.LogDestination.none );

		if ( GameManager.UsePersistantLockOnLines )
		{
			myLineRenderer.enabled = false;
		}

		enemyAircraft_focused = null;
	}

	public void TargetHit_action()
	{
		NAMRU_Debug.Log( $"{name} > {nameof(FriendlyAircraft)}.{nameof(TargetHit_action)}", BM_Enums.LogDestination.console );

		//this is in case I need to do something like...i don't know, log a score to this aircraft or something...Basically, something that only this aircraft cares about 
		//(as opposed to the other friendly aircraft) when it is the one who killed an enemy craft
	}
	/*
	private void OnMouseEnter()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleCursorEnterOnInteractable(this);
	}

	private void OnMouseUp()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleSelectionAttemptOnInteractable(this);

	}

	private void OnMouseExit()
	{
		GameManager.Instance.LogansSelectionSystemManager.HandleCursorExittOnInteractable(this);

	}
	*/

	public void SetFocusedEnemy( EnemyAircraft enemy_passed )
	{
		NAMRU_Debug.Log( $"SetFocusedEnemy('{enemy_passed}'", BM_Enums.LogDestination.console );
		enemyAircraft_focused = enemy_passed;

		if( GameManager.UsePersistantLockOnLines )
		{
			myLineRenderer.enabled = true;
		}
	}

	public void DestroyMe()
	{
		NAMRU_Debug.Log( $"{name} > {nameof(FriendlyAircraft)}.{nameof(DestroyMe)}()", BM_Enums.LogDestination.console );

		Destroy( myMissile.gameObject );
		AircraftManager.Instance.RemoveFriendly( this );
		Destroy( gameObject );
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = true;

		if ( spriteRenderer_main == null )
		{
			NAMRU_Debug.LogError($"<b>'{nameof(spriteRenderer_main)}</b>' reference is null inside <b>{nameof(FriendlyAircraft)}</b>");
			amKosher = false;
		}

		if ( spriteRenderer_highlight == null )
		{
			NAMRU_Debug.LogError($"<b>'{nameof(spriteRenderer_highlight)}</b>' reference is null inside <b>{nameof(FriendlyAircraft)}</b>");
			amKosher = false;
		}

		if ( myLineRenderer == null )
		{
			NAMRU_Debug.LogError($"{nameof(FriendlyAircraft)}.<b>'{nameof(myLineRenderer)}</b>' reference is null");
			amKosher = false;
		}

		return amKosher;
	}

	public void DrawPlayModeGizmos( Stats_FriendlyAircraft stats_passed )
	{
		//Handles.DrawWireDisc( trans.position, Vector3.back, stats_passed.Radius_closeEnoughToFire );
	}

	public void DrawEditorGizmos( Stats_FriendlyAircraft stats_passed )
	{
		//Handles.DrawWireDisc( transform.position, Vector3.back, stats_passed.Radius_closeEnoughToFire );

	}
}
