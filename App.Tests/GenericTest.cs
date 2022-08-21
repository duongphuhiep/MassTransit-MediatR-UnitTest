using MassTransit;
using NSubstitute;
using System.Reflection;

namespace App.Tests
{
    public record BaseResponse
    {
        public string Some { get; init; }
        public string Thing { get; init; }
    }

    public interface IRequest<T> where T : class { }
    public interface IRequest2<T> where T : class { }
    public interface IRequest3 { }

    public record MyResponse1 : BaseResponse { }
    public class MyCommand1 : IRequest<MyResponse1>, IRequest2<MyResponse2>, IRequest3 { }

    public record MyResponse2 : BaseResponse { }
    public class MyCommand2 : IRequest<MyResponse2> { }


    public class Middleware<T> : IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
            throw new NotImplementedException();
        }

        public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var requestType = (TypeInfo)typeof(T);
            var requestInterface = requestType.ImplementedInterfaces.FirstOrDefault(
                p => p.FullName != null
                && p.IsAssignableTo(typeof(IRequest<>))
            );
            var responseType = requestInterface?.GenericTypeArguments.FirstOrDefault();
            if (responseType.BaseType == typeof(BaseResponse) && HasDefaultConstructor(responseType))
            {
                var response = (BaseResponse)Activator.CreateInstance(responseType) with
                {
                    Some = "a",
                    Thing = "b"
                };

                Console.WriteLine(response);
            }
            return Task.CompletedTask;
        }

        private static bool HasDefaultConstructor(Type type)
        {
            return type.GetConstructors().Any(t => !t.GetParameters().Any());
        }
    }

    public class GenericTest
    {
        [Fact]
        public void Try1()
        {
            //Arrange
            var context = Substitute.For<ConsumeContext<MyCommand1>>();
            context.Message.Returns(new MyCommand1());
            var subject = new Middleware<MyCommand1>();

            //Act
            subject.Send(context, null);


        }
    }
}
