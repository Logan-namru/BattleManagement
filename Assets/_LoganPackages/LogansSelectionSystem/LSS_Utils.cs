using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LSS
{
	public static class LSS_Utilities
	{
	
	}

	public enum LSS_SelectionIndicatorMode
	{
		none,
		constant,
		momentary,
	}

	/// <summary>
	/// Interface for allowing the LIS_Manager to interface with outside classes.
	/// </summary>
	public interface I_LSS_Interactable
	{
		/// <summary>
		/// Allows this interactable 
		/// </summary>
        public int MyInteractionLayer
		{
			get; set;
		}

		public List<int> OpposingInteractionLayers
		{
			get; set;
		}

		public bool SelectOpposing { get; set; }

       // public LIS_SelectionIndicatorMode MySelectIndicationMode { get; set; }

        public void HighlightedMe_action();

		public void UnHighlightedMe_action();

		public void SelectedMeAsOpposing_action();

		public void UnSelectedMeAsOpposing_action();

		public void SelectedMyOpposing_action( I_LSS_Interactable interactable );

	}


}

