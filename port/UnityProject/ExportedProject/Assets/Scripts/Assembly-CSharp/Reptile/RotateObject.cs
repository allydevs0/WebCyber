using UnityEngine;

namespace Reptile
{
	public class RotateObject : MonoBehaviour
	{
		[SerializeField]
		private Space rotationSpace;

		[SerializeField]
		private Vector3 rotateVelocity;

		private Transform referenceTransform;

		private Transform myTransform;

		private Rigidbody body;

		private Renderer[] renderers;

		private bool updateRendererVisibility;

		private void Start()
		{
			DetermineUpdateMethod();
			body = GetComponent<Rigidbody>();
			if (body == null)
			{
				body = GetComponentInChildren<Rigidbody>();
			}
			if ((bool)body)
			{
				referenceTransform = new GameObject("rotationObjectReference " + base.gameObject.name).transform;
				referenceTransform.parent = base.transform.parent;
				referenceTransform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			}
			myTransform = base.transform;
		}

		private void OnEnable()
		{
			if (updateRendererVisibility)
			{
				Core.OnFixedUpdate += UpdateRendererVisibility;
			}
		}

		private void OnDisable()
		{
			if (updateRendererVisibility)
			{
				Core.OnFixedUpdate -= UpdateRendererVisibility;
			}
			else
			{
				Core.OnFixedUpdate -= UpdateRotateObject;
			}
		}

		private void OnDestroy()
		{
			if (updateRendererVisibility)
			{
				Core.OnFixedUpdate -= UpdateRendererVisibility;
			}
			else
			{
				Core.OnFixedUpdate -= UpdateRotateObject;
			}
		}

		private void OnBecameInvisible()
		{
			if (Application.isPlaying && !updateRendererVisibility)
			{
				Core.OnFixedUpdate -= UpdateRotateObject;
			}
		}

		private void OnBecameVisible()
		{
			if (Application.isPlaying && !updateRendererVisibility)
			{
				Core.OnFixedUpdate += UpdateRotateObject;
			}
		}

		private void UpdateRotateObject()
		{
			if ((bool)body)
			{
				referenceTransform.Rotate(rotateVelocity * Core.dt, rotationSpace);
				body.MoveRotation(referenceTransform.rotation);
			}
			else
			{
				myTransform.Rotate(rotateVelocity * Core.dt, rotationSpace);
			}
		}

		private void UpdateRendererVisibility()
		{
			if (DetermineRendererVisibility())
			{
				UpdateRotateObject();
			}
		}

		private void DetermineUpdateMethod()
		{
			if (GetComponent<Renderer>() == null)
			{
				Core.OnFixedUpdate += UpdateRendererVisibility;
				updateRendererVisibility = true;
				renderers = GetComponentsInChildren<Renderer>();
			}
		}

		private bool DetermineRendererVisibility()
		{
			for (int i = 0; i < renderers.Length; i++)
			{
				if (renderers[i].isVisible)
				{
					return true;
				}
			}
			return false;
		}
	}
}
