
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using TodoOverlayApp.Models;

namespace TodoOverlayApp.Services.Scheduler
{
    public class TodoItemSchedulerService
    {
        private IScheduler _scheduler;

        public TodoItemSchedulerService()
        {
            InitializeScheduler().Wait();
        }

        public async Task ShutdownAsync()
        {
            if (_scheduler != null && !_scheduler.IsShutdown)
            {
                await _scheduler.Shutdown();
            }
        }

        private async Task InitializeScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();
        }

        public async Task ScheduleTodoItemReminder(AutoTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Cron))
                return;

            IJobDetail job = JobBuilder.Create<TodoItemReminderJob>()
                .WithIdentity(task.Id, "taskReminders")
                .UsingJobData("taskId", task.Id)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{task.Id}_trigger", "taskReminders")
                .WithCronSchedule(task.Cron)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task UnscheduleTodoItemReminder(string taskId)
        {
            await _scheduler.DeleteJob(new JobKey(taskId, "taskReminders"));
        }
    }

    public class TodoItemReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var taskId = context.JobDetail.JobDataMap.GetString("taskId");

            // 从数据库获取task
            var task = await App.AutoTaskRepository.GetByIdAsync(taskId);
            if (task == null) return;

            // 触发定时任务
            App.Current.Dispatcher.Invoke(() =>
            {
                var notification = new HandyControl.Controls.Notification
                {
                    Title = "定时任务触发",
                    Content = $"todo",

                };
                notification.Show();
            });
            //其他todo
            await Console.Out.WriteLineAsync($"Reminder triggered for todo item: {task.Name}");
        }
    }
}
