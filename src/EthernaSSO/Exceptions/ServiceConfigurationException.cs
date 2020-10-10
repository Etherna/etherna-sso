﻿using System;

namespace Etherna.SSOServer.Exceptions
{
    public class ServiceConfigurationException : Exception
    {
        public ServiceConfigurationException()
        {
        }

        public ServiceConfigurationException(string message) : base(message)
        {
        }

        public ServiceConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
