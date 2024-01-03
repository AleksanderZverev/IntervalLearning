namespace GlobalTools
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] 
        private static Random? local;

        /// <summary>
        /// This threads random
        /// </summary>
        public static Random Random 
            => local ??= new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
    }

    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
