using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BM_Enums;
using System;
using NAMRU_data;

public enum NAMRU_LogType
{ 
    None,
    UserMethod,
    UnityAPI,
}

public class NAMRU_Debug
{
    public static int incrementLevel = 0;

    public static void Log( string msg, NAMRU_LogType logType, LogDestination destination_passed = LogDestination.none )
    {
        string formattedString = msg; //todo: format this how I want it depending on the logtype

        Log( formattedString, destination_passed );
        
    }

	public static void Log( string msg, string objectName, string scriptName, string methodName, LogDestination destination_passed = LogDestination.none )
    {

    }
    
	public static void Log( string msg, LogDestination destination_passed = LogDestination.none )
	{
		if ( DestinationIncludesConsole(destination_passed) )
		{
			Debug.Log(msg);
		}

		if ( DestinationIncludesMomentaryDebugLogger(destination_passed) )
		{
            if( !Application.isPlaying )
            {
				Debug.LogWarning( $"Application is not playing. can't momentary log message: '{msg}'" );
			}
			if ( DebugManager.Instance == null)
			{
				Debug.LogWarning( $"{nameof(DebugManager)}.Instance null. can't momentary log message: '{msg}'" );
			}
            else
            {
			    DebugManager.Instance.LogMomentarily( msg );

            }
		}

		if (NamruSessionManager.Instance != null)
		{
			NamruSessionManager.Instance.WriteToLogFile( msg );
		}

	}

	public static void LogWarning(string msg, bool fromThread = false) 
    { 
        Debug.LogWarning("BATTLE MANAGEMENT WARNING! " + msg);

		if ( !fromThread && Application.isPlaying )
        {
			DebugManager.Instance.LogMomentarily( $"<color=yellow>{msg}</color>" );

        }

		if (DataManager.Instance != null)
		{
            NamruSessionManager.Instance.WriteToLogFile(Environment.NewLine + "WARNING!!!!!!!!!!!!!!!!");
			NamruSessionManager.Instance.WriteToLogFile( msg );
			NamruSessionManager.Instance.WriteToLogFile("!!!!!!!!!!!!!!!!!!!" + Environment.NewLine);
		}
	}

	public static void LogError( string msg, bool fromThread = false )
	{
		Debug.LogError( "BATTLE MANAGEMENT ERROR! " + msg );

        if ( !fromThread && Application.isPlaying )
        {
            if( DebugManager.Instance == null )
            {
                Debug.Log( $"dbg mgr instance was null" );
            }

		    DebugManager.Instance.LogMomentarily( $"<color=red>{msg}</color>" );
        }

		if ( DataManager.Instance != null )
		{
			NamruSessionManager.Instance.WriteToLogFile(Environment.NewLine + "ERROR!!!!!!!!!!!!!!!!");
			NamruSessionManager.Instance.WriteToLogFile( msg );
			NamruSessionManager.Instance.WriteToLogFile( "!!!!!!!!!!!!!!!!!!!" + Environment.NewLine );

		}
	}
    
    public static void LogTrialMessage( string msg )
    {
        NamruSessionManager.Instance.WriteToTrialResults( msg );
    }

	private static bool DestinationIncludesConsole( LogDestination destination_passed )
    {
        if( destination_passed == LogDestination.All || destination_passed == LogDestination.console || destination_passed == LogDestination.consoleAndMomentaryDebugLogger )
        {
            return true;
        }

        return false;
    }

    private static bool DestinationIncludesMomentaryDebugLogger( LogDestination destination_passed )
    {
        if( destination_passed == LogDestination.All || destination_passed == LogDestination.momentaryDebugLogger || destination_passed == LogDestination.consoleAndMomentaryDebugLogger )
        {
            return true;
        }

        return false;
    }
}