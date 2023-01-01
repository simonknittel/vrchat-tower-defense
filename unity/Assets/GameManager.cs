
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace SimonKnittel.TowerDefense
{
	public enum GameState
	{
		Pristine,
		Running,
		Won,
		Lost
	}

	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameManager : UdonSharpBehaviour
	{
		public int Gold = 1000;
		public int TotalPlayerLives = 10;
		public int CurrentPlayerLives;
		public int SingleTargetDamageCosts = 100;
		public Waves.WaveManager[] Waves;
		public int TimeMultiplicator = 1;
		public Transform[] Waypoints;
		public GameState GameState = GameState.Pristine;
		public UnityEngine.UI.Text LivesText;
		public UnityEngine.UI.Text WaveSignText;

		VRCPlayerApi _localPlayer;
		TowerTile.Manager _currentHighlightedTowerTile;
		bool _isUserInVR = true;
		TowerTile.TowerTypes _currentInventorySelection = TowerTile.TowerTypes.SingleTargetDamage;

		void Start()
		{
			_localPlayer = Networking.LocalPlayer;
			_isUserInVR = _localPlayer.IsUserInVR();

			ResetGame();
		}

		public void StartGame()
		{
			if (Waves.GetLength(0) == 0) return;
			GameState = GameState.Running;
			Waves[0].SpawnWave();
		}

		public void ResetGame()
		{
			GameState = GameState.Pristine;
			Gold = 1000;
			CurrentPlayerLives = TotalPlayerLives;
			UpdateLivesText();
			UpdateWaveSign();

			foreach (var Wave in Waves)
			{
				Wave.ResetWave();
			}

			// TODO: Reset towers
		}

		void UpdateLivesText()
		{
			LivesText.text = $"Lives {CurrentPlayerLives}/{TotalPlayerLives}";
		}

		void UpdateWaveSign()
		{
			WaveSignText.text = $"Wave 1/{Waves.GetLength(0)}";
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
			if (GameState != GameState.Running) return;

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
			if (GameState != GameState.Running) return;
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
			CurrentPlayerLives -= attackDamage;
			UpdateLivesText();
		}
	}
}
