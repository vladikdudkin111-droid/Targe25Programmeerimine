using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TARge25Shop.ApplicationServices.Services;
using TARge25Shop.Controllers;
using TARge25Shop.Models.OpenWeather;
using Xunit;

namespace TARge25Shop.Tests
{
    public class OpenWeatherTests
    {
        private sealed class WeatherHandler : HttpMessageHandler
        {
            public Uri? RequestUri { get; private set; }
            public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
            public string Json { get; set; } = """
                {"name":"Tallinn","sys":{"country":"EE"},"main":{"temp":8.5,"feels_like":6.2,"humidity":81,"pressure":1012},"weather":[{"description":"pilves"}],"wind":{"speed":4.1}}
                """;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestUri = request.RequestUri;
                return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(Json) });
            }
        }

        private static OpenWeatherServices Service(WeatherHandler handler, string? key = "test-key")
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string?> { ["OpenWeather:ApiKey"] = key }).Build();
            return new OpenWeatherServices(new HttpClient(handler), configuration);
        }

        [Fact]
        public async Task Service_ReadsWeatherAndEncodesCity()
        {
            var handler = new WeatherHandler();
            var result = await Service(handler).OpenWeatherResultHttpClient("Tallinn&appid=other");
            Assert.Equal(6.2, result!.Main!.FeelsLike);
            Assert.Equal("pilves", result.Weather[0].Description);
            Assert.Equal("EE", result.Sys!.Country);
            Assert.Contains("q=Tallinn%26appid%3Dother&appid=test-key", handler.RequestUri!.AbsoluteUri);
            Assert.Contains("units=metric&lang=et", handler.RequestUri.Query);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, "Linna ei leitud")]
        [InlineData(HttpStatusCode.Unauthorized, "API võti ei kehti")]
        [InlineData(HttpStatusCode.TooManyRequests, "limiit")]
        [InlineData(HttpStatusCode.InternalServerError, "ebaõnnestus")]
        public async Task Controller_ShowsSafeApiError(HttpStatusCode status, string message)
        {
            var controller = new OpenWeatherController(Service(new WeatherHandler { Status = status }));
            var view = Assert.IsType<ViewResult>(await controller.Index("Tallinn"));
            Assert.Contains(message, (string)controller.ViewBag.Error);
            Assert.Null(Assert.IsType<OpenWeatherViewModel>(view.Model).Weather);
        }

        [Fact]
        public async Task MissingKey_DoesNotSendRequest()
        {
            var handler = new WeatherHandler();
            var controller = new OpenWeatherController(Service(handler, null));
            await controller.Index("Tallinn");
            Assert.Contains("võti puudub", (string)controller.ViewBag.Error);
            Assert.Null(handler.RequestUri);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task EmptyCity_DoesNotSendRequest(string? city)
        {
            var handler = new WeatherHandler();
            await new OpenWeatherController(Service(handler)).Index(city);
            Assert.Null(handler.RequestUri);
        }

        [Theory]
        [InlineData("invalid json")]
        [InlineData("{}")] 
        public async Task InvalidResponse_ShowsError(string json)
        {
            var controller = new OpenWeatherController(Service(new WeatherHandler { Json = json }));
            await controller.Index("Tallinn");
            Assert.NotNull(controller.ViewBag.Error);
        }
    }
}
