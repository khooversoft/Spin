
using SpinClusterApi.test.Application;
using SpinClusterApi.test.Schedule;

Console.WriteLine("Starting");

var vtest = new ClusterApiFixture();
var test = new SchedulerTests(vtest);

await test.SingleScheduleLifecycleTest1();

Console.WriteLine("Finished");