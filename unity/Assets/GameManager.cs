
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense
{
	public enum GameState
	{
		Pristine,
		Starting,
		WaveSpawning,
		WaveWaiting,
		WaveFinished,
		Won,
		Lost
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameManager : UdonSharpBehaviour
	{
		public int InitialPlayerGold;
		public int CurrentPlayerGold;
		public int TotalPlayerLives;
		public int CurrentPlayerLives;
		public int SingleTargetDamageCosts;
		public int SingleTargetKnockbackCosts;
		public Waves.WaveManager[] Waves;
		public GameObject[] Waypoints;
		public GameState State = GameState.Pristine;
		public UnityEngine.UI.Text LivesText;
		public UnityEngine.UI.Text WaveSignText;
		public GameObject WinLooseSign;
		public GameObject WinText;
		public GameObject LooseText;
		public UnityEngine.UI.Button NextWaveButton;
		public GameObject TowerTilesContainer;
		public GameObject StartGame;
		public GameObject PlayerMenu;

		VRCPlayerApi _localPlayer;
		TowerTile.Manager _currentHighlightedTowerTile;
		bool _isUserInVR = true;
		public TowerTile.TowerTypes CurrentInventorySelection = TowerTile.TowerTypes.SingleTargetDamage;
		int _currentWaveIndex = 0;

		void Start()
		{
			_localPlayer = Networking.LocalPlayer;
			_isUserInVR = _localPlayer.IsUserInVR();

			SwitchState(GameState.Pristine);
		}

		public void SwitchState(GameState newState)
		{
			State = newState;

			switch (newState)
			{
				case GameState.Pristine:
					Pristine();
					break;

				case GameState.Starting:
					StartGame.SetActive(false);
					PlayerMenu.SetActive(true);
					NextWaveButton.interactable = true;
					break;

				case GameState.WaveSpawning:
					SpawnNextWave();
					UpdateWaveSign();
					NextWaveButton.interactable = false;
					break;

				case GameState.WaveWaiting:
					break;

				case GameState.WaveFinished:
					_currentWaveIndex++;
					if (_currentWaveIndex >= Waves.GetLength(0))
					{
						SwitchState(GameState.Won);
						break;
					}
					UpdateWaveSign();
					NextWaveButton.interactable = true;
					break;

				case GameState.Lost:
					UpdateWinLooseSign(2);
					break;

				case GameState.Won:
					UpdateWinLooseSign(1);
					break;

				default:
					break;
			}
		}

		public void UIButtonReset()
		{
			SwitchState(GameState.Pristine);
		}

		public void UIButtonNextWave()
		{
			SwitchState(GameState.WaveSpawning);
		}

		void SpawnNextWave()
		{
			Waves[_currentWaveIndex].SwitchState(TowerDefense.Waves.State.Spawning);
		}

		public void Pristine()
		{
			CurrentPlayerGold = InitialPlayerGold;
			CurrentPlayerLives = TotalPlayerLives;
			_currentWaveIndex = 0;
			UpdateLivesText();
			UpdateWaveSign();
			UpdateWinLooseSign(0);

			foreach (var Wave in Waves)
			{
				Wave.SwitchState(TowerDefense.Waves.State.None);
			}

			var towerTiles = TowerTilesContainer.GetComponentsInChildren<TowerTile.Manager>();
			for (int i = 0; i < towerTiles.GetLength(0); i++)
			{
				towerTiles[i].Reset();
			}

			ResetSelection();

			NextWaveButton.interactable = false;

			PlayerMenu.SetActive(false);
			StartGame.SetActive(true);
		}

		void UpdateLivesText()
		{
			LivesText.text = $"Lives {CurrentPlayerLives}/{TotalPlayerLives}";
		}

		void UpdateWaveSign()
		{
			WaveSignText.text = $"Wave {_currentWaveIndex + 1}/{Waves.GetLength(0)}";
		}

		void Update()
		{
			if (State != GameState.Starting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;

			CastSelectionRay();
		}

		private void CastSelectionRay()
		{
			ResetSelection();

			VRCPlayerApi.TrackingData trackingData;
			if (_isUserInVR)
			{
				trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
			}
			else
			{
				trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
			}
			var origin = trackingData.position;
			var direction = trackingData.rotation * Vector3.forward;
			var ray = new Ray(origin, direction);

			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 5f, 1 << 22))
			{
				if (hit.transform && hit.transform.gameObject.name.Contains("RaycastTarget"))
				{
					_currentHighlightedTowerTile = hit.transform.gameObject.GetComponentInParent<TowerTile.Manager>();
					_currentHighlightedTowerTile.ToggleHighlight(true);
				}
			}
		}

		private void ResetSelection()
		{
			if (_currentHighlightedTowerTile == null) return;

			_currentHighlightedTowerTile.ToggleHighlight(false);
			_currentHighlightedTowerTile = null;
		}

		public void InputUse()
		{
			if (State != GameState.Starting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;
			if (_currentHighlightedTowerTile == null) return;

			switch (CurrentInventorySelection)
			{
				case TowerTile.TowerTypes.SingleTargetDamage:
					if (CurrentPlayerGold < SingleTargetDamageCosts) return;
					if (_currentHighlightedTowerTile.SpawnTower(TowerTile.TowerTypes.SingleTargetDamage) == false) return;
					CurrentPlayerGold -= SingleTargetDamageCosts;
					return;

				case TowerTile.TowerTypes.SingleTargetKnockback:
					if (CurrentPlayerGold < SingleTargetKnockbackCosts) return;
					if (_currentHighlightedTowerTile.SpawnTower(TowerTile.TowerTypes.SingleTargetKnockback) == false) return;
					CurrentPlayerGold -= SingleTargetKnockbackCosts;
					return;

				default:
					return;
			}
		}

		public void EnemyReachedCastle(int attackDamage)
		{
			UpdateLives(-attackDamage);
		}

		void UpdateLives(int delta)
		{
			CurrentPlayerLives += delta;
			UpdateLivesText();
			CheckLooseCondition();
		}

		void CheckLooseCondition()
		{
			if (State != GameState.Starting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;
			if (CurrentPlayerLives > 0) return;
			SwitchState(GameState.Lost);
		}

		void UpdateWinLooseSign(int state)
		{
			switch (state)
			{
				case 0:
					WinText.SetActive(false);
					LooseText.SetActive(false);
					WinLooseSign.SetActive(false);
					break;

				case 1:
					WinText.SetActive(true);
					WinLooseSign.SetActive(true);
					break;

				case 2:
					LooseText.SetActive(true);
					WinLooseSign.SetActive(true);
					break;

				default:
					break;
			}
		}

		public void ChangeInventory(TowerTile.TowerTypes newSelection)
		{
			CurrentInventorySelection = newSelection;
		}
	}
}
