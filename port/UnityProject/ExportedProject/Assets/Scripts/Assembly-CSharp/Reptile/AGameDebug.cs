using UnityEngine;

namespace Reptile
{
	public abstract class AGameDebug
	{
		protected BaseModule baseModule;

		protected SaveManager saveManager;

		protected GameInput gameInput;

		protected UIManager uIManager;

		public abstract bool IsShowingDebugMenu { get; }

		public AGameDebug(BaseModule baseModule, SaveManager saveManager, GameInput gameInput, UIManager uIManager)
		{
			this.baseModule = baseModule;
			this.saveManager = saveManager;
			this.gameInput = gameInput;
			this.uIManager = uIManager;
		}

		public virtual void Dispose()
		{
			baseModule = null;
			saveManager = null;
			gameInput = null;
			uIManager = null;
		}

		public virtual void Update()
		{
			UpdateInput();
		}

		protected virtual void UpdateInput()
		{
			UpdateKeyboardInput();
			UpdateJoystickInput();
		}

		protected abstract void InitializeFPSCounter();

		protected abstract void UpdateKeyboardInput();

		protected abstract void UpdateJoystickInput();

		public abstract void ToggleDebugMenu();

		public abstract void ToggleFPSCounter();

		public abstract void CycleFPSCounterPreset();

		public abstract void RestartStage();

		public abstract void SelectStage(Stage stage);

		public abstract void CycleQualitySettings();

		public abstract void UpdateLocalizationLanguage(SystemLanguage systemLanguage);
	}
}
