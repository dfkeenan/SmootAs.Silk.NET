using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SmoothAs.Silk.NET.Vulkan;

[Serializable]
public class InvalidVulkanApiResultException : VulkanException
{
    public InvalidVulkanApiResultException(Result result)
    {
        Result = result;
    }

    public InvalidVulkanApiResultException(Result result, string message) : base(message)
    {
        Result = result;
    }

    protected InvalidVulkanApiResultException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        Result = (Result)info.GetValue(nameof(Result), typeof(Result))!;
        ResultExpression = info.GetString(nameof(ResultExpression));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        info.AddValue(nameof(Result), Result);

        info.AddValue(nameof(ResultExpression), ResultExpression);

        base.GetObjectData(info, context);
    }

    public Result Result { get; }

    public string? ResultExpression { get; set; }

    public static void ThrowIfInvalid(Result result, string? message = null, [CallerArgumentExpression("result")] string? resultExpression = null)
    {
        if (result == Result.Success) return;

        throw new InvalidVulkanApiResultException(result, message ?? $"Result is not {Result.Success}. Result was {result}. ") { ResultExpression = resultExpression };
    }
}