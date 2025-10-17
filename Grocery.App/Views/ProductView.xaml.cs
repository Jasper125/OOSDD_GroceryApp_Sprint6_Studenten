using Grocery.App.ViewModels;

namespace Grocery.App.Views
{
    public partial class ProductView : ContentPage
    {
        private readonly ProductViewModel _viewModel;

        public ProductView(ProductViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadProducts();
        }
    }
}
