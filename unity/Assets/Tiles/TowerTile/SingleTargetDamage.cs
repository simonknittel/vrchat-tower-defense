
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.TowerTile
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class SingleTargetDamage : Tower
	{
		public int DamageAmount;

		override public void Attack()
		{
			if (CurrentTarget.TakeDamage(DamageAmount))
			{
				RemoveTargetFromCollection(CurrentTarget);
				SelectNextTarget();
			}

			SwitchState(State.Cooldown);
		}
	}
}
