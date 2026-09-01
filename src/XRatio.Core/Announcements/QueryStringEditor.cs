using System.Text;

namespace XRatio.Core.Announcements;

public sealed class QueryStringEditor
{
    private readonly string _prefix;
    private readonly string _fragment;
    private readonly bool _hadQuery;
    private readonly List<QueryPair> _pairs;

    private QueryStringEditor(string prefix, string fragment, bool hadQuery, List<QueryPair> pairs)
    {
        _prefix = prefix;
        _fragment = fragment;
        _hadQuery = hadQuery;
        _pairs = pairs;
    }

    public static QueryStringEditor Parse(string resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        var fragmentIndex = resource.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? resource[fragmentIndex..] : string.Empty;
        var contentEnd = fragmentIndex >= 0 ? fragmentIndex : resource.Length;
        var queryIndex = resource.IndexOf('?', 0, contentEnd);
        if (queryIndex < 0)
            return new QueryStringEditor(resource[..contentEnd], fragment, false, []);

        var pairs = new List<QueryPair>();
        var pairStart = queryIndex + 1;
        while (true)
        {
            var separator = resource.IndexOf('&', pairStart, contentEnd - pairStart);
            var pairEnd = separator >= 0 ? separator : contentEnd;
            pairs.Add(QueryPair.Parse(resource[pairStart..pairEnd]));
            if (separator < 0)
                break;
            pairStart = separator + 1;
        }

        return new QueryStringEditor(resource[..queryIndex], fragment, true, pairs);
    }

    public bool Contains(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        foreach (var pair in _pairs)
        {
            if (pair.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public string? GetLast(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        for (var index = _pairs.Count - 1; index >= 0; index--)
        {
            if (_pairs[index].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return _pairs[index].Value;
        }

        return null;
    }

    public string Rewrite(IReadOnlyDictionary<string, string> updates, IReadOnlySet<string>? removals = null)
    {
        ArgumentNullException.ThrowIfNull(updates);
        if (!_hadQuery)
            return ToString();

        var output = new StringBuilder(Math.Max(_prefix.Length + _fragment.Length + 1, 32));
        output.Append(_prefix).Append('?');
        var wrotePair = false;
        for (var index = 0; index < _pairs.Count; index++)
        {
            var pair = _pairs[index];
            if (removals?.Contains(pair.Name) == true)
                continue;

            if (wrotePair)
                output.Append('&');
            if (TryGetUpdate(updates, pair.Name, out var update))
                output.Append(pair.Name).Append('=').Append(update);
            else
                output.Append(pair.Raw);
            wrotePair = true;
        }

        return output.Append(_fragment).ToString();
    }

    /// <summary>
    /// Rewrites the three counters used by tracker announces without creating
    /// a temporary dictionary for every request.
    /// </summary>
    public string RewriteCounters(string downloaded, string uploaded, string left)
    {
        ArgumentNullException.ThrowIfNull(downloaded);
        ArgumentNullException.ThrowIfNull(uploaded);
        ArgumentNullException.ThrowIfNull(left);
        if (!_hadQuery)
            return ToString();

        var output = new StringBuilder(Math.Max(_prefix.Length + _fragment.Length + 1, 32));
        output.Append(_prefix).Append('?');
        var wrotePair = false;
        foreach (var pair in _pairs)
        {
            if (wrotePair)
                output.Append('&');
            if (pair.Name.Equals("downloaded", StringComparison.OrdinalIgnoreCase))
                output.Append(pair.Name).Append('=').Append(downloaded);
            else if (pair.Name.Equals("uploaded", StringComparison.OrdinalIgnoreCase))
                output.Append(pair.Name).Append('=').Append(uploaded);
            else if (pair.Name.Equals("left", StringComparison.OrdinalIgnoreCase))
                output.Append(pair.Name).Append('=').Append(left);
            else
                output.Append(pair.Raw);
            wrotePair = true;
        }

        return output.Append(_fragment).ToString();
    }

    public override string ToString() =>
        _hadQuery
            ? JoinRawPairs()
            : string.Concat(_prefix, _fragment);

    private string JoinRawPairs()
    {
        var output = new StringBuilder(Math.Max(_prefix.Length + _fragment.Length + 1, 32));
        output.Append(_prefix).Append('?');
        for (var index = 0; index < _pairs.Count; index++)
        {
            if (index > 0)
                output.Append('&');
            output.Append(_pairs[index].Raw);
        }

        return output.Append(_fragment).ToString();
    }

    private static bool TryGetUpdate(
        IReadOnlyDictionary<string, string> updates,
        string name,
        out string value)
    {
        // AnnounceTransformer uses an ordinal-ignore-case dictionary. Keep the
        // fast lookup for that common case, then retain case-insensitive
        // behavior for callers that supply a case-sensitive/custom dictionary.
        if (updates.TryGetValue(name, out value!))
            return true;

        foreach (var item in updates)
        {
            if (item.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = item.Value;
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private readonly record struct QueryPair(string Name, string Value, string Raw)
    {
        public static QueryPair Parse(string raw)
        {
            var equalsIndex = raw.IndexOf('=');
            return equalsIndex < 0
                ? new QueryPair(raw, string.Empty, raw)
                : new QueryPair(raw[..equalsIndex], raw[(equalsIndex + 1)..], raw);
        }
    }
}
