using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class SplineBasedGrindLineGenerator : MonoBehaviour
	{
		[SerializeField]
		private bool generateAtRuntime = true;

		public BezierSpline spline;

		public float interval = 0.6f;

		public float skipNodesWhenAngleLessThan = 5f;

		private List<GameObject> nodeSequence = new List<GameObject>();

		private List<GameObject> nodeSequenceTotal = new List<GameObject>();

		private List<GameObject> lineSequenceTotal = new List<GameObject>();

		private float precision = 0.01f;

		private float distTravelledSinceLastSpawn;

		private float timeTravelled;

		private Vector3 prevPos = Vector3.zero;

		private int nodeIndex = -1;

		private void Awake()
		{
			if (generateAtRuntime)
			{
				Clear();
				SetPrecision();
				Spawn(newParent: false);
			}
		}

		public void Bake()
		{
			Clear();
			SetPrecision();
			Spawn(newParent: true);
			generateAtRuntime = false;
		}

		private void Clear()
		{
			Transform[] allChildren = base.transform.GetAllChildren();
			for (int num = allChildren.Length - 1; num > -1; num--)
			{
				if (allChildren[num].GetComponent<GrindNode>() != null || allChildren[num].GetComponent<GrindLine>() != null)
				{
					if (Application.isPlaying)
					{
						Object.Destroy(allChildren[num].gameObject);
					}
					else
					{
						Object.DestroyImmediate(allChildren[num].gameObject);
					}
				}
			}
			nodeSequence.Clear();
			nodeSequenceTotal.Clear();
			lineSequenceTotal.Clear();
			nodeIndex = -1;
		}

		private void SetPrecision()
		{
			precision = 0.1f;
			float num = float.MaxValue;
			Vector3 zero = Vector3.zero;
			while (num > interval)
			{
				zero = spline.GetPoint(0f);
				num = Vector3.Distance(spline.GetPoint(precision), zero);
				precision /= 2f;
			}
		}

		public void Spawn(bool newParent)
		{
			Transform parent = base.transform;
			if (newParent)
			{
				GameObject obj = new GameObject();
				obj.name = "Grind Path";
				obj.transform.position = base.transform.position;
				parent = obj.transform;
				GrindPath component = base.transform.GetComponent<GrindPath>();
				GrindPath grindPath = obj.AddComponent<GrindPath>();
				grindPath.hardCornerBoostsAllowed = component.hardCornerBoostsAllowed;
				grindPath.softCornerBoostsAllowed = component.softCornerBoostsAllowed;
				grindPath.upwardsGrindJumpAllowed = component.upwardsGrindJumpAllowed;
			}
			while (timeTravelled < 1f)
			{
				Vector3 point = spline.GetPoint(timeTravelled);
				Vector3 direction = spline.GetDirection(timeTravelled);
				distTravelledSinceLastSpawn += Vector3.Distance(prevPos, point);
				bool flag = distTravelledSinceLastSpawn > interval;
				if ((nodeIndex > 2) & flag)
				{
					flag = Vector3.Angle(nodeSequenceTotal[nodeIndex - 1].transform.forward, direction) > skipNodesWhenAngleLessThan;
				}
				if (flag)
				{
					GameObject obj2 = new GameObject();
					obj2.name = "GrindNode" + nodeIndex;
					obj2.layer = LayerMask.NameToLayer("Grinding");
					obj2.AddComponent<GrindNode>();
					Transform transform = obj2.transform;
					Vector3 vector = point;
					transform.transform.localPosition = vector;
					transform.transform.LookAt(vector + direction, spline.GetUp(timeTravelled));
					transform.transform.parent = parent;
					nodeSequence.Add(transform.gameObject);
					nodeSequenceTotal.Add(transform.gameObject);
					nodeIndex++;
					if (nodeSequence.Count == 2)
					{
						GameObject obj3 = new GameObject
						{
							name = "GrindLine" + nodeIndex,
							layer = LayerMask.NameToLayer("Grinding")
						};
						GrindLine grindLine = obj3.AddComponent<GrindLine>();
						Transform transform2 = obj3.transform;
						transform2.position = nodeSequence[0].transform.position * 0.5f + nodeSequence[1].transform.position * 0.5f;
						transform2.up = nodeSequence[1].transform.position - transform2.position;
						CapsuleCollider component2 = obj3.GetComponent<CapsuleCollider>();
						component2.radius = 0.5f;
						component2.height = Vector3.Distance(nodeSequence[0].transform.position, nodeSequence[1].transform.position);
						component2.direction = 1;
						component2.isTrigger = true;
						transform2.parent = parent;
						grindLine.attachedToEnemyGrindLine = null;
						grindLine.cornerBoost = true;
						grindLine.isPole = false;
						grindLine.upwardsGrindJump = false;
						grindLine.n0 = nodeSequence[0].GetComponent<GrindNode>();
						grindLine.n0.grindLines.Add(grindLine);
						grindLine.n1 = nodeSequence[1].GetComponent<GrindNode>();
						grindLine.n1.grindLines.Add(grindLine);
						nodeSequence.RemoveAt(0);
						lineSequenceTotal.Add(transform2.gameObject);
					}
					distTravelledSinceLastSpawn = 0f;
				}
				prevPos = point;
				timeTravelled += precision;
			}
			timeTravelled = 0f;
		}
	}
}
