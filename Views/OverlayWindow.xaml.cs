using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using TodoOverlayApp.Models;
using System.Diagnostics;

namespace TodoOverlayApp.Views
{
    public partial class OverlayWindow : Window
    {
        public ObservableCollection<TodoItem> TodoItems { get; set; }

        private Point _lastPosition;
        
        public OverlayWindow(ObservableCollection<TodoItem> todoItems)
        {
            InitializeComponent();
            TodoItems = todoItems;
            this.DataContext = this;
        }



        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}