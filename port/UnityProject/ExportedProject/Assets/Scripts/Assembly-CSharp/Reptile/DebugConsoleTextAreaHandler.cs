using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Reptile
{
	public class DebugConsoleTextAreaHandler : MonoBehaviour
	{
		private const string RICH_TEXT_COLOR_PREFIX_START = "<color=";

		private const string RICH_TEXT_PREFIX_END = ">";

		private const string RICH_TEXT_COLOR_AFFIX = "</color>";

		private const string COLOR_RED = "red";

		private const string COLOR_YELLOW = "yellow";

		private const string COLOR_WHITE = "white";

		private const int AMOUNT_OF_TABS_ON_STACKTRACE = 1;

		public Canvas rootCanvas;

		public bool useColors = true;

		public bool showTimeStamp = true;

		private Logger logger;

		private GameInput gameInput;

		private Text textComponent;

		private RectTransform myRectTransform;

		private int startFromEntryIndex;

		private int maxStartFromEntryIndex;

		private int maxLineCount;

		private float currentParentRectHeight;

		private bool autoScroll = true;

		private bool isActive;

		public void Awake()
		{
			logger = Core.Instance.Logger;
			gameInput = Core.Instance.GameInput;
			textComponent = GetComponent<Text>();
			myRectTransform = GetComponent<RectTransform>();
			currentParentRectHeight = GetParentRectTransformScaledHeight();
			CalculateLineCount();
			DetermineNewMaxStartIndex();
			UpdateConsoleLogTextArea();
			if (!textComponent.supportRichText)
			{
				useColors = false;
			}
		}

		public void OnDestroy()
		{
		}

		public void OnEnable()
		{
			isActive = true;
			DetermineNewMaxStartIndex();
			UpdateConsoleLogTextArea();
		}

		public void OnDisable()
		{
			isActive = false;
		}

		private void OnLogEntriesAdded()
		{
			if (isActive && autoScroll)
			{
				ScrollDown(1);
			}
		}

		private void DetermineNewMaxStartIndex()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = logger.CurrentLogSize; num >= 0; num--)
			{
				LogEntry logEntry = null;
				if (logger.GetFromTextBuffer(num, out logEntry))
				{
					ProcessLogEntry(logEntry, stringBuilder);
					maxStartFromEntryIndex = num;
					if (HasUsedMaxLineCount(stringBuilder))
					{
						break;
					}
				}
			}
		}

		public void Update()
		{
		}

		private void UpdateInput()
		{
			UpdateAxis();
			UpdateButtons();
		}

		private void UpdateAxis()
		{
			UpdateUnityInputAxis();
			UpdateRewiredInputAxis();
		}

		private void UpdateUnityInputAxis()
		{
			float axis = Input.GetAxis("Vertical");
			UpdateAxisInput(axis);
		}

		private void UpdateRewiredInputAxis()
		{
			float axis = gameInput.GetAxis(42);
			UpdateAxisInput(axis);
		}

		private void UpdateAxisInput(float axisMovement)
		{
			if (axisMovement > 0f)
			{
				ScrollUp(1);
			}
			else if (axisMovement < 0f)
			{
				ScrollDown(1);
			}
		}

		private void UpdateButtons()
		{
			UpdateUnityInputButtons();
			UpdateRewiredInputButtons();
		}

		private void UpdateUnityInputButtons()
		{
			if (Input.GetButtonDown("DebugMenuScrollToTop") || Input.GetKeyDown(KeyCode.F1))
			{
				MoveToStartOfLog();
			}
			if (Input.GetButtonDown("DebugMenuScrollToBottom") || Input.GetKeyDown(KeyCode.F2))
			{
				MoveToEndOfLog();
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				ScrollDown(1);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				ScrollUp(1);
			}
		}

		private void UpdateRewiredInputButtons()
		{
			if (gameInput.GetButtonNew(44))
			{
				MoveToStartOfLog();
			}
			if (gameInput.GetButtonNew(43))
			{
				MoveToEndOfLog();
			}
		}

		private void CalculateLineCount()
		{
			float parentRectTransformScaledHeight = GetParentRectTransformScaledHeight();
			float num = DetermineLineHeightFromTextArea(myRectTransform.rect);
			if (num > 0f)
			{
				maxLineCount = (int)Mathf.Round(parentRectTransformScaledHeight / num);
			}
		}

		private float GetParentRectTransformScaledHeight()
		{
			return myRectTransform.rect.height * rootCanvas.scaleFactor;
		}

		private float DetermineLineHeightFromTextArea(Rect textAreaRect)
		{
			Vector2 extents = textAreaRect.size * 0.5f;
			return textComponent.cachedTextGeneratorForLayout.GetPreferredHeight("A", textComponent.GetGenerationSettings(extents));
		}

		private void UpdateConsoleLogTextArea()
		{
		}

		private void ProcessLogEntriesFromStartIndex(int startIndex, StringBuilder stringBuilder)
		{
			for (int i = 0; i < logger.CurrentLogSize; i++)
			{
				LogEntry logEntry = null;
				if (logger.GetFromTextBuffer(startIndex + i, out logEntry))
				{
					ProcessLogEntry(logEntry, stringBuilder);
					if (HasUsedMaxLineCount(stringBuilder))
					{
						break;
					}
				}
			}
		}

		private void ProcessLogEntry(LogEntry logEntry, StringBuilder stringBuilder)
		{
			stringBuilder.Append(ProcessLogEntryText(logEntry));
		}

		private string ProcessLogEntryText(LogEntry logEntry)
		{
			string text = logEntry.Text;
			if (logEntry.LogType == LogType.Exception || logEntry.LogType == LogType.Error)
			{
				text = AppendStackTrace(text, logEntry.StackTrace);
			}
			if (showTimeStamp)
			{
				text = AppendTimeStampToText(text, logEntry.TimeStamp);
			}
			if (useColors)
			{
				text = AddColorMarkupToText(text, logEntry.LogType);
			}
			return AppendNewLineToText(text);
		}

		private string AppendStackTrace(string text, string stackTrace)
		{
			return text + "\n\t" + AlignStackTrace(stackTrace, 1);
		}

		private string AlignStackTrace(string stackTraceText, int nTabs)
		{
			string text = string.Empty;
			for (int i = 0; i < nTabs; i++)
			{
				text += "\t";
			}
			return stackTraceText.Replace("\n", "\n" + text);
		}

		private string AppendTimeStampToText(string text, TimeSpan timeStamp)
		{
			return "[" + timeStamp.ToString("hh\\:mm\\:ss\\.ffff") + "]: " + text;
		}

		private string AddColorMarkupToText(string text, LogType logType)
		{
			string text2 = DetermineColorForLogEntry(logType);
			StringBuilder stringBuilder = new StringBuilder(text);
			stringBuilder.Insert(0, "<color=" + text2 + ">");
			stringBuilder.Append("</color>");
			return stringBuilder.ToString();
		}

		private string DetermineColorForLogEntry(LogType logType)
		{
			switch (logType)
			{
			default:
				return "white";
			case LogType.Warning:
				return "yellow";
			case LogType.Error:
			case LogType.Exception:
				return "red";
			}
		}

		private string AppendNewLineToText(string text)
		{
			return text + Environment.NewLine;
		}

		private bool HasUsedMaxLineCount(StringBuilder stringBuilder)
		{
			return true;
		}

		public void MoveToEndOfLog()
		{
		}

		public void MoveToStartOfLog()
		{
		}

		public void ScrollUp(int scrollSpeed)
		{
		}

		public void ScrollDown(int scrollSpeed)
		{
		}
	}
}
