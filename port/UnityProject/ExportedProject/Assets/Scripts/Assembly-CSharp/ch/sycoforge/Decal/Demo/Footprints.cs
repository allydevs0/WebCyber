using UnityEngine;

namespace ch.sycoforge.Decal.Demo
{
	public class Footprints : MonoBehaviour
	{
		public EasyDecal DecalPrefab;

		public float DistanceThreshold = 0.8f;

		public float FootDistance = 0.5f;

		private float distance;

		private int index;

		private Vector3 lastPosition;

		public void Start()
		{
			if (DecalPrefab == null)
			{
				Debug.LogError("The DynamicDemo script has no decal prefab attached.");
			}
		}

		public void Update()
		{
			if (DecalPrefab == null)
			{
				return;
			}
			if (distance >= DistanceThreshold)
			{
				Vector3 position = ((index == 0) ? Vector3.right : Vector3.left) * FootDistance * 0.5f;
				Vector3 vector = base.transform.TransformPoint(position);
				if (Physics.Raycast(new Ray(vector + Vector3.up * 0.1f, Vector3.down), out var hitInfo, 200f))
				{
					EasyDecal easyDecal = EasyDecal.ProjectAt(DecalPrefab.gameObject, hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
					easyDecal.AtlasRegionIndex = index;
					easyDecal.transform.rotation = Quaternion.Euler(Vector3.up * base.transform.rotation.eulerAngles.y);
				}
				index = ++index % 2;
				distance = 0f;
			}
			distance += Vector3.Distance(lastPosition, base.transform.position);
			lastPosition = base.transform.position;
		}
	}
}
