
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Waves
{
	public enum State
	{
		None,
		Spawning,
		Waiting,
		Finished,
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class WaveManager : UdonSharpBehaviour
	{
		public VRC.SDK3.Components.VRCObjectPool EnemyPool;
		public float SpawnDuration;
		public GameManager GameManager;
		float _spawnRate;
		public State State = State.None;
		int _spawnIndex = 0;
		[SerializeField]
		int _killedEnemies = 0;

		void Start()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					ResetWave();
					break;

				case State.Spawning:
					SpawnEnemy();
					break;

				case State.Waiting:
					GameManager.SwitchState(GameState.WaveWaiting);
					break;

				case State.Finished:
					if (GameManager.State != GameState.WaveWaiting) return;
					GameManager.SwitchState(GameState.WaveFinished);
					break;

				default:
					break;
			}
		}

		void ResetWave()
		{
			_spawnIndex = 0;
			_killedEnemies = 0;

			foreach (var Enemy in EnemyPool.Pool)
			{
				Enemy.GetComponent<Enemies.EnemyManager>().SwitchState(Enemies.State.None);
			}
		}

		public void UpdateTimeMultiplicator()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SpawnEnemy()
		{
			if (State != State.Spawning) return;

			var spawnedEnemy = EnemyPool.TryToSpawn();
			spawnedEnemy.GetComponent<Enemies.EnemyManager>().SwitchState(Enemies.State.Spawning);
			spawnedEnemy.GetComponent<Enemies.EnemyManager>().SwitchState(Enemies.State.Moving);

			_spawnIndex++;

			if (_spawnIndex >= EnemyPool.Pool.GetLength(0))
			{
				SwitchState(State.Waiting);
				return;
			}

			SendCustomEventDelayedSeconds("SpawnEnemy", _spawnRate);
		}

		public void EnemyKilled()
		{
			_killedEnemies++;

			if (_killedEnemies < EnemyPool.Pool.GetLength(0)) return;
			SwitchState(State.Finished);
		}
	}
}
