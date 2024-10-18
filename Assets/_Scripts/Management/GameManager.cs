using LSS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BM_Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using LoganPackages;
using NAMRU_data;
using UnityEditor;
using Unity.XR.CoreUtils;
using LogansMultiDeviceSystem;

public class GameManager : BM_Object_mono
{
    public static GameManager Instance;

    [Header("STATS")]
	[SerializeField] private List<BattleScenario> battleScenarios = new List<BattleScenario>();

	[Header("REFERENCE - INTERNAL")]
    [SerializeField] private LineRenderer MainLineRenderer;
    Vector3[] lrPositions;

	[Header("REFERENCE - EXTERNAL")]
	[SerializeField] private GameObject renderQuad;
	public GameObject RenderQuad => renderQuad;
	[SerializeField] private XROrigin xrOrigin;
	[SerializeField] private Transform trans_headsetRecenterObject;

    [SerializeField] private LogansCanvasCursor gameCursor;
    public LogansCanvasCursor GameCursor => gameCursor;

	[SerializeField] private LSS_Manager m_lssManager;
    public LSS_Manager LogansSelectionSystemManager => m_lssManager;

    [SerializeField] private Camera camera_main;
    [SerializeField] private Canvas canvas_main;
    public Canvas Canvas_main => canvas_main;


	[Header("TRUTH")]
	public static bool AmPaused = false;
    public static bool UsePersistantLockOnLines = true;

	private int count_enemiesKilledInCurrentScenario = 0;
	public int Count_enemiesKilledInCurrentScenario => count_enemiesKilledInCurrentScenario;

	private int count_enemiesKilledInEntireTrial = 0;
	public int Count_enemiesKilledInEntireTrial => count_enemiesKilledInEntireTrial;
	/// <summary>Flag that gets set to 1 when the render quad is visible, and 0 when not visible. Using an int instead of bool because it makes it simpler to send to fennec</summary>
	private int flag_renderQuadVisible = 1;
    /// <summary>Flag that gets set to 1 when the render quad is visible, and 0 when not visible. Using an int instead of bool because it makes it simpler to send to fennec</summary>
    public int Flag_renderQuadVisible => flag_renderQuadVisible;

	[Header("CONTROLS")]
    public static float Speed_axisControls = 160f;
    public static string Axis_Horizontal = "Horizontal1";
	public static string Axis_Vertical = "Vertical1";
    public static KeyCode Keycode_Select = KeyCode.Joystick1Button0;
	public static KeyCode Keycode_HideQuad = KeyCode.Joystick2Button1;
	public LMDS_Device cursorDevice;
	public LMDS_Device HideQuadDevice;

    [Header("[----------- OPERATOR COMMANDS ----------]")]
	private string udpString = string.Empty;
	[SerializeField] private List<string> commands_loadScenarios;

	[SerializeField] private string command_pause;
	[SerializeField] private string command_unpause;

	[SerializeField] private string command_showQuad;
	[SerializeField] private string command_hideQuad;

	//[Header("[----------- OTHER ----------]")]


	[Header("DEBUG")]
    public bool PauseOnFire = false;


    //[Header("EVENTS")]
    //public static MissileImpactEvent Event_OnMissileImpact;


    private void Awake()
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.Awake()", BM_Enums.LogDestination.none );
 
		DontDestroyOnLoad(this);

		if ( Instance == null)
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.{nameof(Instance)} was null.", BM_Enums.LogDestination.none );
		}
		else if ( Instance != this)
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.{nameof(Instance)} was NOT null. Handing off to new manager...", BM_Enums.LogDestination.none );
            HandoffSingleton( Instance );
			Destroy( Instance.gameObject );
		}

		Instance = this;

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.Awake() end", BM_Enums.LogDestination.none );
	}


	void Start()
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.Start()", BM_Enums.LogDestination.none );

		CheckIfKosher();
        lrPositions = new Vector3[2];

        if ( BM_SceneManager.Instance.AmPastStartScene )
        {
			gameCursor.ResetCursorPosition();
            gameCursor.Event_CursorEnteredInteractable.AddListener( CursorHighlightedEnemyAircraft );
			GameCursor.Event_CursorExitedInteractable.AddListener( CursorUnHighlightedEnemyAircraft );
			gameCursor.Event_CursorClicked.AddListener( CursorClicked_action );

			MainLineRenderer.enabled = false;

			if( NamruSessionManager.Instance != null && NamruSessionManager.Instance.AmUsingIni && DataManager.Instance.Flag_haveSuccesfullyValidatedIniValues )
			{
				PlaceQuad(DataManager.Instance.ReadQuadPos);
			}

			RecenterVRCamera();
		}
		else
		{
			#region HANDLE INPUT DEVICES------------------------------------------------
			if( LMDS_Manager.Instance.HasConnectedDevices )
			{
				Axis_Horizontal = cursorDevice.HorizontalAxisString;
				Axis_Vertical = cursorDevice.VerticalAxisString;

                Keycode_Select = cursorDevice.GetTargetedJoystickKeycode( KeyCode.JoystickButton0 );
				Keycode_HideQuad = HideQuadDevice.GetTargetedJoystickKeycode( KeyCode.JoystickButton1 );
            }
			else
			{
				NAMRU_Debug.LogError( $"No connected devices found. Please connect devices and restart app." );
			}
			#endregion------------------------------------------------------------------------------------
		}

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.Start() end", BM_Enums.LogDestination.none );
	}

	private void Update()
	{
		if ( Input.GetKeyDown(KeyCode.Escape) )
		{
			ProtectedQuit();
		}

		if (BM_SceneManager.Instance.AmPastStartScene)
		{
			if ( Input.GetKeyDown(KeyCode.W) )
			{
				RecenterVRCamera();
				NAMRU_Debug.Log( $"Recentered camera",BM_Enums.LogDestination.momentaryDebugLogger );
			}


			if( !string.IsNullOrEmpty(udpString) )
			{
				HandleUDPString(udpString); //doing it this way instead of calling this method via the session logger is thread safe.
				udpString = string.Empty;
			}

			if( Input.GetKeyDown(Keycode_HideQuad) )
			{
				HideRenderQuad();
			}
		}

	}

	/// <summary>
	/// Hands off data from old manager to new manager. Only call this on the newer instance.
	/// Decided to use this method/system because I want to rely on how this manager is setup with it's serialized references inside the Battle scene, which are different 
	/// than the serialized references inside the start scene, and I want the scene differences to persist/take precendence.
	/// </summary>
	/// <param name="newManager"></param>
	public void HandoffSingleton( GameManager oldManager )
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.{nameof(HandoffSingleton)}()", BM_Enums.LogDestination.none );

		//put anything you want to hand off from the old instance to the new instance in here...
    }

	public void PlaceQuad(Vector3 v)
	{
		NAMRU_Debug.Log ($"{gameObject.name} > {nameof(GameManager)}.{nameof(PlaceQuad)}({v})" );

		renderQuad.transform.position = v;
	}

	public void RecieveUDPString_threadSafe(string s)
	{
		udpString = s;
	}

	private void HandleUDPString( string s )
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.{nameof(HandleUDPString)}({s})", BM_Enums.LogDestination.console );

		if( DebugManager.GlobalDebugOn )
		{
			DebugManager.Instance.LogMomentarily( s );
		}

		if( s == command_pause )
		{
			PauseBattleScene();
		}
		else if( s == command_unpause )
		{
			UnPauseBattleScene();
		}
		else if ( s == command_hideQuad )
		{
			HideRenderQuad();
			//PauseBattleScene();
		}
		else if ( s == command_showQuad )
		{
			ShowRenderQuad();
			//PauseBattleScene();
		}
		else
		{
			if( commands_loadScenarios != null && commands_loadScenarios.Count > 0 && BM_SceneManager.Instance.AmPastStartScene )
			{
				for ( int i = 0; i < commands_loadScenarios.Count; i++ )
				{
					if( s == commands_loadScenarios[i] )
					{
						LoadScenario(i);
						ShowRenderQuad();
						PauseBattleScene();
					}
				}
			}
		}
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(GameManager)}.{nameof(HandleUDPString)}({s}) end", BM_Enums.LogDestination.none );
	}

	public void LoadScenario( int scenarioIndex )
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(LoadScenario)}({scenarioIndex})<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", BM_Enums.LogDestination.console );

		if(  scenarioIndex > battleScenarios.Count )
		{
			NAMRU_Debug.LogError( $"{nameof(LoadScenario)}() was passed an index ('{scenarioIndex}') that was too large for the list of {nameof(battleScenarios)}. Can't process request. Returning early..." );
			return;
		}

		try
		{
			AircraftManager.Instance.LoadScenario(battleScenarios[scenarioIndex] );
			gameCursor.ResetCursorPosition();
			count_enemiesKilledInCurrentScenario = 0;
		}
		catch ( System.Exception e )
		{
			NAMRU_Debug.LogError( $"Caught exception of type: '{e.GetType()}' trying to load scenario. Exception says:" );
			NAMRU_Debug.Log( e.Message );
		}


		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(LoadScenario)}({scenarioIndex}) end", BM_Enums.LogDestination.console );
	}

	[ContextMenu("z call PauseBattleScene()")]
	public void PauseBattleScene()
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(PauseBattleScene)}()", BM_Enums.LogDestination.none );

		AmPaused = true;
		gameCursor.AmPaused = true;
		m_lssManager.AmPaused = true;

		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(PauseBattleScene)}() end", BM_Enums.LogDestination.none );
	}

	[ContextMenu("z call UnPauseBattleScene()")]
	public void UnPauseBattleScene()
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(UnPauseBattleScene)}()", BM_Enums.LogDestination.none );

		AmPaused = false;
		gameCursor.AmPaused = false;
		m_lssManager.AmPaused = false;

		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(UnPauseBattleScene)}() end", BM_Enums.LogDestination.none );
	}

	[ContextMenu("z call ShowRenderQuad()")]
	public void ShowRenderQuad()
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(ShowRenderQuad)}()", BM_Enums.LogDestination.console );

		renderQuad.SetActive( true );
		flag_renderQuadVisible = 1;

		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(ShowRenderQuad)}() end", BM_Enums.LogDestination.none );
	}

	[ContextMenu("z call HideRenderQuad()")]
	public void HideRenderQuad()
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(HideRenderQuad)}()", BM_Enums.LogDestination.console );

		renderQuad.SetActive( false );
		flag_renderQuadVisible = 0;

		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(HideRenderQuad)}() end", BM_Enums.LogDestination.none );
	}

	public void RecenterVRCamera()
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(RecenterVRCamera)}()", BM_Enums.LogDestination.none );

		xrOrigin.MoveCameraToWorldLocation( trans_headsetRecenterObject.position );
		xrOrigin.MatchOriginUpOriginForward( trans_headsetRecenterObject.up, trans_headsetRecenterObject.forward );

		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(RecenterVRCamera)}() end", BM_Enums.LogDestination.none );
	}

    private void CursorHighlightedEnemyAircraft( Transform trans )
    {
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(CursorHighlightedEnemyAircraft)}('{trans.name}')", BM_Enums.LogDestination.none );

        LSS_SelectableObject interactable;
		if( trans.TryGetComponent<LSS_SelectableObject>(out interactable) )
		{
			NAMRU_Debug.Log( $"Succesfully got {nameof(LSS_SelectableObject)} component reference. Calling interaction code...", BM_Enums.LogDestination.none);
			m_lssManager.HandleCursorEnterOnInteractable( interactable );

		}
		else
		{
			NAMRU_Debug.LogError($"{nameof(GameManager)} WAS NOT able to get {nameof(LSS_SelectableObject)} component reference. Exiting early...");

		}
	}

	private void CursorUnHighlightedEnemyAircraft(Transform trans)
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(CursorUnHighlightedEnemyAircraft)}('{trans.name}')", BM_Enums.LogDestination.none);

		LSS_SelectableObject interactable;
		if ( trans.TryGetComponent<LSS_SelectableObject>(out interactable) )
		{
			NAMRU_Debug.Log( $"Succesfully got {nameof(LSS_SelectableObject)} component reference. Calling interaction code...", BM_Enums.LogDestination.none );
			m_lssManager.HandleCursorExittOnInteractable( interactable );

		}
		else
		{
			NAMRU_Debug.LogError( $"WAS NOT able to get {nameof(LSS_SelectableObject)} component reference. Exiting early..." );

		}
	}

	public void HandleGamepadChoice_CursorMovement( int dropdownNumber )
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(HandleGamepadChoice_CursorMovement)}('{dropdownNumber}')", BM_Enums.LogDestination.console );

		cursorDevice = LMDS_Manager.Instance.ConnectedDevices[dropdownNumber];

		Axis_Horizontal = cursorDevice.HorizontalAxisString;
		Axis_Vertical = cursorDevice.VerticalAxisString;
		Keycode_Select = cursorDevice.GetTargetedJoystickKeycode( KeyCode.JoystickButton0 );
		
		try
		{
			PlayerPrefs.SetString( DataManager.PlrPrfKey_CursorDeviceName, cursorDevice.DeviceName );

		}
		catch ( System.Exception e )
		{
            NAMRU_Debug.LogError($"Caught exception of type: '{e.GetType()}' trying to save default value to system. Exception says:");
            NAMRU_Debug.Log(e.Message);
        }
		

        NAMRU_Debug.Log($"{nameof(Axis_Horizontal)} set to: '{Axis_Horizontal}'. {nameof(Axis_Vertical)} set to '{Axis_Vertical}'. " +
			$"{nameof(Keycode_Select)} set to: '{Keycode_Select}'", BM_Enums.LogDestination.All);
	}

	public void HandleGamepadChoice_hideQuad( int dropdownNumber )
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(HandleGamepadChoice_hideQuad)}('{dropdownNumber}')", BM_Enums.LogDestination.console);

		HideQuadDevice = LMDS_Manager.Instance.ConnectedDevices[dropdownNumber];
		Keycode_HideQuad = HideQuadDevice.GetTargetedJoystickKeycode( KeyCode.JoystickButton1 );
        try
        {
            PlayerPrefs.SetString( DataManager.PlrPrfKey_HideQuadDeviceName, HideQuadDevice.DeviceName );

        }
        catch (System.Exception e)
        {
            NAMRU_Debug.LogError($"Caught exception of type: '{e.GetType()}' trying to save default value to system. Exception says:");
            NAMRU_Debug.Log(e.Message);
        }

        NAMRU_Debug.Log( $"{nameof(Keycode_HideQuad)} set to: '{Keycode_HideQuad}'", BM_Enums.LogDestination.All );
	}

	/// <summary>
	/// Note: I'm trying taking away these dropdowns on the start canvas and relying on a managed way of setting these axes/inputs based on the controller choice they make.
	/// </summary>
	/// <param name="s"></param>
	public void HandleHorizontalAxisChoice_action( string s )
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(HandleHorizontalAxisChoice_action)}({s})", BM_Enums.LogDestination.All);

		Axis_Horizontal = s;
	}

	/// <summary>
	/// Note: I'm trying taking away these dropdowns on the start canvas and relying on a managed way of setting these axes/inputs based on the controller choice they make.
	/// </summary>
	/// <param name="s"></param>
	public void HandleVerticalAxisChoice_action(string s)
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(HandleVerticalAxisChoice_action)}({s})", BM_Enums.LogDestination.All );
		Axis_Vertical = s;
	}

	private void CursorClicked_action( Transform trans )
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(CursorClicked_action)}()", BM_Enums.LogDestination.none );

		if( trans == null )
		{
			NAMRU_Debug.Log($"Nothing was clicked on. passed transform was null...", BM_Enums.LogDestination.none );
			m_lssManager.DeselectCurrentlySelectedInteractable();
		}
		else
		{
			LSS_SelectableObject interactable;
			if ( trans.TryGetComponent<LSS_SelectableObject>(out interactable) )
			{
				NAMRU_Debug.Log( $"Succesfully got {nameof(LSS_SelectableObject)} component reference. Calling interaction code...", BM_Enums.LogDestination.none);
				m_lssManager.HandleSelectionAttemptOnInteractable( interactable );

			}
			else
			{
				NAMRU_Debug.LogError($"WAS NOT able to get {nameof(LSS_SelectableObject)} component reference. Exiting early...");
			}
		}
	}

	public void TargetHit_action()
	{
		count_enemiesKilledInCurrentScenario++;
		count_enemiesKilledInEntireTrial++;

	}

	public void ProtectedQuit()
	{
		NAMRU_Debug.Log( $"{nameof(GameManager)}.{nameof(ProtectedQuit)}()", BM_Enums.LogDestination.none );

		if( NamruSessionManager.Instance != null )
		{
			NamruSessionManager.Instance.CloseMe();

		}

#if UNITY_EDITOR
		
			EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif

	}

	public string GetDebugString()
	{
		string str = $"<b><color=red>GameManager</color></b>\n" +
			$"{nameof(cursorDevice)}: '{cursorDevice.DeviceName}'\n" +
			$"{nameof(Keycode_Select)}: '{Keycode_Select}'\n" +
			$"{nameof(Axis_Horizontal)}: '{Axis_Horizontal}'\n" +
			$"{nameof(Axis_Vertical)}: '{Axis_Vertical}'\n\n" +

			$"{nameof(HideQuadDevice)}: '{HideQuadDevice.DeviceName}\n" +
			$"{nameof(Keycode_HideQuad)}: '{Keycode_HideQuad}";

		if ( BM_SceneManager.Instance.AmPastStartScene )
		{
			str += $"xrOrigin pos: '{xrOrigin.transform.position}', rot: '{xrOrigin.transform.rotation}'\n" +
			$"cam pos: '{xrOrigin.Camera.transform.position}', rot: '{xrOrigin.Camera.transform.rotation}'";
		}

		return str;
	}

	public override bool CheckIfKosher()
    {
        bool amKosher = true;

		if ( SceneManager.GetActiveScene().buildIndex > BM_SceneManager.Index_StartScene )
        {
		    if ( MainLineRenderer == null )
            {
                amKosher = false;
                NAMRU_Debug.LogError( "Game manager doesn't have its line renderer reference set!" );
            }

		    if ( camera_main == null )
		    {
                amKosher = false;
			    NAMRU_Debug.LogError( "Game manager doesn't have its camera reference set!" );
		    }

		    if ( canvas_main == null )
		    {
			    amKosher = false;
			    NAMRU_Debug.LogError( $"<b>'{nameof(canvas_main)}'</b> reference not set inside <b>'{nameof(GameManager)}'</b>" );
		    }

			if ( xrOrigin == null)
			{
				amKosher = false;
				NAMRU_Debug.LogError($"<b>'{nameof(xrOrigin)}'</b> reference not set inside <b>'{nameof(GameManager)}'</b>");
			}
 
			if ( trans_headsetRecenterObject == null )
			{
				amKosher = false;
				NAMRU_Debug.LogError($"<b>'{nameof(trans_headsetRecenterObject)}'</b> reference not set inside <b>'{nameof(GameManager)}'</b>");
			}

			if ( m_lssManager == null )
            {
                amKosher = false;
                NAMRU_Debug.LogError( "Game Manager doesn't have a reference set for <b>'m_lisManager'</b>" );
            }

			if ( gameCursor == null)
			{
				amKosher = false;
				NAMRU_Debug.LogError( $"{nameof(GameManager)}.{nameof(gameCursor)} reference not set" );
			}

			if ( renderQuad == null )
			{
				amKosher = false;
				NAMRU_Debug.LogError( $"{nameof(GameManager)}.{nameof(renderQuad)} reference not set" );
			}
		}

		return amKosher;
	}
}
