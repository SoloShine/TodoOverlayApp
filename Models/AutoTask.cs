using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoOverlayApp.Models
{
    public class AutoTask : BaseModel
    {
		private string id;
        /// <summary>
        /// 待办事项ID，这里要和待办事项表关联起来
        /// </summary>
		public string Id
		{
			get { return id; }
			set { id = value; OnPropertyChanged(nameof(Id)); }
		}

        private string? name = string.Empty;
        /// <summary>
        /// 自动任务名称
        /// </summary>
        public string? Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string? cron = string.Empty;
        /// <summary>
        /// Cron表达式，另外单独创建一个表，用来存储自动任务相关内容，这里后面要去掉这个字段，Cron表达式不应该放在待办事项里面
        /// </summary>
        public string? Cron
        {
            get => cron;
            set
            {
                if (cron != value)
                {
                    cron = value;
                    OnPropertyChanged(nameof(Cron));
                }
            }
        }


        private DateTime? nextExecuteTime { get; set; } = null;
        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExecuteTime
        {
            get => nextExecuteTime;
            set
            {
                nextExecuteTime = value;
                OnPropertyChanged(nameof(NextExecuteTime));
            }
        }

        //其他属性todo，需要满足若干功能，如关联待办事项、触发一些拓展能力等

        /// <summary>
        /// 更新下次执行时间
        /// </summary>
        public void UpdateNextExecuteTime()
        {
            if (string.IsNullOrWhiteSpace(Cron))
            {
                NextExecuteTime = null;
                return;
            }

            try
            {
                var cronExpression = new CronExpression(Cron);
                var nextValidTime = cronExpression.GetNextValidTimeAfter(DateTime.Now);
                NextExecuteTime = nextValidTime?.DateTime;
            }
            catch (Exception)
            {
                NextExecuteTime = null;
            }
        }


    }
}
