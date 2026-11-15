using Microsoft.Extensions.Logging;
using Moq;

namespace TechStore1.Tests.Infraestructura;

// Verifica con Moq que se registró un log
public static class LoggerMockExtensions
{
    public static void VerificarLog<T>(this Mock<ILogger<T>> logger, LogLevel nivel, Times veces)
    {
        logger.Verify(l => l.Log(
                nivel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            veces);
    }

    public static void VerificarLogConExcepcion<T, TException>(this Mock<ILogger<T>> logger, LogLevel nivel)
        where TException : Exception
    {
        logger.Verify(l => l.Log(
                nivel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<TException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }
}
