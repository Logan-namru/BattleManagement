using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScenarioCreator : MonoBehaviour
{
	[SerializeField] BattleScenario scenarioToSave;
	GameObject[] sceneFriendlies;
	GameObject[] sceneEnemies;
	GameObject[] sceneNeutrals;

	private void Start()
	{
		gameObject.SetActive(false);
	}

	[ContextMenu("z call SaveSceneToScenario()")]
	public void SaveSceneToScenario()
	{
		Debug.Log( $"{nameof(SaveSceneToScenario)}()" );

#if UNITY_EDITOR
		if (scenarioToSave == null)
		{
			Debug.LogError($"{nameof(scenarioToSave)} was null. Set it in the inspector");
			return;
		}

		scenarioToSave.enemyPositions = new List<Vector3>();
		scenarioToSave.enemyRotations = new List<Quaternion>();

		sceneEnemies = GameObject.FindGameObjectsWithTag("enemyAircraft");
		for ( int i = 0; i < sceneEnemies.Length; i++ )
		{
			if( sceneEnemies[i].transform.parent != null && sceneEnemies[i].transform.parent.parent == transform )
			{
				scenarioToSave.enemyPositions.Add(sceneEnemies[i].transform.position);
				scenarioToSave.enemyRotations.Add(sceneEnemies[i].transform.rotation);
			}
		}

		scenarioToSave.neutralPositions = new List<Vector3>();
		scenarioToSave.neutralRotations = new List<Quaternion>();

		sceneNeutrals = GameObject.FindGameObjectsWithTag("neutralAircraft");
		for (int i = 0; i < sceneNeutrals.Length; i++)
		{
			if (sceneNeutrals[i].transform.parent != null && sceneNeutrals[i].transform.parent.parent == transform)
			{
				scenarioToSave.neutralPositions.Add(sceneNeutrals[i].transform.position);
				scenarioToSave.neutralRotations.Add(sceneNeutrals[i].transform.rotation);
			}
		}

		scenarioToSave.friendlyPositions = new List<Vector3>();
		scenarioToSave.friendlyRotations = new List<Quaternion>();
		sceneFriendlies = GameObject.FindGameObjectsWithTag("friendlyAircraft");
		for( int i = 0; i < sceneFriendlies.Length; i++ )
		{
			if ( sceneFriendlies[i].transform.parent != null && sceneFriendlies[i].transform.parent.parent == transform )
			{
				scenarioToSave.friendlyPositions.Add( sceneFriendlies[i].transform.position );
				scenarioToSave.friendlyRotations.Add( sceneFriendlies[i].transform.rotation );
			}

		}

		UnityEditor.EditorUtility.SetDirty( scenarioToSave );
#endif
	}

}
