namespace Toolbox.Tools;

public enum PlanMode
{
    All = 1,            // All plan(s) must be Ok
    IgnoreError,        // Run all plans, ignore all errors
    First,              // Run until the first OK
}
