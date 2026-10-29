using System;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public class Arm
	{
		public bool armAlive;

		public Transform armRoot;

		public EnemyExtension armExtension;

		public ParticleSystem armSteam;

		public ParticleSystem armSmoke;

		public ParticleSystem armExplosion;

		public GameObject[] armGrindLinesLong;

		public GameObject[] armGrindLinesShort;

		public SkinnedMeshRenderer hand;

		public GameObject handStandIn;

		public void SwitchToShortGrindLines()
		{
			if (armAlive)
			{
				for (int i = 0; i < armGrindLinesLong.Length; i++)
				{
					armGrindLinesLong[i].SetActive(value: false);
				}
				for (int j = 0; j < armGrindLinesShort.Length; j++)
				{
					armGrindLinesShort[j].SetActive(value: true);
				}
			}
		}

		public void SwitchToLongGrindLines()
		{
			if (armAlive)
			{
				for (int i = 0; i < armGrindLinesShort.Length; i++)
				{
					armGrindLinesShort[i].SetActive(value: false);
				}
				for (int j = 0; j < armGrindLinesLong.Length; j++)
				{
					armGrindLinesLong[j].SetActive(value: true);
				}
			}
		}

		public void DeactivateAllGrindLines()
		{
			for (int i = 0; i < armGrindLinesLong.Length; i++)
			{
				armGrindLinesLong[i].SetActive(value: false);
			}
			for (int j = 0; j < armGrindLinesShort.Length; j++)
			{
				armGrindLinesShort[j].SetActive(value: false);
			}
		}

		public void ResetArm()
		{
			armAlive = true;
			hand.enabled = true;
			armRoot.localScale = Vector3.one;
			armRoot.gameObject.SetActive(value: true);
			armExtension.CloseExtension();
			armExtension.ResetGraffitiSpots();
		}

		public bool PlaySteam()
		{
			if (!armSteam.isPlaying)
			{
				armSteam.Play();
				return true;
			}
			return false;
		}

		public bool StopSteam()
		{
			if (armSteam.isPlaying)
			{
				armSteam.Stop();
				return true;
			}
			return false;
		}

		public bool PlayOverheat()
		{
			if (!armSmoke.isPlaying)
			{
				armSmoke.Play();
				return true;
			}
			return false;
		}

		public bool StopOverheat()
		{
			if (armSmoke.isPlaying)
			{
				armSmoke.Stop();
				return true;
			}
			return false;
		}

		public void SetGrindLineRadiuses(float radius)
		{
			if (armAlive)
			{
				for (int i = 0; i < armGrindLinesLong.Length; i++)
				{
					armGrindLinesLong[i].GetComponent<CapsuleCollider>().radius = radius;
				}
				for (int j = 0; j < armGrindLinesShort.Length; j++)
				{
					armGrindLinesShort[j].GetComponent<CapsuleCollider>().radius = radius;
				}
			}
		}
	}
}
