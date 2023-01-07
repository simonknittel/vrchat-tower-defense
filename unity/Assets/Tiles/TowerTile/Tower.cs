
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.TowerTile
{
	public enum State
	{
		None,
		Attacking,
		Cooldown,
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Tower : UdonSharpBehaviour
	{
		public float AttackRate;
		public State State = State.None;
		TowerDefense.Enemies.EnemyManager[] _targetsInRange = new TowerDefense.Enemies.EnemyManager[10];
		public TowerDefense.Enemies.EnemyManager CurrentTarget;

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					CurrentTarget = null;
					break;

				case State.Attacking:
					Attack();
					break;

				case State.Cooldown:
					SendCustomEventDelayedSeconds("CooldownFinished", AttackRate);
					break;

				default:
					break;
			}
		}

		public void CooldownFinished()
		{
			if (CurrentTarget == null)
			{
				SwitchState(State.None);
			}
			else
			{
				SwitchState(State.Attacking);
			}
		}


		void OnTriggerEnter(Collider collider)
		{
			var newTarget = collider.GetComponent<TowerDefense.Enemies.EnemyManager>();

			AddTargetToCollection(newTarget);

			if (CurrentTarget == null)
			{
				CurrentTarget = newTarget;
				if (State != State.Cooldown) SwitchState(State.Attacking);
			}
		}

		void OnTriggerExit(Collider collider)
		{
			var target = collider.GetComponent<TowerDefense.Enemies.EnemyManager>();

			RemoveTargetFromCollection(target);

			if (target == CurrentTarget)
			{
				SelectNextTarget();
			}
		}

		public virtual void Attack()
		{
			SwitchState(State.Cooldown);
		}

		public void AddTargetToCollection(Enemies.EnemyManager target)
		{
			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] != null) continue;
				_targetsInRange[i] = target;
				break;
			}
		}

		public void RemoveTargetFromCollection(Enemies.EnemyManager target)
		{
			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] != target) continue;
				_targetsInRange[i] = null;
				break;
			}
		}

		public void SelectNextTarget()
		{
			CurrentTarget = null;

			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] == null) continue;
				CurrentTarget = _targetsInRange[i];
				break;
			}
		}
	}
}
