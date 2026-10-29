using UnityEngine;

namespace Reptile.Phone
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Email/New Message")]
	public class EmailMessage : AUnlockable
	{
		public int characterNameIDOfSender;

		public string subject;

		public Color profileBackground = Color.white;

		public string[] message = new string[1] { string.Empty };

		public void AssignTimestamp()
		{
			_ = Core.Instance.SaveManager.CurrentSaveSlot.emailIndexTable.Count;
		}
	}
}
