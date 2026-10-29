using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class Mapcontroller : MonoBehaviour
	{
		public enum CameraModes
		{
			Manual = 0,
			Widget = 1,
			FollowPlayerFixedRotation = 2,
			FollowPlayerFreeRotation = 3,
			FollowPlayerPauseScreen = 4
		}

		public static Mapcontroller Instance;

		private float cameraZoom = 2f;

		private int activeStoryPins;

		private Transform cameraTransform;

		private WaitForEndOfFrame waitForEndOfFrame;

		private StoryManager storyManager;

		private List<MapPin> m_MapPins;

		private MapPin[] storyObjectivePins = new MapPin[20];

		private Player m_Player;

		private Coroutine mapUpdateCoroutine;

		private Dictionary<MapPin.PinType, GameObject> m_PinPrefabs;

		private Material mapMaterial;

		[SerializeField]
		private Transform m_MapAnchor;

		[SerializeField]
		private Transform m_PlayerIndicator;

		[SerializeField]
		private Camera m_Camera;

		[SerializeField]
		private Transform m_PinsGroup;

		[Space(5f)]
		[SerializeField]
		private GameObject m_GraffitiPinPrefab;

		[SerializeField]
		private GameObject m_StoryObjectivePin;

		[SerializeField]
		private GameObject m_TaxiPin;

		[SerializeField]
		private GameObject m_ToiletPin;

		[SerializeField]
		private GameObject m_CypherPin;

		[SerializeField]
		private Map preludeMap;

		[SerializeField]
		private Map hideOutMap;

		[SerializeField]
		private Map downHillMap;

		[SerializeField]
		private Map squareMap;

		[SerializeField]
		private Map mallMap;

		[SerializeField]
		private Map towerMap;

		[SerializeField]
		private Map pyramidMap;

		[SerializeField]
		private Map osakaMap;

		[SerializeField]
		private Map DEMO_hideOutMap;

		[SerializeField]
		private RenderTexture defaultRenderTexture;

		[SerializeField]
		private Color defaultCameraBackgroundColor = Color.blue;

		[SerializeField]
		private Color defaultMapLowColor = Color.red;

		[SerializeField]
		private Color defaultMapHighColor = Color.yellow;

		public float CameraDistance { get; set; }

		public float CameraViewAngle { get; set; }

		public float CameraHeightAngle { get; set; }

		public float CameraRotateYOffset { get; set; }

		public float CameraRotateXOffset { get; set; }

		public float CameraZoom
		{
			get
			{
				return cameraZoom;
			}
			set
			{
				cameraZoom = Mathf.Clamp(value, 0.25f, 2f);
			}
		}

		public Vector3 CameraPos { get; private set; }

		public Vector3 CameraFollowLocation { get; set; }

		public Vector3 PlayerPosition => m_Player.transform.position;

		public Quaternion CameraRot { get; private set; }

		public CameraModes CurrentCameraMode { get; set; }

		public Camera Camera => m_Camera;

		public void InitController()
		{
			cameraTransform = m_Camera.transform;
			SetCameraRenderTexture(defaultRenderTexture);
		}

		public void InitMap(Map map, Player player, StoryManager storyManager)
		{
			m_Player = player;
			this.storyManager = storyManager;
			if (m_MapAnchor.childCount != 0)
			{
				Transform[] allChildren = m_MapAnchor.GetAllChildren();
				for (int i = 0; i < allChildren.Length; i++)
				{
					UnityEngine.Object.Destroy(allChildren[i].gameObject);
				}
			}
			GameObject obj = UnityEngine.Object.Instantiate(map.MapObject, m_MapAnchor);
			obj.layer = 27;
			obj.transform.localPosition = map.PositionOffset;
			obj.transform.localEulerAngles = map.EulerOffset;
			m_MapPins = new List<MapPin>();
			CurrentCameraMode = CameraModes.Widget;
			mapUpdateCoroutine = StartCoroutine(MapUpdate());
			mapMaterial = map.MapMaterial;
		}

		public void DisposeMap()
		{
			if (m_Camera != null && m_Camera.targetTexture != null)
			{
				SetCameraRenderTexture(null);
			}
			StopMapUpdate();
		}

		public void StopMapUpdate()
		{
			if (mapUpdateCoroutine != null)
			{
				StopCoroutine(mapUpdateCoroutine);
			}
		}

		public Transform GetCameraTransform()
		{
			return cameraTransform;
		}

		public void RestoreDefaultMapControllerState()
		{
			SetCameraRenderTexture(defaultRenderTexture);
			SetMapCameraBackgroundColor(defaultCameraBackgroundColor);
			SetMapGradient(defaultMapLowColor, defaultMapHighColor);
			CurrentCameraMode = CameraModes.Widget;
		}

		public void SetMapCameraBackgroundColor(Color color)
		{
			m_Camera.backgroundColor = color;
		}

		public void SetCameraRenderTexture(RenderTexture renderTexture)
		{
			m_Camera.targetTexture.Release();
			m_Camera.targetTexture = renderTexture;
		}

		public void SetMapGradient(Color lowColor, Color highColor)
		{
			mapMaterial.SetVector("_ColorLow", lowColor);
			mapMaterial.SetVector("_ColorHigh", highColor);
		}

		public void CreatePins()
		{
			foreach (GraffitiSpot grafSpot in WorldHandler.instance.SceneObjectsRegister.grafSpots)
			{
				MapPin mapPin = CreatePin(MapPin.PinType.GraffitiPin);
				mapPin.AssignGraffitiSpot(grafSpot);
				mapPin.InitMapPin(MapPin.PinType.GraffitiPin);
				ActiveOnChapter componentInParent = grafSpot.GetComponentInParent<ActiveOnChapter>(includeInactive: true);
				if (componentInParent != null)
				{
					mapPin.containingChapterObject = componentInParent.gameObject;
				}
				ProgressObject componentInParent2 = grafSpot.GetComponentInParent<ProgressObject>(includeInactive: true);
				if (componentInParent2 != null)
				{
					mapPin.containingProgressObject = componentInParent2.gameObject;
				}
				mapPin.OnPinEnable();
			}
			activeStoryPins = 20;
			for (int i = 0; i < activeStoryPins; i++)
			{
				MapPin mapPin2 = CreatePin(MapPin.PinType.StoryObjectivePin);
				mapPin2.InitMapPin(MapPin.PinType.StoryObjectivePin);
				storyObjectivePins[i] = mapPin2;
			}
			Taxi[] array = UnityEngine.Object.FindObjectsOfType<Taxi>();
			foreach (Taxi taxi in array)
			{
				MapPin mapPin3 = CreatePin(MapPin.PinType.TaxiPin);
				mapPin3.InitMapPin(MapPin.PinType.TaxiPin);
				mapPin3.AssignTaxi(taxi.gameObject);
			}
			PublicToilet[] array2 = UnityEngine.Object.FindObjectsOfType<PublicToilet>();
			foreach (PublicToilet toiletObject in array2)
			{
				MapPin mapPin4 = CreatePin(MapPin.PinType.ToiletPin);
				mapPin4.InitMapPin(MapPin.PinType.ToiletPin);
				mapPin4.AssignToilet(toiletObject);
			}
			CharacterSelectSpot[] array3 = UnityEngine.Object.FindObjectsOfType<CharacterSelectSpot>();
			foreach (CharacterSelectSpot cypherObject in array3)
			{
				MapPin mapPin5 = CreatePin(MapPin.PinType.CypherPin);
				mapPin5.InitMapPin(MapPin.PinType.CypherPin);
				mapPin5.AssignCypher(cypherObject);
			}
		}

		public void UpdateObjectivePins()
		{
			storyManager.UpdateObjectives();
			int currentAmountOfObjects = 0;
			GameObject[] currentObjectivesObjects = storyManager.GetCurrentObjectivesObjects(out currentAmountOfObjects);
			UpdateStoryObjectivePins(currentObjectivesObjects, currentAmountOfObjects);
		}

		public void EnableMapCamera()
		{
			m_Camera.enabled = true;
		}

		public void DisableMapCamera()
		{
			m_Camera.enabled = false;
		}

		public Map GetMap(Stage stage)
		{
			return stage switch
			{
				Stage.Prelude => preludeMap, 
				Stage.hideout => hideOutMap, 
				Stage.downhill => downHillMap, 
				Stage.square => squareMap, 
				Stage.tower => towerMap, 
				Stage.Mall => mallMap, 
				Stage.osaka => osakaMap, 
				Stage.pyramid => pyramidMap, 
				Stage.DEMO_hideout => DEMO_hideOutMap, 
				_ => pyramidMap, 
			};
		}

		public void SetMapCameraToInGameCamera()
		{
			float cameraViewAngle = (CameraRotateYOffset = m_Player.cam.transform.eulerAngles.y);
			CameraViewAngle = cameraViewAngle;
			if (CameraViewAngle > 360f)
			{
				cameraViewAngle = (CameraRotateYOffset = 0f);
				CameraViewAngle = cameraViewAngle;
			}
			if (CameraViewAngle < 0f)
			{
				cameraViewAngle = (CameraRotateYOffset = 360f);
				CameraViewAngle = cameraViewAngle;
			}
			float num3 = Mathf.Clamp(m_Player.cam.transform.eulerAngles.x, 5f, 60f);
			cameraViewAngle = (CameraRotateXOffset = num3);
			CameraHeightAngle = cameraViewAngle;
		}

		private void Awake()
		{
			Instance = this;
			waitForEndOfFrame = new WaitForEndOfFrame();
			m_PinPrefabs = new Dictionary<MapPin.PinType, GameObject>
			{
				{
					MapPin.PinType.Pin,
					null
				},
				{
					MapPin.PinType.GraffitiPin,
					m_GraffitiPinPrefab
				},
				{
					MapPin.PinType.StoryObjectivePin,
					m_StoryObjectivePin
				},
				{
					MapPin.PinType.TaxiPin,
					m_TaxiPin
				},
				{
					MapPin.PinType.ToiletPin,
					m_ToiletPin
				},
				{
					MapPin.PinType.CypherPin,
					m_CypherPin
				}
			};
		}

		private void OnDestroy()
		{
			StopMapUpdate();
			Instance = null;
		}

		private IEnumerator MapUpdate()
		{
			while (true)
			{
				if (!m_Camera.enabled)
				{
					yield return waitForEndOfFrame;
				}
				UpdatePlayer(m_Player.transform.position, m_Player.transform.eulerAngles.y);
				UpdateCamera();
				UpdatePins();
				yield return waitForEndOfFrame;
			}
		}

		private void UpdatePlayer(Vector3 location, float angle)
		{
			m_PlayerIndicator.localPosition = location;
			m_PlayerIndicator.localEulerAngles = new Vector3(0f, angle, 0f);
		}

		private void UpdateCamera()
		{
			switch (CurrentCameraMode)
			{
			case CameraModes.Widget:
				WidgetDisplayUpdate();
				break;
			case CameraModes.FollowPlayerFixedRotation:
				FollowPlayerFixedRotationUpdate();
				break;
			case CameraModes.FollowPlayerFreeRotation:
				FollowPlayerFreeRotationUpdate();
				break;
			case CameraModes.FollowPlayerPauseScreen:
				FollowPlayerPauseScreen();
				break;
			}
			CameraPos = CalcCamPos();
			CameraRot = CalcCamRot();
			m_Camera.transform.localPosition = CameraPos;
			m_Camera.transform.localRotation = CameraRot;
		}

		private void UpdatePins()
		{
			foreach (MapPin mapPin in m_MapPins)
			{
				mapPin.PinUpdate();
			}
			UpdateObjectivePins();
		}

		private MapPin CreatePin(MapPin.PinType pinType)
		{
			GameObject obj = UnityEngine.Object.Instantiate(m_PinPrefabs[pinType], m_PinsGroup.transform);
			obj.layer = base.gameObject.layer;
			MapPin component = obj.GetComponent<MapPin>();
			if (pinType == MapPin.PinType.StoryObjectivePin)
			{
				component.gameObject.SetActive(value: false);
			}
			component.SetMapController(this);
			m_MapPins.Add(component);
			return component;
		}

		private void UpdateStoryObjectivePins(GameObject[] objectiveObjects, int amountOfObjects)
		{
			_ = activeStoryPins;
			for (int num = activeStoryPins - 1; num >= amountOfObjects; num--)
			{
				storyObjectivePins[num].DisablePin();
			}
			for (int i = 0; i < amountOfObjects; i++)
			{
				storyObjectivePins[i].EnablePin();
				storyObjectivePins[i].AssignGameplayEvent(objectiveObjects[i]);
			}
		}

		private Vector3 CalcCamPos()
		{
			return new Vector3
			{
				x = CameraFollowLocation.x + Mathf.Sin((CameraViewAngle - 180f) * (MathF.PI / 180f)) * (Mathf.Cos(CameraHeightAngle * (MathF.PI / 180f)) * CameraDistance),
				y = CameraFollowLocation.y + Mathf.Sin(CameraHeightAngle * (MathF.PI / 180f)) * CameraDistance,
				z = CameraFollowLocation.z + Mathf.Cos((CameraViewAngle - 180f) * (MathF.PI / 180f)) * (Mathf.Cos(CameraHeightAngle * (MathF.PI / 180f)) * CameraDistance)
			};
		}

		private Quaternion CalcCamRot()
		{
			return Quaternion.LookRotation(CameraFollowLocation - CameraPos);
		}

		private void FollowPlayerFixedRotationUpdate()
		{
			CameraFollowLocation = m_Player.transform.position;
			CameraViewAngle = 0f;
			CameraHeightAngle = 45f;
			CameraDistance = 250f;
		}

		private void FollowPlayerFreeRotationUpdate()
		{
			CameraFollowLocation = m_Player.transform.position;
			CameraViewAngle = m_Player.cam.transform.eulerAngles.y;
			CameraHeightAngle = 45f;
			CameraDistance = 250f;
		}

		private void FollowPlayerPauseScreen()
		{
			CameraFollowLocation = m_Player.transform.position;
			CameraViewAngle = CameraRotateYOffset;
			float cameraViewAngle;
			if (CameraViewAngle > 360f)
			{
				cameraViewAngle = (CameraRotateYOffset = 0f);
				CameraViewAngle = cameraViewAngle;
			}
			if (CameraViewAngle < 0f)
			{
				cameraViewAngle = (CameraRotateYOffset = 360f);
				CameraViewAngle = cameraViewAngle;
			}
			float num3 = Mathf.Clamp(CameraRotateXOffset, 5f, 60f);
			cameraViewAngle = (CameraRotateXOffset = num3);
			CameraHeightAngle = cameraViewAngle;
			CameraDistance = 300f * CameraZoom;
		}

		private void WidgetDisplayUpdate()
		{
			CameraFollowLocation = m_Player.transform.position;
			CameraViewAngle = m_Player.cam.transform.eulerAngles.y;
			CameraHeightAngle = 45f;
			CameraDistance = 250f;
		}
	}
}
