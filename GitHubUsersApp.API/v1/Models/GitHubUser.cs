namespace GitHubUsersApp.API.v1.Models
{
    public class GitHubUser
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Company { get; set; }
        public int FollowersCount { get; set; }
        public int RepositoryCount { get; set; }
        public decimal AveFollowersPerRepoistory => Decimal.Divide(FollowersCount , (RepositoryCount == 0 ? 1 : RepositoryCount));
        public GitHubUser()
        {
            Name = string.Empty;
            Login = string.Empty;
            Company = string.Empty;
            FollowersCount = 0;
            RepositoryCount = 0;
        }
    }
}
