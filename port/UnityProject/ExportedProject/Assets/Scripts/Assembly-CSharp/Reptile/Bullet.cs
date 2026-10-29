using System.Collections.Generic;
using UnityEngine;

namespace Reptile
{
	public class Bullet : MonoBehaviour, IPoolable
	{
		[SerializeField]
		protected LineRenderer tracer;

		[SerializeField]
		protected LayerMask layerMask;

		[SerializeField]
		protected GameObject environmentImpactVFXPrefab;

		[SerializeField]
		protected GameObject playerImpactVFXPrefab;

		[SerializeField]
		protected int damage = 1;

		[SerializeField]
		protected bool bigKnockback;

		protected Vector3 startPos;

		protected Vector3 prevPos;

		protected Transform tf;

		private float tracerLife;

		private float shotLength;

		protected PoolableObjectToName playerImpactEffect;

		protected PoolableObjectToName environmentImpactEffect;

		protected PoolableState state = PoolableState.Pooled;

		protected List<IPoolable> poolables = new List<IPoolable>();

		protected string poolableName;

		private Vector2 uvOffset = Vector2.zero;

		public PoolableState State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
			}
		}

		public string PoolableName
		{
			get
			{
				return poolableName;
			}
			set
			{
				poolableName = value;
			}
		}

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (state == PoolableState.Pooled)
			{
				base.gameObject.SetActive(value: true);
				tf = base.transform;
				tf.position = position;
				tf.rotation = rotation;
				startPos = (prevPos = tf.position);
				if (playerImpactEffect == null)
				{
					playerImpactEffect = new PoolableObjectToName(playerImpactVFXPrefab, playerImpactVFXPrefab.name);
				}
				if (environmentImpactEffect == null)
				{
					environmentImpactEffect = new PoolableObjectToName(environmentImpactVFXPrefab, environmentImpactVFXPrefab.name);
				}
				HitScan();
				state = PoolableState.Active;
			}
		}

		private void HitScan()
		{
			if (Physics.Raycast(startPos, tf.forward, out var hitInfo, 500f, layerMask, QueryTriggerInteraction.Ignore))
			{
				if (hitInfo.collider.gameObject.layer == 9)
				{
					Player component = hitInfo.collider.transform.parent.parent.GetComponent<Player>();
					if (component.ability != component.slideAbility || component.moveStyle != MoveStyle.ON_FOOT)
					{
						GetPoolableFromPoolOrInstantiate(playerImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(tf.forward));
						KnockbackAbility.KnockbackType knockbackType = KnockbackAbility.KnockbackType.STATIONARY;
						if (component.ability == component.danceAbility)
						{
							knockbackType = KnockbackAbility.KnockbackType.NO_KNOCKBACK;
						}
						if (bigKnockback)
						{
							knockbackType = KnockbackAbility.KnockbackType.FAR;
						}
						component.GetHit(damage, tf.forward * -1f, knockbackType);
					}
				}
				if (hitInfo.collider.gameObject.layer == 0 || GroundDetection.LayerIsNonStable(hitInfo.collider.gameObject.layer))
				{
					GetPoolableFromPoolOrInstantiate(environmentImpactEffect).InitPoolable(hitInfo.point, Quaternion.LookRotation(Vector3.forward, hitInfo.normal));
				}
			}
			else
			{
				hitInfo.point = tf.forward * 500f;
			}
			if (tracer != null)
			{
				tracer.SetPosition(0, startPos);
				tracer.SetPosition(1, hitInfo.point);
				shotLength = (hitInfo.point - startPos).magnitude;
				tracerLife = 0.2f;
			}
			else
			{
				Pool();
			}
		}

		private void Update()
		{
			if (tracer != null && tracerLife > 0f)
			{
				uvOffset.x = Mathf.Lerp(1f, (0f - shotLength) / 10f, 1f - tracerLife / 0.2f);
				tracer.material.SetTextureOffset("_MainTex", uvOffset);
				tracerLife -= Core.dt;
				if (tracerLife <= 0f)
				{
					Pool();
					tracerLife = 0f;
				}
			}
		}

		private void SetLinePos()
		{
			tracer.SetPosition(0, startPos);
			tracer.SetPosition(1, tf.position);
		}

		private void Pool()
		{
			state = PoolableState.Pooled;
			base.gameObject.SetActive(value: false);
		}

		protected IPoolable GetPoolableFromPoolOrInstantiate(PoolableObjectToName poolableObject)
		{
			IPoolable poolable = null;
			for (int i = 0; i < poolables.Count; i++)
			{
				if (poolables[i].PoolableName == poolableObject.name && poolables[i].State == PoolableState.Pooled)
				{
					return poolables[i];
				}
			}
			if (poolable == null)
			{
				poolable = Object.Instantiate(poolableObject.obj).GetComponent<IPoolable>();
				poolable.PoolableName = poolableObject.name;
				poolables.Add(poolable);
			}
			return poolable;
		}
	}
}
