using GitHubUsersApp.API.v1.Services;
using GitHubUsersApp.API.v1.Interfaces;
using GitHubUsersApp.API.v1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;

namespace GitHubUsersApp.API.v1.Controllers
{
    [ApiController]
    public class GitHubUserController : ControllerBase
    {
        private IGitHubUserService _service;

        public GitHubUserController(IGitHubUserService service)
        {
            _service = service;
        }

        /// <summary>
        /// This endpoint takes a List<string> usernames: This is a list of github usernames that will be used
        /// to look up basic information from GitHub's public API.
        /// </summary>
        /// <param name="usernames"></param>
        /// <returns>List<GitHubUser> json objects</GitHubUser></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("api/v{version:apiVersion}/retrieveUsers")]
        [ApiVersion("1")]
        public async Task<ActionResult<IEnumerable<GitHubUser>>> retrieveUsers([FromQuery] List<string> usernames)
        {

            if (usernames.Count <= 0) 
            {
                return BadRequest(new Error("Missing Parameter", "usernames parameter is empty"));
            }

            IEnumerable<GitHubUser> gitHubUsers = await _service.GetUsersAsync(usernames.Distinct().ToList());

            if (gitHubUsers == null) 
            {
                return NotFound(new Error("No data", "no users found"));
            }

            IEnumerable<GitHubUser> _usersArranged = gitHubUsers.OrderBy(p => p.Name);

            return Ok(_usersArranged);
        }


    }

}
