using FluentResults;

namespace Domain.Common.ValueObjects;

public class Counter
{
    public int Value { get; private set; }

    private Counter(int initialValue)
    {
        Value = initialValue;
    }

    public static Counter CreateEmpty()
    {
        return new Counter(0);
    }

    public static Result<Counter> Create(int initialValue)
    {
        if (initialValue < 0)
            return Result.Fail("Initial value is less than null");

        return new Counter(initialValue);
    }

    public int Increment()
    {
        return Value++;
    }

    public int Decrement()
    {
        if (Value <= 0)
            return 0;
        
        return Value--;
    }
}