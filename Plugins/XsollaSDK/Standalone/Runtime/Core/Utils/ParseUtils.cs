using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;

namespace Xsolla.Core
{
	internal static class ParseUtils
	{
		private static readonly JsonSerializerSettings serializerSettings;

		static ParseUtils()
		{
			serializerSettings = new JsonSerializerSettings {
				NullValueHandling = NullValueHandling.Ignore
			};
		}

		public static string ToJson<TData>(TData data) where TData : class
		{
			return JsonConvert.SerializeObject(data, serializerSettings);
		}

		public static TData FromJson<TData>(string json) where TData : class
		{
			TData result;

			try
			{
				result = JsonConvert.DeserializeObject<TData>(json, serializerSettings);
			}
			catch (Exception)
			{
				XDebug.LogWarning($"Deserialization failed for {typeof(TData)}");
				result = null;
			}

			return result;
		}

		private static Error ParseError(string json, long? statusCode = null)
		{
			if (json.Contains("statusCode") && json.Contains("errorCode") && json.Contains("errorMessage"))
				return FromJson<Error>(json);

			if (json.Contains("error"))
			{
				if (json.Contains("code") && json.Contains("description"))
					return FromJson<LoginError>(json).ToError();

				if (TryParseShortError(json, statusCode, out var shortError))
					return shortError;
			}

			return null;

			static bool TryParseShortError(string str, long? statusCode, out Error shortError)
			{
				shortError = default;

                // e.g. {"error":"unauthorized"}
                const string needle = "{\"error\":\"";

                var startIndex = str.IndexOf(needle);
                if (startIndex >= 0)
                {
                    var msgStartIndex = startIndex + needle.Length;
                    var msgEndIndex = str.IndexOf('\"', msgStartIndex);

                    if (msgEndIndex >= 0)
                    {
                        var msg = str.Substring(msgStartIndex, msgEndIndex - msgStartIndex);
                        shortError = new Error(
                            statusCode: statusCode != null ? statusCode.ToString() : string.Empty,
                            errorMessage: msg
                        );
						return true;
                    }
                }

				return false;
            }
        }

		public static bool TryParseError(string json, out Error error, long? statusCode = null)
		{
			if (string.IsNullOrEmpty(json))
			{
				error = null;
				return false;
			}

			try
			{
				error = ParseError(json, statusCode);
				return error != null;
			}
			catch (Exception ex)
			{
				error = new Error(ErrorType.InvalidData, errorMessage: ex.Message);
				return true;
			}
		}

		public static bool TryGetValueFromUrl(string url, ParseParameter parameter, out string value)
		{
			var parameterName = parameter.ToString();
			var regex = new Regex($"[&?]{parameterName}=[a-zA-Z0-9._+-]+");
			value = regex.Match(url)
				.Value
				.Replace($"{parameterName}=", string.Empty)
				.Replace("&", string.Empty)
				.Replace("?", string.Empty);

			switch (parameter)
			{
				case ParseParameter.error_code:
				case ParseParameter.error_description:
					value = value?.Replace("+", " ");
					break;
				default:
					XDebug.Log($"Trying to find {parameterName} in URL:{url}");
					break;
			}

			return !string.IsNullOrEmpty(value);
		}
	}
}