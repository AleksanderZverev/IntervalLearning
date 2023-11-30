namespace Infrastructure;

public static class RandomMaster
{
    public static string GenerateSequence(int length)
    {
        var seq = new List<int>(length);

        for (var i = 0; i < length; i++)
        {
            seq.Add(ThreadSafeRandom.Random.Next(0, 10));
        }

        var randomSeq = string.Join("", seq);
        return randomSeq;
    }

    private static readonly string maxUshortString = ushort.MaxValue.ToString();

    /// <summary>
    /// If maxLength is true, then maximum 640 unique values.
    /// If not, then 2250.
    /// </summary>
    /// <param name="isMaxLength"></param>
    /// <returns></returns>
    public static short GenerateShort(bool isMaxLength = false)
    {
        var result = GetRandomDigit(maxUshortString, 0, isMaxLength);

        for (var i = 1; i < maxUshortString.Length; i++)
        {
            result *= 10;
            result += GetRandomDigit(maxUshortString, i, isMaxLength);
        }

        return (short)result;

        static int GetRandomDigit(string maxValueString, int index, bool isMaxLength)
        {
            return (short)ThreadSafeRandom.Random.Next(isMaxLength ? 1 : 0, maxValueString[index] - '0');
        }
    }
}