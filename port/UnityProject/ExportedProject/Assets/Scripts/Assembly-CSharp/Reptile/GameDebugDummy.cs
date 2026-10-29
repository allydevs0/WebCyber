using UnityEngine;

namespace Reptile
{
	public class GameDebugDummy : AGameDebug
	{
		public override bool IsShowingDebugMenu => false;

		public GameDebugDummy(BaseModule baseModule, SaveManager saveManager, GameInput gameInput, UIManager uIManager)
			: base(baseModule, saveManager, gameInput, uIManager)
		{
			gameInput.DisableControllerMap(3);
		}

		protected override void InitializeFPSCounter()
		{
		}

		protected override void UpdateJoystickInput()
		{
		}

		protected override void UpdateKeyboardInput()
		{
		}

		public override void ToggleDebugMenu()
		{
		}

		public override void ToggleFPSCounter()
		{
		}

		public override void CycleFPSCounterPreset()
		{
		}

		public override void RestartStage()
		{
		}

		public override void SelectStage(Stage stage)
		{
		}

		public override void CycleQualitySettings()
		{
		}

		public override void UpdateLocalizationLanguage(SystemLanguage systemLanguage)
		{
		}
	}
}
