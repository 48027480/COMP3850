using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech2_PredictBank : UdonSharpBehaviour
{
    string[] titles = new string[]
    {
        "Michelson and Morley, 1887",
        "Meselson and Stahl, 1958",
        "Geiger and Marsden, 1909",
        "Pasteur's swan-neck flask, early 1860s",
        "Miller's spark experiment, 1952 to 1953"
    };

    string[] questions = new string[]
    {
        "As they slowly turn the whole instrument, what happens to the light and dark fringes?",
        "After exactly one generation in normal nitrogen, how does the DNA settle in the tube?",
        "What happens to the alpha particles when they hit the foil?",
        "While the curved neck stays whole, what happens to the broth?",
        "After about a week of sparks, what is found in the water?"
    };

    string[] details = new string[]
    {
        "Two light beams travel at right angles, bounce back and are combined. The pattern of light and dark fringes shifts if one beam takes longer than the other.",
        "The optics sit on a stone slab floating on mercury so the whole thing can be turned smoothly. Earth's motion through the 'ether' is expected to slow one beam more than the other.",
        "Their calculation predicts a shift of about 0.4 of a fringe. Michelson's smaller 1881 attempt saw no shift, but it was not sensitive enough to settle the question.",

        "E. coli is grown for many generations with heavy nitrogen (15N), so all of its DNA is heavy. Then it is moved into normal nitrogen (14N) and allowed to divide.",
        "The DNA is spun in a cesium chloride solution. Heavy DNA, light DNA and half-and-half DNA each settle at a different height and show up as a band.",
        "Three ideas compete. Conservative: the old double strand stays whole. Semiconservative: each copy keeps one old strand. Dispersive: old and new are mixed through both strands.",

        "A narrow beam of alpha particles is fired at gold foil so thin it has to be supported on a sheet of glass.",
        "Each particle that strikes a screen makes a tiny flash of light, which is watched and counted by eye through a microscope.",
        "The leading model of the atom spreads its positive charge thinly through the whole atom. On that model, the particles should only be nudged by small angles.",

        "Nutrient broth is boiled in a flask to kill any microbes already in it.",
        "The flask's neck is drawn out and bent into a long S-shaped curve. It is left open, so air can still reach the broth.",
        "Earlier, Pasteur pulled air through a cotton filter and found the cotton full of microorganisms.",

        "A sealed glass apparatus holds methane, ammonia, hydrogen and water vapour, with no oxygen.",
        "Water is heated so it evaporates like an ocean, and electric sparks pass through the gas like lightning.",
        "The idea being tested is that early Earth's atmosphere, not living things, could make the chemicals life is built from."
    };

    string[] options = new string[]
    {
        "A shift of about 0.4 fringe, as predicted",
        "A much bigger shift than predicted",
        "No meaningful shift at any angle",
        "A shift only at certain times of day",

        "Two bands: one heavy, one light",
        "One band at an in-between density",
        "One band at light density",
        "Three bands: heavy, in-between and light",

        "All pass through with only small nudges",
        "Most pass through, but a very few bounce back by more than 90 degrees",
        "Most of them bounce back",
        "The foil absorbs nearly all of them",

        "It turns cloudy within a few days",
        "It turns cloudy only at the surface",
        "It stays clear",
        "Mould grows down the neck into the broth",

        "Nothing new, only the starting gases and water",
        "Amino acids, including glycine and alanine",
        "Simple living cells",
        "Only soot and tar"
    };

    int[] correct = new int[] { 2, 1, 1, 2, 1 };

    string[] hint1 = new string[]
    {
        "Each of you holds one piece of the setup. Share all three before you vote.",
        "Each of you holds one piece of the setup. Work out what each model would give.",
        "Each of you holds one piece of the setup. What does the leading model expect, and why might it be wrong?",
        "Each of you holds one piece of the setup. What can get through the curve, and what can't?",
        "Each of you holds one piece of the setup. What was the experiment designed to make?"
    };

    string[] hint2 = new string[]
    {
        "This is one of the most famous results in physics because of what it did not find.",
        "The conservative model would give two separate bands. Something else happened.",
        "The surprise was a tiny number of particles doing something nobody expected.",
        "Air gets in. Does dust?",
        "The building blocks of proteins."
    };

    string[] correctTexts = new string[]
    {
        "Right. They saw no meaningful shift. This null result undercut the idea of a still ether and is often seen as a step toward special relativity.",
        "Right. One in-between band ruled out conservative copying. After a second generation they saw in-between and light bands in equal amounts, which ruled out dispersive copying too.",
        "Right. Most went nearly straight through, but a tiny fraction came back. In 1911 Rutherford explained it with a small, dense nucleus.",
        "Right. The broth stayed clear because dust and microbes were trapped in the curve. Break the neck off and the broth soon turned cloudy.",
        "Right. Miller found amino acids, the building blocks of proteins, including glycine and alanine (Science, 1953). Later reanalysis of his saved samples found even more."
    };

    string[] revealTexts = new string[]
    {
        "The result on record: no meaningful fringe shift, against a predicted 0.4 fringe.",
        "The result on record: one band at an in-between density, which supports semiconservative copying.",
        "The result on record: most passed through, and a very small fraction were turned back by more than 90 degrees.",
        "The result on record: the broth stayed clear while the neck was intact.",
        "The result on record: amino acids, including glycine and alanine."
    };

    public int Count() { return titles.Length; }
    public string GetTitle(int c) { return titles[c]; }
    public string GetQuestion(int c) { return questions[c]; }
    public string GetDetail(int c, int i) { return details[c * 3 + i]; }
    public string GetOption(int c, int i) { return options[c * 4 + i]; }
    public int GetCorrect(int c) { return correct[c]; }
    public string GetHint1(int c) { return hint1[c]; }
    public string GetHint2(int c) { return hint2[c]; }
    public string GetCorrectText(int c) { return correctTexts[c]; }
    public string GetRevealText(int c) { return revealTexts[c]; }
}
