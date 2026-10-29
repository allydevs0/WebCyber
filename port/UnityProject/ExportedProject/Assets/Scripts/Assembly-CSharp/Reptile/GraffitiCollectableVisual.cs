using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	public class GraffitiCollectableVisual : MonoBehaviour
	{
		private enum VisualType
		{
			Collectable = 0,
			DynamicPickup = 1
		}

		[SerializeField]
		private VisualType pickupType;

		[SerializeField]
		private Transform visualTransform;

		private void Awake()
		{
			GraffitiAppEntry graffitiAppEntry = null;
			if (pickupType == VisualType.Collectable)
			{
				graffitiAppEntry = (GraffitiAppEntry)GetComponent<Collectable>().unlock;
			}
			else if (pickupType == VisualType.DynamicPickup)
			{
				graffitiAppEntry = (GraffitiAppEntry)GetComponent<DynamicPickup>().unlock;
			}
			if (graffitiAppEntry != null)
			{
				switch (graffitiAppEntry.Size)
				{
				case GraffitiSize.M:
					visualTransform.localScale = new Vector3(1f, 1f, 1f);
					break;
				case GraffitiSize.L:
					visualTransform.localScale = new Vector3(2f, 1f, 1f);
					break;
				case GraffitiSize.XL:
					visualTransform.localScale = new Vector3(2f, 0.5f, 1f);
					break;
				}
				visualTransform.GetComponent<MeshRenderer>().material.mainTexture = graffitiAppEntry.GraffitiTexture;
			}
		}
	}
}
