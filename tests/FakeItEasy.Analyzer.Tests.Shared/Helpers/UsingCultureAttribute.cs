namespace FakeItEasy.Analyzer.Tests.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using Xunit.Sdk;

    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "No need to access culture name.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UsingCultureAttribute : BeforeAfterTestAttribute
    {
        private readonly CultureInfo culture;

        private CultureInfo originalCulture;
        private CultureInfo originalUiCulture;

        public UsingCultureAttribute(string cultureName)
        {
            this.culture = new CultureInfo(cultureName);
            this.originalCulture = GetCurrentCulture();
            this.originalUiCulture = GetCurrentUiCulture();
        }

        public override void After(MethodInfo methodUnderTest)
        {
            SetCurrentCulture(this.originalCulture);
            SetCurrentUiCulture(this.originalUiCulture);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            this.originalCulture = GetCurrentCulture();
            this.originalUiCulture = GetCurrentUiCulture();
            SetCurrentCulture(this.culture);
            SetCurrentUiCulture(this.culture);
        }

#if FEATURE_THREAD_CURRENTCULTURE
        private static void SetCurrentCulture(CultureInfo culture) => Thread.CurrentThread.CurrentCulture = culture;

        private static void SetCurrentUiCulture(CultureInfo culture) => Thread.CurrentThread.CurrentUICulture = culture;

        private static CultureInfo GetCurrentCulture() => Thread.CurrentThread.CurrentCulture;

        private static CultureInfo GetCurrentUiCulture() => Thread.CurrentThread.CurrentUICulture;
#else
        private static void SetCurrentCulture(CultureInfo culture) => CultureInfo.CurrentCulture = culture;

        private static void SetCurrentUiCulture(CultureInfo culture) => CultureInfo.CurrentUICulture = culture;

        private static CultureInfo GetCurrentCulture() => CultureInfo.CurrentCulture;

        private static CultureInfo GetCurrentUiCulture() => CultureInfo.CurrentUICulture;
#endif
    }
}
