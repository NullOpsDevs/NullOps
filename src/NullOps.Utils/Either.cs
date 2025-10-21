using JetBrains.Annotations;

namespace NullOps.Utils;

[PublicAPI]
public readonly struct Either<TLeft, TRight> : IEquatable<Either<TLeft, TRight>>
    where TLeft : notnull
    where TRight : notnull
{
    private readonly TLeft? left;
    private readonly TRight? right;
    private readonly EitherState state;
    
    public Either(TLeft leftValue)
    {
        left = leftValue;
        state = EitherState.Left;
    }
    
    public Either(TRight rightValue)
    {
        right = rightValue;
        state = EitherState.Right;
    }
    
    public TLeft Left => IsLeft ? left! : throw new InvalidOperationException("Left value is not set");
    public TRight Right => IsRight ? right! : throw new InvalidOperationException("Right value is not set");
    
    public bool IsLeft => state == EitherState.Left;
    public bool IsRight => state == EitherState.Right;
    public bool IsBottom => state == EitherState.Bottom;

    /// <inheritdoc />
    public bool Equals(Either<TLeft, TRight> other)
    {
        if (IsBottom && other.IsBottom)
            return true;
        
        if (IsBottom || other.IsBottom)
            return false;
        
        return IsLeft == other.IsLeft && IsRight == other.IsRight &&
               (IsLeft ? Equals(Left, other.Left) : Equals(Right, other.Right));
    }
    
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Either<TLeft, TRight> other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(state, left, right);
    }

    /// <inheritdoc />
    public override string ToString() => $"{state:G} -> {left?.ToString() ?? right?.ToString() ?? "<null>"}";
    
    public static implicit operator Either<TLeft, TRight>(TLeft left) => new(left);
    public static implicit operator Either<TLeft, TRight>(TRight right) => new(right);
    
    public static implicit operator TLeft(Either<TLeft, TRight> either) => either.Left;
    public static implicit operator TRight(Either<TLeft, TRight> either) => either.Right;

    public static bool operator ==(Either<TLeft, TRight> first, Either<TLeft, TRight> other) => first.Equals(other);
    public static bool operator !=(Either<TLeft, TRight> first, Either<TLeft, TRight> other) => !first.Equals(other);
}
