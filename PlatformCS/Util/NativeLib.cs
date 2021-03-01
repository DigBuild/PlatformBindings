using System.Runtime.CompilerServices;
using AdvancedDLSupport;

[assembly: InternalsVisibleTo("DLSupportDynamicAssembly")]

namespace DigBuild.Platform.Util
{
    internal static class NativeLib
    {
        private static readonly NativeLibraryBuilder Builder = new();

        internal static T Get<T>() where T : class
        {
            return Builder.ActivateInterface<T>("DigBuild.Platform.Native");
        }
    }
}