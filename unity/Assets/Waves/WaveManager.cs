
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Waves
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class WaveManager : UdonSharpBehaviour
	{
		public VRC.SDK3.Components.VRCObjectPool EnemyPool;
		public float SpawnDuration;
		public float AfterSpawnPause;
		public GameManager GameManager;
		float _spawnRate;
		bool _spawning = false;
		int _spawnIndex = 0;

		void Start()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SpawnWave()
		{
			_spawning = true;
			SpawnEnemy();
		}

		public void ResetWave()
		{
			_spawning = false;
			_spawnIndex = 0;

			foreach (var Enemy in EnemyPool.Pool)
			{
				EnemyPool.Return(Enemy);
			}
		}

		public void UpdateTimeMultiplicator()
		{
			_spawnRate = SpawnDuration / EnemyPool.Pool.GetLength(0) / GameManager.TimeMultiplicator;
		}

		public void SpawnEnemy()
		{
			if (_spawning == false) return;

			EnemyPool.TryToSpawn();

			_spawnIndex++;

			if (_spawnIndex >= EnemyPool.Pool.GetLength(0))
			{
				_spawning = false;
				return;
			}

			SendCustomEventDelayedSeconds("SpawnEnemy", _spawnRate);
		}
	}
}
