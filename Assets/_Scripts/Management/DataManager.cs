using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using NAMRU_data;
using UnityEngine.UIElements;
using LoganPackages;
using System.Net;
using LogansMultiDeviceSystem;

public class DataManager : BM_Object_mono
{
	public static DataManager Instance;

	[Header("[----------- INI ----------]")]
	private bool flag_haveSuccesfullyValidatedIniValues = false;
	public bool Flag_haveSuccesfullyValidatedIniValues => flag_haveSuccesfullyValidatedIniValues;
	private Vector3 readInQuadPos = Vector3.zero;
	public Vector3 ReadQuadPos => readInQuadPos;
	private IPAddress readInFennecIP = null;


	[Header("[-------- PLAYERPREFS --------]")]
    public static string PlrPrfKey_CursorDeviceName = "BM_CURSOR_DEVICE_NAME";
    public static string PlrPrfKey_HideQuadDeviceName = "BM_HIDEQUAD_DEVICE_NAME";
	public static string PlrPrfKey_FennecIPAddress = "BM_FENNEC_IP_ADDRESS";

    private void Awake()
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.Awake()" );
		DontDestroyOnLoad(this);

		if( Instance == null )
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.{nameof(Instance)} was null. Setting singleton reference to this..." );
			Instance = this;

			if( NamruSessionManager.Instance != null && !NamruSessionManager.Instance.AmPastFirstLoad )
			{
				NamruSessionManager.Instance.InitializePreliminaryPaths();

				if(NamruSessionManager.Instance.AmDoingLog )
				{
					NamruSessionManager.Instance.TryToFindOrCreateCurrentSessionDirectory();
					NamruSessionManager.Instance.TryToStartLogFileWriter();
				}

				if (NamruSessionManager.Instance.AmUsingIni )
				{
					NAMRU_Debug.Log("sessionmanager says it's using ini. Now asking it to read the ini values...");
					NamruSessionManager.Instance.TryToLoadIni();

					if ( NamruSessionManager.Instance.Flag_HaveSuccesfullyReadAndParsedValidIniFile )
					{
						NAMRU_Debug.Log( $"Located and read ini file...", BM_Enums.LogDestination.All );
						flag_haveSuccesfullyValidatedIniValues = ValidateAndHandleIniFile();

						print($"null: '{NamruSessionManager.Instance == null}'");

						if ( flag_haveSuccesfullyValidatedIniValues )
						{
							NamruSessionManager.Instance.FennecIPAddressString = readInFennecIP.ToString(); //now trying player prefs...

							NAMRU_Debug.Log( $"Read quad position: '{readInQuadPos}'", BM_Enums.LogDestination.All );
							NAMRU_Debug.Log( $"Read fennec ip address: '{readInFennecIP}'", BM_Enums.LogDestination.All ); //now trying player prefs...

                        }
					}
				}

				//now using player prefs note: did this need to be started in awake()? I'm now trying it in start()...
				if ( NamruSessionManager.Instance.AmFennecSending )
				{
					NamruSessionManager.Instance.StartFennec();
				}
				
			}
        }
        else if ( Instance != this ) //todo: would it be better to put this at the top and do a return if true?
		{
			NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.{nameof(Instance)} was NOT null. Destroying this..." );

			Destroy( gameObject );
		}


		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.Awake() end" );

	}

	void Start()
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.Start()" );

		if ( !BM_SceneManager.Instance.AmPastStartScene )
		{
			if( LMDS_Manager.Instance.HasConnectedDevices )
			{
				try
				{
					string Name_savedCursorDevice = string.Empty;
					if ( PlayerPrefs.HasKey(PlrPrfKey_CursorDeviceName) )
					{
						Name_savedCursorDevice = PlayerPrefs.GetString( PlrPrfKey_CursorDeviceName );

						NAMRU_Debug.Log($"got cursor device plrpref key of '{Name_savedCursorDevice}'. Finding this device in loaded list...");
						if( LMDS_Manager.Instance.FindDeviceViaName(Name_savedCursorDevice, ref GameManager.Instance.cursorDevice) )
						{
							NAMRU_Debug.Log($"Default cursor device name WAS present in app settings as ('{Name_savedCursorDevice}'), " +
								$"and found in list of connected devices.",	BM_Enums.LogDestination.console);
						}
						else
						{
							NAMRU_Debug.LogWarning($"Default cursor device '{Name_savedCursorDevice}' not found in loaded devices. " +
								$"Defaulting to ({LMDS_Manager.Instance.ConnectedDevices[0].DeviceName})...");

							GameManager.Instance.cursorDevice = LMDS_Manager.Instance.ConnectedDevices[0];
							Name_savedCursorDevice = GameManager.Instance.cursorDevice.DeviceName;
							PlayerPrefs.SetString(PlrPrfKey_CursorDeviceName, Name_savedCursorDevice);
						}
					}
					else
					{
						GameManager.Instance.cursorDevice = LMDS_Manager.Instance.ConnectedDevices[0];
						Name_savedCursorDevice = GameManager.Instance.cursorDevice.DeviceName;
						PlayerPrefs.SetString( PlrPrfKey_CursorDeviceName, Name_savedCursorDevice );

						NAMRU_Debug.Log($"Default cursor device not yet saved in app settings. " +
							$"Defaulting to ({LMDS_Manager.Instance.ConnectedDevices[0].DeviceName})...",
							BM_Enums.LogDestination.All);
					}

					string Name_SavedHideQuadDevice = string.Empty;
					if ( PlayerPrefs.HasKey(PlrPrfKey_HideQuadDeviceName) )
					{                    
						Name_SavedHideQuadDevice = PlayerPrefs.GetString(PlrPrfKey_HideQuadDeviceName);

						if ( LMDS_Manager.Instance.FindDeviceViaName(Name_SavedHideQuadDevice, ref GameManager.Instance.HideQuadDevice) )
						{
							NAMRU_Debug.Log($"Default 'hide quad' device name WAS present in app settings as ('{Name_SavedHideQuadDevice}'), " +
								$"and found in list of connected devices.", BM_Enums.LogDestination.console);
						}
						else
						{
							NAMRU_Debug.LogWarning($"Default hide-quad device '{Name_SavedHideQuadDevice}' not found in loaded devices. " +
								$"Defaulting to ({LMDS_Manager.Instance.ConnectedDevices[0].DeviceName})...");

							GameManager.Instance.HideQuadDevice = LMDS_Manager.Instance.ConnectedDevices[0];
							Name_SavedHideQuadDevice = GameManager.Instance.HideQuadDevice.DeviceName;
							PlayerPrefs.SetString(PlrPrfKey_HideQuadDeviceName, Name_SavedHideQuadDevice);
						}
					}
					else
					{
						GameManager.Instance.HideQuadDevice = LMDS_Manager.Instance.ConnectedDevices[0];
						Name_SavedHideQuadDevice = GameManager.Instance.HideQuadDevice.DeviceName;
						PlayerPrefs.SetString(PlrPrfKey_HideQuadDeviceName, Name_SavedHideQuadDevice);

						NAMRU_Debug.Log($"Default 'hide quad' device not yet saved in app settings. " +
							$"Defaulting to ({LMDS_Manager.Instance.ConnectedDevices[0].DeviceName})...",
							BM_Enums.LogDestination.All);
					}
				}
				catch (Exception e)
				{
					NAMRU_Debug.LogError($"Caught exception getting app preferences on DataManager.Start(). Exception says: \n" +
						$"{e.ToString()}");
				}
			}

			/*
            if ( PlayerPrefs.HasKey(PlrPrfKey_FennecIPAddress) )
            {
                if ( IPAddress.TryParse(PlayerPrefs.GetString(PlrPrfKey_FennecIPAddress), out readInFennecIP) )
                {
					NAMRU_Debug.Log($"Fennec IP address succesfully loaded.");
                }
				else
				{
                    NAMRU_Debug.LogError($"{nameof(DataManager)} saved fennec IP string '{PlayerPrefs.GetString(PlrPrfKey_FennecIPAddress)}' couldn't be parsed as a valid IP address...");
				}
            }
            else
            {
                NAMRU_Debug.Log($"Default fennec ip address not yet saved in app settings. " +
                    $"Defaulting to '{readInFennecIP}'...",
                    BM_Enums.LogDestination.All);
            }

            NamruSessionManager.Instance.FennecIPAddressString = readInFennecIP.ToString();
			*/
        }

        NAMRU_Debug.Log( $"{gameObject.name} > {nameof(DataManager)}.Start() end", BM_Enums.LogDestination.none );
	 
    }

	private void Update()
	{
		if ( NamruSessionManager.Instance != null && NamruSessionManager.Instance.AmPastFirstLoad && BM_SceneManager.Instance.AmPastStartScene && NamruSessionManager.Instance.AmFennecSending )
		{
			NamruSessionManager.Instance.SendToFennec( $"BM_CursorPosition.X", GameManager.Instance.GameCursor.CalculatedPosition.x );
			NamruSessionManager.Instance.SendToFennec( $"BM_CursorPosition.Y", GameManager.Instance.GameCursor.CalculatedPosition.y );

			NamruSessionManager.Instance.SendToFennec($"BM_ScenarioKillcount", GameManager.Instance.Count_enemiesKilledInCurrentScenario);
			NamruSessionManager.Instance.SendToFennec($"BM_TotalKillcount", GameManager.Instance.Count_enemiesKilledInEntireTrial, true);
		}
	}

	public bool ValidateAndHandleIniFile()
	{
		NAMRU_Debug.Log( $"{nameof(DataManager)}.{nameof(ValidateAndHandleIniFile)}().", BM_Enums.LogDestination.console );

		NAMRU_Debug.Log($"IniValues.length: '{NamruSessionManager.Instance.IniValues.Length}'", BM_Enums.LogDestination.console);

		string[] storeIniLines = NamruSessionManager.Instance.IniValues;

		if (!CSVstringToVector3(storeIniLines[0], out readInQuadPos) )
		{
			NAMRU_Debug.LogError( $"{nameof(DataManager)} decided quad position couldn't be read. Not using ini file..." );
			return false;
		}
		else
		{
			NAMRU_Debug.Log($"{nameof(DataManager)} decided quad position WAS valid...");
		}

		//now trying player prefs instead...
		if( !IPAddress.TryParse(storeIniLines[1], out readInFennecIP) )
		{
			NAMRU_Debug.LogError( $"{nameof(DataManager)} decided fennec IP couldn't be read. Not using ini file..." );
			return false;
		}
		

		return true;
	}

    public static bool CSVstringToVector3( string s, out Vector3 v )
	{
		NAMRU_Debug.Log( $"{nameof(DataManager)}.{nameof(CSVstringToVector3)}()", BM_Enums.LogDestination.none );
		v = Vector3.zero;

		string[] s_parsed = s.Split(',');
		if ( s_parsed.Length != 3 )
		{
			NAMRU_Debug.LogError( $"split string was at length: '{s_parsed.Length}' instead of 3. Cannot parse to Vector3. Returning early..." );
			return false;
		}
		else
		{
			for ( int i = 0; i < 3; i++ )
			{
				float f = 0f;
				if( !float.TryParse(s_parsed[i], out f) )
				{
					NAMRU_Debug.LogError( $"Attempt to parse value '{s_parsed[i]}' to float at position: '{i}' failed. Cannot parse to Vector3. Returning early..." );
					return false;
				}
				else
				{
					if (i == 0)
					{
						v.x = f;
					}
					else if( i == 1)
					{
						v.y = f;
					}
					else
					{
						v.z = f;
					}
				}
			}

			return true;
		}
	}

	[ContextMenu("z call DeletePlayerPrefs()")]
	public void DeletePlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}

	public override bool CheckIfKosher()
	{
		bool amKosher = true;


		return amKosher;
	}
}
