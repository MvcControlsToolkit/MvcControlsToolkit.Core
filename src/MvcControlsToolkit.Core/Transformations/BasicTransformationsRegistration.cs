using System;

using MvcControlsToolkit.Core.Transformations;


namespace MvcControlsToolkit.Core.Views
{
    public static class BasicTransformationsRegistration
    {
        public static void Registration()
        {
            TransformationsRegister.Add(typeof(JsonTransformation<>));
            TransformationsRegister.Add(typeof(EncryptedJsonTransformation<>));
        }
    }
}
