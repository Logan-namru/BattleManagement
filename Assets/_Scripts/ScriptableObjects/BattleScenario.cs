using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "battleScenario", menuName = "BattleManagement/Battle Scenario")]
public class BattleScenario : ScriptableObject
{
    public List<Vector3> enemyPositions = new List<Vector3>();
    public List<Quaternion> enemyRotations = new List<Quaternion>();

	public List<Vector3> neutralPositions = new List<Vector3>();
	public List<Quaternion> neutralRotations = new List<Quaternion>();

	public List<Vector3> friendlyPositions = new List<Vector3>();
	public List<Quaternion> friendlyRotations = new List<Quaternion>();

}
