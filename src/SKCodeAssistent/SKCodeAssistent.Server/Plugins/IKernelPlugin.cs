/*
 * NOTE: This local IKernelPlugin interface is a workshop marker interface — it has
 * no connection to Semantic Kernel's KernelPlugin type.
 *
 * In the Microsoft Agent Framework (v1.0.0), the equivalent concept is AIFunction,
 * created with AIFunctionFactory.Create(). See Services/CodingAssistentSession.cs
 * and the MIGRATION.md for the updated pattern.
 *
 * This interface is retained for workshop structural compatibility but is no longer
 * used in the active code path. Will be removed in a follow-up PR.
 */

namespace SKCodeAssistent.Server.Plugins
{
    /// <summary>
    /// Legacy marker interface — superseded by <see cref="Microsoft.Extensions.AI.AIFunction"/>.
    /// Use <c>AIFunctionFactory.Create()</c> to create tools for Agent Framework agents.
    /// </summary>
    [Obsolete("Use Microsoft.Extensions.AI.AIFunction + AIFunctionFactory.Create() instead.")]
    public interface IKernelPlugin { }
}
