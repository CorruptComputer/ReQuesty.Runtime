namespace ReQuesty.Runtime.Abstractions;

/// <summary>
///   Model to represent only the date component of a DateTime
/// </summary>
/// <param name="dateTime">The <see cref="DateTime"/> object to create the object from.</param>
public struct Time(DateTime dateTime) : IEquatable<Time>
{
    /// <summary>
    ///   Converts the supplied <see cref="TimeOnly"/> parameter to <see cref="Time"/>.
    /// </summary>
    /// <param name="time">The <see cref="TimeOnly"/> to be converted.</param>
    /// <returns>A new <see cref="Time"/> structure whose hours, minutes, seconds and milliseconds are equal to those of the supplied time.</returns>
    public static implicit operator Time(TimeOnly time)
        => new(new DateTime(1, 1, 1, time.Hour, time.Minute, time.Second, time.Millisecond));

    /// <summary>
    ///   Converts the supplied <see cref="Time"/> parameter to <see cref="TimeOnly"/>.
    /// </summary>
    /// <param name="time">The <see cref="Time"/> to be converted.</param>
    /// <returns>A new <see cref="TimeOnly"/> structure whose hours, minutes, seconds and milliseconds are equal to those of the supplied time.</returns>
    public static implicit operator TimeOnly(Time time)
        => new(time.DateTime.Hour, time.DateTime.Minute, time.DateTime.Second, time.DateTime.Millisecond);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
    public bool Equals(Time other)
        => Hour == other.Hour && Minute == other.Minute && Second == other.Second;

    /// <inheritdoc />
    public override bool Equals(object? o)
        => (o is Time other) && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Hour, Minute, Second);
    }

    /// <summary>
    ///   Create a new Time from hours, minutes, and seconds.
    /// </summary>
    /// <param name="hour">The hour.</param>
    /// <param name="minute">The minute.</param>
    /// <param name="second">The second.</param>
    public Time(int hour, int minute, int second)
        : this(new DateTime(1, 1, 1, hour, minute, second)) { }

    /// <summary>
    ///   The <see cref="DateTime"/> representation of the class
    /// </summary>
    public DateTime DateTime { get; } = dateTime;

    /// <summary>
    ///   The hour.
    /// </summary>
    public int Hour
    {
        get
        {
            return DateTime.Hour;
        }
    }

    /// <summary>
    ///   The minute.
    /// </summary>
    public int Minute
    {
        get
        {
            return DateTime.Minute;
        }
    }

    /// <summary>
    ///   The second.
    /// </summary>
    public int Second
    {
        get
        {
            return DateTime.Second;
        }
    }

    /// <summary>
    ///   The time of day, formatted as "HH:mm:ss".
    /// </summary>
    /// <returns>The string time of day.</returns>
    public override string ToString()
    {
        return DateTime.ToString("HH\\:mm\\:ss");
    }
}

