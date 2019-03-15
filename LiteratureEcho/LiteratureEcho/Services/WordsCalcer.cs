using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteratureEcho.Services
{
    public class WordsCalcer
    {
        private readonly IEnumerable<KeyValuePair<int[], string>> _arraysWithAnswers;
        private readonly Dictionary<string, int> _indexMap;
        private int _count = 0;
        public WordsCalcer(IEnumerable<KeyValuePair<string, string>> input)
        {
            Dictionary<string, int> words = new Dictionary<string, int>();
            var keyValuePairs = input as KeyValuePair<string, string>[] ?? input.ToArray();
            foreach (var a in keyValuePairs)
            {
                foreach (var b in a.Key.Split())
                    if (!string.IsNullOrEmpty(b))
                    {
                        if (words.ContainsKey(b))
                            words[b]++;
                        else
                            words.Add(b, 1);
                    }
            }
            _indexMap = words.AsEnumerable().Where(x=>x.Value<5).ToDictionary(x => x.Key, x => _count++);
            _arraysWithAnswers = keyValuePairs.Select(x => new KeyValuePair<int[], string>(GetStringArray(x.Key), x.Value)).ToList();
        }
        public string FindAnswer(string input)
        {
            var array = GetStringArray(input);
            return _arraysWithAnswers.OrderBy(x => FindDiff(x.Key, array)).FirstOrDefault().Value;
        }

        private static int FindDiff(int[] fst, int[] snd)
        {
            int size = fst.Count();
            int diffSum = 0;
            for (int i = 0; i < size; ++i)
            {
                if (fst[i] == 0 || snd[i] == 0)
                {
                    
                }
                else
                {
                    diffSum++;
                }
            }
            return diffSum;
        }

        private int[] GetStringArray(string inputString)
        {
            var returningAnswer = new int[_count];
            for (int i = 0; i < _count; ++i)
                returningAnswer[i] = 0;
            foreach (var word in inputString.Split())
            {
                if (!string.IsNullOrEmpty(word) && _indexMap.ContainsKey(word))
                    returningAnswer[_indexMap[word]]++;
            }

            return returningAnswer;
        }
    }
}
