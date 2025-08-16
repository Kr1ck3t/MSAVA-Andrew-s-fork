using M_SAVA_BLL.Services;
using M_SAVA_Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace M_SAVA_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class AccessCodeController : ControllerBase
    {
        private readonly AccessCodeService _accessCodeService;

        public AccessCodeController(AccessCodeService accessCodeService)
        {
            _accessCodeService = accessCodeService;
        }

        /// <summary>
        /// Creates a new access code for a specified group.
        /// </summary>
        /// <param name="groupId">The ID of the access group.</param>
        /// <param name="expiresAt">The expiration date of the access code.</param>
        /// <param name="maxUses">The maximum number of times the code can be used.</param>
        /// <returns>The ID of the created access code.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAccessCode([FromBody] CreateAccessCodeRequest request)
        {
            try
            {
                var codeId = await _accessCodeService.CreateAccessCodeAsync(
                    request.GroupId,
                    request.ExpiresAt,
                    request.MaxUses
                );
                return Ok(new { CodeId = codeId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Redeems an access code to join a group.
        /// </summary>
        /// <param name="codeId">The ID of the access code to redeem.</param>
        /// <returns>Success message on successful redemption.</returns>
        [HttpPost("redeem/{codeId}")]
        public async Task<IActionResult> RedeemAccessCode(Guid codeId)
        {
            try
            {
                await _accessCodeService.RedeemAccessCodeAsync(codeId);
                return Ok(new { Message = "Access code redeemed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Transfers ownership of an access group to another user.
        /// </summary>
        /// <param name="groupId">The ID of the access group.</param>
        /// <param name="newOwnerId">The ID of the new owner.</param>
        /// <returns>Success message on successful transfer.</returns>
        [HttpPost("transfer-ownership")]
        public async Task<IActionResult> TransferAccessGroupOwnership([FromBody] TransferOwnershipRequest request)
        {
            try
            {
                await _accessCodeService.TransferAccessGroupOwnershipAsync(
                    request.GroupId,
                    request.NewOwnerId
                );
                return Ok(new { Message = "Group ownership transferred successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }
    }

    // Request DTOs to handle input validation
    public class CreateAccessCodeRequest
    {
        public Guid GroupId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int MaxUses { get; set; }
    }

    public class TransferOwnershipRequest
    {
        public Guid GroupId { get; set; }
        public Guid NewOwnerId { get; set; }
    }
}