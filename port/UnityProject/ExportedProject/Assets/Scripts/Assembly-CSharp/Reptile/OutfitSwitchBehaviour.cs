using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class OutfitSwitchBehaviour : BaseMenuTimelineBehaviour
	{
		public override void StartMenu()
		{
			CharacterVisual characterVisual = null;
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (output.sourceObject is OutfitSwitchTrack)
				{
					characterVisual = ((GameObject)director.GetGenericBinding(output.sourceObject)).GetComponent<CharacterVisual>();
				}
			}
			PublicToilet componentInParent = director.gameObject.GetComponentInParent<PublicToilet>();
			Core.Instance.UIManager.outfitSwitchUI.StartMenu(this, characterVisual, componentInParent);
		}
	}
}
