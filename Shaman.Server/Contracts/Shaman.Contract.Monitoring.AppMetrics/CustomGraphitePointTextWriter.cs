using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using App.Metrics;
using App.Metrics.Formatters.Graphite;
using App.Metrics.Formatters.Graphite.Internal;

namespace Shaman.Contract.Monitoring.AppMetrics
{
    public class CustomGraphitePointTextWriter : IGraphitePointTextWriter
    {
        private static readonly HashSet<string> ExcludeTags = new HashSet<string>
            {"app", "env", "server", "mtype", "unit", "unit_rate", "unit_dur"};

        private readonly string _prefix;

        public CustomGraphitePointTextWriter(params string[] pathNodes)
        {
            var stringWriter = new StringWriter();
            foreach (var pathNode in pathNodes)
            {
                stringWriter.Write((string?) GraphiteSyntax.EscapeName(pathNode, false));
                stringWriter.Write(".");
            }
            _prefix = stringWriter.ToString();
        }

        /// <inheritdoc />
        public void Write(TextWriter textWriter, GraphitePoint point, bool writeTimestamp = true)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            var hasPrevious = false;
            var measurementWriter = new StringWriter();
            measurementWriter.Write(_prefix);
            
            var tagsDictionary = point.Tags.ToDictionary(GraphiteSyntax.EscapeName);

            if (tagsDictionary.TryGetValue("mtype", out var metricType) && !string.IsNullOrWhiteSpace(metricType))
            {
                measurementWriter.Write((string?) metricType);
                hasPrevious = true;
            }

            if (hasPrevious)
            {
                measurementWriter.Write(".");
            }

            measurementWriter.Write((string?) GraphiteSyntax.EscapeName(point.Measurement, true));

            var tags = Enumerable.Where<KeyValuePair<string, string>>(tagsDictionary, tag => !ExcludeTags.Contains(tag.Key));

            foreach (var tag in tags)
            {
                measurementWriter.Write('.');
                measurementWriter.Write((string?) GraphiteSyntax.EscapeName(tag.Key));
                measurementWriter.Write('.');
                measurementWriter.Write(tag.Value);
            }

            measurementWriter.Write('.');

            var prefix = measurementWriter.ToString();

            var utcTimestamp = point.UtcTimestamp ?? DateTime.UtcNow;

            foreach (var f in point.Fields)
            {
                textWriter.Write(prefix);
                textWriter.Write((string?) GraphiteSyntax.EscapeName(f.Key));

                textWriter.Write(' ');
                textWriter.Write((string?) GraphiteSyntax.FormatValue(f.Value));

                textWriter.Write(' ');
                textWriter.Write((string?) GraphiteSyntax.FormatTimestamp(utcTimestamp));

                textWriter.Write('\n');
            }
        }
    }
}