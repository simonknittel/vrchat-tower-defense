
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.TowerTile
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class SingleTargetKnockback : Tower
	{
		public float KnockbackAmount;

		override public void Attack()
		{
			if (CurrentTarget.TakeKnockback(KnockbackAmount))
			{
				RemoveTargetFromCollection(CurrentTarget);
				SelectNextTarget();
			}

			SwitchState(State.Cooldown);
		}
	}
}
