using ProcessRunnerCore;

namespace ProcessRunner.ConcreteRunners
{
    public class MinecraftRunner : RunnerBase
    {
        public MinecraftRunner(RunnerInfo info) : base(info)
        {
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
