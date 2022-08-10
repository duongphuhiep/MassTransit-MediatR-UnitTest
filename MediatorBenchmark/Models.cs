using MediatR;

namespace MediatorBenchmark;

public record SampleCommand(string Input) : IRequest<SampleResponse>;

public record SampleResponse(string Output);