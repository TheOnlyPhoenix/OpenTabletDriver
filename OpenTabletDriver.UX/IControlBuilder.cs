using System.Threading.Tasks;
using Eto.Forms;
using JetBrains.Annotations;

namespace OpenTabletDriver.UX
{
    [PublicAPI]
    public interface IControlBuilder
    {
        /// <summary>
        /// Builds a control of <see cref="T"/>.
        /// </summary>
        /// <remarks>
        /// If the control implements <see cref="IAsyncInitialize"/>, it should later be initialized with <see cref="InitializeAll"/>.
        /// </remarks>
        /// <param name="additionalDependencies">Any additional dependencies to be provided to the constructor.</param>
        /// <typeparam name="T">The control type.</typeparam>
        /// <returns>A built control of <see cref="T"/></returns>
        T Build<T>(params object[] additionalDependencies) where T : Control;

        /// <summary>
        /// Initializes all <see cref="IAsyncInitialize"/> controls that were built via <see cref="Build{T}"/>
        /// </summary>
        Task InitializeAll();
    }
}
