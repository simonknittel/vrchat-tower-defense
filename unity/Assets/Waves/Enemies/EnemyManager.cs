
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class EnemyManager : UdonSharpBehaviour
	{
		public TowerDefense.Waves.WaveManager WaveManager;
		public NavMeshAgent NavMeshAgent;
		public int AttackDamage = 1;
		Transform[] _waypoints;
		int _currentWaypointIndex = 0;

		public void Spawn()
		{
			MoveToWaypoint();
		}

		public void Despawn()
		{
			WaveManager.EnemyPool.Return(this.gameObject);
		}

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name.Contains("Waypoint") == false) return;
			_currentWaypointIndex++;
			if (_currentWaypointIndex >= WaveManager.GameManager.Waypoints.GetLength(0)) return;
			MoveToWaypoint();
		}

		void MoveToWaypoint()
		{
			NavMeshAgent.SetDestination(WaveManager.GameManager.Waypoints[_currentWaypointIndex].position);
		}
	}
}
