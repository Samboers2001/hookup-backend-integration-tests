using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using hookup_backend.Dtos;
using hookup_backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace hookup_backend_integration_tests
{
    public class UserControllerTests : IClassFixture<WebApplicationFactory<hookup_backend.Startup>>
    {
        private HttpClient _client;

        public UserControllerTests(WebApplicationFactory<hookup_backend.Startup> fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task POST_register_user()
        {
            var response = await _client.PostAsync("/api/user/register", new StringContent(
                JsonConvert.SerializeObject(new UserCreateDto()
                {
                    Email = "maxverstappen@champion.com",
                    Password = "RedBull1"
                }),
                Encoding.UTF8,
                "application/json"));
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task POST_authenticate_user()
        {
            var response = await _client.PostAsync("/api/user/authenticate", new StringContent(
                JsonConvert.SerializeObject(new AuthenticateRequest()
                {
                    Email = "maxverstappen@champion.com",
                    Password = "RedBull1"
                }),
                Encoding.UTF8,
                "application/json"));
            var authenticateResponse = await response.Content.ReadAsAsync<AuthenticateResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authenticateResponse.JwtToken);
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GET_gets_all_users()
        {
            await POST_authenticate_user();
            var response = await _client.GetAsync("/api/user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var user = JsonConvert.DeserializeObject<UserReadDto[]>(await response.Content.ReadAsStringAsync());
            user.Should().HaveCount(5);
        }
    }
}
