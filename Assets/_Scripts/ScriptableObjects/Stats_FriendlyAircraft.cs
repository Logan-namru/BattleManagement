using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stats_friendlyAircraft", menuName = "BattleManagement/Stats_friendlyAircraft")]
public class Stats_FriendlyAircraft : ScriptableObject
{
	[Range(0f, 0.8f)]
	public float Speed_move = 0.1f;

	[Range(0f, 0.7f)]
	public float Speed_rotation = 0.3f;

	[Range(0f, 1f)]
	public float Threshold_consideredFacing = 0.9f;

	[Range(0f, 10f)]
	public float Radius_closeEnoughToFire = 0.9f;

	public bool CheckIfKosher()
	{
		bool amKosher = true;

		if (Speed_move <= 0f)
		{
			amKosher = false;
			NAMRU_Debug.LogError($"Speed stat needs to be greater than 0!");
		}

		if ( Speed_rotation <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"<b>{nameof(Speed_rotation)}</b> inside <b>{nameof(Stats_FriendlyAircraft)}</b> needs to be greater than 0!" );
		}

		if ( Radius_closeEnoughToFire <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"<b>{nameof(Radius_closeEnoughToFire)}</b> inside <b>{nameof(Stats_FriendlyAircraft)}</b> needs to be greater than 0!" );
		}

		return amKosher;
	}
}
