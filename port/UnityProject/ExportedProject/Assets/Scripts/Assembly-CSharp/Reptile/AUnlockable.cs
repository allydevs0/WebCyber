using UnityEngine;

namespace Reptile
{
	public abstract class AUnlockable : ScriptableObject
	{
		[SerializeField]
		[HideInInspector]
		protected string uid = string.Empty;

		[SerializeField]
		protected bool isDefault;

		public string Uid
		{
			get
			{
				return uid;
			}
			set
			{
				uid = value;
			}
		}

		public bool IsDefault
		{
			get
			{
				return isDefault;
			}
			set
			{
				isDefault = value;
			}
		}
	}
}
