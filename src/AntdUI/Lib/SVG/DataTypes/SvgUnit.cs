// THIS FILE IS PART OF SVG PROJECT
// THE SVG PROJECT IS AN OPENSOURCE LIBRARY LICENSED UNDER THE MS-PL License.
// COPYRIGHT (C) svg-net. ALL RIGHTS RESERVED.
// GITHUB: https://github.com/svg-net/SVG

using System;
using System.Globalization;
using System.Linq;

namespace AntdUI.Svg
{
    /// <summary>
    /// Represents a unit in an Scalable Vector Graphics document.
    /// </summary>
    public struct SvgUnit
    {
        private SvgUnitType _type;
        private float _value;
        private bool _isEmpty;
        private float? _deviceValue;

        /// <summary>
        /// Gets and empty <see cref="SvgUnit"/>.
        /// </summary>
        public static readonly SvgUnit Empty = new SvgUnit(SvgUnitType.User, 0) { _isEmpty = true };

        /// <summary>
        /// Gets an <see cref="SvgUnit"/> with a value of none.
        /// </summary>
        public static readonly SvgUnit None = new SvgUnit(SvgUnitType.None, 0f);

        /// <summary>
        /// Gets a value to determine whether the unit is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        /// <summary>
        /// Gets whether this unit is none.
        /// </summary>
        public bool IsNone
        {
            get { return _type == SvgUnitType.None; }
        }

        /// <summary>
        /// Gets the value of the unit.
        /// </summary>
        public float Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the <see cref="SvgUnitType"/> of unit.
        /// </summary>
        public SvgUnitType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Converts the current unit to one that can be used at render time.
        /// </summary>
        /// <returns>The representation of the current unit in a device value (usually pixels).</returns>
        public float ToDeviceValue(ISvgRenderer renderer, UnitRenderingType renderType, SvgElement owner)
        {
            // If it's already been calculated
            if (_deviceValue.HasValue)
            {
                return _deviceValue.Value;
            }

            if (_value == 0.0f)
            {
                _deviceValue = 0.0f;
                return _deviceValue.Value;
            }

            // http://www.w3.org/TR/CSS21/syndata.html#values
            // http://www.w3.org/TR/SVG11/coords.html#Units

            const float cmInInch = 2.54f;
            int ppi = SvgDocument.Ppi;

            var type = Type;
            var value = Value;

            float points;

            switch (type)
            {
                case SvgUnitType.Em:
                    using (var currFont = GetFont(renderer, owner))
                    {
                        if (currFont == null)
                        {
                            points = (float)(value * 9);
                            _deviceValue = (points / 72.0f) * ppi;
                        }
                        else
                        {
                            _deviceValue = value * (currFont.SizeInPoints / 72.0f) * ppi;
                        }
                    }
                    break;
                case SvgUnitType.Ex:
                    using (var currFont = GetFont(renderer, owner))
                    {
                        if (currFont == null)
                        {
                            points = (float)(value * 9);
                            _deviceValue = (points * 0.5f / 72.0f) * ppi;
                        }
                        else
                        {
                            _deviceValue = value * 0.5f * (currFont.SizeInPoints / 72.0f) * ppi;
                        }
                        break;
                    }
                case SvgUnitType.Centimeter:
                    _deviceValue = (value / cmInInch) * ppi;
                    break;
                case SvgUnitType.Inch:
                    _deviceValue = value * ppi;
                    break;
                case SvgUnitType.Millimeter:
                    _deviceValue = (float)((value / 10) / cmInInch) * ppi;
                    break;
                case SvgUnitType.Pica:
                    _deviceValue = ((value * 12) / 72) * ppi;
                    break;
                case SvgUnitType.Point:
                    _deviceValue = (value / 72) * ppi;
                    break;
                case SvgUnitType.Pixel:
                    _deviceValue = value;
                    break;
                case SvgUnitType.User:
                    _deviceValue = value;
                    break;
                case SvgUnitType.Percentage:
                    // Can't calculate if there is no style owner
                    var boundable = (renderer == null ? (owner == null ? null : owner.OwnerDocument) : renderer.GetBoundable());
                    if (boundable == null)
                    {
                        _deviceValue = value;
                        break;
                    }

                    System.Drawing.SizeF size = boundable.Bounds.Size;

                    switch (renderType)
                    {
                        case UnitRenderingType.Horizontal:
                            _deviceValue = (size.Width / 100) * value;
                            break;
                        case UnitRenderingType.HorizontalOffset:
                            _deviceValue = (size.Width / 100) * value + boundable.Location.X;
                            break;
                        case UnitRenderingType.Vertical:
                            _deviceValue = (size.Height / 100) * value;
                            break;
                        case UnitRenderingType.VerticalOffset:
                            _deviceValue = (size.Height / 100) * value + boundable.Location.Y;
                            break;
                        case UnitRenderingType.Other:
                            // Calculate a percentage value of the normalized viewBox diagonal length. 
                            if (owner == null || owner.OwnerDocument == null || owner.OwnerDocument.ViewBox == null || owner.OwnerDocument.ViewBox.Width == 0 || owner.OwnerDocument.ViewBox.Height == 0) _deviceValue = (float)(Math.Sqrt(Math.Pow(size.Width, 2) + Math.Pow(size.Height, 2)) / Math.Sqrt(2) * value / 100.0);
                            else _deviceValue = (float)(Math.Sqrt(Math.Pow(owner.OwnerDocument.ViewBox.Width, 2) + Math.Pow(owner.OwnerDocument.ViewBox.Height, 2)) / Math.Sqrt(2) * value / 100.0);
                            break;
                    }
                    break;
                default:
                    _deviceValue = value;
                    break;
            }
            return _deviceValue.Value;
        }

        private IFontDefn GetFont(ISvgRenderer renderer, SvgElement owner)
        {
            if (owner == null) return null;
            var visual = owner.Parents.OfType<SvgVisualElement>().FirstOrDefault();
            return visual != null ? visual.GetFont(renderer) : null;
        }

        /// <summary>
        /// Converts the current unit to a percentage, if applicable.
        /// </summary>
        /// <returns>An <see cref="SvgUnit"/> of type <see cref="SvgUnitType.Percentage"/>.</returns>
        public SvgUnit ToPercentage()
        {
            switch (Type)
            {
                case SvgUnitType.Percentage:
                    return this;
                case SvgUnitType.User:
                    return new SvgUnit(SvgUnitType.Percentage, Value * 100);
                default:
                    throw new NotImplementedException();
            }
        }

        #region Equals and GetHashCode implementation
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (!(obj.GetType() == typeof(SvgUnit))) return false;

            var unit = (SvgUnit)obj;
            return (unit.Value == Value && unit.Type == Type);
        }

        public bool Equals(SvgUnit other)
        {
            return _type == other._type && (_value == other._value);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                hashCode += 1000000007 * _type.GetHashCode();
                hashCode += 1000000009 * _value.GetHashCode();
                hashCode += 1000000021 * _isEmpty.GetHashCode();
                hashCode += 1000000033 * _deviceValue.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(SvgUnit lhs, SvgUnit rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SvgUnit lhs, SvgUnit rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        public override string ToString()
        {
            string type = string.Empty;

            switch (Type)
            {
                case SvgUnitType.None:
                    return "none";
                case SvgUnitType.Pixel:
                    type = "px";
                    break;
                case SvgUnitType.Point:
                    type = "pt";
                    break;
                case SvgUnitType.Inch:
                    type = "in";
                    break;
                case SvgUnitType.Centimeter:
                    type = "cm";
                    break;
                case SvgUnitType.Millimeter:
                    type = "mm";
                    break;
                case SvgUnitType.Percentage:
                    type = "%";
                    break;
                case SvgUnitType.Em:
                    type = "em";
                    break;
            }

            return string.Concat(Value.ToString(CultureInfo.InvariantCulture), type);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Svg.SvgUnit"/> to <see cref="float"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator float(SvgUnit value)
        {
            return value.ToDeviceValue(null, UnitRenderingType.Other, null);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="float"/> to <see cref="Svg.SvgUnit"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SvgUnit(float value)
        {
            return new SvgUnit(value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgUnit"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public SvgUnit(SvgUnitType type, float value)
        {
            _isEmpty = false;
            _type = type;
            _value = value;
            _deviceValue = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgUnit"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public SvgUnit(float value)
        {
            _isEmpty = false;
            _value = value;
            _type = SvgUnitType.User;
            _deviceValue = null;
        }

        public static System.Drawing.PointF GetDevicePoint(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
        {
            return new System.Drawing.PointF(x.ToDeviceValue(renderer, UnitRenderingType.Horizontal, owner),
                                             y.ToDeviceValue(renderer, UnitRenderingType.Vertical, owner));
        }
        public static System.Drawing.PointF GetDevicePointOffset(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
        {
            return new System.Drawing.PointF(x.ToDeviceValue(renderer, UnitRenderingType.HorizontalOffset, owner),
                                             y.ToDeviceValue(renderer, UnitRenderingType.VerticalOffset, owner));
        }

        public static System.Drawing.SizeF GetDeviceSize(SvgUnit width, SvgUnit height, ISvgRenderer renderer, SvgElement owner)
        {
            return new System.Drawing.SizeF(width.ToDeviceValue(renderer, UnitRenderingType.HorizontalOffset, owner),
                                            height.ToDeviceValue(renderer, UnitRenderingType.VerticalOffset, owner));
        }
    }

    public enum UnitRenderingType
    {
        Other,
        Horizontal,
        HorizontalOffset,
        Vertical,
        VerticalOffset
    }

    /// <summary>
    /// Defines the various types of unit an <see cref="SvgUnit"/> can be.
    /// </summary>
    public enum SvgUnitType
    {
        /// <summary>
        /// Indicates that the unit holds no value.
        /// </summary>
        None,
        /// <summary>
        /// Indicates that the unit is in pixels.
        /// </summary>
        Pixel,
        /// <summary>
        /// Indicates that the unit is equal to the pt size of the current font.
        /// </summary>
        Em,
        /// <summary>
        /// Indicates that the unit is equal to the x-height of the current font.
        /// </summary>
        Ex,
        /// <summary>
        /// Indicates that the unit is a percentage.
        /// </summary>
        Percentage,
        /// <summary>
        /// Indicates that the unit has no unit identifier and is a value in the current user coordinate system.
        /// </summary>
        User,
        /// <summary>
        /// Indicates the the unit is in inches.
        /// </summary>
        Inch,
        /// <summary>
        /// Indicates that the unit is in centimeters.
        /// </summary>
        Centimeter,
        /// <summary>
        /// Indicates that the unit is in millimeters.
        /// </summary>
        Millimeter,
        /// <summary>
        /// Indicates that the unit is in picas.
        /// </summary>
        Pica,
        /// <summary>
        /// Indicates that the unit is in points, the smallest unit of measure, being a subdivision of the larger <see cref="Pica"/>. There are 12 points in the <see cref="Pica"/>.
        /// </summary>
        Point
    }
}