using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stats_missiles", menuName = "BattleManagement/Stats_Missiles")]
public class Stats_Missiles : ScriptableObject
{
    public float Speed_move;
    public float Speed_turn;
	public float Distance_explode;

	public bool CheckIfKosher()
	{
		bool amKosher = true;

		if( Speed_move <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"'{nameof(Stats_Missiles)}' {nameof(Speed_move)} variable wasn't greater than 0" );
		}

		if ( Speed_turn <= 0f)
		{
			amKosher = false;
			NAMRU_Debug.LogError($"'{nameof(Stats_Missiles)}' {nameof(Speed_turn)} variable wasn't greater than 0");
		}

		if ( Distance_explode <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError($"'{nameof(Stats_Missiles)}' <b>'{nameof(Distance_explode)}'</b> variable wasn't greater than 0");
		}

		return amKosher;
	}
}
