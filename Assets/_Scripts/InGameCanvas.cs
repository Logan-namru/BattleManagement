using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameCanvas : BM_Object_mono
{



	[ContextMenu("z call CheckIfKosher()")]
	public override bool CheckIfKosher()
	{
		NAMRU_Debug.Log( $"{nameof(InGameCanvas)}.{nameof(CheckIfKosher)}()" );

		bool amKosher = true;

		return amKosher;
	}
}
