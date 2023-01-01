
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class EnemyManager : UdonSharpBehaviour
	{
		public TowerDefense.Waves.WaveManager WaveManager;
		public int HitPoints = 1;
		Transform[] _waypoints;

		void Start()
		{
			_waypoints = WaveManager.GameManager.Waypoints;
		}

		public void Despawn()
		{
			WaveManager.EnemyPool.Return(this.gameObject);
		}
	}
}
