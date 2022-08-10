using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MediatorBenchmark;

var summary = BenchmarkRunner.Run(typeof(Bench).Assembly);

//[InliningDiagnoser]
//[EtwProfiler]
//[TailCallDiagnoser]
// [ConcurrencyVisualizerProfiler]
// [NativeMemoryProfiler]
// [ThreadingDiagnoser]
[MemoryDiagnoser]
public class Bench
{
    private readonly MediatRMediatorRunner mrRunner = new();
    private readonly MassTransitMediatorRunner mtRunner = new();

    [Benchmark]
    public async Task UseMediateR()
    {
        await mrRunner.Run();
    }

    [Benchmark]
    public async Task UseMassTransit()
    {
        await mtRunner.Run();
    }
}