using System;

namespace Shaman.Messages.General.Entity
{
    [Serializable]
    public enum VersionType : byte
    {
        DataBase = 1,
        Client = 2,
        Server = 3
    }

    [Serializable]
    public enum VersionComponent : byte
    {
        Major = 1,
        Minor = 2,
        Build = 3
    }

    [Serializable]
    public class CustomVersion
    {
        public static int MaxMajor = 99;
        public static int MaxMinor = 999;
        public static int MaxBuild = 9999;

        public ushort Major { get; set; }
        public ushort Minor { get; set; }
        public ushort Build { get; set; }
        

        private void IncrementMajor()
        {
            if ((this.Major + 1) > MaxMajor)
                throw new Exception($"Maximum major version {MaxMajor} reached");
            this.Major++;
            this.Minor = 0;
            this.Build = 0;
        }

        private void IncrementMinor()
        {
            if ((this.Minor + 1 > MaxMinor))
            {
                IncrementMajor();
            }
            else
            {
                this.Minor++;
                this.Build = 0;
            }
        }

        public void Increment(VersionComponent component)
        {
            switch(component)
            {
                case VersionComponent.Major:
                    IncrementMajor();
                    break;
                case VersionComponent.Minor:
                    IncrementMinor();
                    break;
                case VersionComponent.Build:
                    if ((this.Build + 1) > MaxBuild)
                    {
                        IncrementMinor();
                    }
                    else
                        this.Build++;
                    break;
            }
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}";
        }
    }


}
