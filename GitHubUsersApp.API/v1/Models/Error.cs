namespace GitHubUsersApp.API.v1.Models
{
    public class Error
    {
        public string Type { get; private set; }
        public string Description { get; private set; }

        public Error(string type, string description)
        {
            Type = type;
            Description = description;
        }
    }
}
