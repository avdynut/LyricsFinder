using System;
using System.Collections.Generic;

namespace Win10Watcher
{
    public class PlayerInfo : IEquatable<PlayerInfo>
    {
        /// <summary>
        /// Gets the handle of the window associated with this session's source application.
        /// </summary>
        public IntPtr Hwnd { get; }

        /// <summary>
        /// Gets the process ID of this session's source application.
        /// </summary>
        public uint PID { get; }

        /// <summary>
        /// Gets the application ID of this session's source application.
        /// </summary>
        public string SourceAppId { get; }

        public PlayerInfo(IntPtr hwnd, uint pId, string sourceAppId)
        {
            Hwnd = hwnd;
            PID = pId;
            SourceAppId = sourceAppId;
        }

        public override string ToString()
        {
            return $"{SourceAppId} - {PID} ({Hwnd})";
        }

        #region Equatable
        public override bool Equals(object obj)
        {
            return Equals(obj as PlayerInfo);
        }

        public bool Equals(PlayerInfo other)
        {
            return other != null &&
                   EqualityComparer<IntPtr>.Default.Equals(Hwnd, other.Hwnd) &&
                   PID == other.PID &&
                   SourceAppId == other.SourceAppId;
        }

        public override int GetHashCode()
        {
            int hashCode = 818686938;
            hashCode = hashCode * -1521134295 + Hwnd.GetHashCode();
            hashCode = hashCode * -1521134295 + PID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SourceAppId);
            return hashCode;
        }

        public static bool operator ==(PlayerInfo left, PlayerInfo right)
        {
            return EqualityComparer<PlayerInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(PlayerInfo left, PlayerInfo right)
        {
            return !(left == right);
        }
        #endregion
    }
}
