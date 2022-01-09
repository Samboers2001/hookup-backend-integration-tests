using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using hookup_backend.Dtos;
using hookup_backend.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace hookup_backend_integration_tests
{

    [TestCaseOrderer("hookup_backend_integration_tests.PriorityOrderer", "hookup_backend_integration_tests")]

    public class UserControllerTests : IClassFixture<WebApplicationFactory<hookup_backend.Startup>>
    {
        private HttpClient _client;
        static private int userId;
        static bool MayDelete = false;

        public UserControllerTests(WebApplicationFactory<hookup_backend.Startup> fixture)
        {
            _client = fixture.CreateClient();
            System.Console.WriteLine($"constructor: {userId}");

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
            string content = await response.Content.ReadAsStringAsync();
            userId = Int32.Parse(content);
            System.Console.WriteLine($"register: {userId}");
            userId.Should().BeGreaterOrEqualTo(1);
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
            System.Console.WriteLine($"authenticate: {userId}");
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GET_get_user_by_id()
        {
            await POST_authenticate_user();
            var response = await _client.GetAsync("/api/user/" + userId);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var user = JsonConvert.DeserializeObject<UserReadDto>(await response.Content.ReadAsStringAsync());
            System.Console.WriteLine($"getuserbyid: {userId}");
            user.Email.Should().Be("maxverstappen@champion.com");
        }

        [Fact]
        public async Task GET_gets_all_users()
        {
            await POST_authenticate_user();
            var response = await _client.GetAsync("/api/user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var users = JsonConvert.DeserializeObject<UserReadDto[]>(await response.Content.ReadAsStringAsync());
            System.Console.WriteLine($"getall: {userId}");
            users.Should().HaveCount(4);
            DELETE_deletes_user_by_id();
        }

        public async void DELETE_deletes_user_by_id()
        {
            System.Console.WriteLine($"delete 0: {userId}");
            // await Task.Delay(5000);
            await POST_authenticate_user();
            System.Console.WriteLine($"delete 1: {userId}");
            try
            {
                var response = await _client.DeleteAsync("/api/user/" + userId);
                response.Should().Be(HttpStatusCode.NoContent);
            }
            catch (System.Exception err)
            {
                System.Console.WriteLine($"delete err: {err}");

            }
            System.Console.WriteLine($"delete 2: {userId}");
        }
    }
}
