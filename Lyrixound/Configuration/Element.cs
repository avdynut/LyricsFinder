using System;
using System.Collections.Generic;

namespace Lyrixound.Configuration
{
    /// <summary>
    /// Element with name that can be in enabled/disabled state.
    /// </summary>
    public class Element : IEquatable<Element>
    {
        public string Name { get; }
        public bool IsEnabled { get; set; }

        public Element(string name, bool isEnabled)
        {
            Name = name;
            IsEnabled = isEnabled;
        }

        public override string ToString()
        {
            return $"{Name}:{IsEnabled}";
        }

        #region Equatable
        public override bool Equals(object obj)
        {
            return Equals(obj as Element);
        }

        public bool Equals(Element other)
        {
            return other != null &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator ==(Element left, Element right)
        {
            return EqualityComparer<Element>.Default.Equals(left, right);
        }

        public static bool operator !=(Element left, Element right)
        {
            return !(left == right);
        }
        #endregion
    }
}
