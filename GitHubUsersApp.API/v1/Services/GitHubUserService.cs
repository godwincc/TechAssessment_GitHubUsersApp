using System.Net;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;
using GitHubUsersApp.API.v1.Models;
using GitHubUsersApp.API.v1.Interfaces;
using Ubiety.Dns.Core;

namespace GitHubUsersApp.API.v1.Services
{
    public class GitHubUserService : IGitHubUserService
    {
        private readonly HttpClient _client;

        public GitHubUserService(IHttpClientFactory clientFac)
        {
            _client = clientFac.CreateClient("GitHubAPI");
        }

        public async Task<GitHubUser> GetUserAsync(string username)
        {

            GitHubUser? _user = new();
            int retry = 0;
            while (retry < 5) // implement some more elegant way to retry?
            {
                try
                {

                    HttpResponseMessage _response = await _client.GetAsync("users/" + username);

                    if (_response.IsSuccessStatusCode)
                    {
                        User? _userTemp = JsonSerializer.Deserialize<User>(_response.Content.ReadAsStringAsync().Result);

                        //maybe do some business logic here
                        if (_userTemp != null)
                        {
                            _user.Login = _userTemp.login;
                            _user.Name = _userTemp.name == null ? string.Empty : _userTemp.name;
                            _user.Company = _userTemp.company == null ? string.Empty : _userTemp.company;
                            _user.FollowersCount = _userTemp.followers;
                            _user.RepositoryCount = _userTemp.public_repos;
                            break;
                        }
                        else 
                        {
                            _user = null;
                            WatchDog.WatchLogger.LogError($"Response success but User Object Deserialization returned null. \n Retrying attempt: {retry.ToString()}");
                            retry++; 
                        } //retry?

                    }
                    else if (_response.StatusCode == HttpStatusCode.NotFound) // do not retry if not found
                    {
                        _user = null;
                        break;
                    }
                    else if (_response.StatusCode >= HttpStatusCode.InternalServerError || _response.StatusCode == HttpStatusCode.RequestTimeout) // retry this
                    {
                        //log for investigation//
                        retry++;
                        WatchDog.WatchLogger.LogError($"External API threw an error:{_response.StatusCode.ToString()}. \n Retrying attempt: {retry.ToString()}" );
                    }
                    else // anything else just end
                    {
                        _user = null;
                        WatchDog.WatchLogger.LogError($"External API Return status:{_response.StatusCode.ToString()}.");
                        break;
                    }

                }
                catch (HttpRequestException ex) // retry this;
                {
                    _user = null;
                    retry++;
                    WatchDog.WatchLogger.LogError($"External API threw an error:{ex.Message}. \n Retrying attempt: {retry.ToString()}");
                }
                catch // for other exception just throw and let general handling log this
                {
                    throw;
                }
                finally { }
            }

            return _user;
        }

        public async Task<IEnumerable<GitHubUser>> GetUsersAsync(List<string> usernames)
        {
            ICollection<GitHubUser>? _users = new HashSet<GitHubUser>();

            GitHubUser? _user;

            foreach (var username in usernames)
            {
                _user = await GetUserAsync(username);
                if (_user != null)
                {
                    _users.Add(_user);
                }
            }

            if (_users.Count <= 0)
            {
                _users = null;
            }

            return _users;
        }

        private class User
        {
            public string login { get; set; }
            public int id { get; set; }
            public string node_id { get; set; }
            public string avatar_url { get; set; }
            public string gravatar_id { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string followers_url { get; set; }
            public string following_url { get; set; }
            public string gists_url { get; set; }
            public string starred_url { get; set; }
            public string subscriptions_url { get; set; }
            public string organizations_url { get; set; }
            public string repos_url { get; set; }
            public string events_url { get; set; }
            public string received_events_url { get; set; }
            public string type { get; set; }
            public bool site_admin { get; set; }
            public string name { get; set; }
            public string company { get; set; }
            public string blog { get; set; }
            public object location { get; set; }
            public object email { get; set; }
            public object hireable { get; set; }
            public object bio { get; set; }
            public object twitter_username { get; set; }
            public int public_repos { get; set; }
            public int public_gists { get; set; }
            public int followers { get; set; }
            public int following { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }



    }
}
