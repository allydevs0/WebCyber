using System;
using UnityEngine;
using UnityEngine.UI;

namespace Tayx.Graphy.CustomizationScene
{
	public class G_CUIColorPicker : MonoBehaviour
	{
		[SerializeField]
		private Slider alphaSlider;

		[SerializeField]
		private Image alphaSliderBGImage;

		private Color _color = new Color32(byte.MaxValue, 0, 0, 128);

		private Action<Color> _onValueChange;

		private Action _update;

		public Color Color
		{
			get
			{
				return _color;
			}
			set
			{
				Setup(value);
			}
		}

		public void SetOnValueChangeCallback(Action<Color> onValueChange)
		{
			_onValueChange = onValueChange;
		}

		private static void RGBToHSV(Color color, out float h, out float s, out float v)
		{
			float num = Mathf.Min(color.r, color.g, color.b);
			float num2 = Mathf.Max(color.r, color.g, color.b);
			float num3 = num2 - num;
			if (num3 == 0f)
			{
				h = 0f;
			}
			else if (num2 == color.r)
			{
				h = Mathf.Repeat((color.g - color.b) / num3, 6f);
			}
			else if (num2 == color.g)
			{
				h = (color.b - color.r) / num3 + 2f;
			}
			else
			{
				h = (color.r - color.g) / num3 + 4f;
			}
			s = ((num2 == 0f) ? 0f : (num3 / num2));
			v = num2;
		}

		private static bool GetLocalMouse(GameObject go, out Vector2 result)
		{
			RectTransform rectTransform = (RectTransform)go.transform;
			Vector3 point = rectTransform.InverseTransformPoint(Input.mousePosition);
			result.x = Mathf.Clamp(point.x, rectTransform.rect.min.x, rectTransform.rect.max.x);
			result.y = Mathf.Clamp(point.y, rectTransform.rect.min.y, rectTransform.rect.max.y);
			return rectTransform.rect.Contains(point);
		}

		private static Vector2 GetWidgetSize(GameObject go)
		{
			return ((RectTransform)go.transform).rect.size;
		}

		private GameObject GO(string name)
		{
			return base.transform.Find(name).gameObject;
		}

		private void Setup(Color inputColor)
		{
			alphaSlider.value = inputColor.a;
			alphaSliderBGImage.color = inputColor;
			GameObject satvalGO = GO("SaturationValue");
			GameObject satvalKnob = GO("SaturationValue/Knob");
			GameObject hueGO = GO("Hue");
			GameObject hueKnob = GO("Hue/Knob");
			GameObject result = GO("Result");
			Color[] hueColors = new Color[6]
			{
				Color.red,
				Color.yellow,
				Color.green,
				Color.cyan,
				Color.blue,
				Color.magenta
			};
			Color[] satvalColors = new Color[4]
			{
				new Color(0f, 0f, 0f),
				new Color(0f, 0f, 0f),
				new Color(1f, 1f, 1f),
				hueColors[0]
			};
			Texture2D texture2D = new Texture2D(1, 7);
			for (int i = 0; i < 7; i++)
			{
				texture2D.SetPixel(0, i, hueColors[i % 6]);
			}
			texture2D.Apply();
			hueGO.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0.5f, 1f, 6f), new Vector2(0.5f, 0.5f));
			Vector2 hueSz = GetWidgetSize(hueGO);
			Texture2D satvalTex = new Texture2D(2, 2);
			satvalGO.GetComponent<Image>().sprite = Sprite.Create(satvalTex, new Rect(0.5f, 0.5f, 1f, 1f), new Vector2(0.5f, 0.5f));
			Action resetSatValTexture = () =>
			{
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						satvalTex.SetPixel(k, j, satvalColors[k + j * 2]);
					}
				}
				satvalTex.Apply();
			};
			Vector2 satvalSz = GetWidgetSize(satvalGO);
			RGBToHSV(inputColor, out var Hue, out var Saturation, out var Value);
			Action applyHue = () =>
			{
				int num = Mathf.Clamp((int)Hue, 0, 5);
				int num2 = (num + 1) % 6;
				Color color = Color.Lerp(hueColors[num], hueColors[num2], Hue - (float)num);
				satvalColors[3] = color;
				resetSatValTexture();
			};
			Action applySaturationValue = () =>
			{
				Vector2 vector = new Vector2(Saturation, Value);
				Vector2 vector2 = new Vector2(1f - vector.x, 1f - vector.y);
				Color color = vector2.x * vector2.y * satvalColors[0];
				Color color2 = vector.x * vector2.y * satvalColors[1];
				Color color3 = vector2.x * vector.y * satvalColors[2];
				Color color4 = vector.x * vector.y * satvalColors[3];
				Color color5 = color + color2 + color3 + color4;
				result.GetComponent<Image>().color = color5;
				if (_color != color5)
				{
					color5 = new Color(color5.r, color5.g, color5.b, alphaSlider.value);
					if (_onValueChange != null)
					{
						_onValueChange(color5);
					}
					_color = color5;
					alphaSliderBGImage.color = _color;
				}
			};
			applyHue();
			applySaturationValue();
			satvalKnob.transform.localPosition = new Vector2(Saturation * satvalSz.x, Value * satvalSz.y);
			hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, Hue / 6f * satvalSz.y);
			Action dragH = null;
			Action dragSV = null;
			Action idle = () =>
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (GetLocalMouse(hueGO, out var result2))
					{
						_update = dragH;
					}
					else if (GetLocalMouse(satvalGO, out result2))
					{
						_update = dragSV;
					}
				}
			};
			dragH = () =>
			{
				GetLocalMouse(hueGO, out var result2);
				Hue = result2.y / hueSz.y * 6f;
				applyHue();
				applySaturationValue();
				hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, result2.y);
				if (Input.GetMouseButtonUp(0))
				{
					_update = idle;
				}
			};
			dragSV = () =>
			{
				GetLocalMouse(satvalGO, out var result2);
				Saturation = result2.x / satvalSz.x;
				Value = result2.y / satvalSz.y;
				applySaturationValue();
				satvalKnob.transform.localPosition = result2;
				if (Input.GetMouseButtonUp(0))
				{
					_update = idle;
				}
			};
			_update = idle;
		}

		public void SetRandomColor()
		{
			System.Random random = new System.Random();
			float r = (float)(random.Next() % 1000) / 1000f;
			float g = (float)(random.Next() % 1000) / 1000f;
			float b = (float)(random.Next() % 1000) / 1000f;
			Color = new Color(r, g, b);
		}

		private void Awake()
		{
			Color = new Color32(byte.MaxValue, 0, 0, 128);
		}

		private void Start()
		{
			alphaSlider.onValueChanged.AddListener((float value) =>
			{
				_color = new Color(_color.r, _color.g, _color.b, value);
				alphaSliderBGImage.color = _color;
				if (_onValueChange != null)
				{
					_onValueChange(_color);
				}
			});
		}

		private void Update()
		{
			if (_update != null)
			{
				_update();
			}
		}
	}
}
