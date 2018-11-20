using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GameMode
{
    preGame,
    loading,
    makeLevel,
    levelPrep,
    inLevel
}

public class WordGame : MonoBehaviour
{

    static public WordGame S;

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
	public Color[] wyrdPalette;


    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform letterAnchor, bigLetterAnchor;

    void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    void Start()
    {
        mode = GameMode.loading;

        WordList.INIT();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;

        currLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            // Pick a random level
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {

        }

        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));

        return level;
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.GET_WORDS();

        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];

            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }

            if (i % WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                yield return null;
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();

        // The coroutine is complete, so call SubWordSearchComplete()
        SubWordSearchComplete();
    }

    // Use LINQ to sortthe array received and return a copy
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();
    }

    void Layout()
    {
        // Place the Letters for each subword of currLevel on screen
        wyrds = new List<Wyrd>();

        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        // Determine how many rows of Letters will fit on screen
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        // Make a Wyrd of each level.subWord
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            // if the word is longer than columnWidth, expand it
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // Instantiate a PrefabLetter for each letter of the word
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];
                go = Instantiate<GameObject>(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;

                // Position the Letter
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

                // The % here makes multiple columns line up
                pos.y -= (i % numRows) * letterSize;

				// Move the lett immediately to a position above the screen
				lett.posImmediate = pos + Vector3.up * (20 + i % numRows);

				// Then set the pos for it to interpolate to
                lett.pos = pos;

				// Increment lett.startTime to move wyrds at different times
				lett.timeStart = Time.time + i * 0.05f;

                go.transform.localScale = Vector3.one * letterSize;

                wyrd.Add(lett);
            }

            if (showAllWyrds) wyrd.visible = true;

			// Color the wyrd based on length
			wyrd.Color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];

            wyrds.Add(wyrd);

            // If we've gotten to the numRows(th) row, start a new column
            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        // Place the big Letters
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        // Create a big letter for each letter in the target word
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            // This is similar to the process of a normal letter
            c = currLevel.word[i];
            go = Instantiate<GameObject>(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            // Set the initial position of the big Letters below screen
            pos = new Vector3(0, -100, 0);

			lett.posImmediate = pos;
            lett.pos = pos;

			// Increment lett.timeStart to have big letters come in last
			lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
			lett.easingCuve = Easing.Sin + "-0.18";	// Bouncy easing

            col = bigColorDim;
            lett.color = col;
            lett.visible = true;    // Always 'true' for big letters
            lett.big = true;
            bigLetters.Add(lett);
        }

        bigLetters = ShuffleLetters(bigLetters);	// Shuffle the big Letters
        ArrangeBigLetters();                        // Arrange them on screen

        // Set the mode to be in-game
        mode = GameMode.inLevel;
    }

    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while (letts.Count > 0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return newL;
    }

    void ArrangeBigLetters()
    {
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }

        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

	void Update()
    {
        Letter ltr;
        char c;

        switch (mode)
        {
            case GameMode.inLevel:
                // Iterate through each char input by the player this frame
                foreach (char cIt in Input.inputString)
                {
                    c = System.Char.ToUpperInvariant(cIt);

                    if (upperCase.Contains(c))
                    {
                        ltr = FindNextLetterByChar(c);

                        if (ltr != null)
                        {
                            testWord += c.ToString();

                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected;

                            ArrangeBigLetters();
                        }
                    }

                    if (c == '\b')
                    {
                        if (bigLettersActive.Count == 0) return;
                        if (testWord.Length > 1)
                        {
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        }
                        else
                        {
                            testWord = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1];

                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim;

                        ArrangeBigLetters();
                    }

                    if (c == '\n' || c == '\r')
                    {
                        CheckWord();
                    }

                    if (c == ' ')
                    {
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
    }

	Letter FindNextLetterByChar(char c)
    {
        foreach (Letter ltr in bigLetters)
        {
            if (ltr.c == c)
            {
                return ltr;
            }
        }
        return null;
    }

	public void CheckWord()
    {
        string subWord;
        bool foundTestWord = false;

        List<int> containedWords = new List<int>();

        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            if (wyrds[i].found)
            {
                continue;
            }

            subWord = currLevel.subWords[i];

            if (string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
				ScoreManager.SCORE(wyrds[i], 1);
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)
        {
            int numContained = containedWords.Count;
            int ndx = 0;

            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
				ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);
            }


        }
        ClearBigLettersActive();
    }

	// Highlights a Wyrd
	void HighlightWyrd(int ndx)
    {
		// Activate the subWord
        wyrds[ndx].found = true;	// Let it know it's been found

		// Lighten its color
        wyrds[ndx].Color = (wyrds[ndx].Color + Color.white) / 2f;
        wyrds[ndx].visible = true;
    }

	// Remove all the Letters from bigLettersActive
	void ClearBigLettersActive()
    {
        testWord = "";
        foreach (Letter ltr in bigLettersActive)
        {
            bigLetters.Add(ltr);
            ltr.color = bigColorDim;
        }
        bigLettersActive.Clear();
        ArrangeBigLetters();
    }
}
