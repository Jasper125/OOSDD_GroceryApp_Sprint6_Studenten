using Grocery.Core.Models;
using Grocery.Core.Services;
using System.Windows.Input;

namespace Grocery.App.ViewModels
{
    public class NewProductViewModel : BaseViewModel
    {
        private readonly ProductService _productService;

        public string Name { get; set; }
        public int Stock { get; set; }
        public DateTime ShelfLife { get; set; } = DateTime.Now;
        public decimal Price { get; set; }

        public ICommand SaveCommand { get; }

        public NewProductViewModel(ProductService productService)
        {
            _productService = productService;
            SaveCommand = new Command(SaveProduct);
        }

        private void SaveProduct()
        {
            var newProduct = new Product(
                0,
                Name,
                Stock,
                DateOnly.FromDateTime(ShelfLife),
                Price
            );

            _productService.Add(newProduct);
            Shell.Current.GoToAsync("..");
        }
    }
}
