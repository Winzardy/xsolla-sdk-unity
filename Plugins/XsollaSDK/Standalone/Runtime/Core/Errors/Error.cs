using System;
using System.Collections.Generic;
using System.Text;

namespace Xsolla.Core
{
	[Serializable]
	internal class Error
	{
		public string statusCode;
		public string errorCode;
		public string errorMessage;

		public ErrorType ErrorType { get; set; }
		
		public Dictionary<string, string> AdditionalData { get; set; }

		public Error(
			ErrorType errorType = ErrorType.UnknownError, 
			string statusCode = "", string errorCode = "", string errorMessage = "", 
			Dictionary<string, string> data = null
		) {
			ErrorType = errorType;
			this.statusCode = statusCode;
			this.errorCode = errorCode;
			this.errorMessage = errorMessage;
			AdditionalData = data;
		}

		public static Error UnknownError => new Error();

		public override string ToString()
		{
			var builder = new StringBuilder($"Error: {ErrorType}.");

			if (!string.IsNullOrEmpty(statusCode))
				builder.Append($" Status code: {statusCode}.");

			if (!string.IsNullOrEmpty(errorCode))
				builder.Append($" Error code: {errorCode}.");

			if (!string.IsNullOrEmpty(errorMessage))
				builder.Append($" Message: {errorMessage}.");

			if (AdditionalData != null)
			{
				foreach (var item in AdditionalData)
					builder.Append($" Data: {item.Key}: {item.Value}.");
			}

			return builder.ToString();
		}

		public string ToJson()
		{
			return "{" +
				   $"\"code\":\"{ErrorType}\"," +
				   $"\"message\":\"{ToString()}\"," +
				   "}";
		}
	}
}