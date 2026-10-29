using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class GameplayUI : MonoBehaviour
	{
		public RectTransform gameplayScreen;

		public RectTransform graffitiScreen;

		public TextMeshProUGUI totalScoreLabel;

		public TextMeshProUGUI targetScoreLabel;

		public TextMeshProUGUI totalScoreTitleLabel;

		public TextMeshProUGUI targetScoreTitleLabel;

		public TextMeshProUGUI scoreCalcLabel;

		public TextMeshProUGUI scoreMultiplierLabel;

		public TextMeshProUGUI scoreTrickLabel;

		public TextMeshProUGUI trickNameLabel;

		public RectTransform currentTrickTransform;

		public TextMeshProUGUI tricksInComboLabel;

		public TextMeshProUGUI comboMascotLabel;

		public Image comboMascotIcon;

		public Image comboTimeOutBar;

		public Image comboTimeOutBackdrop;

		public Image contextGraffitiIcon;

		public Image contextTalkIcon;

		public Image breakChainsContextIcon;

		public TextMeshProUGUI contextLabel;

		public TextMeshProUGUI timeLimitLabel;

		public TextMeshProUGUI informationLabel;

		public TextMeshProUGUI musicCollectableUnlockInformation;

		public TextMeshProUGUI repLabel;

		public TextMeshProUGUI addRepLabel;

		public RectTransform repField;

		public GameObject repGroup;

		public GameObject challengeGroup;

		public Image chargeBar;

		public Image chargeBarChunk;

		public Color chargeBarDefaultColor;

		public float chargeBarDefaultColorIndex;

		public float chargeBarTrickingColorIndex;

		public TextMeshProUGUI graffitiTitle;

		public TextMeshProUGUI graffitiNewLabel;

		public Image graffitiTimerBar;

		public Image graffitiTimerBackdrop;

		public RawImage closeUp;

		internal Vector3 lastScoreStartPos = Vector3.zero;

		internal Player assignedPlayer;

		public Sprite chargeBoost;

		public Sprite chargeBoostBig;

		public Sprite musicCollectable;

		public Sprite graffitiCollectable;

		public AnimationCurve pickupScale;

		public AnimationCurve pickupBounce;

		public AnimationCurve pickupTransition;

		public AnimationCurve rushScale;

		public AnimationCurve rushBounce;

		public AnimationCurve rushTransition;

		public AnimationCurve collectableScale;

		public AnimationCurve collectableBounce;

		public AnimationCurve collectableTransition;

		public AnimationCurve fortuneScale;

		public AnimationCurve fortuneBounce;

		public AnimationCurve fortuneTransition;

		public RectTransform maxChargePosition;

		public RectTransform musicCollectablePickupLocation;

		public RectTransform healthBarGroup;

		public Image healthBarFill;

		public Image healthBarFillChunk;

		public Color healthBarDefaultColor;

		public const float healthBarFillMax = 248f;

		public const float inverseHealthBarFillMax = 0.004032258f;

		public RectTransform apb;

		public RectTransform wanted1;

		public RectTransform wanted2;

		public RectTransform wanted3;

		public RectTransform wanted4;

		public RectTransform wanted5;

		public RectTransform wanted6;

		public Image APBTimeOutBar;

		public RectTransform musicTrack;

		public TextMeshProUGUI musicTitleLabel;

		public TextMeshProUGUI musicArtistLabel;

		public GameObject contextLabelGameObject;

		public UIButtonGlyphComponent contextLabelUiButtonGlyphComponent;

		public void Awake()
		{
			StageManager.OnStageInitialized += OnStageInitialized;
		}

		public void Init()
		{
			base.gameObject.SetActive(value: true);
			gameplayScreen.gameObject.SetActive(value: false);
			graffitiScreen.gameObject.SetActive(value: false);
			apb.gameObject.SetActive(value: false);
			closeUp.gameObject.SetActive(value: true);
			closeUp.enabled = false;
			chargeBarDefaultColor = chargeBar.color;
			healthBarDefaultColor = healthBarFill.color;
			SetTrickingChargeBarActive(setActive: false);
			wanted1.gameObject.SetActive(value: false);
			wanted2.gameObject.SetActive(value: false);
			wanted3.gameObject.SetActive(value: false);
			wanted4.gameObject.SetActive(value: false);
			wanted5.gameObject.SetActive(value: false);
			wanted6.gameObject.SetActive(value: false);
			contextLabelGameObject = contextLabel.gameObject;
			contextLabelUiButtonGlyphComponent = contextLabelGameObject.GetComponent<UIButtonGlyphComponent>();
			HideContextLabelUI();
		}

		private void OnStageInitialized()
		{
			gameplayScreen.gameObject.SetActive(value: true);
		}

		public void TurnOn(bool set)
		{
			gameplayScreen.gameObject.SetActive(set);
			assignedPlayer.phone.dynamicGameplayScreen.gameObject.SetActive(set);
		}

		public void LateUpdate()
		{
			if (gameplayScreen.gameObject.activeSelf && (bool)assignedPlayer && (bool)WorldHandler.instance)
			{
				assignedPlayer.UpdateUIDisplay();
			}
		}

		private void OnDestroy()
		{
			StageManager.OnStageInitialized -= OnStageInitialized;
		}

		public void ShowContextLabelUI()
		{
			if (!contextLabelGameObject.activeSelf)
			{
				contextLabelGameObject.SetActive(value: true);
			}
		}

		public void HideContextLabelUI()
		{
			if (contextLabelGameObject.activeSelf)
			{
				contextLabelGameObject.SetActive(value: false);
			}
		}

		public void SetTrickingChargeBarActive(bool setActive)
		{
			if (setActive)
			{
				chargeBar.material.SetFloat("_PaletteIndex", chargeBarTrickingColorIndex);
			}
			else
			{
				chargeBar.material.SetFloat("_PaletteIndex", chargeBarDefaultColorIndex);
			}
		}
	}
}
