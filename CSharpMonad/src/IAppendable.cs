
namespace Monad
{
    /// <summary>
    /// Used to append/combine the current object and the one provided.
    /// This is used to support the operator+ overload on the monad types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAppendable<T>
    {
        T Append(T rhs);
    }
}
