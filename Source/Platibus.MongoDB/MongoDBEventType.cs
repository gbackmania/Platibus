﻿using Platibus.Diagnostics;

namespace Platibus.MongoDB
{
    /// <summary>
    /// Type of <see cref="DiagnosticEvent"/>s related to MongoDB operations
    /// </summary>
    public class MongoDBEventType
    {
        /// <summary>
        /// Thrown when an index is created
        /// </summary>
        public static readonly DiagnosticEventType IndexCreated = new DiagnosticEventType("IndexCreated");

        /// <summary>
        /// Thrown when an index creation operation fails
        /// </summary>
        public static readonly DiagnosticEventType IndexCreationFailed = new DiagnosticEventType("IndexCreationFailed", DiagnosticEventLevel.Warn);
    }
}