using System;

namespace Reptile
{
	public interface IUserId : IEquatable<IUserId>
	{
		object Identifier { get; }

		string UserName { get; }

		bool IsValid { get; }
	}
}
