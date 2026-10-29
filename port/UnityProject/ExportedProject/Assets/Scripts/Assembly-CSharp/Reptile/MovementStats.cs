using System;

namespace Reptile
{
	[Serializable]
	public class MovementStats
	{
		public float runSpeed;

		public float walkSpeed;

		public float groundAcc;

		public float airAcc;

		public float groundDecc;

		public float airDecc;

		public float rotSpeedAtStill;

		public float rotSpeedAtMaxSpeed;

		public float rotSpeedInAir;

		public float grindSpeed;

		public float slideDeccHighSpeed;

		public float slideDeccLowSpeed;
	}
}
