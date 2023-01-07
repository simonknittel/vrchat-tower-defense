
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.TowerTile
{
	public enum TowerTypes
	{
		None,
		SingleTargetDamage,
		SingleTargetKnockback
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Manager : UdonSharpBehaviour
	{
		public MeshRenderer RaycastTargetRenderer;
		public GameObject SingleTargetDamage;
		public GameObject SingleTargetKnockback;
		TowerTypes _activeTower = TowerTypes.None;

		public void ToggleHighlight(bool newValue)
		{
			RaycastTargetRenderer.enabled = newValue;
		}

		public bool SpawnTower(TowerTypes type)
		{
			if (_activeTower != TowerTypes.None) return false;

			_activeTower = type;

			switch (type)
			{
				case TowerTypes.SingleTargetDamage:
					SingleTargetDamage.SetActive(true);
					return true;

				case TowerTypes.SingleTargetKnockback:
					SingleTargetKnockback.SetActive(true);
					return true;

				default:
					return false;
			}
		}
	}
}
