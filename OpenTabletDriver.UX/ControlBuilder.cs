using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eto.Forms;
using JetBrains.Annotations;

namespace OpenTabletDriver.UX
{
    public class ControlBuilder : IControlBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ControlBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly Queue<IAsyncInitialize> _builtControls = new Queue<IAsyncInitialize>();

        public T Build<T>(params object[] additionalDependencies) where T : Control
        {
            var control = _serviceProvider.CreateInstance<T>(additionalDependencies);
            if (control is IAsyncInitialize asyncInitialize)
                _builtControls.Enqueue(asyncInitialize);

            return control;
        }

        public async Task InitializeAll()
        {
            while (_builtControls.TryDequeue(out var control))
                await control.InitializeAsync();
        }
    }
}
