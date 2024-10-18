using NAMRU_data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class NAMRUversioner : MonoBehaviour
{
	[Header("REFERENCE")]
	[SerializeField] private GameObject group_version_body;
	[SerializeField] private TextMeshProUGUI txt_Version;
	[SerializeField] private TextMeshProUGUI txt_date;
	[SerializeField] private TextMeshProUGUI txt_body;

	//[Header("SETTINGS")]
	[SerializeField] private string filePath_changelog;

	//[Header("OTHER")]
	

    void Start()
    {
		group_version_body.SetActive(false);

        filePath_changelog = Path.Combine( NamruSessionManager.Instance.DirPath_NamruDirectory, "changelog.txt" );
		TryGetChangeLog();
	}

	public string[] allLogs;
	public string[] finalSplitLog;
	private void TryGetChangeLog()
	{
		//Debug.Log( $"{gameObject.name} > {nameof(NAMRUversioner)}.{nameof(TryGetChangeLog)}()" );

		try
		{
			if( File.Exists(filePath_changelog) )
			{
				//Debug.Log( "found changelog file" );
				string allTxt = File.ReadAllText( filePath_changelog );
				//Debug.Log( allTxt );

				allLogs = allTxt.Split( "Version" );

				
				finalSplitLog = allLogs[allLogs.Length - 1].Split( Environment.NewLine );

				txt_Version.text = "V " + finalSplitLog[1];
				txt_date.text = finalSplitLog[2];

				string bodyString = "";
				for ( int i = 3; i < finalSplitLog.Length; i++ )
				{
					bodyString += finalSplitLog[i] + Environment.NewLine;
				}
				txt_body.text = bodyString;
			}
			else
			{
				//Debug.Log("Did NOT find changelog file");

			}
		}
		catch ( System.Exception e )
		{

			throw;
		}
	}

	#region UI METHODS ----------------------------------////////////////////
	public void UI_Btn_Version_action()
	{
		NAMRU_Debug.Log($"{nameof(StartSceneCanvas)}.{nameof(UI_Btn_Version_action)}", BM_Enums.LogDestination.console);
		group_version_body.SetActive(!group_version_body.activeSelf);
	}
	#endregion
}
