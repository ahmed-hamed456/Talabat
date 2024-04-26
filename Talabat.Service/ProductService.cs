using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Product_Specs;
using Talabat.Core.Services.Contract;

namespace Talabat.Service
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(ProductSpecParams specParams)
        {
            var spec = new ProductWithBrandWithCategorySpecification(specParams);

            var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(spec);

            return products;
        }
        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            var spec = new ProductWithBrandWithCategorySpecification(productId);

            var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(spec);

            return product;
        }
        public async Task<int> GetCountAsync(ProductSpecParams specParams)
        {
            var countSpec = new ProductWithFilterationWithCountSpecification(specParams);

            var count = await _unitOfWork.Repository<Product>().GetCountAsync(countSpec);

            return count;
        }
        public async Task<IReadOnlyList<ProductBrand>> GetBrandsAsync()
         => await _unitOfWork.Repository<ProductBrand>().GetAllAsync();


        public async Task<IReadOnlyList<productCategory>> GetCategoriesAsync()
            => await _unitOfWork.Repository<productCategory>().GetAllAsync();

    }
}
