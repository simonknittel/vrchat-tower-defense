
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense.Enemies
{
	public enum State
	{
		None,
		Spawning,
		Moving,
		Stunned,
		Killed,
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class EnemyManager : UdonSharpBehaviour
	{
		public TowerDefense.Waves.WaveManager WaveManager;
		public int AttackDamage;
		public int Speed;
		public int TotalHealth;
		public int CurrentHealth;
		public State State = State.None;
		Transform[] _waypoints;
		[SerializeField]
		GameObject _currentWaypoint;
		Vector3 _currentWaypointPosition;
		public Transform CanvasTransform;
		public UnityEngine.UI.Text HealthText;
		VRCPlayerApi _localPlayer;
		public Material OriginalMaterial;
		public Material DamageFlashMaterial;
		public MeshRenderer MeshRenderer;

		void Start()
		{
			_localPlayer = Networking.LocalPlayer;
		}

		public void SwitchState(State newState)
		{
			State = newState;

			switch (newState)
			{
				case State.None:
					Despawn();
					break;

				case State.Spawning:
					_currentWaypoint = WaveManager.GameManager.Waypoints[0];
					CurrentHealth = TotalHealth;
					HealthText.text = $"{CurrentHealth}/{TotalHealth}";
					break;

				case State.Moving:
					_currentWaypointPosition = _currentWaypoint.transform.position;
					break;

				case State.Stunned:
					break;

				case State.Killed:
					Despawn();
					WaveManager.EnemyKilled();
					break;

				default:
					break;
			}
		}

		public void Despawn()
		{
			WaveManager.EnemyPool.Return(this.gameObject);
		}

		void OnTriggerEnter(Collider collider)
		{
			// Only react to current waypoint
			if (collider.gameObject != _currentWaypoint) return;

			var length = WaveManager.GameManager.Waypoints.GetLength(0);

			// Find index of current waypoint in waypoint collection
			int currentsWaypointIndex = 99999;
			for (int i = 0; i < length; i++)
			{
				if (WaveManager.GameManager.Waypoints[i] != _currentWaypoint) continue;
				currentsWaypointIndex = i;
				break;
			}

			// Check if a next waypoint exists
			if (currentsWaypointIndex + 1 >= length) return;

			// Select next waypoint
			_currentWaypoint = WaveManager.GameManager.Waypoints[currentsWaypointIndex + 1];
			_currentWaypointPosition = _currentWaypoint.transform.position;
		}

		void Update()
		{
			UpdateBillboardPositionAndRotation();

			if (State != State.Moving) return;

			var step = Speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, _currentWaypointPosition, step);
		}

		private void UpdateBillboardPositionAndRotation()
		{
			var trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
			CanvasTransform.LookAt(CanvasTransform.position + trackingData.rotation * Vector3.forward, trackingData.rotation * Vector3.up);
		}

		public bool TakeDamage(int attackDamage)
		{
			if (State == State.Killed) return true;

			CurrentHealth -= attackDamage;

			if (CurrentHealth > 0)
			{
				FlashMaterial();
				HealthText.text = $"{CurrentHealth}/{TotalHealth}";
				return false;
			}

			SwitchState(State.Killed);
			return true;
		}

		public bool TakeKnockback(float knockbackAmount)
		{
			if (State == State.Killed) return true;

			var step = knockbackAmount * Time.deltaTime * -1;
			transform.position = Vector3.MoveTowards(transform.position, _currentWaypointPosition, step);

			FlashMaterial();

			return false;
		}

		void FlashMaterial()
		{
			MeshRenderer.material = DamageFlashMaterial;
			SendCustomEventDelayedSeconds("ResetMaterial", .1f);
		}

		public void ResetMaterial()
		{
			MeshRenderer.material = OriginalMaterial;
		}
	}
}
