using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	[Serializable]
	public class StyleSwitchBehaviour : BaseMenuTimelineBehaviour
	{
		public MoveStyle menuType = MoveStyle.SKATEBOARD;

		public override void StartMenu()
		{
			Core.Instance.UIManager.styleSwitchUI.StartMenu(this, menuType);
		}

		public Material[] GetPreviewMaterials()
		{
			GameObject gameObject = null;
			foreach (PlayableBinding output in director.playableAsset.outputs)
			{
				if (output.sourceObject is StyleSwitchTrack)
				{
					gameObject = (GameObject)director.GetGenericBinding(output.sourceObject);
				}
			}
			Material[] result = null;
			if (gameObject != null)
			{
				result = gameObject.GetComponentInChildren<Renderer>().sharedMaterials;
			}
			return result;
		}
	}
}
