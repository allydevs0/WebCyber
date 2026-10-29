using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class EnemyTargets : MonoBehaviour
	{
		[SerializeField]
		private int maxTargetVisuals;

		private List<TargetVisual> targetVisuals = new List<TargetVisual>();

		public GameObject targetVisualPrefab;

		internal float targetColorTimer = 1f;

		public int MaxTargetVisuals => maxTargetVisuals;

		public List<TargetVisual> TargetVisuals
		{
			get
			{
				return targetVisuals;
			}
			set
			{
				targetVisuals = value;
			}
		}

		public Vector3 targetVisualPos
		{
			get
			{
				return targetVisuals[0].pos;
			}
			set
			{
				targetVisuals[0].pos = value;
			}
		}

		public float targetVisualPosY
		{
			get
			{
				return targetVisuals[0].pos.y;
			}
			set
			{
				targetVisuals[0].pos.y = value;
			}
		}

		public Vector3 targetVisualVel
		{
			get
			{
				return targetVisuals[0].vel;
			}
			set
			{
				targetVisuals[0].vel = value;
			}
		}

		public Transform targetVisualTf => targetVisuals[0].tf;

		public TargetVisual targetVisualClass => targetVisuals[0];

		public void Init()
		{
			for (int i = targetVisuals.Count; i < maxTargetVisuals; i++)
			{
				Transform obj = Object.Instantiate(targetVisualPrefab).transform;
				TargetVisual item = new TargetVisual(obj, obj.GetChild(0).GetComponent<MeshRenderer>());
				targetVisuals.Add(item);
			}
		}

		public void HandleTargetColors(bool aboutToFire)
		{
			if (aboutToFire)
			{
				targetColorTimer = (targetColorTimer + 10f * Core.dt) % 1f;
			}
		}

		public void SetOffTargetVisuals()
		{
			for (int i = 0; i < maxTargetVisuals; i++)
			{
				targetVisuals[i].SetOff();
			}
		}

		public void SetOnTargetVisuals()
		{
			for (int i = 0; i < maxTargetVisuals; i++)
			{
				targetVisuals[i].SetOn();
			}
		}

		public bool TargetVisualOn()
		{
			return targetColorTimer > 0.5f;
		}

		public void OnDestroy()
		{
			for (int i = 0; i < maxTargetVisuals; i++)
			{
				if (targetVisuals[i].tf != null)
				{
					Object.Destroy(targetVisuals[i].tf.gameObject);
				}
			}
			targetVisuals.Clear();
		}
	}
}
