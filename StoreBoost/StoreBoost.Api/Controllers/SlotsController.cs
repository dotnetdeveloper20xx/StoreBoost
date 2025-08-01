using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Common.Models;

namespace StoreBoost.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SlotsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SlotsController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Book a specific appointment slot if available.
        /// </summary>
        /// <param name="id">The unique ID of the slot.</param>
        /// <returns>Success status and message.</returns>
        [HttpPost("{id:guid}/book")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<bool>>> BookSlot([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new BookSlotCommand(id));

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
