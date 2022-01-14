using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using OsuParsers.Decoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsuSongSplitter
{
    internal static class OsuSongSplitter
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please drag and drop your file onto this exe!");
            }

            // Decode file from command line (or dragged onto exe).
            Beatmap beatmap = BeatmapDecoder.Decode(args[0]);

            // Get list of bookmarks, adding the ending as a bookmark.
            List<int> bookmarks = beatmap.EditorSection.Bookmarks.ToList();
            bookmarks.Add(beatmap.HitObjects.Last().EndTime);

            // Prepare an Array of List<HitObject>, this will encompass the HitObjects between bookmarks.
            List<HitObject>[] HitObjectsInSection = new List<HitObject>[bookmarks.Count];
            int lastTime = 0;

            // For each bookmark, find all the HitObjects between the last bookmark and the next bookmark.
            for (int i = 0; i < bookmarks.Count; i++)
            {
                HitObjectsInSection[i] = beatmap.HitObjects.Where(ho => ho.StartTime >= lastTime && ho.StartTime <= bookmarks[i]).ToList();
                lastTime = bookmarks[i];
            }

            // External count rather than for index. Counts which file we're on.
            int count = 1;

            // For each section, make a copy of the original, clear the hit objects. Only add the hit objects for this section.
            // Then update the metadata for which difficulty this is.
            foreach (List<HitObject> hoList in HitObjectsInSection)
            {
                Beatmap tempBeatmap = BeatmapDecoder.Decode(args[0]);

                tempBeatmap.MetadataSection.Version += $" Part {count}";
                tempBeatmap.HitObjects.Clear();
                tempBeatmap.HitObjects = hoList;

                // Create a new Filename, replacing chars that may fail to be written to disk.
                string newFileName =
                    $"{tempBeatmap.MetadataSection.ArtistUnicode.Replace("<", "").Replace(">", "").Replace(":", "")} - " +
                    $"{tempBeatmap.MetadataSection.TitleUnicode.Replace("<", "").Replace(">", "").Replace(":", "")} [" +
                    $"{tempBeatmap.MetadataSection.Version.Replace(":", "").Replace("<", "").Replace(">", "")}].osu";

                // Prepare new writing location.
                string directory = Path.GetDirectoryName(args[0]);
                string filepath = directory + @"\" + newFileName;
                beatmap.Save(filepath);
                count++;
            }
        }
    }
}
