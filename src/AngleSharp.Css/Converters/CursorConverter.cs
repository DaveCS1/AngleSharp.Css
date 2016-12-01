﻿using System;
using System.IO;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Text;
using AngleSharp.Css.Values;
using System.Collections.Generic;

namespace AngleSharp.Css.Converters
{
    sealed class CursorConverter : IValueConverter
    {
        public ICssValue Convert(StringSource source)
        {
            var cursor = default(SystemCursor?);
            var definitions = new List<CursorDefinition>();

            while (!source.IsDone)
            {
                var definition = new CursorDefinition();
                definition.Source = source.ParseImageSource();
                var c = source.SkipSpacesAndComments();

                if (definition.Source != null)
                {
                    var x = source.ParseNumber();
                    c = source.SkipSpacesAndComments();
                    var y = source.ParseNumber();
                    c = source.SkipSpacesAndComments();

                    if (x.HasValue != y.HasValue || c != Symbols.Comma)
                        break;

                    source.SkipCurrentAndSpaces();

                    if (x.HasValue)
                    {
                        var xp = new Length(x.Value, Length.Unit.None);
                        var yp = new Length(y.Value, Length.Unit.None);
                        definition.Position = new Point(xp, yp);
                    }

                    definitions.Add(definition);
                }
                else
                {
                    cursor = source.ParseConstant(Map.SystemCursors);

                    if (cursor.HasValue)
                    {
                        return new CursorValue(definitions.ToArray(), cursor.Value);
                    }

                    break;
                }
            }

            return null;
        }

        struct CursorDefinition
        {
            public IImageSource Source;
            public Point? Position;

            public override String ToString()
            {
                var sb = StringBuilderPool.Obtain();

                sb.Append(Source.ToString());

                if (Position.HasValue)
                {
                    sb.Append(Symbols.Space);
                    sb.Append(Position.Value.ToString());
                }

                return sb.ToPool();
            }
        }

        sealed class CursorValue : ICssValue
        {
            private readonly CursorDefinition[] _definitions;
            private readonly SystemCursor _cursor;

            public CursorValue(CursorDefinition[] definitions, SystemCursor cursor)
            {
                _definitions = definitions;
                _cursor = cursor;
            }

            public String CssText
            {
                get
                {
                    var sb = StringBuilderPool.Obtain();

                    foreach (var definition in _definitions)
                    {
                        sb.Append(definition).Append(", ");
                    }

                    sb.Append(_cursor.ToString(Map.SystemCursors));
                    return sb.ToPool();
                }
            }

            public void ToCss(TextWriter writer, IStyleFormatter formatter)
            {
                writer.Write(CssText);
            }
        }
    }
}
