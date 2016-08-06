﻿namespace MyTested.AspNetCore.Mvc.Internal.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    public class TempDataControllerPropertyHelper : ControllerPropertyHelper
    {
        private static readonly ConcurrentDictionary<Type, TempDataControllerPropertyHelper> ControllerPropertiesCache =
            new ConcurrentDictionary<Type, TempDataControllerPropertyHelper>();

        private Func<object, ITempDataDictionary> tempDataGetter;

        public TempDataControllerPropertyHelper(Type controllerType)
            : base(controllerType)
        {
        }

        public Func<object, ITempDataDictionary> TempDataGetter
        {
            get
            {
                if (this.tempDataGetter == null)
                {
                    this.TryCreateTempDataGetterDelegate();
                }

                return this.tempDataGetter;
            }
        }

        public static TempDataControllerPropertyHelper GetTempDataProperties<TController>()
            where TController : class
        {
            return GetTempDataProperties(typeof(TController));
        }

        public static TempDataControllerPropertyHelper GetTempDataProperties(Type type)
        {
            return ControllerPropertiesCache.GetOrAdd(type, _ => new TempDataControllerPropertyHelper(type));
        }

        private void TryCreateTempDataGetterDelegate()
        {
            var tempDataProperty = this.Properties.FirstOrDefault(pr => typeof(ITempDataDictionary).GetTypeInfo().IsAssignableFrom(pr.PropertyType));
            this.ThrowNewInvalidOperationExceptionIfNull(tempDataProperty, nameof(TempDataDictionary));

            this.tempDataGetter = MakeFastPropertyGetter<ITempDataDictionary>(tempDataProperty);
        }
    }
}
