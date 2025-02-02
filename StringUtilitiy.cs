using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NHMPh_music_player
{
    internal static class StringUtilitiy
    {
        //Number to word converter <10
        public static string ReplaceNumbersWithWords(string input)
        {
            string[] parts = input.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                // Attempt to parse the substring as an integer
                if (int.TryParse(parts[i], out int num))
                {
                    // If the parsed number is less than 10, replace it with its string representation
                    if (num < 10)
                    {
                        Console.WriteLine(num + " Found " + parts[i]);
                        for (int j = 1; j <= 10; j++)
                        {
                            parts[i] = NumberToWords(num);
                        }
                    }

                }
            }

            input = string.Join(" ", parts);
            return input;
        }
        private static string NumberToWords(int number)
        {
            if (number < 1 || number > 10)
            {
                throw new ArgumentOutOfRangeException("Number must be between 1 and 10.");
            }

            return numberWords[number];
        }
        private static readonly Dictionary<int, string> numberWords = new Dictionary<int, string>()
        {
            {1, "one"}, {2, "two"}, {3, "three"}, {4, "four"}, {5, "five"},
            {6, "six"}, {7, "seven"}, {8, "eight"}, {9, "nine"}, {10, "ten"}
        };

        public static int ExtractTrackId(string input)
        {
            // Define a regular expression pattern to match the ID number
            string pattern = @"^(\d+)\.";

            // Match the pattern in the input string
            Match match = Regex.Match(input, pattern);

            // Check if the match is successful and extract the ID
            if (match.Success)
            {
                // Extract the ID from the first capturing group

                if (int.TryParse(match.Groups[1].Value, out int id))
                {
                    return id;
                }
            }

            // Return -1 if ID extraction fails
            return -1;
        }

        private static readonly Dictionary<string, string> songException = new Dictionary<string, string>()
        {
            { "WITHYOU", "With You Hoaprox"}, { "BoneyM-Rasputin", "Rasputin"},{ "BoneyM.-Rasputin", "Rasputin"},{"DMDOKURO-Stained,BrutalCalamity", "Stained, Brutal Calamity"},{"NineInchNails-HurtLyricsVideo","Nine Inch Nails - Hurt " },{"FoolsGarden-LemonTree","Lemon Tree"},{"Fool'sGarden-LemonTree","Lemon Tree"},{"O-Zone-DragosteaDinTei","O-zone"}
        };
        public static string ProcessInvailName(string name)
        {
            if (name.Contains('‒'))
                name = name.Replace('‒', '-');
            if (name.Contains(" - Tik Tok"))
                name = name.Replace(" - Tik Tok", "");
            if(name.Contains(" x "))
                name = name.Replace(" x ", " ");
            name = Regex.Replace(name, @"(\([^)]*\)|\[[^\]]*\])|【|】|""""[^""""]*""""""", "");
            string result = name;
            if (!result.Contains("-"))
            {
                result = Regex.Replace(result, @"\|.*$|(\([^)]*\)|\[[^\]]*\])|ft\..*|FT\..*|Ft\..*|feat\..*|Feat\..*|FEAT\..*|【|】|""[^""]*""|LYRICS|VIDEO|★|!", "");
                try { result = songException[result.Replace(" ", "")]; } catch { };
                return result.Replace(" ", "%20");
            }
            string[] parts = Regex.Split(result, @"(?<=\s-\s)|(?<=\s--\s)|(?<=-\s)");
           // string combinedPattern = @"\|.*$|(\([^)]*\)|\[[^\]]*\])|ft\..*|FT\..*|Ft\..*|feat\..*|Feat\..*|FEAT\..*|【|】|""[^""]*""|LYRICS|VIDEO";
            string combinedPattern = @"\|.*$|(\([^)]*\)|\[[^\]]*\])|【|】|""[^""]*""|LYRICS|VIDEO|★|!";
            parts[0] = Regex.Replace(parts[0], combinedPattern, "");
            parts[1] = Regex.Replace(parts[1], combinedPattern, "");
            if (parts[0].Contains(','))
                parts[0] = parts[0].Replace(',', '&');
            parts[0] = Regex.Replace(parts[0], @"(?<!\w)x(?!x|\w)", ",");

            if (parts[0].Contains('&'))
            {
                parts[0] = parts[0].Split('&')[parts[0].Split('&').Length - 1];
            }
            parts[1] = ReplaceNumbersWithWords(parts[1]);
            string processName = parts[0] + "" + parts[1];
            Console.WriteLine(processName.Replace(" ", ""));
            try { processName = songException[processName.Replace(" ", "")]; } catch { }
            return processName.Replace(" ", "%20");

        }

        public static string ExtractId(string href)
        {
            int indexV = href.IndexOf("v=");
            if (indexV != -1)
            {
                // Extract the video ID starting from the index of "v="
                string videoId = href.Substring(indexV + 2);

                // Remove any additional parameters by finding the index of "&"
                int indexAmpersand = videoId.IndexOf("&");

                if (indexAmpersand != -1)
                {
                    videoId = videoId.Substring(0, indexAmpersand);
                }

                return videoId;

            }
            return null;
        }
        public static int EvaluateKeyWord(string key)
        {
            if (IsYouTubePlaylistLink(key))
            {
                Console.WriteLine("playlink");

                return 2;
            }
            else if (IsYouTubeVideoLink(key))
            {
                Console.WriteLine("link");
                return 1;
            }
            else
            {
                Console.WriteLine("search");
                return 3;
            }
        }
        public static bool IsYouTubeVideoLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/watch\?v=|youtu\.be/)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        public static bool IsYouTubePlaylistLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/playlist\?list=|youtube\.com/watch\?v=.+&list=)([a-zA-Z0-9_-]+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        public static bool IsYouTubeAutoPlaylistLink(string input)
        {
            string pattern = @"^(https?://)?(www\.)?(youtube\.com/playlist\?list=|youtube\.com/watch\?v=.+&((list=[a-zA-Z0-9_-]+)|list=RD[a-zA-Z0-9_-]+(&start_radio=\d)?))";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);
            return match.Success;
        }
        public static string RemoveIdAndParentheses(string input)
        {
            // Define a regular expression pattern to match the ID and parentheses and content inside parentheses
            string pattern = @"^\d+\.\s|\([^()]*\)";

            // Replace the matched pattern with an empty string
            string result = Regex.Replace(input, pattern, "");

            return result;
        }
        public static JObject ReadJsonFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonString = reader.ReadToEnd();
                    return JObject.Parse(jsonString);
                }
            }
            else { Console.WriteLine("NotFOund"); }

            return null;
        }
       public static List<(double, string)> ExtractAndParseTimestampsAndLyricsToMilliseconds(string text)
        {
            List<(double, string)> timestampedLyrics = new List<(double, string)>();
            // Regular expression to match the timestamps and lyrics
            Regex regex = new Regex(@"\[(\d{2}):(\d{2})\.(\d{2})\] (.*)");
            MatchCollection matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    // Extract minutes, seconds, and milliseconds from the match
                    int minutes = int.Parse(match.Groups[1].Value);
                    int seconds = int.Parse(match.Groups[2].Value);
                    int milliseconds = int.Parse(match.Groups[3].Value); // Convert to milliseconds

                    TimeSpan timeSpan = new TimeSpan(0,0,minutes, seconds, milliseconds);
                    // Convert the entire timestamp to milliseconds
                    //  int totalMilliseconds = (minutes * 60 * 1000) + (seconds * 1000) + milliseconds;
                    double totalSeconds = timeSpan.TotalSeconds;
                    string lyric = match.Groups[4].Value; // Extract the lyric

                    // Add the total milliseconds and lyric to the list
                    timestampedLyrics.Add((totalSeconds, lyric));
                }
            }

            return timestampedLyrics;
        }
    }
}
