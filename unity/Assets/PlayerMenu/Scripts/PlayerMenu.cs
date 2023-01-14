
using UdonSharp;
using UnityEngine;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class PlayerMenu : UdonSharpBehaviour
	{
		public GameManager GameManager;
		public UnityEngine.UI.Text StatusText;
		public UnityEngine.UI.Button NextWaveButton;

		public void GameStateSwitched(GameState newGameState)
		{
			switch (newGameState)
			{
				case GameState.Starting:
					gameObject.SetActive(true);
					break;

				case GameState.Pristine:
				case GameState.Won:
				case GameState.Lost:
					gameObject.SetActive(false);
					break;

				default:
					break;
			}

			switch (newGameState)
			{
				case GameState.Pristine:
				case GameState.WaveSpawning:
				case GameState.WaveWaiting:
					// NextWaveButton.interactable = false;
					break;

				case GameState.Starting:
				case GameState.WaveFinished:
					// NextWaveButton.interactable = true;
					break;

				case GameState.Won:
				case GameState.Lost:
					gameObject.SetActive(false);
					break;

				default:
					break;
			}

			UpdateStatusText(newGameState);
		}

		private void UpdateStatusText(GameState newGameState)
		{
			switch (newGameState)
			{
				case GameState.Starting:
					StatusText.text = $"First wave - Status: Ready";
					break;

				case GameState.WaveSpawning:
					StatusText.text = $"Current wave: {GameManager.CurrentWaveIndex + 1}/{GameManager.Waves.GetLength(0)} - Status: Spawning enemies";
					break;

				case GameState.WaveWaiting:
					StatusText.text = $"Current wave: {GameManager.CurrentWaveIndex + 1}/{GameManager.Waves.GetLength(0)} - Status: Waiting for all enemies to be killed";
					break;

				case GameState.WaveFinished:
					StatusText.text = $"Next wave: {GameManager.CurrentWaveIndex + 1}/{GameManager.Waves.GetLength(0)} - Status: Ready";
					break;

				default:
					break;
			}
		}

		void Update()
		{
			if (GameManager.State == GameState.Pristine || GameManager.State == GameState.Won || GameManager.State == GameState.Lost) return;

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetDamage);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				GameManager.ChangeInventory(TowerTile.TowerTypes.SingleTargetKnockback);
			}
		}

		public void UIButtonReset()
		{
			GameManager.SwitchState(GameState.Pristine);
		}

		public void UIButtonNextWave()
		{
			GameManager.SwitchState(GameState.WaveSpawning);
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
