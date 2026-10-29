using System.IO;

namespace Reptile
{
	public abstract class ProgressableData : ISaveable
	{
		protected string uid = string.Empty;

		protected ProgressableType progressableType;

		public string Uid
		{
			get
			{
				return uid;
			}
			set
			{
				uid = value;
			}
		}

		public ProgressableType ProgressableType => progressableType;

		public abstract void Write(BinaryWriter writer);

		public abstract void Read(BinaryReader reader);
	}
}
