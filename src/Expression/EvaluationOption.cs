using System;

namespace maskx.Expression
{
    // Summary:
    //     Provides enumerated values to use to set evaluation options.
    [Flags]
    public enum EvaluateOptions
    {
        // Summary:
        //     Specifies that no options are set.
        None = 1,

        //
        // Summary:
        //     No-cache mode. Ingores any pre-compiled expression in the cache.
        NoCache = 2,

        //
        // Summary:
        //     Treats parameters as arrays and result a set of results.
        IterateParameters = 4,
    }
}