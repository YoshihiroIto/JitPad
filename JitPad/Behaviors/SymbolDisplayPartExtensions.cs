using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Biaui;
using Microsoft.CodeAnalysis;

namespace JitPad.Behaviors
{
    // ref: RoslynPad -- https://github.com/aelij/RoslynPad

    internal static class SymbolDisplayPartExtensions
    {
        private const string LeftToRightMarkerPrefix = "\u200e";

        public static string ToVisibleDisplayString(this TaggedText part, bool includeLeftToRightMarker)
        {
            var text = part.ToString();

            if (includeLeftToRightMarker)
            {
                if (part.Tag == TextTags.Punctuation ||
                    part.Tag == TextTags.Space ||
                    part.Tag == TextTags.LineBreak)
                {
                    text = LeftToRightMarkerPrefix + text;
                }
            }

            return text;
        }

        public static Run ToRun(this TaggedText text)
        {
            var s = text.ToVisibleDisplayString(includeLeftToRightMarker: true);
            
            var run = new Run(s)
            {
                FontSize = 12,
                FontWeight = FontWeights.Normal,
                Foreground = text.Tag switch
                {
                    TextTags.Keyword => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0xF9, 0x26, 0x72)),
                    TextTags.Struct => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    TextTags.Enum => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    TextTags.TypeParameter => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    TextTags.Class => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    TextTags.Delegate => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    TextTags.Interface => Caches.GetSolidColorBrush(new ByteColor(0xFF, 0x56, 0xD9, 0xEF)),
                    _ => Caches.GetSolidColorBrush(ByteColor.White)
                }
            };

            return run;
        }
        
        public static TextBlock ToTextBlock(this IEnumerable<TaggedText> text)
        {
            var result = new TextBlock {TextWrapping = TextWrapping.Wrap};

            foreach (var part in text)
            {
                result.Inlines.Add(part.ToRun());
            }

            return result;
        }
    }
}