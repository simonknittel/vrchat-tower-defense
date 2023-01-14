
using UdonSharp;

namespace SimonKnittel.TowerDefense
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Canvas : UdonSharpBehaviour
	{
		public GameManager GameManager;

		public override void Interact()
		{
			GameManager.SwitchState(GameState.Starting);
		}

		public void UIButtonStart()
		{
			GameManager.SwitchState(GameState.Starting);
		}
	}
}
