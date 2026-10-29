using UnityEngine;
using UnityEngine.Playables;

namespace Reptile
{
	public class TalkAbility : Ability
	{
		public NPC npc;

		public PlayableDirector sequence;

		private int idleHash = Animator.StringToHash("idle");

		public TalkAbility(Player playerEntity)
			: base(playerEntity)
		{
			canUseSpraycan = false;
		}

		public override void Init()
		{
			normalMovement = true;
			normalRotation = false;
			targetSpeed = 0f;
			canStartGrind = false;
			canStartWallrun = false;
		}

		public override void OnStartAbility()
		{
			p.SetVelocity(Vector3.zero);
			normalAnimation = true;
			p.userInputEnabled = false;
			p.phone.AllowPhone(isAllowed: false);
		}

		public void StartTalking(NPC setNpc, int dialogueIndex, bool playCharacterTalkingVoice)
		{
			npc = setNpc;
			p.userInputEnabled = true;
			normalAnimation = false;
			npc.TalkSequenceIsSet(dialogueIndex);
			p.SetVelocity(Vector3.zero);
			p.PlayAnim(idleHash, forceOverwrite: true, instant: true);
			if (playCharacterTalkingVoice)
			{
				if (npc.useMiscTalkVoice != AudioClipID.NONE)
				{
					p.AudioManager.PlayVoice(SfxCollectionID.VoiceDialogue, npc.useMiscTalkVoice);
				}
				else if (npc.character != Characters.NONE)
				{
					p.AudioManager.PlayVoice(npc.character, AudioClipID.VoiceTalk);
				}
			}
		}

		public void StopTalking()
		{
			npc = null;
			p.cam.ResetCameraPositionRotation();
			p.StopCurrentAbility();
			p.PlayAnim(idleHash, forceOverwrite: true, instant: true);
		}

		public override void OnStopAbility()
		{
			p.userInputEnabled = true;
			p.phone.AllowPhone(isAllowed: true, completelyTurnOff: false, turnOnWhenAllowed: true);
		}
	}
}
