using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.Events;
using System.Runtime.InteropServices.WindowsRuntime;

namespace LFP
{
	public class LogansFilePicker : MonoBehaviour
	{
		[Header("REFERENCE - INTERNAL")]
		[SerializeField] private TMP_InputField iptFld_dirPath;
		[SerializeField] private TMP_InputField iptFld_InputFileName;


		[SerializeField] private Transform trans_dirs, trans_files;
		[SerializeField] private TextMeshProUGUI txt_CurrentDirectoryName, txt_ChosenFile;

		[Header("REFERENCE - EXTERNAL")]
		[SerializeField] private GameObject prefab_entryButton;

		[SerializeField] private DirPathInfo myPathInfo;
		public DirPathInfo MyPathInfo => myPathInfo;
		private List<LFP_Button> populatedButtons = new List<LFP_Button>();

		[Header("OTHER")]
		public Texture IMG_FileIcon;
		public Texture IMG_FolderIcon;

		/// <summary>
		/// Communicates the currently selected directory path to the outside.
		/// </summary>
		public string SelectedDirPathString => myPathInfo.DirectoryString_full;

		/// <summary>Communicates the currently selected filepath to the outisde. </summary>
		private string selectedFilePathString;
		/// <summary>Communicates the currently selected filepath to the outisde. </summary>
		public string SelectedFilePathString => selectedFilePathString;

		[Header("DEBUGGING")]
		public bool AmDebugging = false;
		private void Awake()
		{
			ClearResults();

		}

		void Start()
		{
			CheckIfKosher();


		}

		[ContextMenu("z_call ClearResults()")]
		private void ClearResults()
		{
			if ( AmDebugging ) Debug.Log($"{nameof(LogansFilePicker)}.ClearResults()...");

			while ( trans_dirs.transform.childCount > 0 )
			{
				DestroyImmediate( trans_dirs.transform.GetChild(0).gameObject );
			}

			while ( trans_files.transform.childCount > 0 )
			{
				DestroyImmediate( trans_files.transform.GetChild(0).gameObject );
			}
			populatedButtons = new List<LFP_Button>();

			txt_CurrentDirectoryName.text = string.Empty;
			txt_ChosenFile.text = string.Empty;
		}

		public void PopulateMeViaPath( string dirPath_passed )
		{
			if (AmDebugging) Debug.Log( $"GetFilesAndDirectories('{dirPath_passed}')..." );
			ClearResults();

			myPathInfo.SetMeViaDirectoryPath( dirPath_passed );
			iptFld_dirPath.text = dirPath_passed;

			txt_CurrentDirectoryName.text = MyPathInfo.GetPresentableAbbreviatedDirString();
			for ( int i = 0; i < myPathInfo.FullStrings_ContainedSubDirectories.Count; i++ )
			{
				GameObject go = Instantiate( prefab_entryButton, trans_dirs.transform );
				LFP_Button btn = go.GetComponent<LFP_Button>();
				btn.Init( this, i, IMG_FolderIcon, true );
				populatedButtons.Add( btn );
			}

			for ( int i = 0; i < myPathInfo.FullStrings_ContainedFilePaths.Count; i++ )
			{
				GameObject go = Instantiate( prefab_entryButton, trans_files.transform );
				LFP_Button btn = go.GetComponent<LFP_Button>();
				btn.Init( this, i, IMG_FileIcon, false );
				populatedButtons.Add(btn);
			}
		}

		public void ClearAllButtonHighlights()
		{
			if( populatedButtons != null && populatedButtons.Count > 0 )
			{
				foreach( LFP_Button btn in populatedButtons )
				{
					btn.SetHighlight( false );
				}
			}
		}

		/// <summary>
		/// Updates UI in response to a file string being selected. This is primarily intended to be triggered by the LFP_Buttons onClick events.
		/// </summary>
		/// <param name="s"></param>
		public void SetSelectedFilePathString( string s )
		{
			selectedFilePathString = s;
			txt_ChosenFile.text = selectedFilePathString;
		}

		#region METHODS STRICTLY FOR UI -------------------///////////////////////
		public void UI_Btn_UpADir_action()
		{
			if (AmDebugging) print( $"{nameof(UI_Btn_UpADir_action)}()" );
			 
			if( myPathInfo.DelimitedDirectoryPathStrings != null && myPathInfo.DelimitedDirectoryPathStrings.Length > 1 )
			{
				PopulateMeViaPath( myPathInfo.PreviousDirectoryString_full );
			}
		}
		
		public void UI_Btn_GetFiles_action()
		{
			PopulateMeViaPath( iptFld_dirPath.text );
		}

		public void UI_Btn_AssetsFolder_action()
		{
			PopulateMeViaPath( Application.dataPath );
			iptFld_dirPath.text = myPathInfo.DirectoryString_full;
		}

		public void UI_Btn_MakeDefault_action()
		{
			throw new NotImplementedException();
		}

		public void UI_Btn_MakeFileDefault_action()
		{
			if (AmDebugging) Debug.Log($"UI_Btn_MakeFileDefault_action()");


		}

		#endregion

		public void SayHay()
		{
			print("hay");
		}

		[ContextMenu("z_call CheckIfKosher()")]
		public bool CheckIfKosher()
		{
			bool amKosher = true;

			if( iptFld_dirPath == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(iptFld_dirPath)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			if ( iptFld_InputFileName == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(iptFld_InputFileName)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			if ( txt_ChosenFile == null )
			{
				amKosher = false;
				Debug.LogError($"'{nameof(txt_ChosenFile)}' inside class: {nameof(LogansFilePicker)} was not set!");
			}

			if ( trans_dirs == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(trans_dirs)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			if ( trans_files == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(trans_files)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			if ( txt_CurrentDirectoryName == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(txt_CurrentDirectoryName)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			if ( prefab_entryButton == null )
			{
				amKosher = false;
				Debug.LogError( $"'{nameof(prefab_entryButton)}' inside class: {nameof(LogansFilePicker)} was not set!" );
			}

			return amKosher;
		}
	}
}
