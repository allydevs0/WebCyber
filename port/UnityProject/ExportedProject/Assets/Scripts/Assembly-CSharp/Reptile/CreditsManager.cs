using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Reptile
{
	public class CreditsManager : MonoBehaviour
	{
		private enum PositionTag
		{
			CENTER = 0,
			LEFT_TOP = 1,
			LEFT_CENTER = 2,
			LEFT_BOTTOM = 3,
			RIGHT_TOP = 4,
			RIGHT_CENTER = 5,
			RIGHT_BOTTOM = 6
		}

		public VideoPlayer videoPlayer;

		public TextMeshProUGUI textBox;

		public Image logoImage;

		public Color32 titlesColour;

		internal string titlesColourText;

		public float startingDelay;

		public float timeBetweenCredits;

		public Vector2 insideOffset = new Vector2(20f, 10f);

		public Sprite[] spritesToUse;

		private int creditsCount;

		private int creditsIndex = 1;

		private bool videoEnded;

		[Tooltip("Order should be that of PositionTag enum! Starting with CENTER rect.")]
		public RectTransform[] availablePositions;

		private void Start()
		{
			creditsCount = (Core.Instance.Localizer as TextMeshProGameTextLocalizer).GetCreditsGroupLength();
			titlesColourText = ConvertToColourCode(titlesColour);
			textBox.gameObject.SetActive(value: false);
			logoImage.gameObject.SetActive(value: false);
			Core.OnCoreUpdatePaused += PauseVideo;
			Core.OnCoreUpdateUnPaused += UnpauseVideo;
			Core.Instance.UnPauseCore(PauseType.ForceOff);
			StartCoroutine(CreditsCycle());
		}

		internal static string ConvertToColourCode(Color32 color)
		{
			return $"#{color.r:X}{color.g:X}{color.b:X}{color.a:X}";
		}

		private void OnDestroy()
		{
			Core.OnCoreUpdatePaused -= PauseVideo;
			Core.OnCoreUpdateUnPaused -= UnpauseVideo;
		}

		private void OnApplicationFocus(bool focus)
		{
			if (focus)
			{
				UnpauseVideo();
			}
			else
			{
				PauseVideo();
			}
		}

		internal void PauseVideo()
		{
			videoPlayer.Pause();
		}

		internal void UnpauseVideo()
		{
			videoPlayer.Play();
		}

		private void ShowNextText(string textToPrint, PositionTag positionTag)
		{
			textBox.gameObject.SetActive(value: true);
			logoImage.gameObject.SetActive(value: false);
			textBox.text = textToPrint;
			TextAlignmentOptions alignment = TextAlignmentOptions.BottomLeft;
			Vector4 zero = Vector4.zero;
			float num = textBox.canvas.RectTransform().sizeDelta.x / 4f;
			switch (positionTag)
			{
			case PositionTag.CENTER:
				alignment = TextAlignmentOptions.Center;
				zero.x = num / 2f;
				zero.z = num / 2f;
				break;
			case PositionTag.LEFT_TOP:
				alignment = TextAlignmentOptions.TopLeft;
				zero.x = 0f;
				zero.z = num * 2f;
				break;
			case PositionTag.LEFT_CENTER:
				alignment = TextAlignmentOptions.BaselineLeft;
				zero.x = 0f;
				zero.z = num * 2f;
				break;
			case PositionTag.LEFT_BOTTOM:
				alignment = TextAlignmentOptions.BottomLeft;
				zero.x = 0f;
				zero.z = num * 2f;
				break;
			case PositionTag.RIGHT_TOP:
				alignment = TextAlignmentOptions.TopRight;
				zero.x = num * 2f;
				zero.z = 0f;
				break;
			case PositionTag.RIGHT_CENTER:
				alignment = TextAlignmentOptions.BaselineRight;
				zero.x = num * 2f;
				zero.z = 0f;
				break;
			case PositionTag.RIGHT_BOTTOM:
				alignment = TextAlignmentOptions.BottomRight;
				zero.x = num * 2f;
				zero.z = 0f;
				break;
			}
			textBox.alignment = alignment;
			textBox.margin = zero;
		}

		private void ShowNextImage(string imageIdentifierText, PositionTag positionTag)
		{
			int result = 0;
			if (!int.TryParse(imageIdentifierText[imageIdentifierText.Length - 1].ToString(), out result) || result >= spritesToUse.Length)
			{
				textBox.gameObject.SetActive(value: false);
				logoImage.gameObject.SetActive(value: false);
				return;
			}
			switch (positionTag)
			{
			case PositionTag.CENTER:
				logoImage.rectTransform.localPosition = new Vector2(0f, 0f);
				break;
			case PositionTag.LEFT_TOP:
				logoImage.rectTransform.localPosition = new Vector2(0f - insideOffset.x, insideOffset.y + logoImage.rectTransform.sizeDelta.y * 0.5f);
				break;
			case PositionTag.LEFT_CENTER:
				logoImage.rectTransform.localPosition = new Vector2(0f - insideOffset.x, 0f - insideOffset.y);
				break;
			case PositionTag.LEFT_BOTTOM:
				logoImage.rectTransform.localPosition = new Vector2(0f - insideOffset.x, 0f - insideOffset.y - logoImage.rectTransform.sizeDelta.y * 0.5f);
				break;
			case PositionTag.RIGHT_TOP:
				logoImage.rectTransform.localPosition = new Vector2(insideOffset.x, insideOffset.y + logoImage.rectTransform.sizeDelta.y * 0.5f);
				break;
			case PositionTag.RIGHT_CENTER:
				logoImage.rectTransform.localPosition = new Vector2(insideOffset.x, insideOffset.y);
				break;
			case PositionTag.RIGHT_BOTTOM:
				logoImage.rectTransform.localPosition = new Vector2(insideOffset.x, 0f - insideOffset.y - logoImage.rectTransform.sizeDelta.y * 0.5f);
				break;
			}
			textBox.gameObject.SetActive(value: false);
			logoImage.gameObject.SetActive(value: true);
			logoImage.sprite = spritesToUse[result];
		}

		private bool ShowNextCredit()
		{
			PositionTag result = PositionTag.CENTER;
			bool flag = false;
			bool flag2 = false;
			string text = string.Empty;
			do
			{
				string localizationKey = creditsIndex.ToString();
				string creditsText = Core.Instance.Localizer.GetCreditsText(localizationKey, 0);
				string creditsText2 = Core.Instance.Localizer.GetCreditsText(localizationKey, 2);
				string creditsText3 = Core.Instance.Localizer.GetCreditsText(localizationKey, 3);
				if (!string.IsNullOrWhiteSpace(creditsText2) && !Enum.TryParse<PositionTag>(creditsText2, out result))
				{
					result = PositionTag.CENTER;
				}
				flag = !string.IsNullOrWhiteSpace(creditsText3);
				if (creditsText[0] == '_')
				{
					ShowNextImage(creditsText, result);
				}
				else
				{
					flag2 = true;
					string creditsText4 = Core.Instance.Localizer.GetCreditsText(localizationKey, 1);
					text = text + "<color=" + titlesColourText + ">" + creditsText + "</color><br>" + creditsText4;
					if (creditsText[0] == '*')
					{
						text = creditsText4 ?? "";
					}
					if (flag)
					{
						text += "<br><br>";
					}
				}
				if (flag)
				{
					creditsIndex++;
					if (creditsIndex > creditsCount)
					{
						break;
					}
				}
			}
			while (flag);
			if (flag2)
			{
				ShowNextText(text, result);
			}
			creditsIndex++;
			return creditsIndex <= creditsCount;
		}

		private void VideoEnded(VideoPlayer videoPlayer)
		{
			videoEnded = true;
			videoPlayer.Stop();
		}

		private void ExitCreditsScene()
		{
			Core.Instance.BaseModule.ShowLoadingScreen();
			Core.Instance.BaseModule.UnloadCreditsScene();
			Core.Instance.BaseModule.LoadStage(Stage.hideout);
		}

		internal IEnumerator CreditsCycle()
		{
			if (!videoPlayer.isPlaying)
			{
				yield return new WaitUntil(() => videoPlayer.isPlaying);
			}
			yield return WaitForSecondsVideoSynced(startingDelay);
			_ = startingDelay;
			_ = startingDelay;
			videoEnded = false;
			videoPlayer.loopPointReached += VideoEnded;
			while (ShowNextCredit())
			{
				yield return WaitForSecondsVideoSynced(timeBetweenCredits);
			}
			yield return WaitForSecondsVideoSynced(timeBetweenCredits);
			textBox.gameObject.SetActive(value: false);
			yield return new WaitUntil(() => videoEnded);
			videoPlayer.loopPointReached -= VideoEnded;
			ExitCreditsScene();
		}

		internal IEnumerator WaitForSecondsVideoSynced(float seconds)
		{
			double endTime = videoPlayer.time + (double)seconds;
			double currentTime = videoPlayer.time;
			long endFrame = videoPlayer.frame + Mathf.RoundToInt(seconds * videoPlayer.frameRate);
			float frameRate = videoPlayer.frameRate;
			endFrame = (long)((float)Mathf.CeilToInt((float)endFrame / frameRate) * frameRate);
			while (videoPlayer.frame < endFrame)
			{
				yield return null;
				if (videoEnded)
				{
					double num = currentTime - (endTime - (double)seconds);
					endTime = (double)(Time.time + seconds) - num;
					while (currentTime < endTime)
					{
						yield return null;
						currentTime = Time.time;
					}
					break;
				}
				currentTime = videoPlayer.time;
			}
		}
	}
}
