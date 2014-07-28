using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skylight;

namespace NaturalLanguageProcessor
{
    internal class Program
    {
        private static readonly Room r = new Room("PWkDElHAYnbEI");

        private static readonly Bot b = new Bot(r, Console.ReadLine(), Console.ReadLine());

        private static readonly List<KeyValuePair<String, int>> Aliases = new List<KeyValuePair<String, int>>();

        private static void Main(String[] args)
        {
            LoadBlockAliases();

            r.Pull.NormalChatEvent += ChatHandler;

            b.LogIn();
            b.Join();
            Console.Write("Phrase to read: ");

            String phrase;
            while ((phrase = Console.ReadLine()) != String.Empty)
            {
                int id = GetId(phrase)[0];
                Console.WriteLine("Result: {0}", (id == -1) ? "none" : id.ToString());

                foreach (var entry in Aliases)
                {
                    if (entry.Value == id)
                    {
                        Console.WriteLine("    {0}", entry.Key);
                    }
                }

                Console.Write("\nPhrase to read: ");
            }
        }

        private static void ChatHandler(ChatEventArgs e)
        {
            if (e.Speaker.Name == r.Owner.Name)
            {
                String rawMessage = e.Message.ToLower();
                List<String> words = rawMessage.Split(' ').ToList();

                if (words[0] != "bot,")
                    return;

                words.RemoveAt(0);

                for (int i = 1; i < words.Count(); i++)
                    {
                        if (words[i] == "the" ||
                            words[i] == "those" ||
                            words[i] == "these" ||
                            words[i] == "a" ||
                            words[i] == "an")
                        {
                            words.RemoveAt(i);
                        }
                    }

                if (words[0] == "replace" || words[0] == "switch")
                {
                    Replace(words, e.Speaker);
                }
                else if (words[0] == "troll")
                {
                    Troll(words, e.Speaker);
                }
                else if (words[0] == "move")
                {
                    Move(words, e.Speaker);
                }
                else if (words[0] == "draw" && words[1] == "circle")
                {
                    int radius = 20;
                    for (int i = 0; i < words.Count; i++)
                    {
                        int.TryParse(words[i], out radius);
                    }

                    for (double i = 0.0; i < 360.0; i += 1)
                    {
                        double angle = i * System.Math.PI / 180;
                        int x = (int)(e.Speaker.BlockX + radius * System.Math.Cos(angle));
                        int y = (int)(e.Speaker.BlockY + radius * System.Math.Sin(angle));

                        if (r.Map[x,y,0].Id == 0)
                            b.Push.Build(BlockIds.Blocks.Basic.PURPLE, x, y);
                    }
                }
            }
        }

        private static void Troll(List<String> words, Player speaker)
        {
            int depth = 5;
            String firstBlockDescription = String.Empty,
                secondBlockDescription = String.Empty;

            words.RemoveAt(0);

            // troll green brick on top of brown brick

            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i] == "on")
                {
                    if (words.Count() >= i + 1)
                    {
                        if (words[i + 1] == "top")
                            words.RemoveAt(i + 1);
                        if (words[i + 2] == "of")
                            words.RemoveAt(i + 2);
                    }

                    words.RemoveAt(i);

                    // i == 3
                    // green brick on brown brick

                    // Get the words from beginning to present.
                    for (int j = 0; j < i; j++)
                    {
                        secondBlockDescription += words[j] + " ";
                    }

                    // Get the words from present to end.
                    for (int j = i; j < words.Count(); j++)
                    {
                        firstBlockDescription += words[j] + " ";
                    }
                }
            }

            int firstBlockId = GetId(firstBlockDescription)[0],
                secondBlockId = GetId(secondBlockDescription)[0];

            if (firstBlockId == -1 && secondBlockId == -1)
            {
                b.Push.Say("I didn't catch that either of those blocks. Try harder.");
                return;
            }
            else if (firstBlockId == -1)
            {
                b.Push.Say("I didn't catch that first block. Try rephrasing it.");
                return;
            }
            else if (secondBlockId == -1)
            {
                b.Push.Say("I didn't catch that second block. Try rephrasing it.");
                return;
            }

            // Troll it up.
            for (int x = speaker.BlockX - 20; x <= speaker.BlockX + 20; x++)
            {
                for (int y = speaker.BlockY - 15; y <= speaker.BlockY + 15; y++)
                {
                    if (r.Map[x, y, 0].Id == firstBlockId && r.Map[x, y - 1, 0].Id == 0)
                    {
                        for (int d = y; d < depth + y; d++)
                        {
                            if (r.Map[x, d, 0].Id == firstBlockId && Tools.Ran.Next(1, 4) == 3)
                            {
                                b.Push.Build(new Block(secondBlockId, x, d));
                            }
                        }
                    }
                }
            }
        }

        private static void Replace(List<String> words, Player speaker)
        {
            String firstBlockDescription = String.Empty,
                secondBlockDescription = String.Empty;

            for (int i = 1; i < words.Count(); i++)
            {
                if (words[i] == "with")
                {
                    // Get the words from beginning to present.
                    for (int j = 1; j < i; j++)
                    {
                        firstBlockDescription += words[j] + " ";
                    }

                    // Get the words from present to end.
                    for (int j = i + 1; j < words.Count(); j++)
                    {
                        secondBlockDescription += words[j] + " ";
                    }
                }
            }

            int oldBlockId = GetId(firstBlockDescription)[0],
                newBlockId = GetId(secondBlockDescription)[0];

            if (oldBlockId == -1 && newBlockId == -1)
            {
                b.Push.Say("I didn't catch that either of those blocks. Try harder.");
            }
            else if (oldBlockId == -1)
            {
                b.Push.Say("I didn't catch that first block. Try rephrasing it.");
            }
            else if (newBlockId == -1)
            {
                b.Push.Say("I didn't catch that second block. Try rephrasing it.");
            }

            for (int x = 0; x < r.Width; x++)
                for (int y = 0; y < r.Height; y++)
                    if (r.Map[x, y, 0].Id == oldBlockId)
                        b.Push.Build(newBlockId, x, y);
        }

        private static void Move(List<String> words, Player speaker)
        {
            bool up = false, down = false, left = false, right = false;
            int difference = 0;

            for (int i = 1; i < words.Count(); i++)
            {
                if (words[i] == "up")
                {
                    up = true;
                    continue;
                }
                else if (words[i] == "down")
                {
                    down = true;
                    continue;
                }
                else if (words[i] == "left")
                {
                    left = true;
                    continue;
                }
                else if (words[i] == "right")
                {
                    right = true;
                    continue;
                }

                int.TryParse(words[i], out difference);

            }

            if (difference == 0)
                difference = 5;

            if (!(up ^ down ^ left ^ right))
            {
                b.Push.Say("I'm confused - do you want me to move it up, down, left, or right?");
            }
            else
            {
                var newMap = new List<Block>();

                for (int x = 1; x < r.Width - 1; x++)
                {
                    for (int y = 1; y < r.Height - 1; y++)
                    {
                        if (r.Map[x, y, 0].Id != 0)
                        {
                            newMap.Add(new Block(r.Map[x, y, 0].Id,
                                x + (left ? -difference : right ? difference : 0),
                                y + (up ? -difference : down ? difference : 0)));
                        }

                        if (r.Map[x, y, 1].Id != 0)
                        {
                            newMap.Add(new Block(r.Map[x, y, 1].Id,
                                x + (left ? -difference : right ? difference : 0),
                                y + (up ? -difference : down ? difference : 0)));
                        }
                    }
                }

                b.Push.Clear();
                newMap.Shuffle();
                b.Push.Build(newMap);
            }
        }

        /// <summary>
        ///     Reads aliases.txt and loads the entries into Aliases for increased comprehension.
        /// </summary>
        private static void LoadBlockAliases()
        {
            // Using statement to automatically dispose of resources once done.
            using (var reader = new StreamReader("aliases.txt"))
            {
                String line;

                // Read line while checking if it's empty.
                while ((line = reader.ReadLine()) != null)
                {
                    String[] parts = line.Split(':');

                    String description = parts[0];

                    Int16 id = Convert.ToInt16(parts[1]);

                    Aliases.Add(new KeyValuePair<string, int>(description, id));
                }
            }
        }

        /// <summary>
        ///     Gets the Ids that are considered similar to the input phrase.
        /// </summary>
        private static List<int> GetId(String phrase, int amountToReturn = 10)
        {
            #region Declaration of lists, arrays, and dictionaries

            // An array of each word in the description.
            String[] words = phrase.Split(' ');

            // [block ID] = amount of entries that contain a shared word.
            var amountShared = new Dictionary<int, int>();

            // [block ID] = words matched / total amount of words
            var percentMatches = new Dictionary<int, double>();

            // List of suggested IDs that are invalid (e.g. are backgrounds despite phrase not containing "bg")
            var toRemove = new List<int>();

            // List form of percentMatches (needed for sorting).

            // The resulting block IDs
            var results = new List<int>(amountToReturn);

            #endregion

            #region Load amountShared

            for (int i = 0; i < words.Count(); i++)
            {
                #region Check current word

                foreach (var entry in
                    GetEntriesContainingString(words[i]))
                {
                    if (!amountShared.Keys.Contains(entry.Value))
                    {
                        amountShared.Add(entry.Value, 1);
                    }
                    else
                    {
                        amountShared[entry.Value]++;
                    }
                }

                #endregion

                #region Check previous word + current word

                if (i > 1)
                {
                    foreach (var entry in
                        GetEntriesContainingString(words[i - 1] + " " + words[i]))
                    {
                        if (!amountShared.Keys.Contains(entry.Value))
                        {
                            amountShared.Add(entry.Value, 1);
                        }
                        else
                        {
                            amountShared[entry.Value]++;
                        }
                    }
                }

                #endregion

                #region Check previous two words + current word

                if (i > 2)
                {
                    foreach (var entry in
                        GetEntriesContainingString(words[i - 2] + " " + words[i - 1] + " " + words[i]))
                    {
                        if (!amountShared.Keys.Contains(entry.Value))
                        {
                            amountShared.Add(entry.Value, 1);
                        }
                        else
                        {
                            amountShared[entry.Value]++;
                        }
                    }
                }

                #endregion

                #region Check previous three words + current word

                if (i > 3)
                {
                    foreach (var entry in
                        GetEntriesContainingString(words[i - 3] + " " + words[i - 2] + " " + words[i - 1] + " " +
                                                   words[i]))
                    {
                        if (!amountShared.Keys.Contains(entry.Value))
                        {
                            amountShared.Add(entry.Value, 1);
                        }
                        else
                        {
                            amountShared[entry.Value]++;
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region Remove invalid ids

            // Remove invalid phrases
            for (int i = 0; i < amountShared.Count; i++)
            {
                int id = amountShared.ElementAt(i).Key;

                if (id >= 500)
                {
                    if (!phrase.Contains("bg") && !phrase.Contains("background"))
                    {
                        toRemove.Add(id);
                    }
                }
                else if (id < 500)
                {
                    if (phrase.Contains("bg") || phrase.Contains("background"))
                    {
                        toRemove.Add(id);
                    }
                }
            }

            if (toRemove.Count < amountShared.Count)
            {
                foreach (int id in toRemove)
                {
                    amountShared.Remove(id);
                }
            }

            #endregion

            #region Load percentMatches

            for (int i = 0; i < amountShared.Count; i++)
            {
                int id = amountShared.ElementAt(i).Key;

                percentMatches[id] = amountShared[id]/(double) words.Count();
            }

            #endregion

            #region Sort all the results

            List<KeyValuePair<int, double>> listPercentMatches = percentMatches.ToList();

            listPercentMatches.Sort(
                (firstPair, nextPair) => amountShared[firstPair.Key].CompareTo(amountShared[nextPair.Key]));

            listPercentMatches.Reverse();

            listPercentMatches.Sort((firstPair, nextPair) => firstPair.Value.CompareTo(nextPair.Value));

            listPercentMatches.Reverse();

            #endregion

            #region Remove all but top results

            if (listPercentMatches.Count >= amountToReturn)
            {
                // Remove all the entries from amountToReturn to the end.
                listPercentMatches.RemoveRange(
                    amountToReturn - 1,
                    listPercentMatches.Count - amountToReturn);
            }

            #endregion

            #region Return results

            foreach (var entry in listPercentMatches)
            {
                results.Add(entry.Key);

                // This displays the info regarding the results.
                //Console.WriteLine("MATCH: {0}; words in common: {1} out of {2} == {3}%",
                //    entry.Key + ((entry.Key >= 10) ? "" : " ") + ((entry.Key >= 100) ? "" : " "),
                //    amountShared[entry.Key] + ((amountShared[entry.Key] >= 10) ? "" : " "),
                //    words.Count() + ((words.Count() >= 10) ? "" : " "),
                //    Math.Round(entry.Value * 100, 3));
            }

            //Console.WriteLine();

            if (listPercentMatches.Count >= 1)
            {
                return results;
            }
            return new List<int> {-1};

            #endregion
        }

        /// <summary>
        ///     Gets all the entries that contain String s
        /// </summary>
        private static IEnumerable<KeyValuePair<string, int>> GetEntriesContainingString(String s)
        {
            var matches = new List<KeyValuePair<String, int>>();

            foreach (var entry in Aliases)
            {
                if (entry.Key == s)
                {
                    matches.Add(entry);
                }
            }

            return matches;
        }
    }
}