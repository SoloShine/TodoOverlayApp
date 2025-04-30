
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp.Models
{
    public class TodoItem : BaseModel
    {
        private string _id = string.Empty;
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        private string _name = string.Empty;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Name));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }
        private string _description = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value) { 
                    _description = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Description));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }
        private string? parentId = string.Empty;
        /// <summary>
        /// 父节点id
        /// </summary>
        public string? ParentId 
        { 
            get => parentId; 
            set 
            {
                if (parentId != value)
                {
                    parentId = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(ParentId));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            } 
        }

        private string _content = "";
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Content));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private bool _isCompleted;
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsCompleted));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private bool isExpanded = false;
        /// <summary>
        /// 是否展开节点
        /// </summary>
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsExpanded));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private string? appPath = string.Empty;
        /// <summary>
        /// 应用路径
        /// </summary>
        public string? AppPath
        {
            get => appPath;
            set
            {
                if (appPath != value)
                {
                    appPath = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(AppPath));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private bool isInjected = false;
        /// <summary>
        /// 是否注入软件
        /// </summary>
        public bool IsInjected
        {
            get => isInjected;
            set
            {
                if (isInjected != value)
                {
                    isInjected = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsInjected));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private TodoItemType todoItemType = TodoItemType.Normal;
        /// <summary>
        /// 待办类型
        /// </summary>
        public TodoItemType TodoItemType
        {
            get => todoItemType;
            set
            {
                if (todoItemType != value)
                {
                    todoItemType = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(TodoItemType));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private DateTime? createdAt = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? GreadtedAt
        {
            get => createdAt;
            set
            {
                if (createdAt != value)
                {
                    createdAt = value;
                    OnPropertyChanged(nameof(GreadtedAt));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

        private DateTime? updatedAt = DateTime.Now;
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt
        {
            get => updatedAt;
            set
            {
                if (updatedAt != value)
                {
                    updatedAt = value;
                    OnPropertyChanged(nameof(UpdatedAt));
                    if (!string.IsNullOrEmpty(Id)) App.TodoItemRepository.UpdateAsync(this).ConfigureAwait(false);
                }
            }
        }

    }

    public enum TodoItemType
    {
        /// <summary>
        /// 普通待办
        /// </summary>
        [Description("普通待办")]
        Normal,
        /// <summary>
        /// 软件待办
        /// </summary>
        [Description("软件待办")]
        App,
    }
}
