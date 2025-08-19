using Infrastructure.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedLayer.Model;

namespace Application.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<CreateProductCommandHandler> _logger;
        public CreateProductCommandHandler(ProductDbContext context, ILogger<CreateProductCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün oluşturma komutu işleniyor: {ProductName}", request.Name);
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    StockQuantity = request.StockQuantity,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Products.Add(product);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Ürün başarıyla oluşturuldu: {ProductId}, {ProductName}",
                    product.Id, product.Name);
                return CreateProductResult.Success(product.Id);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Ürün oluşturulurken hata oluştu: {ProductName}", request.Name);
                return CreateProductResult.Failure("Ürün oluşturulurken bir hata oluştu.");
            }
        }
    }
}
