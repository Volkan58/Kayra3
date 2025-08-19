using MediatR;
using SharedLayer.Dtos;

namespace Application.Queries
{
    public class GetProductsQuery : IRequest<GetProductsResult>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }
    public class GetProductsResult
    {
        public bool IsSuccess { get; set; }
        public ProductListResponse? Products { get; set; }
        public string? ErrorMessage { get; set; }
        public static GetProductsResult Success(ProductListResponse products)
        {
            return new GetProductsResult
            {
                IsSuccess = true,
                Products = products
            };
        }
        public static GetProductsResult Failure(string errorMessage)
        {
            return new GetProductsResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
