using UnityEngine;

namespace Reptile
{
	public class PoolableParticleEffects : MonoBehaviour, IPoolable
	{
		[SerializeField]
		private ParticleSystem system;

		private PoolableState state = PoolableState.Pooled;

		private Transform tf;

		private string m_name;

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
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public void InitPoolable(Vector3 position, Quaternion rotation)
		{
			if (state == PoolableState.Pooled)
			{
				if (tf == null)
				{
					tf = base.transform;
				}
				tf.position = position;
				tf.rotation = rotation;
				base.gameObject.SetActive(value: true);
				state = PoolableState.Active;
				system.Play();
			}
		}

		private void Update()
		{
			if (!system.IsAlive())
			{
				state = PoolableState.Pooled;
				base.gameObject.SetActive(value: false);
			}
		}
	}
}
