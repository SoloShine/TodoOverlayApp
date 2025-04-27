
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _content = "";
        private bool _isCompleted;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }

        private bool isExpanded;
        /// <summary>
        /// 是否展开节点
        /// </summary>
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        private ObservableCollection<TodoItem>? _subItems = [];
        /// <summary>
        /// 子待办项集合
        /// </summary>
        public ObservableCollection<TodoItem>? SubItems
        {
            get => _subItems;
            set
            {
                _subItems = value;
                OnPropertyChanged(nameof(SubItems));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
