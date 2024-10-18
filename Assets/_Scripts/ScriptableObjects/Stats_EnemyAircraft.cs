using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stats_enemyAircraft", menuName = "BattleManagement/Stats_enemyAircraft")]
public class Stats_EnemyAircraft : ScriptableObject
{
    [Range(0f, 0.8f)]
    public float Speed_move = 0.075f;

    [Range(0f, 1f)]
    public float Duration_opposingSelectHighlight = 0.2f;
    public float Duration_Damage = 0.2f;
    public float Duration_Death = 0.5f;

	public Color Color_highlight = new Color(1f, 1f, 1f, 0.2f);
    public Color Color_Damage = new Color( 1f, 1f, 1f, 1f );

	public bool CheckIfKosher()
    {
        bool amKosher = true;

        if( Speed_move <= 0f )
        {
            amKosher = false;
			NAMRU_Debug.LogError($"Speed stat needs to be greater than 0!");
        }

		if ( Duration_opposingSelectHighlight <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError($"<b>{nameof(Duration_opposingSelectHighlight)}</b> inside <b>{nameof(Stats_EnemyAircraft)}</b> needs to be greater than 0!");
		}

		if ( Duration_Damage <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"<b>{nameof(Duration_Damage)}</b> inside <b>{nameof(Stats_EnemyAircraft)}</b> needs to be greater than 0!" );
		}

		if ( Duration_Death <= 0f )
		{
			amKosher = false;
			NAMRU_Debug.LogError( $"<b>{nameof(Duration_Death)}</b> inside <b>{nameof(Stats_EnemyAircraft)}</b> needs to be greater than 0!");
		}

		return amKosher;
    }
}
