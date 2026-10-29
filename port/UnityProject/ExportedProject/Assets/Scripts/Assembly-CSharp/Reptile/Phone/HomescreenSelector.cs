using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class HomescreenSelector : MonoBehaviour
	{
		[SerializeField]
		private Image m_Background;

		[SerializeField]
		private Image m_Arrow;

		public Image Background => m_Background;

		public Image Arrow => m_Arrow;
	}
}
