using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftManager : BM_Object_mono
{
    public static AircraftManager Instance;

    public Stats_EnemyAircraft Stats_EnemyAircraft;
	public Stats_FriendlyAircraft Stats_FriendlyAircraft;
	public Stats_Missiles Stats_FriendlyMissiles;

	[Header("REFERENCE - EXTERNAL")]
    [SerializeField] private List<EnemyAircraft> enemyAircraftList;
    public List<EnemyAircraft> AircraftList => enemyAircraftList;
    [SerializeField] private List<FriendlyAircraft> friendlyAircraftList;
    public List<FriendlyAircraft> FriendlyAircraftList => friendlyAircraftList;
	public Sprite NeutralSprite;

    [Header("[----- PREFABS -----]")]
    [SerializeField] private GameObject go_prefab_enemyAircraft;
    [SerializeField] private GameObject go_prefab_friendlyAircraft;
	[SerializeField] private GameObject go_prefab_missile;
	public GameObject GO_Prefab_Missile => go_prefab_missile;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
    {
        CheckIfKosher();
    }

	public void LoadScenario( BattleScenario scenario )
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(LoadScenario)}()" );

		NAMRU_Debug.Log("Now deleting enemies...");
		if ( enemyAircraftList != null )
        {
			NAMRU_Debug.Log( $"{nameof(enemyAircraftList)} not null. Deleting '{enemyAircraftList.Count}' aircraft..." );
			List<EnemyAircraft> savedEnemyList = new List<EnemyAircraft>( enemyAircraftList ); //I create this "saved" list, because if I used the original list, it changes the '.Count' property and ruins the following iteration...
			for ( int i = 0; i < savedEnemyList.Count; i++ )
			{
				NAMRU_Debug.Log( $"iterating craft number: '{i}'" );

				savedEnemyList[i].DestroyMe();
			}

			NAMRU_Debug.Log( $"End of iterating. {nameof(enemyAircraftList)}.Count now: '{enemyAircraftList.Count}'" );
		}
		else
		{
			NAMRU_Debug.Log( $"{nameof(enemyAircraftList)} was null. Not deleting any aircraft..." );

		}

		NAMRU_Debug.Log( "Now creating enemies..." );
		enemyAircraftList = new List<EnemyAircraft>();
        if( scenario.enemyPositions != null && scenario.enemyPositions.Count > 0 )
        {
			NAMRU_Debug.Log( $"scenario.{nameof(scenario.enemyPositions)} not null and above 0. Creating '{scenario.enemyPositions.Count}' aircraft..." );

			for ( int i = 0; i < scenario.enemyPositions.Count; i++ )
            {
				NAMRU_Debug.Log($"iterating craft number: '{i}'");

				EnemyAircraft enemy = Instantiate( go_prefab_enemyAircraft, scenario.enemyPositions[i], scenario.enemyRotations[i], transform ).GetComponent<EnemyAircraft>();
				enemy.name += $" {(i)}";

				RegisterEnemy( enemy );
            }
        }
        else
        {
            NAMRU_Debug.LogError( $"enemyPositions list was either null or 0 count in scenario." );
        }

		if ( scenario.neutralPositions != null && scenario.neutralPositions.Count > 0 )
		{
			NAMRU_Debug.Log($"scenario.{nameof(scenario.neutralPositions)} not null and above 0. Creating '{scenario.neutralPositions.Count}' aircraft...");

			for ( int i = 0; i < scenario.neutralPositions.Count; i++ )
			{
				NAMRU_Debug.Log( $"iterating craft number: '{i}'" );

				EnemyAircraft enemy = Instantiate( go_prefab_enemyAircraft, scenario.neutralPositions[i], scenario.neutralRotations[i], transform ).GetComponent<EnemyAircraft>();
				enemy.name += $" {(i)}";
				RegisterEnemy(enemy);
				enemy.MakeNeutral();
			}
		}
		else
		{
			//NAMRU_Debug.LogError($"neutralPositions list was either null or 0 count in scenario."); //Leaving this out in case we want scenario with no neutrals...
		}

		NAMRU_Debug.Log( "Now deleting friendlies..." );
		if ( friendlyAircraftList != null )
		{
			NAMRU_Debug.Log( $"{nameof(friendlyAircraftList)} not null. Deleting '{friendlyAircraftList.Count}' aircraft..." );

			List<FriendlyAircraft> savedFriendliesList = new List<FriendlyAircraft>( friendlyAircraftList ); //I create this "saved" list, because if I used the original list, it changes the '.Count' property and ruins the following iteration...
			for ( int i = 0; i < savedFriendliesList.Count; i++ )
			{
				savedFriendliesList[i].DestroyMe();
			}
			NAMRU_Debug.Log( $"End of iterating. {nameof(friendlyAircraftList)}.Count now: '{friendlyAircraftList.Count}'" );
		}
		else
		{
			NAMRU_Debug.Log( $"{nameof(friendlyAircraftList)} was null. Not deleting any aircraft..." );

		}

		NAMRU_Debug.Log( "Now creating friendlies..." );
		friendlyAircraftList = new List<FriendlyAircraft>();
		if ( scenario.friendlyPositions != null && scenario.friendlyPositions.Count > 0 )
		{
			NAMRU_Debug.Log( $"scenario.{nameof(scenario.friendlyPositions)} not null and above 0. Creating '{scenario.friendlyPositions.Count}' aircraft..." );

			for ( int i = 0; i < scenario.friendlyPositions.Count; i++ )
			{
				NAMRU_Debug.Log( $"iterating craft number: '{i}'" );
				FriendlyAircraft friendly = Instantiate( go_prefab_friendlyAircraft, scenario.friendlyPositions[i], scenario.friendlyRotations[i], transform ).GetComponent<FriendlyAircraft>();
				friendly.name += $" {(i)}";

				RegisterFriendly( friendly );
			}
		}
		else
		{
			NAMRU_Debug.LogError( $"enemyPositions list was either null or 0 count in scenario." );
		}

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(LoadScenario)}() end", BM_Enums.LogDestination.none );
	}

    public void RegisterEnemy( EnemyAircraft enemyAircraft )
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RegisterEnemy)}()", BM_Enums.LogDestination.console );

		if ( enemyAircraftList == null )
        {
            enemyAircraftList = new List<EnemyAircraft>();
        }

        enemyAircraftList.Add( enemyAircraft );
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RegisterEnemy)}() end", BM_Enums.LogDestination.console );
	}

	public void RemoveEnemy( EnemyAircraft enemyAircraft )
    {
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RemoveEnemy)}()", BM_Enums.LogDestination.console );

		if ( enemyAircraftList != null && enemyAircraftList.Contains(enemyAircraft) )
		{
			NAMRU_Debug.Log( $"list valid. Removing enemy aircraft...", BM_Enums.LogDestination.console );
			enemyAircraftList.Remove(enemyAircraft);
		}

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RemoveEnemy)}() end", BM_Enums.LogDestination.console );
	}

	public void RegisterFriendly( FriendlyAircraft friendly )
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RegisterFriendly)}()", BM_Enums.LogDestination.console );

		if ( friendlyAircraftList == null )
		{
			friendlyAircraftList = new List<FriendlyAircraft>();
		}

		friendlyAircraftList.Add( friendly );

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RegisterFriendly)}() end", BM_Enums.LogDestination.console );
	}

	public void RemoveFriendly( FriendlyAircraft friendly )
	{
		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RemoveFriendly)}()", BM_Enums.LogDestination.console );

		if ( friendlyAircraftList != null && friendlyAircraftList.Contains(friendly) )
		{
			friendlyAircraftList.Remove( friendly );
		}

		NAMRU_Debug.Log( $"{gameObject.name} > {nameof(AircraftManager)}.{nameof(RemoveFriendly)}() end", BM_Enums.LogDestination.console );
	}

	public void EnemyDied( EnemyAircraft enemy_passed )
	{
		NAMRU_Debug.Log( $"{nameof(AircraftManager)}.{nameof(EnemyDied)}('{enemy_passed}')", BM_Enums.LogDestination.console );

		foreach ( FriendlyAircraft fa in friendlyAircraftList )
		{
			if ( fa.EnemyAircraft_focused != null && fa.EnemyAircraft_focused == enemy_passed )
			{
				fa.RelinquishLockOn();
			}
		}
	}

	public override bool CheckIfKosher()
    {
        bool amKosher = true;

        if( Stats_EnemyAircraft == null )
        {
            amKosher= false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(Stats_EnemyAircraft)}'</b> is null or empty inside <b>{nameof(AircraftManager)}</b>." );
		}
		else if ( !Stats_EnemyAircraft.CheckIfKosher() )
        {
            amKosher = false;
        }

		if ( Stats_FriendlyAircraft == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(Stats_FriendlyAircraft)}'</b> is null or empty inside <b>{nameof(AircraftManager)}</b>." );
		}
		else if ( !Stats_FriendlyAircraft.CheckIfKosher() )
		{
			amKosher = false;
		}

		if ( Stats_FriendlyMissiles == null )
		{
			amKosher = false;
			NAMRU_Debug.LogWarning( $"<b>'{nameof(Stats_FriendlyMissiles)}'</b> is null or empty inside <b>{nameof(AircraftManager)}</b>." );
		}
		else if ( !Stats_FriendlyMissiles.CheckIfKosher() )
		{
			amKosher = false;
		}

		if ( go_prefab_enemyAircraft == null )
        {
			NAMRU_Debug.LogError( $"<b>'{nameof(go_prefab_enemyAircraft)}</b>' reference is null inside <b>{nameof(AircraftManager)}</b>" );
			amKosher = false;
		}

		if ( go_prefab_friendlyAircraft == null )
		{
			NAMRU_Debug.LogError( $"<b>'{nameof(go_prefab_friendlyAircraft)}</b>' reference is null inside <b>{nameof(AircraftManager)}</b>" );
			amKosher = false;
		}

		if ( go_prefab_missile == null )
		{
			NAMRU_Debug.LogError( $"<b>'{nameof(go_prefab_missile)}</b>' reference is null inside <b>{nameof(AircraftManager)}</b>" );
			amKosher = false;
		}

		if ( NeutralSprite == null )
		{
			NAMRU_Debug.LogError($"<b>'{nameof(NeutralSprite)}</b>' reference is null inside <b>{nameof(AircraftManager)}</b>");
			amKosher = false;
		}

		return amKosher;
    }
}
