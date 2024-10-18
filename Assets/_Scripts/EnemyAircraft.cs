using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSS;

public class EnemyAircraft : BM_Object_mono
{
    [Header("REFERENCE")]
    private Transform trans;
	[SerializeField] private SpriteRenderer spriteRenderer_main;
	[SerializeField] private SpriteRenderer spriteRenderer_highlight;
	[SerializeField] private LSS_SelectableObject mySelectableObject;

	//Logan's Interaction System
	public int MyInteractionLayer { get; set; } = 1;
	public List<int> OpposingInteractionLayers { get; set; }
	public bool SelectOpposing { get; set; }

	public float cu_opposingHighlight = 0f;
	public float cu_damage = 0f;
	public float cd_death = 0f;

	[Header("TRUTH")]
	public bool AmNeutral = false;

	Stats_EnemyAircraft MyStats => AircraftManager.Instance.Stats_EnemyAircraft;


	private void Awake()
	{
		trans = GetComponent<Transform>();

	}

	void Start()
    {
		cu_opposingHighlight = MyStats.Duration_opposingSelectHighlight;
		cu_damage = MyStats.Duration_Damage;

		mySelectableObject.UAction_HighlightMe += HighlightedMe_action;
		mySelectableObject.UAction_UnHighlightMe += UnHighlightedMe_action;
		mySelectableObject.UAction_SelectedMeAsOpposing += SelectedMeAsOpposing_action;
		//mySelectableObject.UAction_SelectedMyOpposing
		mySelectableObject.MyInteractionLayer = 1;

		if( AmNeutral )
		{
			spriteRenderer_main.sprite = AircraftManager.Instance.NeutralSprite;
			spriteRenderer_main.color = new Color(0.7f, 0.7f, 0.7f);
			spriteRenderer_highlight.sprite = AircraftManager.Instance.NeutralSprite;
			spriteRenderer_main.color = new Color(0.7f, 0.7f, 0.7f);
		}
	}

	void Update()
    {
		if ( GameManager.AmPaused )
		{
			return;
		}

		moveAlarms();

		trans.Translate( trans.up * MyStats.Speed_move * Time.deltaTime, Space.World );

	}

	public void MakeNeutral()
	{
		AmNeutral = true;

	}

	private void moveAlarms()
	{
		if (cu_opposingHighlight < MyStats.Duration_opposingSelectHighlight)
		{
			spriteRenderer_highlight.color = LogansColorUtilities.BlipColor(new Color(1f, 1f, 1f, 0f), Color.white, spriteRenderer_highlight.color,
				MyStats.Duration_opposingSelectHighlight, cu_opposingHighlight, 0.4f
			);

			cu_opposingHighlight += Time.deltaTime;

			if (cu_opposingHighlight > MyStats.Duration_opposingSelectHighlight)
			{
				cu_opposingHighlight = MyStats.Duration_opposingSelectHighlight;
				spriteRenderer_highlight.color = Color.white;
				UnHighlightedMe_action();
			}
		}

		if (cu_damage < MyStats.Duration_Damage)
		{
			spriteRenderer_highlight.color = LogansColorUtilities.BlipColor(new Color(1f, 1f, 1f, 0f), Color.white, spriteRenderer_highlight.color,
				MyStats.Duration_opposingSelectHighlight, cu_damage, 0.4f
			);

			cu_damage += Time.deltaTime;

			if (cu_damage > MyStats.Duration_Damage)
			{
				cu_damage = MyStats.Duration_Damage;
				spriteRenderer_main.color = Color.white;
				cd_death = MyStats.Duration_Death;
			}
		}

		if (cd_death > 0f)
		{
			spriteRenderer_main.color = new Color(spriteRenderer_main.color.r, spriteRenderer_main.color.g, spriteRenderer_main.color.b,
				(cd_death / MyStats.Duration_Death));
			cd_death -= Time.deltaTime;
			if (cd_death < 0f)
			{
				cd_death = 0f;
				NAMRU_Debug.Log($"{name} death reached. Destroying...");
				DestroyMe();
			}
		}
	}

	#region NECESSARY LOGANS INTERACTION SYSTEM METHODS -------//////////
	public void HighlightedMe_action()
	{
		spriteRenderer_highlight.enabled = true;
		spriteRenderer_highlight.color = MyStats.Color_highlight;
	}

	public void SelectedMeAsOpposing_action()
	{
		NAMRU_Debug.Log( $"'{name}' SelectMeAsOpposing()" );
		HighlightedMe_action();

		spriteRenderer_highlight.color = new Color( 1f, 1f, 1f, 0f );
		cu_opposingHighlight = 0f;
		//Time.timeScale = 0.05f;
		//Debug.Log($"ts set for '{name}', clr: '{spriteRenderer_highlight.color}'");
	}

	public void UnHighlightedMe_action()
	{
		spriteRenderer_highlight.enabled = false;

	}

	public void UnSelectedMeAsOpposing_action()
	{

	}

	public void SelectedMyOpposing_action( I_LSS_Interactable interactable )
	{

	}

	#endregion
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

	public void DamageMe()
	{
		NAMRU_Debug.Log( $"{name}.DamageMe()", BM_Enums.LogDestination.none );
		cu_damage = 0f;

		AircraftManager.Instance.EnemyDied( this );
	}

	public void DestroyMe()
	{
		NAMRU_Debug.Log( $"{name} > {nameof(EnemyAircraft)}.{nameof(DestroyMe)}()", BM_Enums.LogDestination.console );
		AircraftManager.Instance.RemoveEnemy( this );
		Destroy( gameObject );
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = true;

		if( spriteRenderer_main == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError("EnemyAircraft needs main sprite renderer reference to be set via the inspector.");
		}

		if ( spriteRenderer_highlight == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError("EnemyAircraft needs highlight sprite renderer reference to be set via the inspector.");
		}

		return amKosher;
	}
}
