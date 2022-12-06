using System;
using Models;

namespace Service.Abstractions
{
	public interface IQueueEngine
	{
		void EnqueueLink();
		Link DequeueLink();
	}
}

