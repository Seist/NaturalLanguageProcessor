using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skylight;

namespace NaturalLanguageProcessor
{
    internal class Program
    {
        private static readonly Room r = new Room("PWpGz9IPyba0I");

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
                List<String> args = rawMessage.Split(' ').ToList();

                

                if (args[0] == "replace" || args[0] == "switch")
                {
                    String firstBlockDescription = String.Empty,
                        secondBlockDescription = String.Empty;

                    for (int i = 1; i < args.Count(); i++)
                    {
                        if (args[i] == "the" ||
                            args[i] == "those" ||
                            args[i] == "these" ||
                            args[i] == "a" ||
                            args[i] == "an")
                        {
                            args.RemoveAt(i);
                        }
                    }

                    for (int i = 1; i < args.Count(); i++)
                    {
                        if (args[i] == "with")
                        {
                            // Get the words from beginning to present.
                            for (int j = 1; j < i; j++)
                            {
                                firstBlockDescription += args[j] + " ";
                            }

                            // Get the words from present to end.
                            for (int j = i + 1; j < args.Count(); j++)
                            {
                                secondBlockDescription += args[j] + " ";
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
                // troll green brick on top of the brown brick
                else if (args[0] == "troll")
                {
                    int depth = 5;
                    String firstBlockDescription = String.Empty,
                        secondBlockDescription = String.Empty;

                    for (int i = 1; i < args.Count(); i++)
                    {
                        if (args[i] == "the" ||
                            args[i] == "those" ||
                            args[i] == "these" ||
                            args[i] == "a" ||
                            args[i] == "an")
                        {
                            args.RemoveAt(i);
                        }
                    }

                    // troll green brick on top of brown brick

                    for (int i = 1; i < args.Count(); i++)
                    {
                        if (args[i] == "on")
                        {
                            if (args.Count() >= i + 1)
                            {
                                if (args[i + 1] == "top")
                                    args.RemoveAt(i + 1);
                                if (args[i + 2] == "of")
                                    args.RemoveAt(i + 2);
                            }
                            // i == 3
                            // troll green brick on brown brick

                            // Get the words from beginning to present.
                            for (int j = 1; j < i; j++)
                            {
                                secondBlockDescription += args[j] + " ";
                            }

                            // Get the words from present to end.
                            for (int j = i + 1; j < args.Count(); j++)
                            {
                                firstBlockDescription += args[j] + " ";
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
                    for (int x = e.Speaker.BlockX - 20; x <= e.Speaker.BlockX + 20; x++)
                    {
                        for (int y = e.Speaker.BlockY - 15; y <= e.Speaker.BlockY + 15; y++)
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
                else if (args[0] == "move")
                {
                    bool up = false, down = false, left = false, right = false;
                    int difference = 0;

                    for (int i = 1; i < args.Count(); i++)
                    {
                        if (args[i] == "up")
                        {
                            up = true;
                            continue;
                        }
                        else if (args[i] == "down")
                        {
                            down = true;
                            continue;
                        }
                        else if (args[i] == "left")
                        {
                            left = true;
                            continue;
                        }
                        else if (args[i] == "right")
                        {
                            right = true;
                            continue;
                        }

                        int.TryParse(args[i], out difference);

                    }

                    if (difference == 0)
                        difference = 5;

                    if (!(up ^ down ^ left ^ right))
                    {
                        b.Push.Say("I'm confused - do you want me to move it up, down, left, or right?");
                    }
                    else
                    {
                        for (int x = 50; x < 100; x++)
                            for (int y = 20; y > 0; y--)
                                b.Push.Build(r.Map[x, y, 0].Id,
                                    x + (left ? -difference : right ? difference : 0),
                                    y + (up ? -difference : down ? difference : 0));
                    }
                }
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