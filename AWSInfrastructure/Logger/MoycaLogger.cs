﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace AWSInfrastructure.Logger
{
    [System.Flags]
    public enum LogLevel
    {
        TRACE,   // log everything
        INFO,    // log only info
        DEBUG, 
        WARN,
        ERROR
    }
    public class MoycaLogger
    {
        private ILambdaContext context;
        private LogLevel level;

        public MoycaLogger(ILambdaContext ct, LogLevel lvl)
        {
            this.context = ct;
            this.level = lvl;
        }
        public void INFO(string className, string message)
        {
            if (level == LogLevel.INFO || level == LogLevel.TRACE || level == LogLevel.WARN)
            {
                this.context.Logger.LogLine("INFO: " + className + ": " + message);
            }
        }

        public void INFO(string className, string function, string message)
        {
            if (level == LogLevel.INFO || level == LogLevel.TRACE || level == LogLevel.WARN)
            {
                this.context.Logger.LogLine("INFO: " + className + ": " + function + ": " + message);
            }
        }


        public void DEBUG(string className, string message)
        {
            if (level == LogLevel.DEBUG || level == LogLevel.TRACE || level == LogLevel.WARN)
            {
                this.context.Logger.LogLine("DEBUG: " + className + ": " + message);
            }
        }

        public void DEBUG(string className, string function, string message)
        {
            if (level == LogLevel.DEBUG || level == LogLevel.TRACE || level == LogLevel.WARN)
            {
                this.context.Logger.LogLine("DEBUG: " + className + ": " + function + ": " + message);
            }
        }

        public void WARN(string className, string function, string message)
        {
            if (level == LogLevel.WARN || level == LogLevel.TRACE)
            {
                this.context.Logger.LogLine("WARNING: " + className + ": " + function + ": " + message);
            }
        }

    }
}
