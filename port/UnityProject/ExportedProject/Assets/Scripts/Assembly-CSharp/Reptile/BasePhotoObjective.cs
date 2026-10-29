using System.Collections;
using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	public abstract class BasePhotoObjective : MonoBehaviour
	{
		private Transform photoObjectiveTransform;

		[Tooltip("Assign this or the collider that is responsible for detecting photos taken for this photo objective, otherwise it will not work.")]
		[SerializeField]
		private Renderer photoObjectiveRenderer;

		[Tooltip("Assign this or the renderer that is responsible for detecting photos taken for this photo objective, otherwise it will not work.")]
		[SerializeField]
		private Collider photoObjectiveCollider;

		[Tooltip("The maximum distance from where a photo can be taken of this objective.")]
		[SerializeField]
		private float maxDistance = 22f;

		[Tooltip("The minimum distance from where a photo can be taken of this objective.")]
		[SerializeField]
		private float minDistance;

		[Tooltip("A scale from 0 to 1, where 0 is the top of the object and 1 is the middle of the object.")]
		[Range(0f, 1f)]
		[SerializeField]
		private float topOfObjectAdjustmentFactor = 0.1f;

		private bool hasCollider;

		public void InitPhotoObjective(Collider collider, Renderer renderer)
		{
			photoObjectiveCollider = collider;
			photoObjectiveRenderer = renderer;
			if (photoObjectiveCollider != null)
			{
				hasCollider = true;
			}
		}

		protected abstract IEnumerator HandlePhotoSucces();

		protected virtual void Awake()
		{
			photoObjectiveTransform = base.transform;
			minDistance = Mathf.Max(minDistance, 0f);
			if (photoObjectiveCollider != null)
			{
				hasCollider = true;
			}
		}

		protected void OnEnable()
		{
			AppCamera.OnTakenPhoto += OnTakenPhoto;
		}

		protected void OnDisable()
		{
			AppCamera.OnTakenPhoto -= OnTakenPhoto;
		}

		protected void OnDestroy()
		{
			AppCamera.OnTakenPhoto -= OnTakenPhoto;
		}

		private void OnTakenPhoto(Camera photoCamera)
		{
			if (!ValidateObjectiveBounds())
			{
				return;
			}
			Vector3 position = photoCamera.transform.position;
			float sqrMagnitude = (photoObjectiveTransform.position - position).sqrMagnitude;
			if (IsWithinRange(sqrMagnitude))
			{
				Bounds objectiveBounds = GetObjectiveBounds();
				Vector3[] objectTestPositions = CreateObjectTestPositions(position, objectiveBounds);
				if (IsInCameraViewport(photoCamera, objectTestPositions))
				{
					StartCoroutine(HandlePhotoSucces());
				}
			}
		}

		private bool ValidateObjectiveBounds()
		{
			if (photoObjectiveCollider == null || !photoObjectiveCollider.enabled)
			{
				hasCollider = false;
			}
			if (!hasCollider && (photoObjectiveRenderer == null || !photoObjectiveRenderer.enabled))
			{
				return false;
			}
			return true;
		}

		private bool IsWithinRange(float distanceSqrd)
		{
			float num = maxDistance * maxDistance;
			float num2 = minDistance * minDistance;
			if (distanceSqrd >= num2)
			{
				return distanceSqrd <= num;
			}
			return false;
		}

		private Bounds GetObjectiveBounds()
		{
			if (hasCollider)
			{
				return photoObjectiveCollider.bounds;
			}
			return photoObjectiveRenderer.bounds;
		}

		private Vector3[] CreateObjectTestPositions(Vector3 cameraPosition, Bounds photoObjectiveBounds)
		{
			float num = 1f - topOfObjectAdjustmentFactor;
			return new Vector3[4]
			{
				photoObjectiveBounds.center,
				photoObjectiveBounds.center + new Vector3(0f, photoObjectiveBounds.extents.y * num, 0f),
				photoObjectiveBounds.center - new Vector3(0f, photoObjectiveBounds.extents.y * num, 0f),
				photoObjectiveBounds.ClosestPoint(cameraPosition)
			};
		}

		private bool IsInCameraViewport(Camera camera, Vector3[] objectTestPositions)
		{
			for (int i = 0; i < objectTestPositions.Length; i++)
			{
				if (IsWithinViewportBounds(camera, objectTestPositions[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool IsWithinViewportBounds(Camera camera, Vector3 position)
		{
			Vector3 vector = camera.WorldToViewportPoint(position);
			if (vector.z < 0f)
			{
				return false;
			}
			if (vector.x >= 0f && vector.x < 1f && vector.y >= 0f)
			{
				return vector.y < 1f;
			}
			return false;
		}
	}
}
