using System;

namespace SData.Internal
{
    public sealed class LoadingException : Exception
    {
        private LoadingException() { }
        public static readonly LoadingException Instance = new LoadingException();
    }
}
