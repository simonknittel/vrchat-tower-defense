
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	public enum MovementState
	{
		None,
		Moving,
		Stunned,
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class EnemyManager : UdonSharpBehaviour
	{
		public TowerDefense.Waves.WaveManager WaveManager;
		public int AttackDamage = 1;
		public int Speed = 1;
		public MovementState MovementState = MovementState.None;
		Transform[] _waypoints;
		int _currentWaypointIndex = 0;

		public void Spawn()
		{
			MovementState = MovementState.Moving;
		}

		public void Despawn()
		{
			WaveManager.EnemyPool.Return(this.gameObject);
			MovementState = MovementState.None;
			_currentWaypointIndex = 0;
		}

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name.Contains("Waypoint") == false) return;
			_currentWaypointIndex++;
			if (_currentWaypointIndex >= WaveManager.GameManager.Waypoints.GetLength(0))
			{
				MovementState = MovementState.None;
				return;
			};
		}

		void Update()
		{
			if (MovementState != MovementState.Moving) return;

			var step = Speed * Time.deltaTime;
			var targetPosition = WaveManager.GameManager.Waypoints[_currentWaypointIndex].position;
			transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
		}
	}
}
