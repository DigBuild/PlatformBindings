using System.Runtime.CompilerServices;
using AdvancedDLSupport;

[assembly: InternalsVisibleTo("DLSupportDynamicAssembly")]

namespace DigBuildPlatformCS.Util
{
    internal static class NativeLib
    {
        private static readonly NativeLibraryBuilder Builder = new();

        internal static T Get<T>() where T : class
        {
            return Builder.ActivateInterface<T>("DigBuildPlatformCPP");
        }
    }
}