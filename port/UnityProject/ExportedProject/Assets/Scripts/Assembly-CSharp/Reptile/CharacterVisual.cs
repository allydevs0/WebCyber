using UnityEngine;

namespace Reptile
{
	public class CharacterVisual : MonoBehaviour
	{
		public enum MoveStylePropMode
		{
			ON_BACK = 0,
			ACTIVE = 1,
			OFF = 2
		}

		private Transform bmxFrameBone;

		private Transform bmxHandlebarsBone;

		private Transform bmxWheelFBone;

		private Transform bmxWheelRBone;

		private Transform bmxGearBone;

		private Transform bmxPedalLBone;

		private Transform bmxPedalRBone;

		public bool canBlink;

		public bool hasBoostPack = true;

		public MoveStyle moveStyle;

		public bool feetIK = true;

		public float groundAngleLimit;

		public bool handIKActiveR = true;

		public bool handIKActiveL = true;

		public Vector3 bodyPosTarget;

		public Transform tf;

		public Transform handIKTargetR;

		public Transform handIKTargetL;

		public Transform footR;

		public Transform footL;

		public Transform lowerLegR;

		public Transform lowerLegL;

		public Transform handR;

		public Transform handL;

		public Transform head;

		public Transform boostpackBone;

		public Transform skateboardBone;

		public Transform root;

		public bool hasEffects;

		public BoostpackEffectMode boostpackEffectMode;

		public FrictionEffectMode frictionEffectMode;

		public PlayerVisualEffects VFX = new PlayerVisualEffects();

		public PlayerMoveStyleProps moveStyleProps = new PlayerMoveStyleProps();

		public float boostPackEffectStandardScale = 0.7f;

		public SkinnedMeshRenderer mainRenderer;

		public GameObject characterObject;

		public Animator anim;

		internal ParticleSystem dustParticles;

		internal bool isGirl;

		internal bool isMechanical;

		public Animation boostPackEffectAnimation;

		internal float lookAtWeight;

		internal Vector3 lookAtPos;

		internal bool phoneActive;

		internal Vector3 lookAtVel;

		internal GameObject lookAtSubject;

		internal int bounceAnimHash;

		public void Init(Characters character, RuntimeAnimatorController animatorController, bool IK, float setGroundAngleLimit)
		{
			tf = base.transform;
			anim = GetComponentInChildren<Animator>();
			anim.runtimeAnimatorController = animatorController;
			anim.applyRootMotion = false;
			if (IK)
			{
				anim.gameObject.AddComponent<InverseKinematicsRelay>();
			}
			groundAngleLimit = setGroundAngleLimit;
			characterObject = anim.gameObject;
			characterObject.transform.localPosition = Vector3.zero;
			characterObject.transform.rotation = Quaternion.LookRotation(tf.forward);
			characterObject.layer = 15;
			mainRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
			mainRenderer.receiveShadows = false;
			mainRenderer.gameObject.layer = 15;
			root = anim.transform.Find("root");
			head = root.FindRecursive("head");
			boostpackBone = root.FindRecursive("jetpack");
			footR = root.FindRecursive("footr");
			footL = root.FindRecursive("footl");
			lowerLegR = root.FindRecursive("leg2r");
			lowerLegL = root.FindRecursive("leg2l");
			handR = root.FindRecursive("handr");
			handL = root.FindRecursive("handl");
			handIKTargetL = root.parent.Find("handlIK");
			handIKTargetR = root.parent.Find("handrIK");
			bounceAnimHash = GetCharacterBounceAnim(character);
			switch (character)
			{
			case Characters.frank:
				canBlink = true;
				tf.localScale = Vector3.one * 1.1f;
				break;
			case Characters.girl1:
				canBlink = true;
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hair");
				isGirl = true;
				break;
			case Characters.ringdude:
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("extraRing");
				break;
			case Characters.metalHead:
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("chain1");
				break;
			case Characters.headMan:
				canBlink = true;
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("chain1");
				break;
			case Characters.headManNoJetpack:
				canBlink = true;
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("chain1");
				hasBoostPack = false;
				break;
			case Characters.blockGuy:
			{
				canBlink = true;
				DynamicBone dynamicBone16 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone16.m_Root = head.FindRecursive("hair1l");
				dynamicBone16.m_Stiffness = 0.9f;
				dynamicBone16.m_Damping = 0.9f;
				dynamicBone16.m_Inert = 0.9f;
				DynamicBone dynamicBone17 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone17.m_Root = head.FindRecursive("hair1r");
				dynamicBone17.m_Stiffness = 0.9f;
				dynamicBone17.m_Damping = 0.9f;
				dynamicBone17.m_Inert = 0.9f;
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("strap1l");
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("strap1r");
				break;
			}
			case Characters.spaceGirl:
			{
				canBlink = true;
				DynamicBone dynamicBone14 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone14.m_Root = root.FindRecursive("sleeveR");
				dynamicBone14.m_Damping = 0.3f;
				dynamicBone14.m_Elasticity = 0.16f;
				dynamicBone14.m_Stiffness = 0.7f;
				DynamicBone dynamicBone15 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone15.m_Root = root.FindRecursive("sleeveL");
				dynamicBone15.m_Damping = 0.3f;
				dynamicBone15.m_Elasticity = 0.16f;
				dynamicBone15.m_Stiffness = 0.7f;
				isGirl = true;
				break;
			}
			case Characters.angel:
			{
				DynamicBone dynamicBone12 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone12.m_Root = root.FindRecursive("bretelr1");
				dynamicBone12.m_Stiffness = 0.78f;
				DynamicBone dynamicBone13 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone13.m_Root = root.FindRecursive("bretell1");
				dynamicBone13.m_Stiffness = 0.78f;
				isGirl = true;
				break;
			}
			case Characters.dj:
			{
				canBlink = true;
				DynamicBone dynamicBone10 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone10.m_Root = root.FindRecursive("sleever");
				dynamicBone10.m_Damping = 0.3f;
				dynamicBone10.m_Elasticity = 0.16f;
				dynamicBone10.m_Stiffness = 0.9f;
				DynamicBone dynamicBone11 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone11.m_Root = root.FindRecursive("sleevel");
				dynamicBone11.m_Damping = 0.3f;
				dynamicBone11.m_Elasticity = 0.16f;
				dynamicBone11.m_Stiffness = 0.9f;
				break;
			}
			case Characters.dummy:
			{
				DynamicBone dynamicBone8 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone8.m_Root = root.FindRecursive("wingr1");
				dynamicBone8.m_Stiffness = 0.7f;
				DynamicBone dynamicBone9 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone9.m_Root = root.FindRecursive("wingl1");
				dynamicBone9.m_Stiffness = 0.7f;
				break;
			}
			case Characters.eightBall:
				isMechanical = true;
				break;
			case Characters.medusa:
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairA1");
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairB1");
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairC1");
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairD1");
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairE1");
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hairF1");
				isGirl = true;
				tf.localScale = Vector3.one * 1.07f;
				break;
			case Characters.boarder:
				canBlink = true;
				base.gameObject.AddComponent<DynamicBone>().m_Root = head.FindRecursive("hair");
				break;
			case Characters.prince:
			{
				DynamicBone dynamicBone7 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone7.m_Root = root.FindRecursive("coatRoot");
				dynamicBone7.m_Stiffness = 0.6f;
				dynamicBone7.m_Damping = 0.25f;
				tf.localScale = Vector3.one * 0.83f;
				break;
			}
			case Characters.legendFace:
			{
				canBlink = true;
				DynamicBone dynamicBone6 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone6.m_Root = root.FindRecursive("beanie1");
				dynamicBone6.m_Stiffness = 0.6f;
				dynamicBone6.m_Damping = 0.25f;
				break;
			}
			case Characters.oldheadPlayer:
			{
				canBlink = true;
				DynamicBone dynamicBone5 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone5.m_Root = root.FindRecursive("cape1");
				dynamicBone5.m_Stiffness = 0.04f;
				dynamicBone5.m_Elasticity = 0.09f;
				dynamicBone5.m_Damping = 0.4f;
				break;
			}
			case Characters.robot:
				isMechanical = true;
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("chain1");
				break;
			case Characters.skate:
			{
				canBlink = true;
				DynamicBone dynamicBone3 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone3.m_Root = root.FindRecursive("hair1");
				dynamicBone3.m_Stiffness = 0.7f;
				dynamicBone3.m_Damping = 0.7f;
				DynamicBone dynamicBone4 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone4.m_Root = root.FindRecursive("hair6");
				dynamicBone4.m_Stiffness = 0.7f;
				dynamicBone4.m_Damping = 0.7f;
				isGirl = true;
				break;
			}
			case Characters.wideKid:
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("cord1");
				break;
			case Characters.futureGirl:
			{
				canBlink = true;
				DynamicBone dynamicBone2 = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone2.m_Root = root.FindRecursive("ponytail1");
				dynamicBone2.m_Stiffness = 0.7f;
				dynamicBone2.m_Damping = 0.7f;
				isGirl = true;
				break;
			}
			case Characters.pufferGirl:
			{
				DynamicBone dynamicBone = base.gameObject.AddComponent<DynamicBone>();
				dynamicBone.m_Root = root.FindRecursive("coat1");
				dynamicBone.m_Damping = 0.25f;
				dynamicBone.m_Elasticity = 0.16f;
				dynamicBone.m_Stiffness = 0.7f;
				isGirl = true;
				break;
			}
			case Characters.bunGirl:
				base.gameObject.AddComponent<DynamicBone>().m_Root = root.FindRecursive("capsule");
				isGirl = true;
				break;
			case Characters.eightBallBoss:
				isMechanical = true;
				break;
			case Characters.jetpackBossPlayer:
				canBlink = true;
				break;
			}
		}

		public static int GetCharacterFreestyleAnim(Characters c)
		{
			switch (c)
			{
			case Characters.frank:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle4");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle11");
				}
				return Animator.StringToHash("freestyle5");
			case Characters.girl1:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle4");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle6");
				}
				return Animator.StringToHash("freestyle14");
			case Characters.ringdude:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle4");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle3");
				}
				return Animator.StringToHash("freestyle12");
			case Characters.metalHead:
			case Characters.headMan:
			case Characters.headManNoJetpack:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle2");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle19");
				}
				return Animator.StringToHash("freestyle14");
			case Characters.blockGuy:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle1");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle18");
				}
				return Animator.StringToHash("freestyle10");
			case Characters.spaceGirl:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle17");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle6");
				}
				return Animator.StringToHash("freestyle12");
			case Characters.angel:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle1");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle5");
				}
				return Animator.StringToHash("freestyle14");
			case Characters.dj:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle3");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle10");
				}
				return Animator.StringToHash("freestyle19");
			case Characters.dummy:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle6");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle15");
				}
				return Animator.StringToHash("freestyle17");
			case Characters.eightBall:
			case Characters.eightBallBoss:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle2");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle10");
				}
				return Animator.StringToHash("freestyle11");
			case Characters.medusa:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle7");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle10");
				}
				return Animator.StringToHash("freestyle17");
			case Characters.boarder:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle1");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle6");
				}
				return Animator.StringToHash("freestyle8");
			case Characters.prince:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle7");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle8");
				}
				return Animator.StringToHash("freestyle9");
			case Characters.legendFace:
			case Characters.legendMetalHead:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle8");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle16");
				}
				return Animator.StringToHash("freestyle18");
			case Characters.oldheadPlayer:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle18");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle14");
				}
				return Animator.StringToHash("freestyle10");
			case Characters.robot:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle18");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle9");
				}
				return Animator.StringToHash("freestyle16");
			case Characters.skate:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle13");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle17");
				}
				return Animator.StringToHash("freestyle15");
			case Characters.wideKid:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle18");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle14");
				}
				return Animator.StringToHash("freestyle10");
			case Characters.futureGirl:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle2");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle13");
				}
				return Animator.StringToHash("freestyle15");
			case Characters.pufferGirl:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle3");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle11");
				}
				return Animator.StringToHash("freestyle19");
			case Characters.bunGirl:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle7");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle12");
				}
				return Animator.StringToHash("freestyle14");
			case Characters.jetpackBossPlayer:
				if (Random.value < 0.33f)
				{
					return Animator.StringToHash("freestyle9");
				}
				if (Random.value > 0.5f)
				{
					return Animator.StringToHash("freestyle15");
				}
				return Animator.StringToHash("freestyle12");
			default:
				return Animator.StringToHash("freestyle1");
			}
		}

		public static int GetCharacterBounceAnim(Characters c)
		{
			return c switch
			{
				Characters.frank => Animator.StringToHash("softBounce2"), 
				Characters.girl1 => Animator.StringToHash("softBounce10"), 
				Characters.ringdude => Animator.StringToHash("softBounce15"), 
				Characters.metalHead => Animator.StringToHash("bounce"), 
				Characters.headMan => Animator.StringToHash("softBounce1"), 
				Characters.headManNoJetpack => Animator.StringToHash("softBounce1"), 
				Characters.blockGuy => Animator.StringToHash("softBounce4"), 
				Characters.spaceGirl => Animator.StringToHash("softBounce3"), 
				Characters.angel => Animator.StringToHash("softBounce12"), 
				Characters.dj => Animator.StringToHash("softBounce6"), 
				Characters.dummy => Animator.StringToHash("softBounce14"), 
				Characters.eightBall => Animator.StringToHash("softBounce11"), 
				Characters.medusa => Animator.StringToHash("softBounce18"), 
				Characters.boarder => Animator.StringToHash("softBounce13"), 
				Characters.prince => Animator.StringToHash("softBounce2"), 
				Characters.legendFace => Animator.StringToHash("softBounce1"), 
				Characters.oldheadPlayer => Animator.StringToHash("softBounce7"), 
				Characters.robot => Animator.StringToHash("softBounce1"), 
				Characters.skate => Animator.StringToHash("softBounce1"), 
				Characters.wideKid => Animator.StringToHash("softBounce3"), 
				Characters.futureGirl => Animator.StringToHash("softBounce5"), 
				Characters.pufferGirl => Animator.StringToHash("softBounce8"), 
				Characters.bunGirl => Animator.StringToHash("softBounce9"), 
				Characters.eightBallBoss => Animator.StringToHash("softBounce11"), 
				Characters.jetpackBossPlayer => Animator.StringToHash("softBounce13"), 
				_ => Animator.StringToHash("bounce"), 
			};
		}

		public void DoMoveStyleEquipEffect(MoveStyle moveStyle)
		{
			switch (moveStyle)
			{
			case MoveStyle.SKATEBOARD:
				moveStyleProps.skateboard.GetComponentInChildren<ParticleSystem>().Play();
				break;
			case MoveStyle.INLINE:
				moveStyleProps.skateL.GetComponentInChildren<ParticleSystem>().Play();
				moveStyleProps.skateR.GetComponentInChildren<ParticleSystem>().Play();
				break;
			case MoveStyle.BMX:
				moveStyleProps.bmxFrame.GetComponentInChildren<ParticleSystem>().Play();
				break;
			}
		}

		private void SetLayerRecursively(GameObject gameObject, int newLayer)
		{
			gameObject.layer = newLayer;
			Transform[] allChildren = gameObject.transform.GetAllChildren();
			foreach (Transform transform in allChildren)
			{
				SetLayerRecursively(transform.gameObject, newLayer);
			}
		}

		public void InitVFX(PlayerVisualEffects prefabs)
		{
			Transform transform = boostpackBone.FindRecursive("boost");
			hasEffects = transform != null;
			if (hasEffects)
			{
				VFX.frictionEffect = Object.Instantiate(prefabs.frictionEffect);
				VFX.frictionEffect.transform.parent = tf;
				VFX.frictionEffect.transform.SetToIdentity();
				VFX.frictionEffect.transform.localScale = Vector3.one * 0.1f;
				SetLayerRecursively(VFX.frictionEffect, 15);
				VFX.frictionBlueEffect = Object.Instantiate(prefabs.frictionBlueEffect);
				VFX.frictionBlueEffect.transform.parent = tf;
				VFX.frictionBlueEffect.transform.SetToIdentity();
				VFX.frictionBlueEffect.transform.localScale = Vector3.one * 0.1f;
				SetLayerRecursively(VFX.frictionBlueEffect, 15);
				VFX.boostpackEffect = Object.Instantiate(prefabs.boostpackEffect);
				boostPackEffectAnimation = VFX.boostpackEffect.GetComponent<Animation>();
				VFX.boostpackBlueEffect = Object.Instantiate(prefabs.boostpackBlueEffect);
				Transform obj = VFX.boostpackEffect.transform;
				Transform parent = (VFX.boostpackBlueEffect.transform.parent = transform);
				obj.parent = parent;
				VFX.boostpackEffect.transform.SetToIdentity();
				VFX.boostpackBlueEffect.transform.SetToIdentity();
				Transform obj2 = VFX.boostpackEffect.transform;
				Vector3 localScale = (VFX.boostpackBlueEffect.transform.localScale = Vector3.one * boostPackEffectStandardScale);
				obj2.localScale = localScale;
				SetLayerRecursively(VFX.boostpackEffect, 15);
				SetLayerRecursively(VFX.boostpackEffect, 15);
				VFX.boostpackTrail = Object.Instantiate(prefabs.boostpackTrail);
				VFX.boostpackTrail.transform.parent = transform;
				VFX.boostpackTrail.transform.SetToIdentity();
				VFX.boostpackTrail.SetActive(value: false);
				VFX.boostpackTrail.layer = 15;
				SetLayerRecursively(VFX.boostpackTrail, 15);
				Transform transform3 = root.FindRecursive("propr");
				if (transform3 != null)
				{
					VFX.spraycan = Object.Instantiate(prefabs.spraycan);
					VFX.spraycan.transform.parent = transform3;
					VFX.spraycan.transform.SetToIdentity();
					VFX.spraycan.layer = 15;
				}
				VFX.spraypaint = Object.Instantiate(prefabs.spraypaint);
				VFX.spraypaint.transform.parent = VFX.spraycan.transform;
				VFX.spraypaint.transform.SetToIdentity();
				SetLayerRecursively(VFX.spraypaint, 15);
				Transform transform4 = root.FindRecursive("propl");
				if (transform4 != null)
				{
					VFX.phone = Object.Instantiate(prefabs.phone);
					VFX.phone.transform.parent = transform4;
					VFX.phone.transform.SetToIdentity();
					VFX.phone.transform.localScale = Vector3.one * 1f;
					VFX.phone.layer = 15;
				}
				VFX.dust = Object.Instantiate(prefabs.dust);
				VFX.dust.transform.parent = tf;
				VFX.dust.transform.SetToIdentity();
				SetLayerRecursively(VFX.dust, 15);
				dustParticles = VFX.dust.GetComponent<ParticleSystem>();
				VFX.ring = Object.Instantiate(prefabs.ring);
				VFX.ring.transform.parent = tf;
				VFX.ring.transform.SetToIdentity();
				SetLayerRecursively(VFX.ring, 15);
				SetPhone(set: false);
				SetSpraycan(set: false);
				SetBoostpackEffect(BoostpackEffectMode.OFF);
				SetFrictionEffect(FrictionEffectMode.OFF);
			}
		}

		public void InitMoveStyleProps(PlayerMoveStyleProps prefabs)
		{
			bmxFrameBone = root.parent.FindRecursive("bmxFrame");
			moveStyleProps.bmxFrame = Object.Instantiate(prefabs.bmxFrame, bmxFrameBone);
			moveStyleProps.bmxFrame.transform.SetToIdentity();
			moveStyleProps.bmxFrame.layer = 15;
			bmxHandlebarsBone = root.parent.FindRecursive("bmxHandlebars");
			moveStyleProps.bmxHandlebars = Object.Instantiate(prefabs.bmxHandlebars, bmxHandlebarsBone);
			moveStyleProps.bmxHandlebars.transform.SetToIdentity();
			moveStyleProps.bmxHandlebars.layer = 15;
			bmxWheelFBone = root.parent.FindRecursive("bmxWheelF");
			moveStyleProps.bmxWheelF = Object.Instantiate(prefabs.bmxWheelF, bmxWheelFBone);
			moveStyleProps.bmxWheelF.transform.SetToIdentity();
			moveStyleProps.bmxWheelF.layer = 15;
			bmxWheelRBone = root.parent.FindRecursive("bmxWheelR");
			moveStyleProps.bmxWheelR = Object.Instantiate(prefabs.bmxWheelR, bmxWheelRBone);
			moveStyleProps.bmxWheelR.transform.SetToIdentity();
			moveStyleProps.bmxWheelR.layer = 15;
			bmxGearBone = root.parent.FindRecursive("bmxGear");
			moveStyleProps.bmxGear = Object.Instantiate(prefabs.bmxGear, bmxGearBone);
			moveStyleProps.bmxGear.transform.SetToIdentity();
			moveStyleProps.bmxGear.layer = 15;
			bmxPedalLBone = root.parent.FindRecursive("bmxPedalL");
			moveStyleProps.bmxPedalL = Object.Instantiate(prefabs.bmxPedalL, bmxPedalLBone);
			moveStyleProps.bmxPedalL.transform.SetToIdentity();
			moveStyleProps.bmxPedalL.layer = 15;
			bmxPedalRBone = root.parent.FindRecursive("bmxPedalR");
			moveStyleProps.bmxPedalR = Object.Instantiate(prefabs.bmxPedalR, bmxPedalRBone);
			moveStyleProps.bmxPedalR.transform.SetToIdentity();
			moveStyleProps.bmxPedalR.layer = 15;
			SetBMXPropsMode(MoveStylePropMode.ACTIVE);
			skateboardBone = root.parent.FindRecursive("skateboard");
			moveStyleProps.skateboard = Object.Instantiate(prefabs.skateboard);
			moveStyleProps.skateboard.layer = 15;
			SetSkateboardPropsMode(MoveStylePropMode.ACTIVE);
			moveStyleProps.skateR = Object.Instantiate(prefabs.skateR);
			moveStyleProps.skateR.layer = 15;
			moveStyleProps.skateL = Object.Instantiate(prefabs.skateL);
			moveStyleProps.skateL.layer = 15;
			SetInlineSkatesPropsMode(MoveStylePropMode.ACTIVE);
		}

		public void SetMoveStyleVisualProps(Player player, MoveStyle setMoveStyle, bool forceOff = false)
		{
			MoveStylePropMode bMXPropsMode = ((setMoveStyle == MoveStyle.BMX) ? MoveStylePropMode.ACTIVE : MoveStylePropMode.OFF);
			MoveStylePropMode inlineSkatesPropsMode = ((setMoveStyle == MoveStyle.INLINE) ? MoveStylePropMode.ACTIVE : MoveStylePropMode.OFF);
			MoveStylePropMode skateboardPropsMode = ((setMoveStyle == MoveStyle.SKATEBOARD) ? MoveStylePropMode.ACTIVE : MoveStylePropMode.OFF);
			if (player != null && setMoveStyle == MoveStyle.ON_FOOT && !forceOff)
			{
				if (player.moveStyleEquipped == MoveStyle.SKATEBOARD)
				{
					skateboardPropsMode = MoveStylePropMode.ON_BACK;
				}
				else if (player.moveStyleEquipped == MoveStyle.INLINE)
				{
					inlineSkatesPropsMode = MoveStylePropMode.ON_BACK;
				}
				else if (player.moveStyleEquipped == MoveStyle.BMX)
				{
					bMXPropsMode = MoveStylePropMode.ON_BACK;
				}
			}
			SetSkateboardPropsMode(skateboardPropsMode);
			SetInlineSkatesPropsMode(inlineSkatesPropsMode);
			SetBMXPropsMode(bMXPropsMode);
		}

		public void SetMoveStyleVisualAnim(Player player, MoveStyle setMoveStyle, GameObject specialSkateboard = null)
		{
			feetIK = false;
			bodyPosTarget = Vector3.zero;
			moveStyle = setMoveStyle;
			handIKActiveR = (handIKActiveL = moveStyle == MoveStyle.BMX);
			if (player != null)
			{
				switch (moveStyle)
				{
				case MoveStyle.ON_FOOT:
					anim.runtimeAnimatorController = player.animatorController;
					break;
				case MoveStyle.BMX:
					anim.runtimeAnimatorController = player.animatorControllerBMX;
					break;
				case MoveStyle.SKATEBOARD:
				case MoveStyle.SPECIAL_SKATEBOARD:
					anim.runtimeAnimatorController = player.animatorControllerSkateboard;
					break;
				case MoveStyle.INLINE:
					anim.runtimeAnimatorController = player.animatorControllerSkates;
					break;
				}
			}
		}

		private void SetSkateboardPropsMode(MoveStylePropMode mode)
		{
			moveStyleProps.skateboard.SetActive(mode == MoveStylePropMode.ACTIVE);
			switch (mode)
			{
			case MoveStylePropMode.ACTIVE:
				moveStyleProps.skateboard.transform.parent = skateboardBone;
				moveStyleProps.skateboard.transform.SetToIdentity();
				moveStyleProps.skateboard.transform.localScale = Vector3.one * 1f;
				break;
			case MoveStylePropMode.ON_BACK:
				moveStyleProps.skateboard.transform.parent = boostpackBone;
				moveStyleProps.skateboard.transform.localPosition = new Vector3(-0.04f, 0f, -0.017f);
				moveStyleProps.skateboard.transform.localRotation = Quaternion.Euler(0f, 0f, 270f);
				moveStyleProps.skateboard.transform.localScale = Vector3.one * 0.63f;
				break;
			}
		}

		private void SetInlineSkatesPropsMode(MoveStylePropMode mode)
		{
			moveStyleProps.skateL.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.skateR.SetActive(mode == MoveStylePropMode.ACTIVE);
			switch (mode)
			{
			case MoveStylePropMode.ACTIVE:
				moveStyleProps.skateL.transform.parent = footL;
				moveStyleProps.skateR.transform.parent = footR;
				moveStyleProps.skateL.transform.SetToIdentity();
				moveStyleProps.skateR.transform.SetToIdentity();
				moveStyleProps.skateL.transform.localScale = Vector3.one * 1f;
				moveStyleProps.skateR.transform.localScale = Vector3.one * 1f;
				break;
			case MoveStylePropMode.ON_BACK:
				moveStyleProps.skateL.transform.parent = boostpackBone;
				moveStyleProps.skateR.transform.parent = boostpackBone;
				moveStyleProps.skateL.transform.localPosition = new Vector3(-0.09f, -0.039f, -0.266f);
				moveStyleProps.skateR.transform.localPosition = new Vector3(-0.09f, -0.175f, -0.25f);
				moveStyleProps.skateL.transform.localRotation = Quaternion.Euler(290f, 25f, 74f);
				moveStyleProps.skateR.transform.localRotation = Quaternion.Euler(60f, 20f, 280f);
				moveStyleProps.skateL.transform.localScale = Vector3.one * 0.63f;
				moveStyleProps.skateR.transform.localScale = Vector3.one * 0.63f;
				break;
			}
		}

		private void SetBMXPropsMode(MoveStylePropMode mode)
		{
			moveStyleProps.bmxFrame.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxHandlebars.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxWheelR.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxWheelF.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxGear.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxPedalL.SetActive(mode == MoveStylePropMode.ACTIVE);
			moveStyleProps.bmxPedalR.SetActive(mode == MoveStylePropMode.ACTIVE);
			if (mode == MoveStylePropMode.ACTIVE)
			{
				moveStyleProps.bmxWheelF.transform.parent = bmxWheelFBone;
				moveStyleProps.bmxWheelF.transform.SetToIdentity();
				moveStyleProps.bmxWheelF.transform.localScale = Vector3.one * 1f;
			}
		}

		public void SetBoostpackEffect(BoostpackEffectMode set, float overrideScale = -1f)
		{
			if (!hasEffects)
			{
				return;
			}
			if (!hasBoostPack)
			{
				set = BoostpackEffectMode.OFF;
			}
			BoostpackEffectMode boostpackEffectMode = this.boostpackEffectMode;
			this.boostpackEffectMode = set;
			float num = boostPackEffectStandardScale;
			float boosterAnimSpeed = 1f;
			switch (set)
			{
			case BoostpackEffectMode.OFF:
				VFX.boostpackEffect.SetActive(value: false);
				VFX.boostpackBlueEffect.SetActive(value: false);
				break;
			case BoostpackEffectMode.ON:
				if (!VFX.boostpackEffect.activeSelf)
				{
					VFX.boostpackEffect.SetActive(value: true);
				}
				VFX.boostpackBlueEffect.SetActive(value: false);
				break;
			case BoostpackEffectMode.BOOSTING:
				VFX.boostpackEffect.SetActive(value: false);
				if (!VFX.boostpackBlueEffect.activeSelf)
				{
					VFX.boostpackBlueEffect.SetActive(value: true);
				}
				num = 1.5f;
				boosterAnimSpeed = 1.2f;
				break;
			case BoostpackEffectMode.GRIND_CORNERING:
				if (boostpackEffectMode != BoostpackEffectMode.GRIND_CORNERING)
				{
					VFX.boostpackEffect.SetActive(value: false);
				}
				if (!VFX.boostpackEffect.activeSelf)
				{
					VFX.boostpackEffect.SetActive(value: true);
				}
				VFX.boostpackBlueEffect.SetActive(value: false);
				num = 1.1f;
				boosterAnimSpeed = 1.2f;
				break;
			case BoostpackEffectMode.GRAFFITI:
				if (!VFX.boostpackEffect.activeSelf)
				{
					VFX.boostpackEffect.SetActive(value: true);
				}
				VFX.boostpackBlueEffect.SetActive(value: false);
				num = 0.5f;
				boosterAnimSpeed = 0.65f;
				break;
			case BoostpackEffectMode.AIRDASH:
				if (!VFX.boostpackEffect.activeSelf)
				{
					VFX.boostpackEffect.SetActive(value: true);
				}
				VFX.boostpackBlueEffect.SetActive(value: false);
				num = 1.65f;
				break;
			}
			if (overrideScale != -1f)
			{
				num = overrideScale;
			}
			SetBoosterAnimSpeed(boosterAnimSpeed);
			VFX.boostpackEffect.transform.localScale = Vector3.one * num;
			VFX.boostpackBlueEffect.transform.localScale = new Vector3(num, num * 0.75f, num * 0.75f);
		}

		public void SetFrictionEffect(FrictionEffectMode set)
		{
			if (hasEffects)
			{
				frictionEffectMode = set;
				VFX.frictionEffect.SetActive(set == FrictionEffectMode.ON);
				VFX.frictionBlueEffect.SetActive(set == FrictionEffectMode.BLUE);
			}
		}

		public void SetBoosterAnimSpeed(float speed)
		{
			boostPackEffectAnimation["boostIdle"].speed = speed;
		}

		public void SetSpraycan(bool set, Characters c = Characters.NONE)
		{
			if (VFX.spraycan != null && set != VFX.spraycan.activeSelf)
			{
				VFX.spraycan.SetActive(set && c != Characters.oldheadPlayer);
			}
			handIKActiveR = !set && moveStyle == MoveStyle.BMX;
		}

		public void SetPhone(bool set)
		{
			if (VFX.phone != null && set != VFX.phone.activeSelf)
			{
				VFX.phone.SetActive(set);
			}
			handIKActiveL = !set && moveStyle == MoveStyle.BMX;
		}

		public void SetDustEmission(int rate)
		{
			if (dustParticles == null)
			{
				dustParticles = VFX.dust.GetComponent<ParticleSystem>();
			}
			ParticleSystem.EmissionModule emission = dustParticles.emission;
			emission.rateOverTime = rate;
		}

		public void SetVisible(bool set)
		{
			characterObject.SetActive(set);
		}

		public void ClearLookAtSubject()
		{
			phoneActive = false;
			lookAtSubject = null;
		}

		public void LookAtSubject(GameObject subject, Vector3 position)
		{
			if ((lookAtSubject == null || lookAtSubject == subject) && !phoneActive && Vector3.Dot(tf.forward, (position - head.position).normalized) > -0.2f)
			{
				lookAtSubject = subject;
				lookAtPos = Vector3.Lerp(lookAtPos, position, 6f * Core.dt);
			}
		}

		public void InverseKinematics(int layerIndex)
		{
			if (phoneActive)
			{
				lookAtSubject = VFX.phone;
				lookAtPos = Vector3.Lerp(lookAtPos, VFX.phone.transform.position + Vector3.up * 0.25f, Mathf.Clamp(lookAtVel.magnitude * Core.dt * 3f, 6f * Core.dt, 1f));
			}
			if ((bool)lookAtSubject)
			{
				lookAtWeight = Mathf.Min(lookAtWeight + Core.dt * 2.2f, 0.55f);
			}
			else
			{
				float dt = Core.dt;
				lookAtWeight = Mathf.Max(lookAtWeight - dt * 1.6f, 0f);
				lookAtPos = Vector3.Lerp(lookAtPos, head.position + head.forward, Mathf.Clamp(lookAtVel.magnitude * dt * 3f, 6f * dt, 1f));
			}
			anim.SetLookAtWeight(lookAtWeight);
			anim.SetLookAtPosition(lookAtPos);
			if (handIKActiveR && (bool)handIKTargetR)
			{
				anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
				anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
				anim.SetIKPosition(AvatarIKGoal.RightHand, handIKTargetR.position);
				anim.SetIKRotation(AvatarIKGoal.RightHand, handIKTargetR.rotation);
			}
			else
			{
				anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
				anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
			}
			if (handIKActiveL && (bool)handIKTargetL)
			{
				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
				anim.SetIKPosition(AvatarIKGoal.LeftHand, handIKTargetL.position);
				anim.SetIKRotation(AvatarIKGoal.LeftHand, handIKTargetL.rotation);
			}
			else
			{
				anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
			}
			if (feetIK)
			{
				float num = 0.5f;
				float maxDistance = 1.2f;
				float num2 = 0.5f;
				float num3 = groundAngleLimit * 0.65f;
				Vector3 vector = Vector3.zero;
				Vector3 vector2 = Vector3.zero;
				float num4 = footR.position.y - tf.position.y;
				Vector3 origin = footR.position + Vector3.up * num;
				int layerMask = 10485761;
				if (Physics.Raycast(origin, Vector3.down, out var hitInfo, maxDistance, layerMask) && Vector3.Angle(hitInfo.normal, Vector3.up) < groundAngleLimit)
				{
					Vector3 axis = Vector3.Cross(Vector3.up, hitInfo.normal);
					float num5 = Vector3.Angle(Vector3.up, hitInfo.normal);
					Quaternion quaternion = Quaternion.AngleAxis(((num5 < num3) ? 1f : num2) * num5, axis);
					Vector3 goalPosition = hitInfo.point + num4 * Vector3.up;
					vector = Vector3.Project(hitInfo.point - tf.position, Vector3.up);
					anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
					anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
					anim.SetIKPosition(AvatarIKGoal.RightFoot, goalPosition);
					anim.SetIKRotation(AvatarIKGoal.RightFoot, quaternion * anim.GetIKRotation(AvatarIKGoal.RightFoot));
				}
				else
				{
					anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
					anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
				}
				num4 = footL.position.y - tf.position.y;
				if (Physics.Raycast(footL.position + Vector3.up * num, Vector3.down, out hitInfo, maxDistance, layerMask) && Vector3.Angle(hitInfo.normal, Vector3.up) < groundAngleLimit)
				{
					Vector3 axis2 = Vector3.Cross(Vector3.up, hitInfo.normal);
					float num6 = Vector3.Angle(Vector3.up, hitInfo.normal);
					Quaternion quaternion2 = Quaternion.AngleAxis(((num6 < num3) ? 1f : num2) * num6, axis2);
					Vector3 goalPosition2 = hitInfo.point + num4 * Vector3.up;
					vector2 = Vector3.Project(hitInfo.point - tf.position, Vector3.up);
					anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
					anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
					anim.SetIKPosition(AvatarIKGoal.LeftFoot, goalPosition2);
					anim.SetIKRotation(AvatarIKGoal.LeftFoot, quaternion2 * anim.GetIKRotation(AvatarIKGoal.LeftFoot));
				}
				else
				{
					anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
					anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
				}
				Vector3 b = (((vector2.sqrMagnitude > vector.sqrMagnitude) ? vector2 : vector) + (vector2 + vector) * 0.5f) * 0.5f;
				bodyPosTarget = Vector3.Lerp(bodyPosTarget, b, 0.15f);
			}
			else
			{
				bodyPosTarget = Vector3.Lerp(bodyPosTarget, Vector3.zero, 0.05f);
				anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
				anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
				anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
				anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
			}
			anim.bodyPosition += bodyPosTarget;
		}
	}
}
