using System;

namespace Utility.Assertions
{
    public static class Assert
    {
        public static void IsNotNull<T>(T value, string message = "")
        {
            if (value == null) throw new NullReferenceException(message);
        }
    }
}