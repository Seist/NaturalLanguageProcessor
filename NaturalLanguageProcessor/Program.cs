namespace NaturalLanguageProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Skylight;
    using System.IO;

    class Program
    {
        private static void Main(String[] args)
        {
            LoadBlockAliases();

            r.Pull.NormalChatEvent += ChatHandler;

            b.LogIn();
            b.Join();
            Console.Write("Phrase to read: ");

            String phrase = String.Empty;
            while ((phrase = Console.ReadLine()) != String.Empty)
            {
                int id = GetId(phrase)[0];
                Console.WriteLine("Result: {0}", (id == -1) ? "none" : id.ToString());

                foreach (KeyValuePair<String, int> entry in Aliases)
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

                if (args.Count() < 2)
                    return;

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
            }
        }

        /// <summary>
        /// Reads aliases.txt and loads the entries into Aliases for increased comprehension.
        /// </summary>
        private static void LoadBlockAliases()
        {
            // Using statement to automatically dispose of resources once done.
            using (StreamReader reader = new StreamReader("aliases.txt"))
            {
                String line = String.Empty;

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
        /// Gets the Ids that are considered similar to the input phrase.
        /// </summary>
        private static List<int> GetId(String phrase, int amountToReturn = 10)
        {
            #region Declaration of lists, arrays, and dictionaries
            // An array of each word in the description.
            String[] words = phrase.Split(' ');

            // [block ID] = amount of entries that contain a shared word.
            Dictionary<int, int> amountShared = new Dictionary<int, int>();

            // [block ID] = words matched / total amount of words
            Dictionary<int, double> percentMatches = new Dictionary<int, double>();

            // List of suggested IDs that are invalid (e.g. are backgrounds despite phrase not containing "bg")
            List<int> toRemove = new List<int>();

            // List form of percentMatches (needed for sorting).
            List<KeyValuePair<int, double>> listPercentMatches = new List<KeyValuePair<int, double>>();

            // The resulting block IDs
            List<int> results = new List<int>(amountToReturn);
            #endregion

            #region Load amountShared
            for (int i = 0; i < words.Count(); i++)
            {
                #region Check current word
                foreach (KeyValuePair<String, int> entry in
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
                    foreach (KeyValuePair<String, int> entry in
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
                    foreach (KeyValuePair<String, int> entry in
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
                    foreach (KeyValuePair<String, int> entry in
                        GetEntriesContainingString(words[i - 3] + " " + words[i - 2] + " " + words[i - 1] + " " + words[i]))
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

                percentMatches[id] = (double)amountShared[id] / (double)words.Count();
            }
            #endregion

            #region Sort all the results
            listPercentMatches = percentMatches.ToList();

            listPercentMatches.Sort((firstPair, nextPair) =>
            {
                return amountShared[firstPair.Key].CompareTo(amountShared[nextPair.Key]);
            });

            listPercentMatches.Reverse();

            listPercentMatches.Sort((firstPair, nextPair) =>
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            });

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
            foreach (KeyValuePair<int, double> entry in listPercentMatches)
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
            else
            {
                return new List<int>() { -1 };
            }
            #endregion
        }

        /// <summary>
        /// Gets all the entries that contain String s
        /// </summary>
        private static List<KeyValuePair<String, int>> GetEntriesContainingString(String s)
        {
            List<KeyValuePair<String, int>> matches = new List<KeyValuePair<String, int>>();

            foreach (KeyValuePair<String, int> entry in Aliases)
            {
                if (entry.Key == s)
                {
                    matches.Add(entry);
                }
            }

            return matches;
        }

        private static Room r = new Room("PWpGz9IPyba0I");

        private static Bot b = new Bot(r, Console.ReadLine(), Console.ReadLine());

        private static List<KeyValuePair<String, int>> Aliases = new List<KeyValuePair<String, int>>();
    }
}