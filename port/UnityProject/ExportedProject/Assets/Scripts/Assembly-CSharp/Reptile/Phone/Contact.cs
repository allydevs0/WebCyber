using System;
using UnityEngine;

namespace Reptile.Phone
{
	[Serializable]
	public struct Contact : IComparable<Contact>
	{
		public int characterID;

		public Sprite avatar;

		public int CompareTo(Contact other)
		{
			return characterID.CompareTo(other.characterID);
		}
	}
}
