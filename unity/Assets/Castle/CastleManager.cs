
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class CastleManager : UdonSharpBehaviour
	{
		public GameManager GameManager;

		void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.name.Contains("Enemy") == false) return;
			GameManager.EnemyReachedCastle();
			collider.GetComponent<Enemies.EnemyManager>().Despawn();
		}
	}
}
