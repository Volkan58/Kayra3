using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLayer.Dtos;

namespace ProductServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IMediator mediator, ILogger<ProductController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(typeof(ProductListResponse), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductListRequest request)
        {
            try
            {
                _logger.LogInformation("Ürün listesi API çağrısı: Page={Page}, PageSize={PageSize}",
                    request.Page, request.PageSize);

                var query = new GetProductsQuery
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    SortBy = request.SortBy,
                    SortDirection = request.SortDirection
                };

                var result = await _mediator.Send(query);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Ürün listesi başarıyla alındı: {Count} ürün",
                        result.Products?.Products.Count ?? 0);
                    return Ok(result.Products);
                }

                _logger.LogWarning("Ürün listesi alınamadı: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesi alınırken beklenmeyen hata");
                return StatusCode(500, "Ürün listesi alınırken bir hata oluştu.");
            }
        }


        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CreateProductResult), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 401)]
        [ProducesResponseType(typeof(string), 409)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Ürün oluşturma API çağrısı: {ProductName}", request.Name);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("Geçersiz model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(string.Join(", ", errors));
                }

                var command = new CreateProductCommand
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,

                };

                var result = await _mediator.Send(command);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Ürün başarıyla oluşturuldu: {ProductId}, {ProductName}",
                        result.ProductId, request.Name);
                    return CreatedAtAction(nameof(GetProducts), new { id = result.ProductId }, result);
                }

                _logger.LogWarning("Ürün oluşturulamadı: {ProductName}, Hata: {Error}",
                    request.Name, result.ErrorMessage);

                if (result.ErrorMessage?.Contains("zaten") == true)
                {
                    return Conflict(result.ErrorMessage);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturulurken beklenmeyen hata: {ProductName}", request.Name);
                return StatusCode(500, "Ürün oluşturulurken bir hata oluştu.");
            }
        }
        [HttpGet("health")]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult Health()
        {
            return Ok("Product Service is running");
        }
    }
}
