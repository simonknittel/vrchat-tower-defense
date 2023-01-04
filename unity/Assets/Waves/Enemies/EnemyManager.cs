
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	public enum State
	{
		None,
		Spawning,
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
		public int TotalHealth = 3;
		public int CurrentHealth = 3;
		public State State = State.None;
		Transform[] _waypoints;
		int _currentWaypointIndex = 0;
		Vector3 _currentWaypointPosition;
		public UnityEngine.UI.Text HealthText;

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					Despawn();
					break;

				case State.Spawning:
					_currentWaypointIndex = 0;
					CurrentHealth = TotalHealth;
					HealthText.text = $"{CurrentHealth}/{TotalHealth}";
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

		public bool Attacked(int attackDamage)
		{
			CurrentHealth -= attackDamage;
			if (CurrentHealth > 0)
			{
				HealthText.text = $"{CurrentHealth}/{TotalHealth}";
				return false;
			}
			SwitchState(State.Killed);
			return true;
		}
	}
}
