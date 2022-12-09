namespace Service.Abstractions
{
	public interface IEngine
	{
		Task Crawl(CancellationToken cancellationToken);
	}
}

