
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp.Models
{
    public class TodoItem : BaseModel
    {
        private string id = string.Empty;
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        private string name = string.Empty;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string description = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                if (description != value) { 
                    description = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Description));
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
                }
            } 
        }

        private string content = "";
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        private bool isCompleted;
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsCompleted));
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
