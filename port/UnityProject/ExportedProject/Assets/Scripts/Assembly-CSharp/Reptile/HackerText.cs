using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Reptile
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class HackerText : MonoBehaviour
	{
		private Coroutine generateAnimationCoroutine;

		private string m_Text;

		private bool m_Idle;

		[Tooltip("Caret added at the end, leave empty to remove it")]
		[SerializeField]
		private char m_CaretCharacter = '_';

		private const string DEFAULT_CHAR_TABLE = "abcdefghijklmnopqurstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-=_+[]{}|;:,./<>?;'";

		public string Text
		{
			get
			{
				return m_Text;
			}
			set
			{
				SetText(value);
			}
		}

		public TextMeshProUGUI TextMesh { get; private set; }

		private void Awake()
		{
			TextMesh = GetComponent<TextMeshProUGUI>();
			SetText(TextMesh.text);
		}

		public void SetText(string newText)
		{
			m_Text = newText;
		}

		public void PlayGenerateAnimation(float typeDuration, float decodeDuration, string charTable = "abcdefghijklmnopqurstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-=_+[]{}|;:,./<>?;'")
		{
			StopAllCoroutines();
			generateAnimationCoroutine = StartCoroutine(GenerateAnimation(m_Text, typeDuration, decodeDuration, charTable));
		}

		private void OnDisable()
		{
			if (generateAnimationCoroutine != null)
			{
				StopCoroutine(generateAnimationCoroutine);
				TextMesh.text = m_Text;
			}
		}

		private IEnumerator GenerateAnimation(string target, float typeDuration, float decodeDuration, string charTable)
		{
			m_Idle = false;
			string output = string.Empty;
			float startTypeTime = Time.time;
			while (output != target)
			{
				float time = Time.time;
				float f = Mathf.Clamp01((time - startTypeTime) / typeDuration) * (float)target.Length;
				while ((float)output.Length < Mathf.Floor(f))
				{
					output += charTable[Random.Range(0, charTable.Length)];
				}
				StringBuilder stringBuilder = new StringBuilder(output);
				float num = (time - startTypeTime - decodeDuration) / typeDuration;
				for (int i = 0; i < output.Length; i++)
				{
					float num2 = (float)i / ((float)target.Length - 1f);
					if (num >= num2)
					{
						stringBuilder[i] = target[i];
					}
					else if (num < num2)
					{
						stringBuilder[i] = charTable[Random.Range(0, charTable.Length)];
					}
				}
				output = stringBuilder.ToString();
				TextMesh.text = output + m_CaretCharacter;
				yield return new WaitForEndOfFrame();
			}
			m_Idle = true;
		}
	}
}
