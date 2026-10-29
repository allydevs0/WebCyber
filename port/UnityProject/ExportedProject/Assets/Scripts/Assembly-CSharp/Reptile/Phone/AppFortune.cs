using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile.Phone
{
	public class AppFortune : App
	{
		private delegate void buttonAction();

		private enum FortuneState
		{
			Initial = 0,
			Showing = 1,
			Blocked = 2
		}

		[SerializeField]
		private GameObject initialPanel;

		[SerializeField]
		private GameObject blockedPanel;

		[SerializeField]
		private GameObject bottomAskPrompt;

		[SerializeField]
		private Color textSelectedColor = Color.blue;

		[SerializeField]
		private TextMeshProUGUI[] yesNoButton = new TextMeshProUGUI[2];

		[SerializeField]
		private Image[] yesNoImage = new Image[2];

		[SerializeField]
		private TextMeshProUGUI m_FortuneLabel;

		[SerializeField]
		private RectTransform fortuneRectTransform;

		[SerializeField]
		private RectTransform eightBallRectTransform;

		[SerializeField]
		private RectTransform eyeRectTransform;

		[SerializeField]
		private Image irisImage;

		[SerializeField]
		private Sprite eightSprite;

		[SerializeField]
		private Sprite[] shapeSprites;

		[SerializeField]
		private float eightBallYPositionFortuneReveal;

		[SerializeField]
		private float eightBallYPositionOutOfScreenBottom = -1200f;

		[SerializeField]
		private float eyeYPositionFortuneReveal = -10f;

		[SerializeField]
		private Transform fortunePickupPosition;

		[SerializeField]
		private FortuneRare[] rareFortunes;

		[SerializeField]
		private TextMeshProUGUI zzzText;

		[SerializeField]
		private float zzzMaxScale = 5f;

		[SerializeField]
		private float zzzScaleDelta = 0.25f;

		[SerializeField]
		private float zzzOffsetX = -200f;

		[SerializeField]
		private float zzzOffsetY = 50f;

		private RectTransform rectTransform;

		private Sequence openAppSequence;

		private Sequence closeAppSequence;

		private int selectedButton;

		private FortuneState state;

		private Player currentPlayer;

		private float boostAmount;

		private int boostPickups;

		private Sequence revealFortune;

		private Tween eightBallTween;

		private float zzzScale;

		private Image selectedButtonImage;

		private Vector3 originalPositionEightBall;

		private Vector3 originalPositionEye;

		protected override void OnAppInit()
		{
			rectTransform = GetComponent<RectTransform>();
			m_FortuneLabel.rectTransform.localScale = Vector3.zero;
			originalPositionEightBall = eightBallRectTransform.localPosition;
			originalPositionEye = eyeRectTransform.localPosition;
			ResetToInitial();
			InitRevealSequence();
			openAppSequence = DOTween.Sequence();
			openAppSequence.SetAutoKill(autoKillOnCompletion: false);
			openAppSequence.Pause();
			openAppSequence.AppendCallback(() =>
			{
				selectedButton = 0;
				state = FortuneState.Initial;
				SelectButton(yesNoImage[0], yesNoButton[0]);
				DeselectButton(yesNoImage[1], yesNoButton[1]);
				if (Core.Instance.SaveManager.CurrentSaveSlot.fortuneLastCrack.Date.Equals(DateTime.Now.Date))
				{
					state = FortuneState.Blocked;
					initialPanel.SetActive(value: false);
					blockedPanel.SetActive(value: true);
					bottomAskPrompt.SetActive(value: false);
					StartCoroutine(AnimateZZZText());
				}
			});
			openAppSequence.Append(base.transform.DOScaleY(1f, 0.15f));
			closeAppSequence = DOTween.Sequence();
			closeAppSequence.SetAutoKill(autoKillOnCompletion: false);
			closeAppSequence.Pause();
			closeAppSequence.Append(base.transform.DOScaleY(0f, 0.15f));
			closeAppSequence.AppendCallback(() =>
			{
				base.MyPhone.CloseCurrentApp();
			});
		}

		protected override void OnAppTerminate()
		{
			if (eightBallTween != null)
			{
				eightBallTween.Kill();
			}
			revealFortune.Kill();
			openAppSequence.Kill();
			closeAppSequence.Kill();
		}

		private void ResetToInitial()
		{
			initialPanel.SetActive(value: true);
			blockedPanel.SetActive(value: false);
			bottomAskPrompt.SetActive(value: true);
			irisImage.sprite = eightSprite;
			base.transform.localScale = new Vector3(1f, 0f, 1f);
			fortuneRectTransform.localScale = new Vector3(0f, 0f, 0f);
			eightBallRectTransform.localPosition = originalPositionEightBall;
			eyeRectTransform.localPosition = originalPositionEye;
		}

		public override void OnAppEnable()
		{
			ResetToInitial();
			openAppSequence.Restart();
		}

		public override void OnAppDisable()
		{
			StopAllCoroutines();
			if (eightBallTween != null)
			{
				eightBallTween.Kill();
			}
			if (revealFortune.IsPlaying())
			{
				revealFortune.Pause();
			}
		}

		public override void OnPressUp()
		{
			if (state == FortuneState.Initial)
			{
				ButtonUpDown(-1);
			}
		}

		public override void OnPressDown()
		{
			if (state == FortuneState.Initial)
			{
				ButtonUpDown(1);
			}
		}

		public override void OnPressRight()
		{
			if (state == FortuneState.Initial)
			{
				ActivateButton();
			}
		}

		public override void OnReleaseLeft()
		{
			closeAppSequence.Restart();
		}

		private void ButtonUpDown(int direction = 0)
		{
			if ((selectedButton != 0 || direction != -1) && (selectedButton != 1 || direction != 1))
			{
				DeselectButton(yesNoImage[selectedButton], yesNoButton[selectedButton]);
				selectedButton += direction;
				SelectButton(yesNoImage[selectedButton], yesNoButton[selectedButton]);
			}
		}

		private void ActivateButton()
		{
			Sequence s = DOTween.Sequence();
			selectedButtonImage = yesNoImage[selectedButton];
			s.AppendCallback(() =>
			{
				selectedButtonImage.enabled = false;
			});
			s.AppendInterval(0.1f);
			s.AppendCallback(() =>
			{
				selectedButtonImage.enabled = true;
			});
			s.AppendInterval(0.1f);
			s.AppendCallback(() =>
			{
				selectedButtonImage.enabled = false;
			});
			s.AppendInterval(0.1f);
			s.AppendCallback(() =>
			{
				selectedButtonImage.enabled = true;
			});
			s.AppendCallback(() =>
			{
				if (selectedButton == 0)
				{
					ShowFortune();
				}
				else
				{
					closeAppSequence.Restart();
				}
			});
		}

		private void SelectButton(Image buttonImage, TextMeshProUGUI text)
		{
			buttonImage.enabled = true;
			text.color = textSelectedColor;
			if (text.text[0] != '>')
			{
				text.text = ">" + text.text;
			}
		}

		private void DeselectButton(Image buttonImage, TextMeshProUGUI text)
		{
			buttonImage.enabled = false;
			text.color = Color.white;
			if (text.text[0] == '>')
			{
				text.text = text.text.TrimStart('>');
			}
		}

		public void ShowFortune()
		{
			DateTime now = DateTime.Now;
			if (!Core.Instance.SaveManager.CurrentSaveSlot.fortuneLastCrack.Date.Equals(now.Date))
			{
				bool num = UnityEngine.Random.Range(0, 5) == 0;
				int num2 = 0;
				FortuneCookie fortuneCookie = null;
				_ = Core.Instance.SaveManager.CurrentSaveSlot;
				if (num)
				{
					num2 = UnityEngine.Random.Range(0, rareFortunes.Length);
					fortuneCookie = rareFortunes[num2];
					UnlockableSaveData unlockableSaveDataFor = Core.Instance.Platform.User.GetUnlockableSaveDataFor(rareFortunes[num2]);
					_ = unlockableSaveDataFor.IsUnlocked;
					unlockableSaveDataFor.IsUnlocked = true;
				}
				else
				{
					num2 = UnityEngine.Random.Range(0, m_Unlockables.Length);
					fortuneCookie = (FortuneCookie)m_Unlockables[num2];
					UnlockableSaveData unlockableSaveDataFor2 = Core.Instance.Platform.User.GetUnlockableSaveDataFor(m_Unlockables[num2]);
					_ = unlockableSaveDataFor2.IsUnlocked;
					unlockableSaveDataFor2.IsUnlocked = true;
				}
				StartCoroutine(RevealFortune(fortuneCookie));
			}
		}

		private IEnumerator RevealFortune(FortuneCookie pickedFortune)
		{
			m_FortuneLabel.text = pickedFortune.advice;
			m_FortuneLabel.rectTransform.DOScale(Vector3.one, 0.5f);
			currentPlayer = WorldHandler.instance.GetCurrentPlayer();
			boostAmount = currentPlayer.maxBoostCharge * pickedFortune.boostPercentage;
			boostPickups = (int)(boostAmount / Pickup.boostChargeValue + 0.5f);
			eightBallTween = eightBallRectTransform.DOMoveY(eightBallYPositionFortuneReveal, 2f);
			bottomAskPrompt.SetActive(value: false);
			float shapeInterval = 0.4f;
			int i = 0;
			while (i < 18)
			{
				int num = UnityEngine.Random.Range(0, shapeSprites.Length);
				Sprite sprite = shapeSprites[num];
				irisImage.sprite = sprite;
				yield return new WaitForSeconds(shapeInterval);
				shapeInterval *= 0.5f;
				int num2 = i + 1;
				i = num2;
			}
			irisImage.sprite = shapeSprites[2];
			yield return eightBallTween.WaitForCompletion();
			revealFortune.Restart();
		}

		private void InitRevealSequence()
		{
			revealFortune = DOTween.Sequence();
			revealFortune.SetAutoKill(autoKillOnCompletion: false);
			revealFortune.Pause();
			revealFortune.Join(eightBallRectTransform.DOMoveY(eightBallYPositionOutOfScreenBottom, 1f));
			revealFortune.Join(eyeRectTransform.DOAnchorPosY(eyeYPositionFortuneReveal, 1f));
			revealFortune.Join(fortuneRectTransform.DOScale(1f, 1f));
			revealFortune.AppendCallback(() =>
			{
				currentPlayer.AddBoostCharge(boostAmount);
				currentPlayer.StartCoroutine(ShowPickups(currentPlayer, boostPickups));
			});
		}

		private IEnumerator ShowPickups(Player currentPlayer, int boostPickups)
		{
			WaitForSeconds waitSeconds = new WaitForSeconds(0.01f);
			int i = 0;
			while (i < boostPickups)
			{
				currentPlayer.PickupVisualization(fortunePickupPosition.position, Pickup.PickUpType.FORTUNE_BOOST, worldPos: false);
				yield return waitSeconds;
				int num = i + 1;
				i = num;
			}
			yield return null;
		}

		private IEnumerator AnimateZZZText()
		{
			zzzText.text = "ZZZ";
			zzzText.ForceMeshUpdate();
			TMP_TextInfo textInfo = zzzText.textInfo;
			Vector3[][] copyOfVertices = new Vector3[0][];
			if (copyOfVertices.Length < textInfo.meshInfo.Length)
			{
				copyOfVertices = new Vector3[textInfo.meshInfo.Length][];
			}
			for (int i = 0; i < textInfo.meshInfo.Length; i++)
			{
				int num = textInfo.meshInfo[i].vertices.Length;
				copyOfVertices[i] = new Vector3[num];
			}
			float[] alphas = new float[3] { 0.99f, 0.66f, 0.33f };
			while (true)
			{
				_ = textInfo.characterCount;
				int firstCharacterIndex = textInfo.lineInfo[0].firstCharacterIndex;
				int lastCharacterIndex = textInfo.lineInfo[0].lastCharacterIndex;
				_ = (textInfo.characterInfo[firstCharacterIndex].bottomLeft + textInfo.characterInfo[lastCharacterIndex].topRight) * 0.5f;
				Vector3[] array = new Vector3[3]
				{
					textInfo.characterInfo[lastCharacterIndex].bottomLeft - textInfo.characterInfo[firstCharacterIndex].bottomLeft,
					textInfo.characterInfo[lastCharacterIndex].bottomLeft - textInfo.characterInfo[lastCharacterIndex - firstCharacterIndex >> 1].bottomLeft,
					textInfo.characterInfo[lastCharacterIndex].bottomLeft - textInfo.characterInfo[lastCharacterIndex].bottomLeft
				};
				for (int num2 = lastCharacterIndex; num2 >= firstCharacterIndex; num2--)
				{
					if (textInfo.characterInfo[num2].isVisible)
					{
						int materialReferenceIndex = textInfo.characterInfo[num2].materialReferenceIndex;
						int vertexIndex = textInfo.characterInfo[num2].vertexIndex;
						Color32[] colors = textInfo.meshInfo[materialReferenceIndex].colors32;
						Vector3[] vertices = textInfo.meshInfo[materialReferenceIndex].vertices;
						Vector3 vector = (Vector2)((vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2f);
						copyOfVertices[materialReferenceIndex][vertexIndex] = vertices[vertexIndex] - vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 1] = vertices[vertexIndex + 1] - vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 2] = vertices[vertexIndex + 2] - vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 3] = vertices[vertexIndex + 3] - vector;
						zzzScale += zzzScaleDelta * Core.dt;
						float num3 = (zzzScale + alphas[num2] * zzzMaxScale) % zzzMaxScale;
						float t = (zzzScale / zzzMaxScale + alphas[num2]) % 1f;
						float num4 = Mathf.Lerp(0f, zzzOffsetX, t);
						float num5 = Mathf.Lerp(0f, zzzOffsetY, t);
						byte a = (byte)(int)Mathf.Lerp(255f, 0f, t);
						Matrix4x4 matrix4x = Matrix4x4.TRS(new Vector3(array[num2].x + num4, array[num2].y + num5, 0f), Quaternion.identity, Vector3.one * num3);
						copyOfVertices[materialReferenceIndex][vertexIndex] = matrix4x.MultiplyPoint3x4(copyOfVertices[materialReferenceIndex][vertexIndex]);
						copyOfVertices[materialReferenceIndex][vertexIndex + 1] = matrix4x.MultiplyPoint3x4(copyOfVertices[materialReferenceIndex][vertexIndex + 1]);
						copyOfVertices[materialReferenceIndex][vertexIndex + 2] = matrix4x.MultiplyPoint3x4(copyOfVertices[materialReferenceIndex][vertexIndex + 2]);
						copyOfVertices[materialReferenceIndex][vertexIndex + 3] = matrix4x.MultiplyPoint3x4(copyOfVertices[materialReferenceIndex][vertexIndex + 3]);
						copyOfVertices[materialReferenceIndex][vertexIndex] += vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 1] += vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 2] += vector;
						copyOfVertices[materialReferenceIndex][vertexIndex + 3] += vector;
						colors[vertexIndex] = new Color32(colors[vertexIndex].r, colors[vertexIndex].g, colors[vertexIndex].b, a);
						colors[vertexIndex + 1] = new Color32(colors[vertexIndex + 1].r, colors[vertexIndex + 1].g, colors[vertexIndex + 1].b, a);
						colors[vertexIndex + 2] = new Color32(colors[vertexIndex + 2].r, colors[vertexIndex + 2].g, colors[vertexIndex + 2].b, a);
						colors[vertexIndex + 3] = new Color32(colors[vertexIndex + 3].r, colors[vertexIndex + 3].g, colors[vertexIndex + 3].b, a);
					}
				}
				for (int j = 0; j < textInfo.meshInfo.Length; j++)
				{
					textInfo.meshInfo[j].mesh.vertices = copyOfVertices[j];
					zzzText.UpdateGeometry(textInfo.meshInfo[j].mesh, j);
					zzzText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
				}
				yield return null;
			}
		}
	}
}
