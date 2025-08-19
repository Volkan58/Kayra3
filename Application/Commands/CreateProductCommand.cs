using MediatR;

namespace Application.Commands
{
    public class CreateProductCommand : IRequest<CreateProductResult>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

    }

    public class CreateProductResult
    {
        public bool IsSuccess { get; set; }

        public Guid? ProductId { get; set; }

        public string? ErrorMessage { get; set; }
        public static CreateProductResult Success(Guid productId)
        {
            return new CreateProductResult
            {
                IsSuccess = true,
                ProductId = productId
            };
        }
        public static CreateProductResult Failure(string errorMessage)
        {
            return new CreateProductResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

}
