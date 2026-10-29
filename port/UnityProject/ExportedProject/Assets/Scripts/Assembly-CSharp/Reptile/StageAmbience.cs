using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "StageAmbience", menuName = "Reptile/Audio/Stage Ambience")]
	public class StageAmbience : ScriptableObject
	{
		public const SfxCollectionID sfxCollectionID = SfxCollectionID.AmbienceSfx;

		[AudioClipFilter("sfxCollectionID")]
		public AudioClipID[] stageAmbiences;

		public AudioClipID GetStageAmbience(Stage stage)
		{
			AudioClipID result = AudioClipID.NONE;
			if (stage >= Stage.city && (int)stage < stageAmbiences.Length)
			{
				result = stageAmbiences[(int)stage];
			}
			return result;
		}
	}
}
