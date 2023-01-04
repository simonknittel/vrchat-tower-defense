
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	public enum State
	{
		None,
		Moving,
		Stunned,
		Killed,
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class EnemyManager : UdonSharpBehaviour
	{
		public TowerDefense.Waves.WaveManager WaveManager;
		public int AttackDamage = 1;
		public int Speed = 1;
		public State State = State.None;
		Transform[] _waypoints;
		int _currentWaypointIndex = 0;
		Vector3 _currentWaypointPosition;

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					Despawn();
					break;

				case State.Moving:
					_currentWaypointPosition = WaveManager.GameManager.Waypoints[_currentWaypointIndex].position;
					break;

				case State.Stunned:
					break;

				case State.Killed:
					Despawn();
					WaveManager.EnemyKilled();
					break;

				default:
					break;
			}
		}

		public void Despawn()
		{
			WaveManager.EnemyPool.Return(this.gameObject);
			_currentWaypointIndex = 0;
		}

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name.Contains("Waypoint") == false) return;

			_currentWaypointIndex++;
			if (_currentWaypointIndex >= WaveManager.GameManager.Waypoints.GetLength(0)) return;
			_currentWaypointPosition = WaveManager.GameManager.Waypoints[_currentWaypointIndex].position;
		}

		void Update()
		{
			if (State != State.Moving) return;

			var step = Speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, _currentWaypointPosition, step);
		}
	}
}
