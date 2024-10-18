using LoganPackages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LFP;
using UnityEngine.UI;
using TMPro;
using NAMRU_data;
using System.IO;
using System.Linq;
using System;
using LogansMultiDeviceSystem;

public class StartSceneCanvas : BM_Object_mono
{
    [SerializeField] private LogansFilePicker filePicker;

    [SerializeField] private TMP_InputField ipt_participantID;

	[SerializeField] private LogansVersionDisplayer versionDisplayer;

	[SerializeField] private LogansCanvasCursor canvasCursor;
	private RawImage ri_cursor;

	#region Options Submenu --------------------------------------
	[Header("OPTIONS SUB-MENU")]
	[SerializeField] private GameObject group_options;
	[SerializeField] private Toggle tgl_persistantLockOnLines;

	[SerializeField] private Toggle tgl_controlWithAxes;
	[SerializeField] private TMP_Dropdown dd_horizontalAxis, dd_verticalAxis;

	[SerializeField] private TMP_Dropdown dd_cursorMovementDevice, dd_hideQuadDevice;

	[SerializeField] private Slider slider_cursorSpeed;
	[SerializeField] private TextMeshProUGUI txt_cursorSpeed;

    [Header("OTHER")]
    [SerializeField] private TMP_InputField ipt_fennecIP;

	#endregion

	//[Header("ALARMS")]
	private float cd_cursorClicked = 0f;
	private float duration_cd_cursorClicked = 0.1f;

	bool havePassedStart = false;
	/// <summary>
	/// Allows me to programatically make the start screne automatically go to the battle scene without user input.
	/// </summary>
	private bool autoBypassStartScene = false;

	void Start()
    {
		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.Start() begin." );

		CheckIfKosher();

		filePicker.gameObject.SetActive(false);
		filePicker.transform.position = filePicker.transform.parent.position;

		group_options.SetActive(false);
		group_options.transform.position = group_options.transform.parent.position;

		dd_horizontalAxis.value = 0;
		dd_verticalAxis.value = 3;

		slider_cursorSpeed.value = GameManager.Speed_axisControls;
		txt_cursorSpeed.text = $"Cursor speed: {GameManager.Speed_axisControls.ToString("#.##")}";

        LMDS_Manager.Instance.PopulateDropdown( dd_cursorMovementDevice );
        LMDS_Manager.Instance.PopulateDropdown( dd_hideQuadDevice );
        // Now set the initial value of the controller dropdowns based on the default values in the game manager...
        // This is in case the controller indices change on the computer, and I therefore want to
        // change the default value of these axes on start...

        dd_cursorMovementDevice.value = GameManager.Instance.cursorDevice.Index_InDeviceList;
        dd_hideQuadDevice.value = GameManager.Instance.HideQuadDevice.Index_InDeviceList;

        versionDisplayer.SetBodyActiveState(false);
		versionDisplayer.FilePath = Path.Combine( NamruSessionManager.Instance.DirPath_NamruDirectory, "changelog.txt" );
		versionDisplayer.TryGetAndParseChangeLogFile();

		ri_cursor = canvasCursor.transform.GetChild(0).GetComponent<RawImage>();
		canvasCursor.Event_CursorClicked.AddListener( CursorClicked_action );

		havePassedStart = true;

		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.Start() end." );
	}

	private void Update()
	{
		if( cd_cursorClicked > 0f )
		{
			cd_cursorClicked -= Time.deltaTime;

			if( cd_cursorClicked < 0f )
			{
				cd_cursorClicked = 0f;
				ri_cursor.color = Color.white;
			}
		}

		if( Input.GetKeyDown(GameManager.Keycode_HideQuad) )
		{
			NAMRU_Debug.Log("Hide Quad Pressed", BM_Enums.LogDestination.momentaryDebugLogger);
		}
	}

	private void CursorClicked_action( Transform trans )
	{
		//NAMRU_Debug.Log( "cursor clicked", BM_Enums.LogDestination.momentaryDebugLogger );
		cd_cursorClicked = duration_cd_cursorClicked;
		ri_cursor.color = Color.green;

	}

	#region UI METHODS -----------------------////////////////////////
	public void UI_Btn_Start_action()
	{
		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_Btn_Start_action)}", BM_Enums.LogDestination.console );

		// Note: this whole check used to only be on the connect button method, but I decided to combine the connect button and start button logic together.
        if ( NamruSessionManager.Instance.AmListeningViaUDP )
        {
            bool rslt = NamruSessionManager.Instance.StartUDPListening();

            if (rslt == true)
            {
                DebugManager.Instance.LogMomentarily("<color=green>Succesful UDP Listening initialization.</color>");
            }
            else
            {
                NAMRU_Debug.LogError($"Had issue initializing UDP listener.");
				return;
            }
        }

        BM_SceneManager.Instance.ChangeScene( BM_SceneManager.Index_BattleScene );
	}

	public void UI_Btn_Connect_action()
	{
		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_Btn_Connect_action)}", BM_Enums.LogDestination.console );

		if ( NamruSessionManager.Instance.AmListeningViaUDP )
		{
			bool rslt = NamruSessionManager.Instance.StartUDPListening();

			if ( rslt == true )
			{
				DebugManager.Instance.LogMomentarily( "<color=green>Succesful UDP Listening initialization.</color>" );
			}
			else
			{
				NAMRU_Debug.LogError( $"Could not connect" );
			}
		}
	}
	public void UI_Btn_ChooseFile_action()
    {
        NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_Btn_ChooseFile_action)}", BM_Enums.LogDestination.console );

        filePicker.gameObject.SetActive( true );
    }

	public void UI_Btn_Options_action()
	{
		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_Btn_Options_action)}", BM_Enums.LogDestination.console );
		group_options.SetActive( true );
	}

	public void UI_Toggle_PersistantLockOnLines_action()
	{
		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_Toggle_PersistantLockOnLines_action)}.", BM_Enums.LogDestination.console );
		GameManager.UsePersistantLockOnLines = tgl_persistantLockOnLines.isOn;
	}

	public void UI_dd_Horizontal_action()
	{
		if ( !havePassedStart )
			return;

		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_dd_Horizontal_action)}.", BM_Enums.LogDestination.console );

		GameManager.Instance.HandleHorizontalAxisChoice_action( dd_horizontalAxis.options[dd_horizontalAxis.value].text.ToString() );

		NAMRU_Debug.Log( $"{nameof(GameManager.Axis_Horizontal)} set to: '{GameManager.Axis_Horizontal}'", BM_Enums.LogDestination.All );
	}

	public void UI_dd_Vertical_action()
	{
		if (!havePassedStart)
			return;

		NAMRU_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_dd_Vertical_action)}.", BM_Enums.LogDestination.console );

		GameManager.Instance.HandleVerticalAxisChoice_action( dd_verticalAxis.options[dd_verticalAxis.value].text.ToString() );

		NAMRU_Debug.Log( $"{nameof(GameManager.Axis_Vertical)} set to: '{GameManager.Axis_Vertical}'", BM_Enums.LogDestination.All );
	}

	public void UI_slider_cursorSpeed_action()
	{
		if (!havePassedStart)
			return;

		//BM_Debug.Log( $"{nameof(StartSceneCanvas)}.{nameof(UI_slider_cursorSpeed_action)}.", BM_Enums.LogDestination.console ); //I think these logs would be too much considering it's a slider
		//BM_Debug.Log($"dropdown value: '{slider_cursorSpeed}'", BM_Enums.LogDestination.console);

		GameManager.Speed_axisControls = slider_cursorSpeed.value;
		txt_cursorSpeed.text = $"Cursor speed: {GameManager.Speed_axisControls.ToString("#.##")}";
	}

	public void UI_dd_cursorMovementGamepad_action()
	{
		if (!havePassedStart)
			return;

		GameManager.Instance.HandleGamepadChoice_CursorMovement( dd_cursorMovementDevice.value );
	}

	public void UI_dd_hideQuad_action()
	{
		if (!havePassedStart)
			return;

		GameManager.Instance.HandleGamepadChoice_hideQuad( dd_hideQuadDevice.value );

	}

	public void UI_IptFld_FennecIP_action()
	{

	}
	#endregion

	public override bool CheckIfKosher()
    {
        bool amKosher = true;

        if( filePicker == null )
        {
            amKosher = false;
            NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(filePicker)} reference was null!" );
        }

		if ( versionDisplayer == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(versionDisplayer)} reference was null!" );
		}
		else
		{
			if( !versionDisplayer.CheckIfKosher() )
			{
				NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(versionDisplayer)} CheckIfKosher failed!");
			}
		}

		if ( ipt_participantID == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(ipt_participantID)} reference was null!" );
		}

		if ( canvasCursor == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError($"{nameof(StartSceneCanvas)}.{nameof(canvasCursor)} reference was null!");
		}
		if ( group_options == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(group_options)} reference was null!" );
		}

		if ( tgl_persistantLockOnLines == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(tgl_persistantLockOnLines)} reference was null!" );
		}

		if ( tgl_controlWithAxes == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError($"{nameof(StartSceneCanvas)}.{nameof(tgl_controlWithAxes)} reference was null!");
		}

		if ( dd_horizontalAxis == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(dd_horizontalAxis)} reference was null!" );
		}

		if ( dd_verticalAxis == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(dd_verticalAxis)} reference was null!" );
		}

		if ( slider_cursorSpeed == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(slider_cursorSpeed)} reference was null!" );
		}

		if ( txt_cursorSpeed == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(txt_cursorSpeed)} reference was null!" );
		}

		if ( dd_cursorMovementDevice == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(dd_cursorMovementDevice)} reference was null!" );
		}

		if ( dd_hideQuadDevice == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError($"{nameof(StartSceneCanvas)}.{nameof(dd_hideQuadDevice)} reference was null!");
		}

		if ( ipt_fennecIP == null )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"{nameof(StartSceneCanvas)}.{nameof(ipt_fennecIP)} reference was null!" );
		}

        return amKosher;
    }
}
