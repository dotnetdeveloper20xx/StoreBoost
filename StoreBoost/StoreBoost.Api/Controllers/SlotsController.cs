using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Features.Slots.Queries.GetAvailable;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;

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

        /// <summary>
        /// Retrieves all appointment slots (both available and booked).
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all appointment slots, including their start time and booking status.
        /// Slots are returned in DTO format and wrapped in a consistent ApiResponse structure.
        /// </remarks>
        /// <returns>
        /// 200 OK with list of slots on success.  
        /// 500 Internal Server Error if something goes wrong.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SlotDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<SlotDto>>>> GetAllSlots()
        {
            var result = await _mediator.Send(new GetSlotsQuery());
            return Ok(result);
        }

        /// <summary>
        /// Retrieves only appointment slots that are available (not yet booked).
        /// </summary>
        /// <remarks>
        /// Useful for users who want to choose from open timeslots only.
        /// </remarks>
        /// <returns>200 OK with available slot list, 500 on server error.</returns>
        [HttpGet("available")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SlotDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SlotDto>>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<SlotDto>>>> GetAvailableSlots()
        {
            var result = await _mediator.Send(new GetAvailableSlotsQuery());
            return Ok(result);
        }


    }
}
