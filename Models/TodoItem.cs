
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TodoOverlayApp.Models
{
    public class TodoItem : BaseModel
    {
        private string id = string.Empty;
        /// <summary>
        /// Ψһ��ʶ��
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
        /// ����
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
        /// ����
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        private string? parentId = string.Empty;
        /// <summary>
        /// ���ڵ�id
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
        /// ����
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

        private bool isCompleted = false;
        /// <summary>
        /// �Ƿ����
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
                    if (isCompleted)
                    {
                        CompletedAt = DateTime.Now;
                    }
                    else
                    {
                        CompletedAt = null;
                    }
                    OnPropertyChanged(nameof(IsCompleted));
                }
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
        /// Ӧ��·��
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
        /// �Ƿ�ע�����
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
        /// ��������
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
        /// ����ʱ��
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
        /// ����ʱ��
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

        public DateTime? completedAt { get; set; } = null;
        /// <summary>
        /// ���ʱ��
        /// </summary>
        public DateTime? CompletedAt
        {
            get => completedAt;
            set
            {
                if (completedAt != value)
                {
                    completedAt = value;
                    OnPropertyChanged(nameof(CompletedAt));
                }
            }
        }

        private DateTime? startTime = null;
        /// <summary>
        /// ��ʼʱ��
        /// </summary>
        public DateTime? StartTime 
        { 
            get => startTime; 
            set 
            { 
                startTime = value; 
                OnPropertyChanged(nameof(StartTime)); 
            } 
        }

        private DateTime? reminderTime = null;
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? ReminderTime 
        { 
            get => reminderTime; 
            set 
            { 
                reminderTime = value; 
                OnPropertyChanged(nameof(ReminderTime)); 
            } 
        }
        private DateTime? endTime = null;
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? EndTime 
        { 
            get => endTime; 
            set 
            { 
                endTime = value; 
                OnPropertyChanged(nameof(EndTime)); 
            } 
        }
    }

    public enum TodoItemType
    {
        /// <summary>
        /// ��ͨ����
        /// </summary>
        [Description("��ͨ����")]
        Normal,
        /// <summary>
        /// �������
        /// </summary>
        [Description("�������")]
        App,
    }
}
