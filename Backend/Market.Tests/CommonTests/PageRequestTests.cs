using Market.Application.Common;

namespace Market.Tests.CommonTests;

public sealed class PageRequestTests
{
    [Fact]
    public void Defaults_ShouldUseFirstPageAndDefaultPageSize()
    {
        var request = new PageRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(10, request.PageSize);
        Assert.Equal(0, request.SkipCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-25)]
    public void Page_ShouldNormalizeNonPositiveValuesToFirstPage(int page)
    {
        var request = new PageRequest { Page = page };

        Assert.Equal(1, request.Page);
        Assert.Equal(0, request.SkipCount);
    }

    [Fact]
    public void SkipCount_ShouldNeverBeNegativeForInvalidPageInput()
    {
        var request = new PageRequest { Page = -3, PageSize = 25 };

        Assert.Equal(1, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.True(request.SkipCount >= 0);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-5, 10)]
    [InlineData(101, 100)]
    [InlineData(50, 50)]
    public void PageSize_ShouldDefaultOrClampInvalidValues(int pageSize, int expected)
    {
        var request = new PageRequest { PageSize = pageSize };

        Assert.Equal(expected, request.PageSize);
    }
}
