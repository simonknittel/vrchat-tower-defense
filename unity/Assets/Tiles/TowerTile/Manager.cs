
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
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Manager : UdonSharpBehaviour
	{
		public MeshRenderer RaycastTargetRenderer;
		public GameObject SingleTargetDamage;
		TowerTypes _activeTower = TowerTypes.None;

		public void ToggleHighlight(bool newValue)
		{
			RaycastTargetRenderer.enabled = newValue;
		}

		public bool SpawnTower(TowerTypes type)
		{
			if (_activeTower != TowerTypes.None) return false;

			switch (type)
			{
				case TowerTypes.SingleTargetDamage:
					SingleTargetDamage.SetActive(true);
					_activeTower = type;
					return true;

				default:
					return false;
			}
		}
	}
}
