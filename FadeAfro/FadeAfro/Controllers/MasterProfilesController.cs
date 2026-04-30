using FadeAfro.Application.Features.MasterProfiles.DismissMaster;
using FadeAfro.Application.Features.MasterProfiles.GetAllMasterProfiles;
using FadeAfro.Application.Features.MasterProfiles.GetMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.GetMasterProfilePhoto;
using FadeAfro.Application.Features.MasterProfiles.GetMyMasterProfile;
using FadeAfro.Application.Features.MasterProfiles.SetAsMaster;
using FadeAfro.Application.Features.MasterProfiles.UpdateMasterProfileDescription;
using FadeAfro.Application.Features.MasterProfiles.UploadMasterProfilePhoto;
using FadeAfro.Domain.Constants;
using FadeAfro.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FadeAfro.Controllers;

[ApiController]
[Route("api/master-profiles")]
[Tags("Master Profiles")]
[Authorize]
public class MasterProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MasterProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("get/all")]
    [SwaggerOperation(Summary = "Get all master profiles")]
    public async Task<IActionResult> GetMasterProfiles()
    {
        var getAllMasterProfilesQuery = new GetAllMasterProfilesQuery();
        
        var response = await _mediator.Send(getAllMasterProfilesQuery);
        return Ok(response);
    }
    
    [HttpGet("get/{masterProfileId:guid}")]
    [SwaggerOperation(Summary = "Get master's profile")]
    public async Task<IActionResult> GetMasterProfile(Guid masterProfileId)
    {
        var getMasterProfileQuery = new GetMasterProfileQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterProfileQuery);
        return Ok(response);
    }
    
    [HttpGet("get/me")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Get my master profile")]
    public async Task<IActionResult> GetMyMasterProfile()
    {
        var getMyMasterProfileQuery = new GetMyMasterProfileQuery(
            User.GetUserId());
        
        var response = await _mediator.Send(getMyMasterProfileQuery);
        return Ok(response);
    }
    
    [HttpGet("get/{masterProfileId:guid}/photo")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Get master profile photo")]
    public async Task<IActionResult> GetMasterPhoto(Guid masterProfileId)
    {
        var getMasterPhotoQuery =  new GetMasterProfilePhotoQuery(masterProfileId);
        
        var response = await _mediator.Send(getMasterPhotoQuery);
        return File(response.Stream, response.ContentType);
    }
    
    [HttpPut("update/me/description")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(Summary = "Update my master profile description")]
    public async Task<IActionResult> UpdateMyMasterProfile([FromBody] UpdateMasterProfileDescriptionRequest request)
    {
        var updateMasterProfileDescriptionCommand = new UpdateMasterProfileDescriptionCommand(
            User.GetUserId(),
            request.Description);
        
        await _mediator.Send(updateMasterProfileDescriptionCommand);
        return NoContent();
    }
    
    [HttpPost("upload/me/photo")]
    [Authorize(Roles = Roles.Master)]
    [SwaggerOperation(
        Summary = "Upload my master profile photo",
        Description = "Uploads a photo for the master profile. Allowed formats: JPEG, PNG, WebP. Max size: 5 MB.")]
    public async Task<IActionResult> UploadMyMasterProfilePhoto(IFormFile file)
    {
        var uploadMasterPhotoCommand = new UploadMasterProfilePhotoCommand(
            User.GetUserId(),
            file.OpenReadStream(),
            Path.GetExtension(file.FileName),
            file.Length);
        
        await _mediator.Send(uploadMasterPhotoCommand);
        return NoContent();
    }

    [HttpPost("assign/{userId:guid}")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(
        Summary = "Set user as master",
        Description = "Assigns the Master role to an existing user and creates an associated master profile.")]
    public async Task<IActionResult> AssignMaster(Guid userId)
    {
        var setAsMasterCommand = new SetAsMasterCommand(userId);
        
        await _mediator.Send(setAsMasterCommand);
        return Ok();
    }
    
    [HttpDelete("dismiss/{masterId:guid}")]
    [Authorize(Roles = Roles.Owner)]
    [SwaggerOperation(
        Summary = "Dismiss master",
        Description = "Revokes the Master role and deletes the master profile.")]
    public async Task<IActionResult> DismissMaster(Guid masterId)
    {
        var dismissMasterCommand = new DismissMasterCommand(masterId);
        
        await _mediator.Send(dismissMasterCommand);
        return NoContent();
    }
}
