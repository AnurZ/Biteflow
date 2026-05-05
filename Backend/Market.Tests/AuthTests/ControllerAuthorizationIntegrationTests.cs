using System.Net;
using System.Net.Http.Json;

namespace Market.Tests.AuthTests;

public sealed class ControllerAuthorizationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private const string CaptchaBypassToken = "10000000-aaaa-bbbb-cccc-000000000001";

    private readonly CustomWebApplicationFactory<Program> _factory;

    public ControllerAuthorizationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("waiter1", "waiter1", "/api/TableLayout")]
    [InlineData("string", "string", "/api/Analytics/revenue-per-day")]
    [InlineData("string", "string", "/api/inventoryitem")]
    public async Task StaffOrAdmin_ShouldAccessRepresentativeProtectedEndpoints(
        string username,
        string password,
        string url)
    {
        var client = await _factory.GetAuthenticatedClientAsync(username, password);

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [MemberData(nameof(CustomerForbiddenRequests))]
    public async Task Customer_ShouldBeForbiddenFromStaffAndAdminEndpoints(Func<HttpClient, Task<HttpResponseMessage>> send)
    {
        var client = await CreateCustomerClientAsync();

        var response = await send(client);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/Analytics/revenue-per-day")]
    [InlineData("/api/TableReservation")]
    public async Task Anonymous_ShouldBeRejectedFromProtectedEndpoints(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public static IEnumerable<object[]> CustomerForbiddenRequests()
    {
        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/MealCategory", new
                {
                    Name = "Forbidden category",
                    Description = "Customer must not create meal categories"
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/DiningTable", new
                {
                    Number = 901,
                    NumberOfSeats = 4,
                    TableType = 0,
                    TableLayoutId = 1,
                    X = 0,
                    Y = 0
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PostAsJsonAsync("/api/TableLayout", new
                {
                    Name = "Forbidden layout",
                    BackgroundColor = "#ffffff"
                }))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.GetAsync("/api/Analytics/revenue-per-day"))
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
            {
                var form = new MultipartFormDataContent();
                form.Add(new ByteArrayContent("not-an-image"u8.ToArray()), "File", "test.txt");
                return client.PostAsync("/api/File/upload", form);
            })
        };

        yield return new object[]
        {
            (Func<HttpClient, Task<HttpResponseMessage>>)(client =>
                client.PatchAsJsonAsync("/api/TableReservation/update-status", new
                {
                    Id = 1,
                    Status = 1
                }))
        };
    }

    private async Task<HttpClient> CreateCustomerClientAsync()
    {
        var email = $"customer-{Guid.NewGuid():N}@example.test";
        const string password = "customer123";
        var anonymousClient = _factory.CreateClient();

        var registerResponse = await anonymousClient.PostAsJsonAsync("/api/auth/register/customer", new
        {
            Email = email,
            Password = password,
            DisplayName = "Authorization Test Customer",
            CaptchaToken = CaptchaBypassToken
        });
        registerResponse.EnsureSuccessStatusCode();

        return await _factory.GetAuthenticatedClientAsync(email, password);
    }
}
