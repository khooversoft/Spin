namespace Toolbox.Tools;

public enum PlanMode
{
    // All plan(s) must be Ok
    All = 1,

    // Run all plans, ignore all errors
    IgnoreError,

    // Run until the first OK
    First,
}
