using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "GameParameters", menuName = "Reptile/GameSetup/GameParameters", order = 53)]
	public class GameParameters : ScriptableObject
	{
		public string startFromStage = string.Empty;
	}
}
