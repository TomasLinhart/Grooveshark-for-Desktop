using System;

namespace Grooveshark
{
    public class Song
    {
        public int AlbumID { get; set; }
        public int TrackNum { get; set; }
        public int ArtistID { get; set; }
        public double EstimateDuration { get; set; }
        public Uri Art { get; set; }
        public string AlbumName { get; set; }
        public int ID { get; set; }
        public string ArtistName { get; set; }
        public int Vote { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var song = obj as Song;
            if (song == null) return false;

            return song.AlbumID == AlbumID && song.TrackNum == TrackNum && song.ArtistID == ArtistID &&
                   song.EstimateDuration == EstimateDuration && song.Art == Art && song.AlbumName == AlbumName &&
                   song.ID == ID && song.ArtistName == ArtistName && song.Vote == Vote && song.Name == Name;
        }
    }
}
