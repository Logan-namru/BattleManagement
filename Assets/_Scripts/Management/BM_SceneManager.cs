using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BM_SceneManager : BM_Object_mono
{
    public static BM_SceneManager Instance;

	public static int Index_StartScene = 0;
	public static int Index_BattleScene = 1;

	//[Header("TRUTH")]c
	public bool AmPastStartScene
	{
		get
		{
			return SceneManager.GetActiveScene().buildIndex > Index_StartScene;
		}
	}

	/// <summary>
	/// Allows me to programatically make the start screne automatically go to the battle scene without user input.
	/// </summary>
	[SerializeField] private bool autoBypassStartScene = false;

	private void Awake()
	{
		NAMRU_Debug.Log( $"{nameof(BM_SceneManager)}.Awake()" );
		DontDestroyOnLoad( this );

		if ( Instance == null )
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(BM_SceneManager)}.{nameof(Instance)} was null. Setting singleton reference to this..." );
			Instance = this;
		}
		else if ( Instance != this )
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(BM_SceneManager)}.{nameof(Instance)} was NOT null. Destroying this..." );

			Destroy( gameObject );
		}
		NAMRU_Debug.Log( $"{nameof(BM_SceneManager)}.Awake() end", BM_Enums.LogDestination.none );

	}

	void Start()
    {
		NAMRU_Debug.Log( $"{nameof(BM_SceneManager)}.Start()", BM_Enums.LogDestination.none );

		if ( autoBypassStartScene )
		{
			NAMRU_Debug.Log( $"{autoBypassStartScene} flag set to true. bypassing start scene..." );
			ChangeScene( Index_BattleScene );

		}

		NAMRU_Debug.Log($"{nameof(BM_SceneManager)}.Start() end", BM_Enums.LogDestination.none);

	}

	public void ChangeScene( int i )
	{
		NAMRU_Debug.Log( $">>>>>>>>>>>>>>>>>>>>>>>>>>>{nameof(BM_SceneManager)}.{nameof(ChangeScene)}({i})<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", BM_Enums.LogDestination.console );

		SceneManager.LoadScene( i );
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = true;


		return amKosher;
	}
}
