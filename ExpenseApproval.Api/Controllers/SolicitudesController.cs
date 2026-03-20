using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Application.Interfaces;
using ExpenseApproval.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ExpenseApproval.Api.Controllers
{
    [Route("api/solicitudes")]
    [ApiController]
    public class SolicitudesController : ControllerBase
    {
        private readonly IExpenseRequestService _service;

        public SolicitudesController(IExpenseRequestService service) => _service = service;

        [HttpGet]
        [SwaggerOperation(Summary = "Lista solicitudes con filtros opcionales")]
        [ProducesResponseType(typeof(IReadOnlyList<ExpenseRequestReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<ExpenseRequestReadDto>>> Get(
        [FromQuery] RequestStatus? estado,
        [FromQuery] string? categoria,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        CancellationToken ct)
        {
            var query = new ExpenseRequestQuery
            {
                Status = estado,
                Category = categoria,
                DateFrom = fechaDesde,
                DateTo = fechaHasta
            };

            var result = await _service.GetAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Obtiene una solicitud por id")]
        [ProducesResponseType(typeof(ExpenseRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExpenseRequestReadDto>> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Crea una nueva solicitud de gasto")]
        [ProducesResponseType(typeof(ExpenseRequestReadDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ExpenseRequestReadDto>> Create([FromBody] ExpenseRequestCreateDto dto, CancellationToken ct)
        {
            var created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Actualiza una solicitud (solo si está Pendiente)")]
        [ProducesResponseType(typeof(ExpenseRequestReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ExpenseRequestReadDto>> Update([FromRoute] Guid id, [FromBody] ExpenseRequestUpdateDto dto, CancellationToken ct)
        {
            var updated = await _service.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        [HttpPost("{id:guid}/approve")]
        [SwaggerOperation(Summary = "Aprueba una solicitud (solo si está Pendiente) y registra fecha de decisión")]
        [ProducesResponseType(typeof(ExpenseRequestReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExpenseRequestReadDto>> Approve([FromRoute] Guid id, [FromBody] DecisionDto dto, CancellationToken ct)
        {
            var result = await _service.ApproveAsync(id, dto, ct);
            return Ok(result);
        }

        [HttpPost("{id:guid}/reject")]
        [SwaggerOperation(Summary = "Rechaza una solicitud (solo si está Pendiente) y registra fecha de decisión")]
        [ProducesResponseType(typeof(ExpenseRequestReadDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExpenseRequestReadDto>> Reject([FromRoute] Guid id, [FromBody] RejectDto dto, CancellationToken ct)
        {
            var result = await _service.RejectAsync(id, dto, ct);
            return Ok(result);
        }

        [HttpGet("metrics")]
        [SwaggerOperation(Summary = "Métricas agregadas de solicitudes")]
        [ProducesResponseType(typeof(ExpenseRequestMetricsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExpenseRequestMetricsDto>> Metrics(CancellationToken ct)
        {
            var result = await _service.GetMetricsAsync(ct);
            return Ok(result);
        }
    }
}
