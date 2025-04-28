
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        public TodoItem()
        {
            // ȷ��ÿ��TodoItem����ΨһID
            Id = Guid.NewGuid().ToString();
        }
        // Ψһ��ʶ��
        public string Id { get; set; }

        private string _content = "";
        private bool _isCompleted;

        /// <summary>
        /// ���ڵ�id
        /// </summary>
        public string ParentId { get; set; }


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

        private bool isExpanded = false;
        /// <summary>
        /// �Ƿ�չ���ڵ�
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

        private ObservableCollection<TodoItem> _subItems = [];
        /// <summary>
        /// �Ӵ������
        /// </summary>
        public ObservableCollection<TodoItem> SubItems
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
