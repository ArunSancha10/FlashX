using Quartz;

namespace outofoffice.Jobs
{
    public class ExampleJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Job executed at: {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
