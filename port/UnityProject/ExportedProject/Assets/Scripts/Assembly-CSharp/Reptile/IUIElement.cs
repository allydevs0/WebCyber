using UnityEngine;

namespace Reptile
{
	public interface IUIElement
	{
		bool IsEnabled { get; set; }

		bool AllowMouseInput { get; }

		GameObject GetFirstFocusGameObject();

		void Activate();

		void Deactivate();

		GameObject GetRoot();
	}
}
