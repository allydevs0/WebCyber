using System;
using UnityEngine;

namespace Reptile
{
	public class DynamicJunkUpdater : MonoBehaviour
	{
		[Serializable]
		public struct DynamicJunkInfo
		{
			[HideInInspector]
			public int offsetInArray;

			[HideInInspector]
			public int rollingIndex;

			public int count;

			public GameObject prefab;
		}

		private JunkBehaviour junkBehaviour;

		private bool isUpdating;

		public DynamicJunkInfo[] junkInfo = new DynamicJunkInfo[2]
		{
			new DynamicJunkInfo
			{
				prefab = null,
				count = 10,
				rollingIndex = 0
			},
			new DynamicJunkInfo
			{
				prefab = null,
				count = 5,
				rollingIndex = 0
			}
		};

		private Junk[] dynamicJunk;

		private void Awake()
		{
			Vector3 position = default;
			position.x = 0f;
			position.y = -1000f;
			position.z = 0f;
			int num = 0;
			for (int i = 0; i < junkInfo.Length; i++)
			{
				junkInfo[i].offsetInArray = num;
				num += junkInfo[i].count;
			}
			dynamicJunk = new Junk[num];
			int num2 = 0;
			for (int j = 0; j < junkInfo.Length; j++)
			{
				for (int k = num2; k < num2 + junkInfo[j].count; k++)
				{
					dynamicJunk[k] = UnityEngine.Object.Instantiate(junkInfo[j].prefab, position, Quaternion.identity).GetComponent<Junk>();
					dynamicJunk[k].destroyTimer = 0f;
				}
				num2 += junkInfo[j].count;
			}
			junkBehaviour = new JunkBehaviour();
			junkBehaviour.Init(dynamicJunk);
			for (int l = 0; l < dynamicJunk.Length; l++)
			{
				JunkBehaviour.SetJunkToNotUpdate(junkBehaviour, dynamicJunk[0]);
			}
		}

		private void OnDestroy()
		{
			isUpdating = false;
			junkBehaviour = null;
			Core.OnUpdate -= OnCoreUpdate;
		}

		private void OnCoreUpdate()
		{
			JunkBehaviour.JunkUpdate(junkBehaviour, Core.dt);
		}

		public Rigidbody AddJunk(DynamicJunk junkToSpawn, Vector3 position, Quaternion rotation)
		{
			DynamicJunkInfo dynamicJunkInfo = junkInfo[(int)junkToSpawn];
			int offsetInArray = dynamicJunkInfo.offsetInArray;
			int num = -1;
			int num2 = dynamicJunk.Length - 1;
			int startIndex = dynamicJunk[num2]._startIndex;
			while (JunkBehaviour.IsJunkNonUpdating(junkBehaviour.nonupdatingJunkIndex, num2))
			{
				if (startIndex < offsetInArray || startIndex >= offsetInArray + dynamicJunkInfo.count)
				{
					num2--;
					startIndex = dynamicJunk[num2]._startIndex;
					continue;
				}
				num = num2;
				break;
			}
			if (num < 0)
			{
				num2 = 0;
				startIndex = dynamicJunk[num2]._startIndex;
				while (JunkBehaviour.IsJunkMoving(junkBehaviour.kickedJunkIndex, num2))
				{
					if (startIndex < offsetInArray || startIndex >= offsetInArray + dynamicJunkInfo.count)
					{
						num2++;
						startIndex = dynamicJunk[num2]._startIndex;
						continue;
					}
					num = num2;
					break;
				}
			}
			Junk junk = dynamicJunk[num];
			junk.originPosition = position;
			junk.originRotation = rotation;
			junk.destroyTimer = 0f;
			JunkBehaviour.RestoreSingle(junkBehaviour, junk);
			if (!isUpdating)
			{
				Core.OnUpdate += OnCoreUpdate;
				isUpdating = true;
			}
			return junk.rigidBody;
		}
	}
}
