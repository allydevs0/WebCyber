using UnityEngine;

namespace Reptile.Phone
{
	[CreateAssetMenu(menuName = "Reptile/Phone/Create Contact List")]
	public class PhoneContacts : ScriptableObject
	{
		public Contact[] contacts = new Contact[26];
	}
}
