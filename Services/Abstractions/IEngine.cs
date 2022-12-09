namespace Services.Abstractions
{
	public interface IEngine
	{
		Task Crawl(CancellationToken cancellationToken);
	}
}

