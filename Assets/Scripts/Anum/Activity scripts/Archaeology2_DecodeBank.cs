using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Archaeology2_DecodeBank : UdonSharpBehaviour
{
    public string[] scriptNames = new string[]
    {
        "Egyptian Hieroglyphs",
        "Linear B",
        "Cuneiform",
        "Maya Glyphs",
        "Ogham"
    };

    public string[] cardText = new string[]
    {
        "𐀒 𐀜 𐀰", "ᚋ ᚐ ᚊ", "𓅓 𓂋 𓄿", "𐎭 𐎠 𐎼",
        "𐀒 𐀜 𐀰", "𓅓 𓂋 𓄿", "𝋠 𝋥 𝋭", "ᚋ ᚐ ᚊ",
        "ᚋ ᚐ ᚊ", "𝋠 𝋥 𝋭", "𓅓 𓂋 𓄿", "𐎭 𐎠 𐎼",
        "𐎭 𐎠 𐎼", "𝋠 𝋥 𝋭", "𐀒 𐀜 𐀰", "𓅓 𓂋 𓄿",
        "𝋠 𝋥 𝋭", "𐎭 𐎠 𐎼", "ᚋ ᚐ ᚊ", "𐀒 𐀜 𐀰"
    };

    public int[] cardCorrect = new int[] { 2, 0, 3, 1, 2 };

    [TextArea(3, 6)]
    public string[] keyText = new string[]
    {
        "𓊪  P      𓏏  T      𓅱  O\n𓃭  L      𓅓  M      𓇋  I\n𓋴  S      𓎡  K      𓄿  A\n𓂧  D      𓂋  R",
        "𐀒  KO      𐀜  NO      𐀰  SO\n𐀴  TI      𐀪  RI      𐀡  PO\n𐀆  DE      𐀀  A      𐀖  MI\n𐀛  NI",
        "𐎭  DA      𐎠  A      𐎼  RA\n𐎹  YA      𐎺  VA      𐎢  U\n𐏁  SHA      𐎧  XA      𐎰  THA\n𐎡  I      𐏀  ZA      𐎣  KA",
        "𝋠  0      𝋡  1      𝋢  2\n𝋥  5      𝋦  6      𝋪  10\n\nA dot is 1, a bar is 5 and the shell is 0.",
        "ᚁ  B      ᚂ  L      ᚅ  N\nᚉ  C      ᚊ  Q      ᚋ  M\nᚌ  G      ᚐ  A      ᚑ  O\nᚒ  U      ᚓ  E      ᚔ  I\n\nRead from ᚛ to ᚜."
    };

    public string[] seg1Text = new string[]
    {
        "𓊪𓏏𓅱𓃭𓅓𓇋𓋴",
        "𐀒𐀜𐀰",
        "𐎭𐎠𐎼𐎹𐎺𐎢𐏁",
        "𝋤",
        "᚛ᚋᚐᚊᚔ᚜"
    };

    public string[] seg2Text = new string[]
    {
        "𓎡𓃭𓇋𓅱𓊪𓄿𓂧𓂋𓄿",
        "𐀴𐀪𐀡𐀆",
        "𐎧𐏁𐎠𐎹𐎰𐎡𐎹   𐎺𐏀𐎼𐎣",
        "𝋭   𝋠   𝋠   𝋠   𝋠",
        "᚛ᚋᚒᚉᚑᚔ᚜"
    };

    public string[] seg1Options = new string[]
    {
        "K-T-O-L-M-I-S", "P-T-O-L-M-I-S", "P-T-O-M-L-I-S", "P-D-O-L-M-I-S",
        "KO-NO-SO", "NO-KO-SO", "KO-NI-SO", "KO-NO-PO",
        "DA-A-RA-VA-YA-U-SHA", "RA-A-DA-YA-VA-U-SHA", "DA-A-RA-YA-VA-I-SHA", "DA-A-RA-YA-VA-U-SHA",
        "5", "2", "4", "9",
        "MAGI", "NAQI", "MAQE", "MAQI"
    };

    public string[] seg2Options = new string[]
    {
        "K-L-I-O-P-A-T-R-A", "P-L-I-O-K-A-D-R-A", "K-L-I-O-P-A-D-L-A", "K-L-I-O-P-A-D-R-A",
        "TI-RI-PO-A", "TI-NI-PO-DE", "TI-RI-PO-DE", "RI-TI-PO-DE",
        "XA-SHA-A-YA-THA-I-YA  VA-ZA-RA-DA", "XA-SHA-A-YA-THA-I-YA  VA-ZA-RA-KA", "XA-SHA-A-YA-THA-U-YA  VA-ZA-RA-KA", "KA-SHA-A-YA-THA-I-YA  VA-ZA-RA-XA",
        "13.0.0.0.0", "8.0.0.0.0", "13.5.0.0.0", "3.0.0.0.0",
        "MUQOI", "MUCOI", "MUNOI", "MUCOE"
    };

    public int[] seg1Correct = new int[] { 1, 0, 3, 2, 3 };
    public int[] seg2Correct = new int[] { 3, 2, 1, 0, 1 };

    public string[] seg1Meaning = new string[]
    {
        "Part one: P-T-O-L-M-I-S, the name Ptolemy, written inside a royal oval called a cartouche.",
        "Part one: KO-NO-SO, Knossos, the palace on Crete where many of these clay tablets were found.",
        "Part one: DA-A-RA-YA-VA-U-SHA, the name of the Persian king Darius.",
        "Part one: 4, as in 4 Ajaw, the Maya day on which a great calendar cycle began and ended.",
        "Part one: MAQI, which means 'son of'. It is part of the most common Ogham stone formula, 'X son of Y'."
    };

    public string[] seg2Meaning = new string[]
    {
        "Part two: K-L-I-O-P-A-D-R-A, Cleopatra. The signs for P, L and O are shared with Ptolemy, which helped prove the readings.",
        "Part two: TI-RI-PO-DE, 'tripods'. A tablet found at Pylos in 1952 lists tripod cauldrons next to little drawings of them. Read with Ventris's sign values in 1953, the words matched the drawings.",
        "Part two: XA-SHA-A-YA-THA-I-YA VA-ZA-RA-KA, 'great king'. Darius calls himself this at Behistun.",
        "Part two: 13.0.0.0.0, the end of the 13th b'ak'tun, a cycle of about 5,125 years. It fell on about 21 December 2012 (some experts say 23 December), the end of a calendar cycle, not the end of the world.",
        "Part two: MUCOI, usually translated 'of the tribe of', though experts still debate its exact meaning. Ogham stones usually give a person's name and their family."
    };

    public string[] seg1Hint1 = new string[]
    {
        "Not quite. Match each sign to the key one at a time, left to right.",
        "Not quite. Each Linear B sign stands for a whole syllable. Match all three.",
        "Not quite. Match each wedge sign to the key, left to right.",
        "Not quite. Count the dots. Is there a bar?",
        "Not quite. Count the strokes on each letter carefully."
    };

    public string[] seg1Hint2 = new string[]
    {
        "Look at the second sign: it is the bread loaf, not the hand.",
        "The first sign is KO and the middle one is NO.",
        "The sixth sign is U, not I.",
        "There is no bar, just four dots.",
        "The third letter has five short strokes on one side of the line. That is Q, not G."
    };

    public string[] seg2Hint1 = new string[]
    {
        "Not quite. Check each sign again, especially the hand and the mouth.",
        "Not quite. Check the second sign. Is it RI or NI?",
        "Not quite. Check the last sign of the second word.",
        "Not quite. Read the first sign again: each bar is 5 and each dot is 1.",
        "Not quite. Check the third letter. Count its strokes."
    };

    public string[] seg2Hint2 = new string[]
    {
        "The hand is D and the mouth is R.",
        "The second sign is RI and the last is DE.",
        "The last sign of the second word is KA.",
        "Two bars and three dots make 13, and the shells are 0.",
        "Four strokes is C. Five would be N."
    };

    public string[] facts = new string[]
    {
        "Decoded using the Rosetta Stone's three parallel texts. Jean-Francois Champollion announced the breakthrough in 1822.",
        "An early Greek writing system used by the Mycenaeans. Deciphered in 1952 by Michael Ventris, an architect working on it in his spare time.",
        "One of the earliest writing systems, from Mesopotamia. A key breakthrough came from the Behistun Inscription, copied at real risk by Henry Rawlinson in the 1830s and 1840s. Its Old Persian cuneiform, the signs you just read, was cracked first.",
        "A writing system from Mesoamerica. Progress stalled for decades until Yuri Knorozov's 1952 article showing that many glyphs stood for sounds, not just pictures.",
        "An early medieval script used to write Old Irish, cut as straight lines across the edge of a standing stone rather than written flat on its face."
    };

    public int GetCount()
    {
        return scriptNames.Length;
    }
}
