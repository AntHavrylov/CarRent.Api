using CarRent.Api.Mapping;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers
{

   

    [ApiController]
    public class Users : ControllerBase
    {
        private readonly IValidator<CreateOrUpdateUserRequest> _userRequestValidator;
        private readonly IUserService _userService;

        public Users(IUserService userService,
            IValidator<CreateOrUpdateUserRequest> userRequestValidator)
        {
            _userService = userService;
            _userRequestValidator = userRequestValidator;
        }


        [HttpPost(ApiEndpoints.Users.Create)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateOrUpdateUserRequest request,
            CancellationToken token) 
        {
            await _userRequestValidator.ValidateAndThrowAsync(request,token);
            var user = request.MapToUser();
            var result = await _userService.CreateAsync(user,token);
            if (!result) 
            {
                return Conflict();
            }
            return CreatedAtAction(nameof(GetByIdAsync), new { user.Id }, user.MapToResponse());
        }

        [HttpGet(ApiEndpoints.Users.Get)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id,
            CancellationToken token)
        { 
            var user = await _userService.GetByIdAsync(id,token);
            return user is not null ? Ok(user.MapToResponse()) : NotFound();      
        }

        [HttpGet(ApiEndpoints.Users.GetAll)]
        public async Task<IActionResult> GetAllAsync(CancellationToken token)
        {
            var users = await _userService.GetAllAsync(token);
            
            
            return user is not null ? Ok(user.MapToResponse()) : NotFound();
        }

        [HttpPut(ApiEndpoints.Users.Update)]
        public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid id,
            [FromBody] CreateOrUpdateUserRequest request,
            CancellationToken token)
        {
            await _userRequestValidator.ValidateAndThrowAsync(request);
            var user = request.MapToUser(id);
            var updatedUser = await _userService.UpdateAsync(user, token);
            return updatedUser is not null ? Ok(updatedUser.MapToResponse()) : NotFound() ;
        }

        [HttpDelete(ApiEndpoints.Users.Delete)]
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id,
            CancellationToken token)
        {
            var result = await _userService.DeleteByIdAsync(id,token);
            return result ? Ok(result) : NotFound();
        }

    }
}
