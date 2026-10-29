using System;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class TaxiBehaviour : BaseMenuTimelineBehaviour
	{
		public override void StartMenu()
		{
			NPC taxiNPC = null;
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (output.sourceObject is TaxiTrack)
				{
					taxiNPC = (NPC)director.GetGenericBinding(output.sourceObject);
				}
			}
			Core.Instance.UIManager.taxiUI.Show(taxiNPC, this);
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			base.OnBehaviourPlay(playable, info);
			BaseMenuTimelineBehaviour.LoopAnimationTracks(director, info);
		}
	}
}
