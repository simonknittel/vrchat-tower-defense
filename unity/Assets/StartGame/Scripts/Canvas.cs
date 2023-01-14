
using UdonSharp;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Canvas : UdonSharpBehaviour
	{
		public StartGame StartGame;

		public override void Interact()
		{
			StartGame.GameManager.SwitchState(GameState.Starting);
		}
	}
}
