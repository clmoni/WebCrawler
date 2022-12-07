using Service;

namespace Crawler.Service.Tests;

public class LinkRepositoryTests
{
    [Fact]
    public void AddVisited_WhenGivenLink_AddsVisitedLinkAsKeyAndInitialisesChildContainer()
    {
        // arrange
        var linksDictionary= new Dictionary<string, List<string?>>();
        var linkRepository = new LinkRepository(linksDictionary);
        const string expectedVisitedLink = "http://test.com";
        
        // act
        linkRepository.AddVisited(new Uri(expectedVisitedLink));
        
        // assert
        linksDictionary.Count.Should().Be(1);
        linksDictionary[expectedVisitedLink.ToUpper()].Should().BeEmpty();
    }
    
    [Fact]
    public void AddVisited_WhenGivenLinkAlreadyVisit_ShouldNotAddAgain()
    {
        // arrange
        var linksDictionary= new Dictionary<string, List<string?>>();
        var linkRepository = new LinkRepository(linksDictionary);
        const string expectedVisitedLink = "http://test.com";
        linkRepository.AddVisited(new Uri(expectedVisitedLink));

        // act
        linkRepository.AddVisited(new Uri(expectedVisitedLink));
        
        // assert
        linksDictionary.Count.Should().Be(1);
        linksDictionary[expectedVisitedLink.ToUpper()].Should().BeEmpty();
    }
    
    [Fact]
    public void AddVisitedChild_WhenGivenChildLink_ShouldAddToParent()
    {
        // arrange
        var linksDictionary= new Dictionary<string, List<string?>>();
        var linkRepository = new LinkRepository(linksDictionary);
        const string expectedVisitedLink = "http://test.com";
        const string expectedChildLink = "http://test.com/child";
        linkRepository.AddVisited(new Uri(expectedVisitedLink));

        // act
        linkRepository.AddChildOfVisited(new Uri(expectedVisitedLink), new Uri(expectedChildLink));
        
        // assert
        linksDictionary[expectedVisitedLink.ToUpper()].Count.Should().Be(1);
        linksDictionary[expectedVisitedLink.ToUpper()].Should().Contain(expectedChildLink);
    }
}