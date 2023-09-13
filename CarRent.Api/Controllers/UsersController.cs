using CarRent.Api.Auth;
using CarRent.Api.Mapping;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers
{

    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IValidator<CreateOrUpdateUserRequest> _userRequestValidator;
        private readonly IUsersService _userService;

        public UsersController(IUsersService userService,
            IValidator<CreateOrUpdateUserRequest> userRequestValidator)
        {
            _userService = userService;
            _userRequestValidator = userRequestValidator;
        }

        [AllowAnonymous]
        [HttpPost(ApiEndpoints.Users.Create)]
        public async Task<IActionResult> Create([FromBody] CreateOrUpdateUserRequest request,
            CancellationToken token)
        {
            await _userRequestValidator.ValidateAndThrowAsync(request, token);
            var user = (request,Guid.NewGuid()).Adapt<User>(MapsterConfiguration.UserConfig);
            var result = await _userService.CreateAsync(user, token);
            if (!result)
            {
                return Conflict();
            }
            return CreatedAtAction(nameof(GetById), new { user.Id }, user.Adapt<UserResponse>());
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpGet(ApiEndpoints.Users.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid id,
            CancellationToken token)
        {
            var user = await _userService.GetByIdAsync(id, token);
            return user is not null ? Ok(user.Adapt<UserResponse>()) : NotFound();
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpGet(ApiEndpoints.Users.GetAll)]
        public async Task<IActionResult> GetAll(CancellationToken token)
        {
            var users = await _userService.GetAllAsync(token);
            return Ok(users.Adapt<IEnumerable<UserResponse>>());
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPut(ApiEndpoints.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id,
            [FromBody] CreateOrUpdateUserRequest request,
            CancellationToken token)
        {
            await _userRequestValidator.ValidateAndThrowAsync(request, token);
            var user = (request,id).Adapt<User>(MapsterConfiguration.UserConfig);
            var updatedUser = await _userService.UpdateAsync(user, token);
            return updatedUser is not null ? Ok(updatedUser.Adapt<UserResponse>()) : NotFound();
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Users.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            CancellationToken token)
        {
            var result = await _userService.DeleteByIdAsync(id, token);
            return result ? Ok(result) : NotFound();
        }

    }
}
