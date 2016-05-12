using System;
using System.Windows;
using MyDictionary.ViewModels;

namespace Dictionary
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowViewModel viewModel = new WindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            //Bind the view model to the view (UI).
            this.DataContext = viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}