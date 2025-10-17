using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using Grocery.App.Views;
using System.Collections.ObjectModel;

namespace Grocery.App.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;

        public ObservableCollection<Product> Products { get; set; }
        public Command AddNewProductCommand { get; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;
            Products = new ObservableCollection<Product>();

            AddNewProductCommand = new Command(async () =>
                await Shell.Current.GoToAsync(nameof(NewProductView)));

            LoadProducts();
        }

        public void LoadProducts()
        {
            Products.Clear();

            var allProducts = _productService.GetAll();
            foreach (var product in allProducts)
            {
                Products.Add(product);
            }
        }
    }
}
