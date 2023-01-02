
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
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class WaveManager : UdonSharpBehaviour
	{
		public VRC.SDK3.Components.VRCObjectPool EnemyPool;
		public float SpawnDuration;
		public float AfterSpawnPause;
		public GameManager GameManager;
		float _spawnRate;
		State _state = State.None;
		int _spawnIndex = 0;

		void Start()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SpawnWave()
		{
			_state = State.Spawning;
			SpawnEnemy();
		}

		public void ResetWave()
		{
			_state = State.None;
			_spawnIndex = 0;

			foreach (var Enemy in EnemyPool.Pool)
			{
				Enemy.GetComponent<Enemies.EnemyManager>().Despawn();
			}
		}

		public void UpdateTimeMultiplicator()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SpawnEnemy()
		{
			if (_state != State.Spawning) return;

			var spawnedEnemy = EnemyPool.TryToSpawn();
			spawnedEnemy.GetComponent<Enemies.EnemyManager>().Spawn();

			_spawnIndex++;

			if (_spawnIndex >= EnemyPool.Pool.GetLength(0))
			{
				_state = State.Waiting;
				return;
			}

			SendCustomEventDelayedSeconds("SpawnEnemy", _spawnRate);
		}
	}
}
