using BM_Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BM_Object_mono : MonoBehaviour
{
	private void Awake()
	{
		
	}

	public virtual bool CheckIfKosher()
    {
        NAMRU_Debug.Log( nameof(CheckIfKosher), BM_Enums.LogDestination.none);
        return true;
    }

    /// <summary>
    /// For conveniently calling on this single object by itself via the editor
    /// </summary>
    [ContextMenu("z call CheckIfKosher_standalone()")]
    public void CheckIfKosher_standalone()
    {
        bool amKosher = CheckIfKosher();

        if (amKosher)
        {
            Debug.Log("<color=green>All was kosher</color>");
        }
    }


    private void Log(string msg, string methodName, LogDestination destination_passed = LogDestination.none)
    {

    }

	private void StartMethodLog( string methodName, LogDestination destination_passed = LogDestination.none )
	{
        string formattedMessage = name + ">>" + methodName;
        NAMRU_Debug.Log( formattedMessage, destination_passed );
        NAMRU_Debug.incrementLevel++;
	}

    private void EndMethodLog( string methodName, string msg = "" )
    {
		string formattedMessage = $"{name} >> {methodName}";
		//NAMRU_Debug.Log(formattedMessage, destination_passed);
		NAMRU_Debug.incrementLevel++;
	}
}
