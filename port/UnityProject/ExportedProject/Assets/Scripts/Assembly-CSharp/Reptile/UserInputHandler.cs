using System;
using UnityEngine;

namespace Reptile
{
	public class UserInputHandler : MonoBehaviour
	{
		[Serializable]
		public struct InputBuffer
		{
			public int frame;

			public bool sprayButtonHeldPrevFrame;

			public bool sprayButtonHeld;

			public bool sprayButtonNew;

			public bool jumpButtonHeld;

			public bool jumpButtonNew;

			public bool trick1ButtonHeld;

			public bool trick1ButtonNew;

			public bool trick2ButtonHeld;

			public bool trick2ButtonNew;

			public bool trick3ButtonHeld;

			public bool trick3ButtonNew;

			public bool slideButtonHeld;

			public bool slideButtonNew;

			public bool danceButtonHeld;

			public bool danceButtonNew;

			public bool boostButtonHeld;

			public bool switchStyleButtonNew;

			public bool switchStyleButtonHeld;

			public bool boostButtonNew;

			public bool walkButtonHeld;

			public bool walkButtonNew;

			public float moveAxisX;

			public float moveAxisY;
		}

		private GameInput gameInput;

		public InputBuffer PollInputs(ref InputBuffer inputBuffer)
		{
			inputBuffer.jumpButtonNew = gameInput.GetButtonNew(7);
			inputBuffer.jumpButtonHeld = gameInput.GetButtonHeld(7);
			inputBuffer.sprayButtonHeldPrevFrame = gameInput.GetButtonHeldPrevFrame(10);
			inputBuffer.sprayButtonHeld = gameInput.GetButtonHeld(10);
			inputBuffer.sprayButtonNew = gameInput.GetButtonNew(10);
			inputBuffer.boostButtonHeld = gameInput.GetButtonHeld(18);
			inputBuffer.boostButtonNew = gameInput.GetButtonNew(18);
			inputBuffer.trick1ButtonHeld = gameInput.GetButtonHeld(15);
			inputBuffer.trick1ButtonNew = gameInput.GetButtonNew(15);
			inputBuffer.trick2ButtonHeld = gameInput.GetButtonHeld(12);
			inputBuffer.trick2ButtonNew = gameInput.GetButtonNew(12);
			inputBuffer.trick3ButtonHeld = gameInput.GetButtonHeld(65);
			inputBuffer.trick3ButtonNew = gameInput.GetButtonNew(65);
			inputBuffer.switchStyleButtonHeld = gameInput.GetButtonHeld(11);
			inputBuffer.switchStyleButtonNew = gameInput.GetButtonNew(11);
			inputBuffer.slideButtonHeld = gameInput.GetButtonHeld(8);
			inputBuffer.slideButtonNew = gameInput.GetButtonNew(8);
			inputBuffer.danceButtonHeld = gameInput.GetButtonHeld(17);
			inputBuffer.danceButtonNew = gameInput.GetButtonNew(17);
			inputBuffer.walkButtonHeld = gameInput.GetButtonHeld(16);
			inputBuffer.walkButtonNew = gameInput.GetButtonNew(16);
			inputBuffer.moveAxisX = gameInput.GetAxis(5);
			inputBuffer.moveAxisY = gameInput.GetAxis(6);
			return inputBuffer;
		}

		private void Awake()
		{
			gameInput = Core.Instance.GameInput;
		}
	}
}
