namespace backend.Utilities
{
    public class QueryParameterBuilder
    {
        private readonly List<KeyValuePair<string, string>> _parameters = new();

        public QueryParameterBuilder AddParameter<T>(string name, T? value, bool encode = true) where T : class
        {
            if (value != null)
            {
                var stringValue = value.ToString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    _parameters.Add(new KeyValuePair<string, string>(
                        name,
                        encode ? Uri.EscapeDataString(stringValue) : stringValue
                    ));
                }
            }

            return this;
        }

        public QueryParameterBuilder AddParameter<T>(string name, T? value) where T : struct
        {
            if (value.HasValue)
            {
                var stringValue = value.Value switch
                {
                    bool b => b.ToString().ToLowerInvariant(),
                    DateTime dt => dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    TimeSpan ts => $"\"{ts:hh\\:mm}\"",
                    _ => value.ToString()
                };

                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    _parameters.Add(new KeyValuePair<string, string>(name, stringValue));
                }
            }

            return this;
        }

        public string Build()
        {
            return _parameters.Count == 0
                ? string.Empty
                : "?" + string.Join("&", _parameters.Select(p => $"{p.Key}={p.Value}"));
        }
    }
}