
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense
{
	public enum GameState
	{
		Pristine,
		Waiting,
		WaveSpawning,
		WaveWaiting,
		WaveFinished,
		Won,
		Lost
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameManager : UdonSharpBehaviour
	{
		public int Gold = 200;
		public int TotalPlayerLives = 10;
		public int CurrentPlayerLives;
		public int SingleTargetDamageCosts = 100;
		public Waves.WaveManager[] Waves;
		public int TimeMultiplicator = 1;
		public Transform[] Waypoints;
		public GameState State = GameState.Pristine;
		public UnityEngine.UI.Text LivesText;
		public UnityEngine.UI.Text WaveSignText;
		public GameObject WinLooseSign;
		public GameObject WinText;
		public GameObject LooseText;
		public UnityEngine.UI.Button NextWaveButton;

		VRCPlayerApi _localPlayer;
		TowerTile.Manager _currentHighlightedTowerTile;
		bool _isUserInVR = true;
		TowerTile.TowerTypes _currentInventorySelection = TowerTile.TowerTypes.SingleTargetDamage;
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
					ResetGame();
					break;

				case GameState.Waiting:
					NextWaveButton.interactable = false;
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

		public void UIButtonStart()
		{
			SwitchState(GameState.WaveSpawning);
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

		public void ResetGame()
		{
			Gold = 200;
			CurrentPlayerLives = TotalPlayerLives;
			_currentWaveIndex = 0;
			UpdateLivesText();
			UpdateWaveSign();
			UpdateWinLooseSign(0);

			foreach (var Wave in Waves)
			{
				Wave.SwitchState(TowerDefense.Waves.State.None);
			}

			// TODO: Reset towers
		}

		void UpdateLivesText()
		{
			LivesText.text = $"Lives {CurrentPlayerLives}/{TotalPlayerLives}";
		}

		void UpdateWaveSign()
		{
			WaveSignText.text = $"Wave {_currentWaveIndex + 1}/{Waves.GetLength(0)}";
		}

		void SetTimeMultiplicator(int newValue)
		{
			TimeMultiplicator = newValue;

			foreach (var Wave in Waves)
			{
				Wave.UpdateTimeMultiplicator();
			}
		}

		public void TimeMultiplicator1x()
		{
			SetTimeMultiplicator(1);
		}

		public void TimeMultiplicator2x()
		{
			SetTimeMultiplicator(2);
		}

		public void TimeMultiplicator4x()
		{
			SetTimeMultiplicator(4);
		}

		void Update()
		{
			if (State != GameState.Waiting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;

			if (_currentHighlightedTowerTile != null)
			{
				_currentHighlightedTowerTile.ToggleHighlight(false);
				_currentHighlightedTowerTile = null;
			}

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
			if (Physics.Raycast(ray, out hit, 5f))
			{
				if (hit.transform && hit.transform.gameObject.name.Contains("RaycastTarget"))
				{
					_currentHighlightedTowerTile = hit.transform.gameObject.GetComponentInParent<TowerTile.Manager>();
					_currentHighlightedTowerTile.ToggleHighlight(true);
				}
			}
		}

		public void InputUse()
		{
			if (State != GameState.Waiting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;
			if (_currentHighlightedTowerTile == null) return;

			switch (_currentInventorySelection)
			{
				case TowerTile.TowerTypes.SingleTargetDamage:
					if (Gold < SingleTargetDamageCosts) return;
					if (_currentHighlightedTowerTile.SpawnTower(TowerTile.TowerTypes.SingleTargetDamage) == false) return;
					Gold -= SingleTargetDamageCosts;
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
			if (State != GameState.Waiting && State != GameState.WaveSpawning && State != GameState.WaveWaiting && State != GameState.WaveFinished) return;
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
	}
}
