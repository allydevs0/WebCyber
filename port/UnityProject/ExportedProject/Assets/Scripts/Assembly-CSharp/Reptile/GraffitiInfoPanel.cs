using DG.Tweening;
using Reptile.Phone;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class GraffitiInfoPanel : MonoBehaviour
	{
		[Space(5f)]
		[SerializeField]
		private RawImage m_GraffitiImage;

		[SerializeField]
		private TextMeshProUGUI m_TitleLabel;

		[SerializeField]
		private TextMeshProUGUI m_ArtistLabel;

		[SerializeField]
		private GameObject m_StripePrefab;

		[SerializeField]
		private TextMeshProUGUI sizeMarkerText;

		[SerializeField]
		private GraffitiInfoPanelTargetSet[] targetSets;

		private int lastActiveSet;

		private AnimatedStripe[] stripes;

		private Vector2[] imageSizes;

		private Sequence comboAnimation;

		private void Awake()
		{
			for (int i = 0; i < targetSets.Length; i++)
			{
				targetSets[i].Initalize();
				targetSets[i].gameObject.SetActive(value: false);
			}
			lastActiveSet = 0;
			imageSizes = new Vector2[4]
			{
				new Vector2(500f, 500f),
				new Vector2(500f, 500f),
				new Vector2(1000f, 500f),
				new Vector2(1024f, 256f)
			};
			int num = 7;
			Transform parent = base.transform.Find("Stripes");
			stripes = new AnimatedStripe[num];
			for (int j = 0; j < stripes.Length; j++)
			{
				GameObject gameObject = Object.Instantiate(m_StripePrefab, parent);
				gameObject.SetActive(value: false);
				stripes[j] = gameObject.GetComponent<AnimatedStripe>();
			}
		}

		private void OnDestroy()
		{
			if (comboAnimation != null && comboAnimation.active)
			{
				comboAnimation.Kill();
			}
		}

		public void Show(GraffitiAppEntry content)
		{
			m_TitleLabel.text = content.Title;
			m_ArtistLabel.text = content.Artist;
			m_GraffitiImage.texture = content.GraffitiTexture;
			switch (content.Size)
			{
			case GraffitiSize.S:
				sizeMarkerText.text = "S";
				break;
			case GraffitiSize.M:
				sizeMarkerText.text = "M";
				break;
			case GraffitiSize.L:
				sizeMarkerText.text = "L";
				break;
			case GraffitiSize.XL:
				sizeMarkerText.text = "XL";
				break;
			}
			int size = (int)content.Size;
			m_GraffitiImage.rectTransform.sizeDelta = imageSizes[size];
			int num = size - 1;
			if (lastActiveSet != num && lastActiveSet != -1)
			{
				targetSets[lastActiveSet].gameObject.SetActive(value: false);
			}
			targetSets[num].gameObject.SetActive(value: true);
			lastActiveSet = num;
			int num2 = stripes.Length - (3 - size);
			for (int num3 = stripes.Length - 1; num3 >= num2; num3--)
			{
				stripes[num3].gameObject.SetActive(value: false);
			}
			targetSets[num].gameObject.SetActive(value: true);
			if (content.Combos != GraffitiArt.Combos.NONE)
			{
				ShowCombo(content, num2);
			}
			base.transform.DOScale(1f, 0.15f).SetEase(Ease.OutExpo);
		}

		public void ShowCombo(GraffitiAppEntry currentEntry, int currentTargetsLength)
		{
			int num = (int)currentEntry.Combos;
			int[] array = new int[currentTargetsLength];
			for (int num2 = currentTargetsLength - 1; num2 >= 0; num2--)
			{
				array[num2] = num % 10;
				num /= 10;
			}
			if (comboAnimation != null && comboAnimation.active)
			{
				comboAnimation.Kill();
			}
			comboAnimation = DOTween.Sequence();
			comboAnimation.SetLoops(-1);
			Image[] targets = targetSets[(int)(currentEntry.Size - 1)].Targets;
			for (int i = 0; i < currentTargetsLength; i++)
			{
				Vector2 anchoredPosition = targets[array[i]].rectTransform.anchoredPosition;
				Vector2 anchoredPosition2 = targets[array[(i + 1) % currentTargetsLength]].rectTransform.anchoredPosition;
				RectTransform rectTransform = stripes[i].RectTransform();
				rectTransform.anchoredPosition = anchoredPosition;
				rectTransform.localScale = Vector3.one;
				rectTransform.sizeDelta = new Vector2(0f, rectTransform.sizeDelta.y);
				rectTransform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0f, 0f, 90f) * (anchoredPosition2 - anchoredPosition));
				Vector2 vector = new Vector2(Vector2.Distance(anchoredPosition, anchoredPosition2), rectTransform.sizeDelta.y);
				stripes[i].gameObject.SetActive(value: true);
				stripes[i].UpdateStripeSize(vector);
				comboAnimation.Append(rectTransform.DOSizeDelta(vector, 0.25f));
				comboAnimation.AppendInterval(0.25f);
			}
		}

		public void Hide()
		{
			if (comboAnimation != null && comboAnimation.active)
			{
				comboAnimation.Kill();
			}
			base.transform.DOScale(0f, 0.15f).SetEase(Ease.OutExpo);
		}
	}
}
