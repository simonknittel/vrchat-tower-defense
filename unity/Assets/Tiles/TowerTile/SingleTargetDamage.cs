
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
	public class SingleTargetDamage : UdonSharpBehaviour
	{
		public int AttackDamage = 1;
		public float AttackRate = 1f;
		public State State = State.None;
		TowerDefense.Enemies.EnemyManager[] _targetsInRange = new TowerDefense.Enemies.EnemyManager[10];
		TowerDefense.Enemies.EnemyManager _currentTarget;

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					_currentTarget = null;
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

		void OnTriggerEnter(Collider collider)
		{
			var newTarget = collider.GetComponent<TowerDefense.Enemies.EnemyManager>();

			// Add newTarget to collection
			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] != null) continue;
				_targetsInRange[i] = newTarget;
				break;
			}

			if (_currentTarget == null)
			{
				_currentTarget = newTarget;
				SwitchState(State.Attacking);
			}
		}

		void OnTriggerExit(Collider collider)
		{
			var target = collider.GetComponent<TowerDefense.Enemies.EnemyManager>();

			// Remove target from collection
			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] != target) continue;
				_targetsInRange[i] = null;
				break;
			}

			if (target == _currentTarget)
			{
				SelectNextTarget();
			}
		}

		void Attack()
		{
			Debug.Log(_currentTarget.name);
			if (_currentTarget.Attacked(AttackDamage))
			{
				// Remove _currentTarget from collection
				for (int i = 0; i < _targetsInRange.GetLength(0); i++)
				{
					if (_targetsInRange[i] != _currentTarget) continue;
					_targetsInRange[i] = null;
					break;
				}

				SelectNextTarget();
			}

			SwitchState(State.Cooldown);
		}

		public void CooldownFinished()
		{
			if (_currentTarget == null)
			{
				SwitchState(State.None);
			}
			else
			{
				SwitchState(State.Attacking);
			}
		}

		void SelectNextTarget()
		{
			_currentTarget = null;

			for (int i = 0; i < _targetsInRange.GetLength(0); i++)
			{
				if (_targetsInRange[i] == null) continue;
				_currentTarget = _targetsInRange[i];
				break;
			}
		}
	}
}
