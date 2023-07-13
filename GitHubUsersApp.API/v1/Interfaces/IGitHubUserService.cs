using GitHubUsersApp.API.v1.Models;

namespace GitHubUsersApp.API.v1.Interfaces
{
    public interface IGitHubUserService
    {
        Task<GitHubUser> GetUserAsync(string username);

        Task<IEnumerable<GitHubUser>> GetUsersAsync(List<string> usernames);
    }
}
