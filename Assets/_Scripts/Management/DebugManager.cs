using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using LoganPackages;
using UnityEngine.UI;
using UnityEditor;

public class DebugManager : BM_Object_mono
{
    public static DebugManager Instance;

    [Header("[-----------REFERENCE----------]")]
    [SerializeField] DebugCanvas debugCanvas;
	public DebugCanvas DebugCanvasInstance => debugCanvas;
	[SerializeField] private MomentaryDebugLogger momentaryDebugLogger;

	[SerializeField] private List<FriendlyAircraft> debuggableFriendlies = new List<FriendlyAircraft>();
    [SerializeField] private Stats_FriendlyAircraft stats_friendlies;

	[SerializeField] private Camera mainCam, rendTexCam;

	//[Header("[-----------TRUTH----------]")]
	/// <summary>
	/// Allows different scripts to coordinate debug behavior based on a global static value.
	/// </summary>
	public static bool GlobalDebugOn = false;

	[Header("DEBUG")]
    [SerializeField] private bool drawDebugGizmos = false;
    [SerializeField] private List<BM_Object_mono> debuggableObjects = new List<BM_Object_mono>();

	private void Awake()
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DebugManager)}.Awake()", BM_Enums.LogDestination.none );

		//DontDestroyOnLoad(this); //decided that I don't need this to persist across scenes

		Instance = this;
		/*
		if ( Instance == null )
		{
			BM_Debug.Log( $"{gameObject.name} > {nameof(DebugManager)}.{nameof(Instance)} was null. Setting singleton reference to this..." );
			Instance = this;
		}
		else if ( Instance != this )
		{
			BM_Debug.Log( $"{gameObject.name} > {nameof(DebugManager)}.{nameof(Instance)} was NOT null. Destroying this..." );

			Destroy( gameObject );
		}
		*/
	}

	void Start()
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DebugManager)}.Start()", BM_Enums.LogDestination.none );

		CheckIfKosher();

		NAMRU_Debug.Log($"{gameObject.name} > {nameof(DebugManager)}.Start() end", BM_Enums.LogDestination.none);

	}

	public void LogMomentarily( string msg )
	{
		momentaryDebugLogger.LogMomentarily( msg );
	}

	public void SetTimeScale_managed(float ts)
	{
		NAMRU_Debug.Log($"{nameof(GameManager)}.{nameof(SetTimeScale_managed)}('{ts}')", BM_Enums.LogDestination.All);

        if( debugCanvas != null )
        {
            debugCanvas.UpdateTimeSlider(ts);
        }

		Time.timeScale = ts;
	}

	public override bool CheckIfKosher()
    {
        bool amKosher = true;

		if ( debugCanvas == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"<b>'{nameof(debugCanvas)}'</b> reference not set inside '{nameof(DebugManager)}'");
		}


		if (momentaryDebugLogger == null)
		{
			amKosher = false;
			NAMRU_Debug.LogWarning($"{nameof(DebugManager)}.{nameof(momentaryDebugLogger)} reference was null.");
		}

		return amKosher;
    }

	[SerializeField] private Vector3 vOffset_CamLabels = new Vector3();
	public void OnDrawGizmos()
	{
        if( !drawDebugGizmos )
        {
            return;
        }

#if UNITY_EDITOR
		if( debuggableFriendlies != null && debuggableFriendlies.Count > 0 && stats_friendlies != null )
        {
            foreach( FriendlyAircraft fa in debuggableFriendlies )
            {
                if( Application.isPlaying )
                {
					fa.DrawPlayModeGizmos( stats_friendlies );
                }
                else
                {
					fa.DrawEditorGizmos( stats_friendlies );
                }
            }
        }

		if( !Application.isPlaying )
		{
			if( mainCam != null )
			{
				Handles.Label( mainCam.transform.position + vOffset_CamLabels, "MAIN" );
			}

			if ( rendTexCam != null)
			{
				Handles.Label( rendTexCam.transform.position + vOffset_CamLabels, "RENDCAM" );
			}
		}
#endif
	}

	[ContextMenu("z call CheckIfAllAreKosher()")]
	private void CheckIfAllAreKosher()
	{
		bool amKosher = true;
		if ( !CheckIfKosher() )
		{
			amKosher = false;
		}

		if( debuggableObjects == null || debuggableObjects.Count <= 0 )
		{
			Debug.LogError( "debuggableObjects list was null or 0 count" );
			return;
		}
		else
		{
			foreach( BM_Object_mono obj in debuggableObjects )
			{
				//print($"checking '{obj.name}'...");
				if( !obj.CheckIfKosher() )
				{
					amKosher = false;
				}
			}
		}

		if ( amKosher )
		{
			Debug.Log("<color=green><b>All appears to be kosher!</b></color>");
		}
	}

	[ContextMenu("z call SimulateLogs()")]
	public void SimulateLogs()
	{
		NAMRU_Debug.LogWarning($"This is a warning log");

		NAMRU_Debug.LogError($"This is an error log");
	}

	[ContextMenu("z call TryErrorLog()")]
	public void TryErrorLog()
	{
		NAMRU_Debug.LogError("This is an error message.");
	}

	[ContextMenu("z call TryWarningLog()")]
	public void TryWarningLog()
	{
		NAMRU_Debug.LogWarning("This is a warning message.");
	}

	[ContextMenu("z call TryLogToAllDestinations()")]
	public void TryLogToAllDestinations()
	{
		NAMRU_Debug.Log("This is a warning message.", BM_Enums.LogDestination.All);
	}
}
