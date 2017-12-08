﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Platibus.SampleApi.Widgets
{
    [Serializable]
    public class WidgetNotFoundException : Exception
    {
        public string WidgetId { get; }

        public WidgetNotFoundException(string widgetId) : base("Widget " + widgetId + " not found")
        {
            WidgetId = widgetId;
        }

        /// <summary>
        /// Initializes a serialized <see cref="WidgetNotFoundException"/> from
        /// a streaming context
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public WidgetNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            WidgetId = info.GetString("WidgetId");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown. </param><param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination. </param><exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception><filterpriority>2</filterpriority><PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/></PermissionSet>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("WidgetId", WidgetId);
        }
    }
}