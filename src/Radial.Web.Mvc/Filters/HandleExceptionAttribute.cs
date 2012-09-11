﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Net;

namespace Radial.Web.Mvc.Filters
{
    /// <summary>
    /// Represents an attribute that is used to handle and log an exception that is thrown by an action method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HandleExceptionAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandleExceptionAttribute"/> class.
        /// </summary>
        public HandleExceptionAttribute()
            : this(ExceptionOutputStyle.System, 9999)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleExceptionAttribute"/> class.
        /// </summary>
        /// <param name="outputStyle">The exception output style.</param>
        /// <param name="defaultErrorCode">The default error code.</param>
        /// <param name="defaultHttpStatusCode">The default http status code.</param>
        public HandleExceptionAttribute(ExceptionOutputStyle outputStyle, int defaultErrorCode, HttpStatusCode? defaultHttpStatusCode = HttpStatusCode.InternalServerError)
        {
            OutputStyle = outputStyle;
            DefaultErrorCode = defaultErrorCode;
            DefaultHttpStatusCode = defaultHttpStatusCode.Value;
        }

        /// <summary>
        /// Gets the output style.
        /// </summary>
        public ExceptionOutputStyle OutputStyle
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the default error code.
        /// </summary>
        public int DefaultErrorCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the default http status code.
        /// </summary>
        public HttpStatusCode DefaultHttpStatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext"/> parameter is null.</exception>
        public override void OnException(ExceptionContext filterContext)
        {
            Logger.Default.Fatal(filterContext.Exception);


            if (OutputStyle == ExceptionOutputStyle.System)
            {
                base.OnException(filterContext);
                return;
            }

            filterContext.ExceptionHandled = true;

            HttpKnownFaultException hkfe = filterContext.Exception as HttpKnownFaultException;

            ExceptionOutputData data = new ExceptionOutputData
            {
                ErrorCode = hkfe != null ? hkfe.ErrorCode : DefaultErrorCode,
                RequestUrl = HttpKits.MakeRelativeUrl(filterContext.HttpContext.Request.Url.ToString()).Replace("~", string.Empty),
                ErrorMessage = filterContext.Exception.Message
            };

            if (OutputStyle == ExceptionOutputStyle.Json)
                HttpKits.WriteJson<ExceptionOutputData>(data);
            if (OutputStyle == ExceptionOutputStyle.Xml)
                HttpKits.WriteXml(data.ToXml());

            HttpStatusCode scode = DefaultHttpStatusCode;

            if (hkfe != null && hkfe.StatusCode.HasValue)
                scode = hkfe.StatusCode.Value;

            filterContext.Result = new HttpStatusCodeResult((int)scode);
        }
    }
}