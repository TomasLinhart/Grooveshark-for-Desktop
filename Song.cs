using System;

namespace Grooveshark
{
    public class Song
    {
        public int AlbumID { get; set; }
        public int TrackNum { get; set; }
        public int ArtistID { get; set; }
        public double EstimateDuration { get; set; }
        public string CoverArtFilename { get; set; }
        public string AlbumName { get; set; }
        public int ID { get; set; }
        public string ArtistName { get; set; }
        public int Vote { get; set; }
        public string Name { get; set; }

        public bool Equals(Song other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.AlbumID == AlbumID && other.TrackNum == TrackNum && other.ArtistID == ArtistID &&
                other.EstimateDuration.Equals(EstimateDuration) && Equals(other.CoverArtFilename, CoverArtFilename) &&
                Equals(other.AlbumName, AlbumName) && other.ID == ID && Equals(other.ArtistName, ArtistName) &&
                other.Vote == Vote && Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Song)) return false;
            return Equals((Song) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = AlbumID;
                result = (result*397) ^ TrackNum;
                result = (result*397) ^ ArtistID;
                result = (result*397) ^ EstimateDuration.GetHashCode();
                result = (result*397) ^ (CoverArtFilename != null ? CoverArtFilename.GetHashCode() : 0);
                result = (result*397) ^ (AlbumName != null ? AlbumName.GetHashCode() : 0);
                result = (result*397) ^ ID;
                result = (result*397) ^ (ArtistName != null ? ArtistName.GetHashCode() : 0);
                result = (result*397) ^ Vote;
                result = (result*397) ^ (Name != null ? Name.GetHashCode() : 0);
                return result;
            }
        }
    }
}
