using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace LoganPackages_archived
{
	public class MomentaryDebugLogger_archived : MonoBehaviour
	{
		[Header("[-----------REFERENCE----------]")]
		public TextMeshProUGUI TMP_MomentaryDebugLogger;

		[Header("[-----------STATS----------]")]
		[SerializeField] private float duration_momentaryDebugLoggerMessages = 1f;
		[SerializeField] private int count_maxDebugLogMessages = 0;

		//[Header("[-----------OTHER----------]")]
		[SerializeField] private List<string> dbgStrings = new List<string>();
		[SerializeField] private List<float> dbgCountdowns = new List<float>();

		[SerializeField] private string threadsafeString;


		private void Awake()
		{
			//Setting the following text to empty in Awake() instead of Start() prevents it from
			//potentially getting erased if it gets logged on Start()...

			TMP_MomentaryDebugLogger.text = string.Empty;
		}

		void Start()
		{
			CheckIfKosher();
			//dbgStrings = new List<string>();  //commenting these out because if left in, debug messages sent on Start() from other classes won't leave the momentarydebuglogger.
			//dbgCountdowns = new List<float>();
		}

		void Update()
		{
			if( !string.IsNullOrEmpty(threadsafeString) )
			{
				LogMomentarily( threadsafeString );
				threadsafeString = string.Empty;
			}

			if ( dbgStrings != null && dbgStrings.Count > 0 )
			{
				bool updateMade = false;

				for ( int i = dbgStrings.Count - 1; i > -1; i-- )
				{
					if ( i > count_maxDebugLogMessages )
					{
						dbgStrings.RemoveAt(i);
						dbgCountdowns.RemoveAt(i);
						updateMade = true;
					}
					else
					{
						dbgCountdowns[i] -= Time.deltaTime;

						if ( dbgCountdowns[i] <= 0f )
						{
							dbgStrings.RemoveAt(i);
							dbgCountdowns.RemoveAt(i);
							updateMade = true;
						}
					}
				}

				if ( updateMade )
				{
					UpdateTemporaryDebugLoggerText();
				}
			}
		}

		public void LogMomentarily( string msg )
		{
			dbgStrings.Insert( 0, msg );
			dbgCountdowns.Insert( 0, duration_momentaryDebugLoggerMessages );
			UpdateTemporaryDebugLoggerText();
		}

		public void LogMomentarily_threadSafe(string msg)
		{
			threadsafeString = msg;
		}

		private void UpdateTemporaryDebugLoggerText()
		{
			if ( dbgStrings != null && dbgStrings.Count > 0 )
			{
				StringBuilder sb = new StringBuilder();
				foreach ( string s in dbgStrings )
				{
					sb.AppendLine(s);
				}

				TMP_MomentaryDebugLogger.text = sb.ToString();
			}
			else
			{
				TMP_MomentaryDebugLogger.text = string.Empty;
			}
		}

		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if ( TMP_MomentaryDebugLogger == null )
			{
				amKosher = false;
				NAMRU_Debug.LogError("TMP_MomentaryDebugLogger reference was not set in MomentaryDebugLogger!");
			}

			if ( duration_momentaryDebugLoggerMessages <= 0f )
			{
				amKosher = false;
				NAMRU_Debug.LogWarning("<b>'duration_momentaryDebugLoggerMessages'</b> not set inside MomentaryDebugLogger!");
			}

			if ( count_maxDebugLogMessages <= 0 )
			{
				amKosher = false;
				NAMRU_Debug.LogWarning("<b>'count_maxDebugLogMessages'</b> needs to be greater than 0 inside MomentaryDebugLogger!");
			}

			return amKosher;
		}
	}
}