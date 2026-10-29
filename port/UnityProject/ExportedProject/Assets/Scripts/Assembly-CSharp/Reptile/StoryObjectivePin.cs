using System;
using UnityEngine;

namespace Reptile
{
	public class StoryObjectivePin : MapPin
	{
		private enum PosState
		{
			clamped = 0,
			onLocation = 1
		}

		private const float SWITCH_DELAY = 0.1f;

		private float m_SwitchTimer;

		private PosState m_CurrentState;

		[SerializeField]
		private Vector3 m_PinLocation;

		[SerializeField]
		private GameObject inViewGraphic;

		[SerializeField]
		private GameObject outOfViewGraphic;

		private Plane[] frustumPlanes;

		private float[] frustumOffsets = new float[4];

		private float currentFrustumOffset;

		private Vector3 closestFrustumPoint = Vector3.zero;

		private float distanceToPlane = float.PositiveInfinity;

		public override void Awake()
		{
			base.Awake();
			frustumPlanes = new Plane[6];
		}

		public void OnDestroy()
		{
			frustumPlanes = null;
		}

		public override void SetLocation()
		{
			m_PinLocation = objectiveTransform.position;
		}

		public override void OnPinEnable()
		{
			m_CurrentState = (IsWithinFrustrum(base.m_mapController.transform.TransformPoint(m_PinLocation)) ? PosState.onLocation : PosState.clamped);
		}

		public override void PinUpdate()
		{
			if (m_ObjectiveObject == null || !m_ObjectiveObject.activeSelf)
			{
				return;
			}
			switch ((PosState)(IsWithinFrustrum(base.m_mapController.transform.TransformPoint(m_PinLocation)) ? 1 : 0))
			{
			case PosState.clamped:
				if (m_CurrentState != PosState.clamped)
				{
					m_SwitchTimer += Time.deltaTime;
					if (m_SwitchTimer >= 0.1f)
					{
						m_CurrentState = PosState.clamped;
						inViewGraphic.SetActive(value: false);
						outOfViewGraphic.SetActive(value: true);
					}
				}
				break;
			case PosState.onLocation:
				if (m_CurrentState != PosState.onLocation)
				{
					m_SwitchTimer -= Time.deltaTime;
					if (m_SwitchTimer <= 0f)
					{
						m_CurrentState = PosState.onLocation;
						inViewGraphic.SetActive(value: true);
						outOfViewGraphic.SetActive(value: false);
					}
				}
				break;
			}
			switch (m_CurrentState)
			{
			case PosState.clamped:
				ClampedUpdate();
				break;
			case PosState.onLocation:
				RelativeUpdate();
				break;
			}
			base.PinUpdate();
		}

		protected virtual void RelativeUpdate()
		{
			base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, m_PinLocation, 10f * Time.deltaTime);
		}

		protected virtual void ClampedUpdate()
		{
			Vector3 vector = base.m_mapController.transform.InverseTransformPoint(closestFrustumPoint);
			Vector3 vector2 = base.m_mapController.PlayerPosition - m_PinLocation;
			Vector3 to = vector - m_PinLocation;
			float num = Vector3.Angle(vector2, to);
			float num2 = to.magnitude / Mathf.Cos(num * (MathF.PI / 180f));
			Vector3 vector3 = vector2.normalized * (num2 + currentFrustumOffset);
			base.transform.localPosition = m_PinLocation + vector3;
			if (m_PinLocation - base.transform.localPosition != Vector3.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(m_PinLocation - base.transform.localPosition, Vector3.up);
			}
		}

		private bool IsWithinFrustrum(Vector3 worldPos)
		{
			GeometryUtility.CalculateFrustumPlanes(base.m_mapController.Camera, frustumPlanes);
			distanceToPlane = float.PositiveInfinity;
			bool result = true;
			for (int i = 0; i < 4; i++)
			{
				if (!frustumPlanes[i].GetSide(worldPos))
				{
					result = false;
					Vector3 vector = frustumPlanes[i].ClosestPointOnPlane(worldPos);
					float distanceToPoint = frustumPlanes[i].GetDistanceToPoint(worldPos);
					if (distanceToPoint < distanceToPlane)
					{
						closestFrustumPoint = vector;
						distanceToPlane = distanceToPoint;
						currentFrustumOffset = frustumOffsets[i];
					}
				}
			}
			return result;
		}
	}
}
