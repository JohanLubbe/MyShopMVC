using NewShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace NewShop.DataAccess.InMemory
{
    public class ProductCategoryRepository
    {
        ObjectCache cache = MemoryCache.Default;
        List<ProductCategory> productcategories;

        // Constructor
        public ProductCategoryRepository()
        {
            productcategories = cache["productcategories"] as List<ProductCategory>;
            if (productcategories == null)
            {
                productcategories = new List<ProductCategory>();
            }
        }

        public void Commit()
        {
            cache["productcategories"] = productcategories;
        }

        // Create the endpoint methods
        public void Insert(ProductCategory p)
        {
            productcategories.Add(p);
        }

        public void Update(ProductCategory productCategory)
        {
            ProductCategory productCategoryToUpdate = productcategories.Find(p => p.Id == productCategory.Id);

            if (productCategoryToUpdate != null)
            {
                productCategoryToUpdate = productCategory;
            }
            else
            {
                throw new Exception("Product Category could not be found to update");
            }
        }

        public ProductCategory Find(string Id)
        {
            ProductCategory productCategory = productcategories.Find(p => p.Id == Id);

            if (productCategory != null)
            {
                return (productCategory);
            }
            else
            {
                throw new Exception("Product Catgeory could not be found");
            }
        }

        public IQueryable<ProductCategory> Collection()
        {
            return productcategories.AsQueryable();
        }

        public void Delete(string Id)
        {
            ProductCategory productCategoryToDelete = productcategories.Find(p => p.Id == Id);

            if (productCategoryToDelete != null)
            {
                productcategories.Remove(productCategoryToDelete);
            }
            else
            {
                throw new Exception("Could not delete product Category");

            }

        }
    }
}
