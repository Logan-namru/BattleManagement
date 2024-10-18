using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;

namespace LFP
{
    public class LFP_Button : MonoBehaviour
    {
        [Header("REFERENCE - INTERNAL")]
        [SerializeField] TextMeshProUGUI tmp;
        [SerializeField] RawImage ri_icon, ri_bg;

		//[Header("REFERENCE - INTERNAL")]
		LogansFilePicker myManager;

		//[Header("OTHER")]
        [SerializeField] string fullDirString;

		[Header("TRUTH")]
		public bool AmDirectoryButton = true;

		private void Start()
		{
			//Debug.Log($"start for '{name}'");
			ri_bg.enabled = false;
		}

		public void Init( string s_lbl, string s_fullPath, Texture tex )
	    {
		    tmp.text = s_lbl;
            fullDirString = s_fullPath;
            ri_icon.texture = tex;

	    }

		public void Init( LogansFilePicker lfp, int i, Texture tex, bool asDirectory )
		{
			AmDirectoryButton = asDirectory;

			if( asDirectory )
			{
				tmp.text = lfp.MyPathInfo.EndStrings_ContainedSubDirectories[i];
				fullDirString = lfp.MyPathInfo.FullStrings_ContainedSubDirectories[i];
				ri_icon.texture = tex; myManager = lfp;
			}
			else
			{
				tmp.text = lfp.MyPathInfo.EndStrings_ContainedFilePaths[i];
				fullDirString = lfp.MyPathInfo.FullStrings_ContainedFilePaths[i];
				ri_icon.texture = tex; myManager = lfp;
			}
		}
		public void ButtonClickAction()
        {
			if ( myManager.AmDebugging ) Debug.Log($"ButtonClickAction() {nameof(fullDirString)}: '{fullDirString}'");

			if( AmDirectoryButton )
			{
				myManager.PopulateMeViaPath( fullDirString );
			}
			else
			{
				//Debug.Log("I'm a file!");
				myManager.ClearAllButtonHighlights();
				ri_bg.enabled = true;
				myManager.SetSelectedFilePathString( fullDirString );

			}
        }

		public void SetHighlight( bool b )
		{
			ri_bg.enabled = b;
		}
    }
}
