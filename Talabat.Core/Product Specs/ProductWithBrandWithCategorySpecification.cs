using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Core.Product_Specs
{
    public class ProductWithBrandWithCategorySpecification : BaseSpecifications<Product>
    {
        public ProductWithBrandWithCategorySpecification(ProductSpecParams specParams) 
            :base( P =>
                    (string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search))&&
                    (!specParams.BrandId.HasValue || P.BrandId == specParams.BrandId) &&
                    (!specParams.CategoryId.HasValue || P.CategoryId == specParams.CategoryId)

                 )
        {
            AddIncludes();

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "priceAsc":
                        AddOrderBy(p=>p.Price); 
                        break;
                    case "priceDesc":
                        AddOrderByDesc(p => p.Price);
                        break;
                    default: 
                        AddOrderBy(p => p.Name);
                        break;
                }
            }
            else
                AddOrderBy(p => p.Name);

            ApplyPagenation((specParams.PageIndex - 1) * specParams.PageSize,specParams.PageSize);
        }

        public ProductWithBrandWithCategorySpecification(int id)
            : base(P=>P.Id==id)
        {
            AddIncludes();
        }

        private void AddIncludes()
        {
            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);
        }
    }
}
