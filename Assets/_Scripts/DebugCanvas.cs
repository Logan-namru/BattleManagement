using LoganPackages;
using NAMRU_data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DebugCanvas : BM_Object_mono
{
	[Header("[-----------REFERENCE----------]")]
	[SerializeField, Tooltip("The canvas that has the visuals will be a child of this canvas so that the visuals can be disabled while this canvas continues to listen for debug button presses and logs errors")] private Canvas canvas_debug_visuals;
	[SerializeField] private MomentaryDebugLogger momentaryDebugLogger;

	[SerializeField] Toggle toggle_debugVisuals;
	[SerializeField] private Toggle toggle_pauseOnFire;
	[SerializeField] private Slider slider_timescale;
	[SerializeField] private TextMeshProUGUI tmp_slider_timescale;

	[SerializeField] private TextMeshProUGUI txt_logansInteractionSystem;

	[Header("[---------- REFERENCE {INPUT) ---------]")]
	[SerializeField] private Slider slider_horizontalAxis;
	[SerializeField] private TextMeshProUGUI txt_horizontalAxis;
	[SerializeField] private Slider slider_horizontalAxis1;
	[SerializeField] private TextMeshProUGUI txt_horizontalAxis1;
	[SerializeField] private Slider slider_horizontalAxis2;
	[SerializeField] private TextMeshProUGUI txt_horizontalAxis2;

	[SerializeField] private Slider slider_verticalAxis;
	[SerializeField] private TextMeshProUGUI txt_verticalAxis;
	[SerializeField] private Slider slider_verticalAxis1;
	[SerializeField] private TextMeshProUGUI txt_verticalAxis1; 
	[SerializeField] private Slider slider_verticalAxis2;
	[SerializeField] private TextMeshProUGUI txt_verticalAxis2;

	[SerializeField] private TextMeshProUGUI txt_mousePos;

	[SerializeField] private TextMeshProUGUI txt_selection;

	[Header("[---------- REFERENCE {GAME MANAGER) ---------]")]
	[SerializeField] private TextMeshProUGUI txt_gameManagerDebug;

	[Header("[---------- REFERENCE {CANVAS CUROSRO) ---------]")]
	[SerializeField] private TextMeshProUGUI txt_canvasCursorDebug;

	[Header("[---------- QUAD ---------]")]
	[SerializeField] TextMeshProUGUI txt_quadPos;


	[Header("[----------- REFERENCE [EXTERNAL] ----------]")]
	[SerializeField] private LogansCanvasCursor canvasCursor;


	/// <summary>
	/// Allows you to use shift as a button multiplier/modifier if desired.
	/// </summary>
	[SerializeField] private bool amHoldingShift = false;

	void Start()
    {
		CheckIfKosher();

        transform.position = transform.parent.position;
		canvas_debug_visuals.transform.position = transform.position;
		canvas_debug_visuals.enabled = false;

		if( tmp_slider_timescale != null )
		{
			tmp_slider_timescale.text = "1.0";
		}

		if( slider_timescale != null )
		{
			slider_timescale.value = 1f;
		}
	}

    void Update()
    {
		if( BM_SceneManager.Instance.AmPastStartScene )
		{
			txt_logansInteractionSystem.text = GameManager.Instance.LogansSelectionSystemManager.GetDiagnosticString();
		}


		if ( Input.GetKeyDown(KeyCode.F1) )
		{
			NAMRU_Debug.Log("Debug key triggered...", BM_Enums.LogDestination.console);
			toggle_debugVisuals.isOn = !toggle_debugVisuals.isOn;
			NAMRU_Debug.Log($"{nameof(DebugManager.GlobalDebugOn)}: '{DebugManager.GlobalDebugOn}'", BM_Enums.LogDestination.console);

		}

		if (DebugManager.GlobalDebugOn)
		{
			amHoldingShift = false;

			if (Input.GetKey(KeyCode.LeftShift))
			{
				amHoldingShift = true;
			}

			float hAxis = Input.GetAxis("Horizontal");
			float vAxis = Input.GetAxis("Vertical");
			slider_horizontalAxis.value = hAxis;
			txt_horizontalAxis.text = hAxis.ToString("#.##");
			slider_verticalAxis.value = vAxis;
			txt_verticalAxis.text = vAxis.ToString("#.##");

			float h1Axis = Input.GetAxis("Horizontal1");
			float v1Axis = Input.GetAxis("Vertical1");
			slider_horizontalAxis1.value = h1Axis;
			txt_horizontalAxis1.text = h1Axis.ToString("#.##");
			slider_verticalAxis1.value = v1Axis;
			txt_verticalAxis1.text = v1Axis.ToString("#.##");

			float h2Axis = Input.GetAxis("Horizontal2");
			float v2Axis = Input.GetAxis("Vertical2");
			slider_horizontalAxis2.value = h2Axis;
			txt_horizontalAxis2.text = h2Axis.ToString("#.##");
			slider_verticalAxis2.value = v2Axis;
			txt_verticalAxis2.text = v2Axis.ToString("#.##");

			txt_selection.text = $"AnyJyStckBtn0: '{Input.GetKey(KeyCode.JoystickButton0)}'\n" +
				$"Joystick1Button0: '{Input.GetKey(KeyCode.Joystick1Button0)}'\n" +
				$"Joystick2Button0: '{Input.GetKey(KeyCode.Joystick2Button0)}'\n\n" +
				$"AnyJyStckBtn1: '{Input.GetKey(KeyCode.JoystickButton1)}'\n" +
				$"Joystick1Button1: '{Input.GetKey(KeyCode.Joystick1Button1)}'\n" +
				$"Joystick2Button1: '{Input.GetKey(KeyCode.Joystick2Button1)}'\n";

			if (BM_SceneManager.Instance.AmPastStartScene)
			{
				txt_mousePos.text = canvasCursor.CalculatedPosition.ToString("#.##");

				if (Input.GetKeyDown(KeyCode.Keypad7))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.up * 0.1f;
				}
				else if (Input.GetKeyDown(KeyCode.Keypad1))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.down * 0.1f;
				}
				else if (Input.GetKeyDown(KeyCode.Keypad6))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.right * 0.1f;
				}
				else if (Input.GetKeyDown(KeyCode.Keypad4))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.left * 0.1f;
				}
				else if (Input.GetKeyDown(KeyCode.Keypad8))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.forward * 0.1f;
				}
				else if (Input.GetKeyDown(KeyCode.Keypad2))
				{
					GameManager.Instance.RenderQuad.transform.position += Vector3.back * 0.1f;
				}

				txt_quadPos.text = GameManager.Instance.RenderQuad.transform.position.ToString();

				txt_canvasCursorDebug.text = GameManager.Instance.GameCursor.GetDebugString();

			}

			txt_gameManagerDebug.text = GameManager.Instance.GetDebugString();

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} loading scenario 1 from keypress...", BM_Enums.LogDestination.All);
				GameManager.Instance.LoadScenario(0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} loading scenario 2 from keypress...", BM_Enums.LogDestination.All);

				GameManager.Instance.LoadScenario(1);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} loading scenario 3 from keypress...", BM_Enums.LogDestination.All);

				GameManager.Instance.LoadScenario(2);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} loading scenario 4 from keypress...", BM_Enums.LogDestination.All);

				GameManager.Instance.LoadScenario(3);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} loading scenario 5 from keypress...", BM_Enums.LogDestination.All);

				GameManager.Instance.LoadScenario(4);
			}

			if (Input.GetKeyDown(KeyCode.P))
			{
				NAMRU_Debug.Log($"{nameof(DebugCanvas)} > P key triggered. toggling pause/unpause from keypress...", BM_Enums.LogDestination.All);

				if (!GameManager.AmPaused)
				{
					GameManager.Instance.PauseBattleScene();
				}
				else
				{
					GameManager.Instance.UnPauseBattleScene();
				}
			}
		}
	}

	public void LogMomentarily( string msg )
	{
		momentaryDebugLogger.LogMomentarily( msg );
	}

	bool preventTimeScaleAction = false;
	public void UpdateTimeSlider(float time_passed)
	{
		preventTimeScaleAction = true;

		slider_timescale.value = time_passed;
		tmp_slider_timescale.text = time_passed.ToString("#.##");

		preventTimeScaleAction = false;
	}

	#region UI METHODS ----------------/////////////
	public void UI_ToggleDebugFeatures()
	{
		NAMRU_Debug.Log( "ToggleDebugFeatures()", BM_Enums.LogDestination.All );
		canvas_debug_visuals.enabled = toggle_debugVisuals.isOn;
		DebugManager.GlobalDebugOn = toggle_debugVisuals.isOn;

	}

	public void UI_TogglePauseOnFire_action()
	{
		NAMRU_Debug.Log($"{name}.{nameof(UI_TogglePauseOnFire_action)}()", BM_Enums.LogDestination.console);

		GameManager.Instance.PauseOnFire = toggle_pauseOnFire.isOn;
	}

	public void UI_Slider_timescale_action()
	{
		if ( preventTimeScaleAction )
		{
			return;
		}

		NAMRU_Debug.Log($"{nameof(UI_Slider_timescale_action)}()" );

		Time.timeScale = slider_timescale.value;
		tmp_slider_timescale.text = Time.timeScale.ToString("#.##");
	}

	[ContextMenu("z call UI_Btn_ResetTime_action()")]
	public void UI_Btn_ResetTime_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_ResetTime_action)}()", BM_Enums.LogDestination.All);

		DebugManager.Instance.SetTimeScale_managed(1f);
	}

	//Scenario buttons ------------------
	[ContextMenu("z call UI_Btn_pause0_action()")]
	public void UI_Btn_pause0_action()
	{
		NAMRU_Debug.Log($"{nameof(DebugCanvas)}.{nameof(UI_Btn_pause0_action)}()", BM_Enums.LogDestination.All);

		GameManager.Instance.RecieveUDPString_threadSafe( "pause=0" );
		EventSystem.current.SetSelectedGameObject( null ); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_pause1_action()")]
	public void UI_Btn_pause1_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_pause1_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe("pause=1");
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_show0_action()")]
	public void UI_Btn_show0_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_show0_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe("show=0");
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_show1_action()")]
	public void UI_Btn_show1_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_show1_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe("show=1");
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_scenario1_action()")]
	public void UI_Btn_scenario1_action()
	{
		NAMRU_Debug.Log($"{nameof(DebugCanvas)}.{nameof(UI_Btn_scenario1_action)}()", BM_Enums.LogDestination.All);

		GameManager.Instance.RecieveUDPString_threadSafe("scenario=1");
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_scenario2_action()")]
	public void UI_Btn_scenario2_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_scenario2_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe( "scenario=2" );
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_scenario3_action()")]
	public void UI_Btn_scenario3_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_scenario3_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe( "scenario=3" );
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_scenario4_action()")]
	public void UI_Btn_scenario4_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_scenario4_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe( "scenario=4" );
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}

	[ContextMenu("z call UI_Btn_scenario5_action()")]
	public void UI_Btn_scenario5_action()
	{
		NAMRU_Debug.Log( $"{nameof(DebugCanvas)}.{nameof(UI_Btn_scenario5_action)}()", BM_Enums.LogDestination.All );

		GameManager.Instance.RecieveUDPString_threadSafe( "scenario=5" );
		EventSystem.current.SetSelectedGameObject(null); // So that we don't accidentally select something else with the controller
	}
	#endregion

	[ContextMenu("z call CheckIfKosher()")]
	public override bool CheckIfKosher()
	{
		bool amKosher = true;

		if ( momentaryDebugLogger == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(momentaryDebugLogger)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( canvas_debug_visuals == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(canvas_debug_visuals)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( toggle_debugVisuals == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(toggle_debugVisuals)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( toggle_pauseOnFire == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(toggle_pauseOnFire)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( slider_timescale == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(slider_timescale)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( tmp_slider_timescale == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(tmp_slider_timescale)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( txt_logansInteractionSystem == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(txt_logansInteractionSystem)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( slider_horizontalAxis == null)
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(slider_horizontalAxis)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( txt_horizontalAxis == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(txt_horizontalAxis)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( slider_verticalAxis == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(slider_verticalAxis)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( txt_verticalAxis == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(txt_verticalAxis)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( txt_mousePos == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(txt_mousePos)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
		}

		if ( txt_selection == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(txt_selection)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( txt_gameManagerDebug == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(txt_gameManagerDebug)}'</b> reference not set inside '{nameof(DebugCanvas)}'");
		}

		if ( BM_SceneManager.Instance != null &&  BM_SceneManager.Instance.AmPastStartScene )
        {
			if ( canvasCursor == null )
			{
				amKosher = false;
				NAMRU_Debug.LogWarning( $"<b>'{nameof(canvasCursor)}'</b> reference not set inside '{nameof(DebugCanvas)}'" );
			}
		}

        return amKosher;
	}
}
