using GitHubUsersAPI.Models;

namespace GitHubUsersAPI.Interfaces
{
    public interface IGitHubUserService
    {
        Task<GitHubUser> GetUser(string username);

        Task<IEnumerable<GitHubUser>> GetUsers(List<string> usernames);
    }
}
