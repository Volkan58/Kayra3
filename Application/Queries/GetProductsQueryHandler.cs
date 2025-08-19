using Infrastructure.Cache;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedLayer.Dtos;

namespace Application.Queries
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
    {
        private readonly ProductDbContext _context;
        private readonly IRedisCacheService _cacheService;
        private readonly ILogger<GetProductsQueryHandler> _logger;

        public GetProductsQueryHandler(ProductDbContext context, IRedisCacheService cacheService, ILogger<GetProductsQueryHandler> logger)
        {
            _context = context;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Ürün listesi sorgusu işleniyor: Page={Page}, PageSize={PageSize}",
                    request.Page, request.PageSize);

                // Cache key oluştur
                var cacheKey = GenerateCacheKey(request);

                // Cache'den veri almayı dene
                var cachedResult = await _cacheService.GetAsync<ProductListResponse>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Ürün listesi cache'den alındı: {CacheKey}", cacheKey);
                    return GetProductsResult.Success(cachedResult);
                }

                // Veritabanından veri al
                var query = _context.Products
                    .Where(p => p.IsActive);

                // Filtreleri uygula
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                           p.Description.Contains(request.SearchTerm));
                }

                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);
                }

                // Toplam sayıyı al
                var totalCount = await query.CountAsync(cancellationToken);

                // Sıralama uygula
                if (!string.IsNullOrEmpty(request.SortBy))
                {
                    query = request.SortBy.ToLower() switch
                    {
                        "name" => request.SortDirection == "desc"
                            ? query.OrderByDescending(p => p.Name)
                            : query.OrderBy(p => p.Name),
                        "price" => request.SortDirection == "desc"
                            ? query.OrderByDescending(p => p.Price)
                            : query.OrderBy(p => p.Price),
                        "createdat" => request.SortDirection == "desc"
                            ? query.OrderByDescending(p => p.CreatedAt)
                            : query.OrderBy(p => p.CreatedAt),
                        _ => query.OrderBy(p => p.CreatedAt)
                    };
                }
                else
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }

                // Sayfalama uygula
                var products = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                // DTO'ya dönüştür
                var productDtos = products.Select(p => new ProductResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                var result = new ProductListResponse
                {
                    Products = productDtos,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                // Cache'e kaydet (5 dakika)
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Ürün listesi başarıyla alındı: {Count} ürün");

                return GetProductsResult.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesi alınırken hata oluştu");
                return GetProductsResult.Failure("Ürün listesi alınırken bir hata oluştu.");
            }
        }
        private string GenerateCacheKey(GetProductsQuery request)
        {
            var keyParts = new List<string>
        {
            "products",
            $"page:{request.Page}",
            $"pageSize:{request.PageSize}"
        };

            if (!string.IsNullOrEmpty(request.SearchTerm))
                keyParts.Add($"search:{request.SearchTerm}");

            if (request.MinPrice.HasValue)
                keyParts.Add($"minPrice:{request.MinPrice}");

            if (request.MaxPrice.HasValue)
                keyParts.Add($"maxPrice:{request.MaxPrice}");

            if (!string.IsNullOrEmpty(request.SortBy))
                keyParts.Add($"sortBy:{request.SortBy}");

            if (!string.IsNullOrEmpty(request.SortDirection))
                keyParts.Add($"sortDir:{request.SortDirection}");

            return string.Join(":", keyParts);
        }
    }
}

