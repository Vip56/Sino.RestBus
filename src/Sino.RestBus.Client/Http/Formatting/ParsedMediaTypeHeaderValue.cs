using System;
using System.Net.Http.Headers;

namespace Sino.RestBus.Client.Http.Formatting
{
    internal struct ParsedMediaTypeHeaderValue
    {
        private const char MediaRangeAsterisk = '*';
        private const char MediaTypeSubtypeDelimiter = '/';

        private readonly string _mediaType;
        private readonly int _delimiterIndex;
        private readonly bool _isAllMediaRange;
        private readonly bool _isSubtypeMediaRange;

        public ParsedMediaTypeHeaderValue(MediaTypeHeaderValue mediaTypeHeaderValue)
        {
            string mediaType = _mediaType = mediaTypeHeaderValue.MediaType;
            _delimiterIndex = mediaType.IndexOf(MediaTypeSubtypeDelimiter);

            _isAllMediaRange = false;
            _isSubtypeMediaRange = false;
            int mediaTypeLength = mediaType.Length;
            if (_delimiterIndex == mediaTypeLength - 2)
            {
                if (mediaType[mediaTypeLength - 1] == MediaRangeAsterisk)
                {
                    _isSubtypeMediaRange = true;
                    if (_delimiterIndex == 1 && mediaType[0] == MediaRangeAsterisk)
                    {
                        _isAllMediaRange = true;
                    }
                }
            }
        }

        public bool IsAllMediaRange
        {
            get { return _isAllMediaRange; }
        }

        public bool IsSubtypeMediaRange
        {
            get { return _isSubtypeMediaRange; }
        }

        public bool TypesEqual(ref ParsedMediaTypeHeaderValue other)
        {
            if (_delimiterIndex != other._delimiterIndex)
            {
                return false;
            }
            return String.Compare(_mediaType, 0, other._mediaType, 0, _delimiterIndex, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool SubTypesEqual(ref ParsedMediaTypeHeaderValue other)
        {
            int _subTypeLength = _mediaType.Length - _delimiterIndex - 1;
            if (_subTypeLength != other._mediaType.Length - other._delimiterIndex - 1)
            {
                return false;
            }
            return String.Compare(_mediaType, _delimiterIndex + 1, other._mediaType, other._delimiterIndex + 1, _subTypeLength, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
