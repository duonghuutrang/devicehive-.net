﻿using System;

namespace DeviceHive.Data.Model
{
    /// <summary>
    /// Represents meta-information about the current API.
    /// </summary>
    public class ApiInfo
    {
        #region Public Properties

        /// <summary>
        /// API version.
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// Current server timestamp.
        /// </summary>
        public DateTime ServerTimestamp { get; set; }

        #endregion
    }
}