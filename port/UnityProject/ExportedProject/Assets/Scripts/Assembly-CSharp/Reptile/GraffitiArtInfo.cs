using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	[CreateAssetMenu(fileName = "graffitiArtInfo", menuName = "Reptile/Graffiti Art Info", order = 52)]
	public class GraffitiArtInfo : ScriptableObject
	{
		[SerializeField]
		public List<GraffitiArt> graffitiArt;

		public GraffitiArt FindByTitle(string grafTitle)
		{
			if (grafTitle != null && grafTitle.Equals("Swipe"))
			{
				grafTitle = "Base";
			}
			return graffitiArt.Find((GraffitiArt g) => g.title == grafTitle);
		}

		public GraffitiArt FindByMaterialName(string matName)
		{
			return graffitiArt.Find((GraffitiArt g) => g.graffitiMaterial.name == matName);
		}

		public List<GraffitiArt> FindByArtist(string artistName)
		{
			return graffitiArt.FindAll((GraffitiArt g) => g.artistName == artistName);
		}

		public List<GraffitiArt> FindBySize(GraffitiSize size)
		{
			return graffitiArt.FindAll((GraffitiArt g) => g.graffitiSize == size);
		}

		public static bool TargetIsNotAllowed(int availableTargets, int target)
		{
			return (availableTargets & (1 << target)) == 0;
		}

		public int GetAvailableTargets(List<GraffitiArt> unlockedGraffitiArt, List<int> targetSequence, GraffitiSize grafSize)
		{
			int num = 0;
			int num2 = (int)(grafSize + 3);
			int count = targetSequence.Count;
			if (grafSize == GraffitiSize.S || count == num2)
			{
				return int.MaxValue;
			}
			if (count != 0)
			{
				_ = targetSequence[count - 1];
			}
			for (int i = 0; i < unlockedGraffitiArt.Count; i++)
			{
				int combos = (int)unlockedGraffitiArt[i].combos;
				int num3 = (int)Mathf.Pow(10f, num2 - 1 - count);
				int num4 = combos / num3 % 10;
				int num5 = 1;
				for (int j = 0; j < count; j++)
				{
					num3 = (int)Mathf.Pow(10f, num2 - 1 - (count - j - 1));
					int num6 = combos / num3 % 10;
					num5 &= Convert.ToInt32(num6 == targetSequence[count - j - 1]);
				}
				num |= num5 << num4;
			}
			return num;
		}

		public GraffitiArt FindByMaterial(Material m)
		{
			if (m == null)
			{
				return null;
			}
			foreach (GraffitiArt item in graffitiArt)
			{
				if (item.graffitiMaterial.name == m.name)
				{
					return item;
				}
			}
			return null;
		}

		public GraffitiArt FindBySequence(List<int> graffitiGameHitSequence)
		{
			int num = 0;
			int count = graffitiGameHitSequence.Count;
			for (int i = 0; i < count; i++)
			{
				num += graffitiGameHitSequence[i] * (int)Mathf.Pow(10f, count - i - 1);
			}
			GraffitiSize size = (GraffitiSize)(count % 4 + 1);
			List<GraffitiArt> list = FindBySize(size);
			GraffitiArt.Combos seq = (GraffitiArt.Combos)num;
			return list.Find((GraffitiArt x) => x.combos == seq && Core.Instance.Platform.User.GetUnlockableSaveDataFor(x.unlockable).IsUnlocked);
		}

		public GraffitiArt FindByCharacter(Characters character)
		{
			GraffitiArt result = FindByTitle("Red");
			switch (character)
			{
			case Characters.wideKid:
				result = FindByTitle("Mesh");
				break;
			case Characters.bunGirl:
				result = FindByTitle("Shine");
				break;
			case Characters.pufferGirl:
				result = FindByTitle("Rise");
				break;
			case Characters.headMan:
			case Characters.headManNoJetpack:
				result = FindByTitle("Faux");
				break;
			case Characters.ringdude:
				result = FindByTitle("Coil");
				break;
			case Characters.dummy:
				result = FindByTitle("Solace");
				break;
			case Characters.angel:
				result = FindByTitle("Rave");
				break;
			case Characters.blockGuy:
				result = FindByTitle("Tryce");
				break;
			case Characters.spaceGirl:
				result = FindByTitle("Bel");
				break;
			case Characters.girl1:
				result = FindByTitle("Vinyl");
				break;
			case Characters.legendFace:
				result = FindByTitle("Felix");
				break;
			case Characters.metalHead:
			case Characters.legendMetalHead:
				result = FindByTitle("Red");
				break;
			case Characters.frank:
			case Characters.prince:
				result = FindByTitle("‘F’ Is For Family");
				break;
			case Characters.eightBall:
			case Characters.eightBallBoss:
				result = FindByTitle("8O TUP");
				break;
			case Characters.medusa:
				result = FindByTitle("Elegant E");
				break;
			case Characters.boarder:
				result = FindByTitle("DT");
				break;
			case Characters.dj:
			case Characters.futureGirl:
				result = FindByTitle("Eye-con");
				break;
			case Characters.jetpackBossPlayer:
				result = FindByTitle("Irene");
				break;
			case Characters.oldheadPlayer:
				result = FindByTitle("Boombap");
				break;
			case Characters.skate:
				result = FindByTitle("Jay");
				break;
			case Characters.robot:
				result = FindByTitle("Base");
				break;
			}
			return result;
		}

		public GraffitiArt FindCrewArt(Crew crew, GraffitiSize size)
		{
			string matName = "";
			switch (crew)
			{
			case Crew.FRANKS:
				matName = "franksGraf_" + size;
				break;
			case Crew.ROBOTS:
				matName = "DOTEXEGraf_" + size;
				break;
			case Crew.MEDUSAS:
				matName = "EclipseGraf_" + size;
				break;
			case Crew.PLAYERS:
			case Crew.ROGUE:
				matName = "playersGraf_" + size.ToString() + "0";
				break;
			case Crew.SAMURAIS:
				matName = "DevilTheoryGraf_" + size;
				break;
			case Crew.FUTURES:
				matName = "FUTURISMGraf_" + size;
				break;
			}
			return FindByMaterialName(matName);
		}
	}
}
