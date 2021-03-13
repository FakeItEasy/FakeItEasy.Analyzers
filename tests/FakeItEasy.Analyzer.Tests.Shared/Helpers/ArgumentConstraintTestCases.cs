namespace FakeItEasy.Analyzer.Tests.Helpers
{
    using System;
    using Xunit;

    public static class ArgumentConstraintTestCases
    {
        /// <summary>
        /// Create test cases from the given argument constraints.
        /// </summary>
        /// <param newm="constraints">
        /// List of constraints to use as a seed. Each constraint must use the "A"
        /// argument constraint-building entry point. Test cases using "An" will
        /// be built to augment these.
        /// </param>
        public static TheoryData<string> From(params string[] constraints)
        {
            var theoryData = new TheoryData<string>();
            foreach (var constraint in constraints)
            {
                ValidateConstraint(constraint);

                theoryData.Add(constraint);
            }

            return theoryData;
        }

        private static void ValidateConstraint(string constraint)
        {
            if (constraint.Length < 2 ||
                constraint[0] != 'A' ||
                (constraint[1] != '<' && constraint[1] != '('))
            {
                throw new ArgumentException(
                    $"Constraint '{constraint}' is not an argument constraint built using the 'A' entry point.",
                    nameof(constraint));
            }
        }
    }
}
