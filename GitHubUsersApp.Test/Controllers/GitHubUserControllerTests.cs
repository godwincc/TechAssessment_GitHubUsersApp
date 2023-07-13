using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoFixture;
using FluentAssertions;
using GitHubUsersApp.API.v1.Controllers;
using GitHubUsersApp.API.v1.Interfaces;
using GitHubUsersApp.API.v1.Models;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI.Common;
using Assert = Xunit.Assert;

namespace GitHubUsersApp.Test.Controllers
{
    public class GitHubUserControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IGitHubUserService> _serviceMock;
        private readonly GitHubUserController _contrl;

        private readonly Random rand = new Random();
        public GitHubUserControllerTests()
        {
            _fixture = new Fixture();
            _serviceMock = _fixture.Freeze<Mock<IGitHubUserService>>();
            _contrl = new GitHubUserController(_serviceMock.Object);
        }

        

        [Fact]
        public async Task retrieveUsers_ReturnOk_WhenDataIsFound()
        {
            //-Arrange-
            var _usernames = _fixture.Create<List<string>> ();
            
            var _response = _fixture.Create<IEnumerable<GitHubUser>>();
            _serviceMock.Setup(x => x.GetUsersAsync(_usernames)).ReturnsAsync(_response);

            //-Act-
            var _result = await _contrl.retrieveUsers(_usernames).ConfigureAwait(false);

            //-Assert-
            Xunit.Assert.IsType<OkObjectResult>(_result.Result);
            Xunit.Assert.IsAssignableFrom<ActionResult<IEnumerable<GitHubUser>>>(_result);
        }

        [Fact]
        public async Task retrieveUsers_ReturnBad_WhenParameterIsMissing()
        {
            //-Arrange-
            List<string> _usernames = new List<string>(); //empty string

            var _response = _fixture.Create<IEnumerable<GitHubUser>>();
            _serviceMock.Setup(x => x.GetUsersAsync(_usernames)).ReturnsAsync(_response);

            //-Act-
            var _result = await _contrl.retrieveUsers(_usernames).ConfigureAwait(false);

            //-Assert-
            Xunit.Assert.IsType<BadRequestObjectResult>(_result.Result);

        }

        [Fact]
        public async Task retrieveUsers_ReturnNotFound_WhenNoDataIsRetrieved()
        {
            //-Arrange-
            var _usernames = _fixture.Create<List<string>>();

            IEnumerable<GitHubUser> _response = null;
            _serviceMock.Setup(x => x.GetUsersAsync(_usernames)).ReturnsAsync(_response);

            //-Act-
            var _result = await _contrl.retrieveUsers(_usernames).ConfigureAwait(false);

            //-Assert-
            Xunit.Assert.IsType<NotFoundObjectResult>(_result.Result);
        }

       
    }
}
