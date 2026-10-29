using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "SceneLibrary", menuName = "Reptile/Scenes/Scene Library")]
	public class SceneLibrary : ScriptableObject
	{
		[SerializeField]
		[SceneInspector]
		public string coreScenePath = string.Empty;

		[SerializeField]
		[SceneInspector]
		public string introScenePath = string.Empty;

		[SerializeField]
		[SceneInspector]
		public string mainMenuScenePath = string.Empty;

		[SerializeField]
		[SceneInspector]
		public string creditsScenePath = string.Empty;

		[SerializeField]
		[SceneInspector]
		public string bootstrapScenePath = string.Empty;
	}
}
