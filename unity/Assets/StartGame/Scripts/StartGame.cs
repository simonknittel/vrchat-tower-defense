
using UdonSharp;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class StartGame : UdonSharpBehaviour
	{
		public GameManager GameManager;

		public void GameStateSwitched(GameState newGameState)
		{
			switch (newGameState)
			{
				case GameState.Pristine:
					gameObject.SetActive(true);
					break;

				case GameState.Starting:
					gameObject.SetActive(false);
					break;

				default:
					break;
			}
		}

		public void UIButtonStart()
		{
			GameManager.SwitchState(GameState.Starting);
		}
	}
}
