namespace Photinizer.Utilities;

public static class IdGenerator<T>
{
    public static int NewId
    {
        get
        {
            while (true)
            {
                // Read current, compute next, normalize to positive range
                int current = Volatile.Read(ref field);
                int next = current + 1;
                if (next <= 0) next = 1;

                // CAS: if no one changed field since 'current', publish 'next'
                if (Interlocked.CompareExchange(ref field, next, current) == current)
                    return next;
            }
        }
    }
}
