using App.Models;

namespace App.Services
{
    public class ProductService : List<ProductModel>
    {
        public ProductService()
        {
            this.AddRange(new ProductModel[] {
                new ProductModel(){Id = 1, Name = "Ipoho",Price = 100},
                new ProductModel(){Id = 2, Name = "Xiaomi",Price = 200},
                new ProductModel(){Id = 3, Name = "Samsung",Price = 300},
                new ProductModel(){Id = 4, Name = "Google",Price = 400}
            });
        }
    }
}