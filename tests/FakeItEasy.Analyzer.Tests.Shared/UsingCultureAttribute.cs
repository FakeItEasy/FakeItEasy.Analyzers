namespace FakeItEasy.Tests.TestHelpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using Xunit.Sdk;

    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "No need to access culture name.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UsingCultureAttribute : BeforeAfterTestAttribute
    {
        private readonly string cultureName;

        private CultureInfo originalCulture = CultureInfo.CurrentCulture;
        private CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        public UsingCultureAttribute(string cultureName)
        {
            this.cultureName = cultureName;
        }

        public override void After(MethodInfo methodUnderTest)
        {
            CultureInfo.CurrentCulture = this.originalCulture;
            CultureInfo.CurrentUICulture = this.originalUiCulture;
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            this.originalCulture = CultureInfo.CurrentCulture;
            this.originalUiCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentCulture = new CultureInfo(this.cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(this.cultureName);
        }
    }
}
