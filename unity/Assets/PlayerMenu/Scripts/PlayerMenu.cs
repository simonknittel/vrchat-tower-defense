
using UdonSharp;
using UnityEngine;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class PlayerMenu : UdonSharpBehaviour
	{
		public GameManager GameManager;

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetDamage);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetKnockback);
			}
		}

		public void UIButtonSingleTargetDamage()
		{
			GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetDamage);
		}

		public void UIButtonSingleTargetKnockback()
		{
			GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetKnockback);
		}
	}
}
