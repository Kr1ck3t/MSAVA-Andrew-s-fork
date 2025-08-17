using M_SAVA_BLL.Services;
using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [Route("api/accesscodes")]
    [ApiController]
    [Authorize]
    public class AccessCodeController : ControllerBase
    {
        private readonly AccessCodeService _accessCodeService;

        public AccessCodeController(AccessCodeService accessCodeService)
        {
            _accessCodeService = accessCodeService ?? throw new ArgumentNullException(nameof(accessCodeService));
        }

        [HttpPost("create")]
        public async Task<ActionResult<Guid>> CreateAccessCode(
            [FromQuery][Required] int maxUses,
            [FromQuery][Required] DateTime expiresAt,
            [FromQuery][Required] Guid accessGroupId,
            [FromBody][Required] AccessGroupDB accessGroup)
        {
            Guid id = await _accessCodeService.CreateNewAccessCode(maxUses, expiresAt, accessGroupId, accessGroup);
            return Ok(id);
        }

        [HttpPost("redeem/{codeId:guid}")]
        public async Task<ActionResult> RedeemAccessCode(Guid codeId)
        {
            await _accessCodeService.JoinGroupWithAccessCode(codeId);
            return Ok("joind group successfuly");
        }

        [HttpPost("redeem-subgroup/{codeId:guid}")]
        public async Task<ActionResult> RedeemAccessCodeForSubgroup(Guid codeId)
        {
            await _accessCodeService.AddSubgroupAsync(codeId);
            return Ok("Subgroup added successfully via access code.");
        }


        [HttpPost("transfer-ownership")]
        public async Task<ActionResult> TransferOwnership([FromQuery] Guid groupId, [FromQuery] Guid newOwnerId)
        {
            await _accessCodeService.TransferOwnershipWithAccessCode(groupId, newOwnerId);
            return Ok("Ownership transferred successfully.");
        }


    }
}

    /*[HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guid>> CreateInviteCode(
            [FromQuery][Required] int maxUses, 
            [FromQuery][Required] int expiresInHours)
        {
            
            Guid id = await _inviteCodeService.CreateNewInviteCode(maxUses, expiresAt);
            return Ok(id);
        }*/


/*Your job is to implement access codes, codes which let a user join 
an access group. These access codes can also be used to add that access 
group as a subgroup to another access group (pay close attention to exactly 
what that means). They can only be issued by the owner of the access group. 
You are also to implement the owner letting another user become the owner,
 i.e. transferring ownership. 
You'll have to make a brand new controller and service for all this.*/