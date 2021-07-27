namespace ProcessRunner.ConcreteRunners
{
    public class MinecraftRunner : RunnerBase
    {
        public MinecraftRunner(string fileName) : base(fileName)
        {
        }

        public MinecraftRunner(string fileName, string processName) : base(fileName, processName)
        {
        }

        protected override void OnStop()
        {
            base.OnStop();
        }
    }
}
